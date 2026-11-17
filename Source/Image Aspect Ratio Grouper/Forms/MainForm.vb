Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms


Imports ImageAspectRatioGrouper.Controls
Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Theming

#End Region

Namespace ImageAspectRatioGrouper.Forms

    ''' <summary>
    ''' The main window: configuration, execution controls and the virtual console.
    ''' <para></para>
    ''' This class only orchestrates the UI. All the image logic lives in <see cref="ImageGroupingService"/>.
    ''' </summary>
    Partial Public NotInheritable Class MainForm

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The culture used for numeric formatting in the UI (thousands separators, etc).
        ''' </summary>
        Private Shared ReadOnly FormatCulture As CultureInfo = CultureInfo.GetCultureInfo("en-US")

        ''' <summary>
        ''' The UI-agnostic grouping service.
        ''' </summary>
        Private ReadOnly groupingService As New ImageGroupingService()

        ''' <summary>
        ''' Reusable buffer that receives the drained progress reports (UI thread only).
        ''' </summary>
        Private ReadOnly drainedReports As New List(Of ImageGroupingProgress)()

#End Region

#Region " Fields "

        ''' <summary>
        ''' The cancellation source of the running operation, or <see langword="Nothing"/> when idle.
        ''' </summary>
        Private cancellationSource As CancellationTokenSource

        ''' <summary>
        ''' The progress sink of the running operation, or <see langword="Nothing"/> when idle.
        ''' Worker threads only enqueue into it; <see cref="progressTimer"/> drains it on the UI thread.
        ''' </summary>
        Private progressSink As BufferedProgress(Of ImageGroupingProgress)

        ''' <summary>
        ''' Whether an operation is running.
        ''' </summary>
        Private isProcessing As Boolean

        ''' <summary>
        ''' Whether the user asked to close the window while an operation was running.
        ''' </summary>
        Private isCloseRequested As Boolean

        ''' <summary>
        ''' Whether the persisted preferences have already been loaded into the controls.
        ''' <para></para>
        ''' Until then, the "save preference" reaction of the control change events is suppressed.
        ''' This covers <c>InitializeComponent</c> too: the designer assigns default values (e.g. the
        ''' numeric boxes' <c>Value</c>), which raise change events that must never overwrite the persisted values.
        ''' </summary>
        Private arePreferencesLoaded As Boolean

        ''' <summary>
        ''' The highest processed count reported for the running operation
        ''' (reports from parallel workers may arrive slightly out of order).
        ''' </summary>
        Private highestProcessedCount As Integer

        ''' <summary>
        ''' The total file count reported for the running operation.
        ''' </summary>
        Private reportedTotalCount As Integer

        ''' <summary>
        ''' Whether the running operation has reported at least one error.
        ''' </summary>
        Private hasErrors As Boolean

        ''' <summary>
        ''' Whether the running operation has reported at least one warning.
        ''' </summary>
        Private hasWarnings As Boolean

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="MainForm"/> class.
        ''' </summary>
        Public Sub New()
            Me.InitializeComponent()

            Me.Text = $"{My.Application.Info.Title} v{My.Application.Info.Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpper()})"
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnLoad(e As EventArgs)
            MyBase.OnLoad(e)

            Me.LoadPreferences()
            Me.AttachDragDropHandlers()
            Me.PropagateToolTipsToChildControls()
            Me.WriteWelcomeBanner()
            Me.ResetProgress()
            Me.UpdateUiState()

            ' The command-line argument of the main window: a directory to select right away, or an error to report.
            Dim startup As StartupArguments = StartupArguments.Current
            If startup.Directory IsNot Nothing Then
                Me.SetTargetDirectory(startup.Directory)
            ElseIf startup.ErrorMessage IsNot Nothing Then
                Me.logConsole.AppendLine($"[ERROR] {startup.ErrorMessage}", LogSeverity.Error)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
            If Me.isProcessing Then
                e.Cancel = True

                If Me.isCloseRequested Then
                    Return ' Already waiting for the cancellation to finish.
                End If

                Dim answer As DialogResult =
                    MessageBox.Show(Me,
                                    "An operation is in progress." & Environment.NewLine &
                                    "Do you want to abort it and exit the application?",
                                    My.Application.Info.Title,
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

                If answer = DialogResult.Yes Then
                    ' The form closes itself once the background task has completely finished.
                    Me.isCloseRequested = True
                    Me.RequestCancellation()
                End If
            End If

            MyBase.OnFormClosing(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
            ' Never persist control values that were not loaded from the preferences in the first place.
            If Me.arePreferencesLoaded Then
                ' Reading Value also commits any text still being typed in the numeric boxes.
                UserPreferences.AspectRatioTolerance = Me.toleranceNumericUpDown.Value
                UserPreferences.MinimumHeight = Me.minimumHeightNumericUpDown.Value
                UserPreferences.SearchRecursively = Me.recursiveToggle.Checked
                UserPreferences.AspectRatio = Me.GetSelectedAspectRatio()
                UserPreferences.Save()
            End If

            MyBase.OnFormClosed(e)
        End Sub

#End Region

#Region " Event Handlers (Menu) "

        ''' <summary>
        ''' Handles the File > Open Directory menu item.
        ''' </summary>
        Private Sub OpenDirectoryMenuItem_Click(sender As Object, e As EventArgs) Handles openDirectoryMenuItem.Click
            Me.BrowseForDirectory()
        End Sub

        ''' <summary>
        ''' Handles the File > Exit menu item.
        ''' </summary>
        Private Sub ExitMenuItem_Click(sender As Object, e As EventArgs) Handles exitMenuItem.Click
            Me.Close()
        End Sub

        ''' <summary>
        ''' Handles the Help > Command-Line Reference menu item.
        ''' </summary>
        Private Sub CommandLineReferenceMenuItem_Click(sender As Object, e As EventArgs) Handles commandLineReferenceMenuItem.Click
            Using referenceForm As New CommandLineReferenceForm()
                referenceForm.ShowDialog(Me)
            End Using
        End Sub

        ''' <summary>
        ''' Handles the Help > About menu item.
        ''' </summary>
        Private Sub AboutMenuItem_Click(sender As Object, e As EventArgs) Handles aboutMenuItem.Click
            Using aboutBox As New AboutBox()
                aboutBox.ShowDialog(Me)
            End Using
        End Sub

        ''' <summary>
        ''' Switches and persists the theme (toggle in the right corner of the menu bar).
        ''' </summary>
        Private Sub ThemeToggleItem_CheckedChanged(sender As Object, e As EventArgs) Handles themeToggleItem.CheckedChanged
            If Not Me.arePreferencesLoaded Then
                Return
            End If

            Dim theme As AppTheme = If(Me.themeToggleItem.Checked, AppTheme.Dark, AppTheme.Light)
            ThemeManager.SetTheme(theme)
            Me.UpdateThemeCaption()
            UserPreferences.Theme = theme
            UserPreferences.Save()
        End Sub

#End Region

#Region " Event Handlers (Configuration) "

        ''' <summary>
        ''' Handles the Browse button.
        ''' </summary>
        Private Sub BrowseButton_Click(sender As Object, e As EventArgs) Handles browseButton.Click
            Me.BrowseForDirectory()
        End Sub

        ''' <summary>
        ''' Persists the recursion preference.
        ''' </summary>
        Private Sub RecursiveToggle_CheckedChanged(sender As Object, e As EventArgs) Handles recursiveToggle.CheckedChanged
            If Not Me.arePreferencesLoaded Then
                Return
            End If

            UserPreferences.SearchRecursively = Me.recursiveToggle.Checked
            UserPreferences.Save()
        End Sub

        ''' <summary>
        ''' Persists the aspect ratio preference.
        ''' </summary>
        Private Sub AspectRatioComboBox_SelectedIndexChanged(sender As Object, e As EventArgs) Handles aspectRatioComboBox.SelectedIndexChanged
            If Not Me.arePreferencesLoaded Then
                Return
            End If

            UserPreferences.AspectRatio = Me.GetSelectedAspectRatio()
            UserPreferences.Save()
        End Sub

        ''' <summary>
        ''' Persists the tolerance preference.
        ''' </summary>
        Private Sub ToleranceNumericUpDown_ValueChanged(sender As Object, e As EventArgs) Handles toleranceNumericUpDown.ValueChanged
            If Not Me.arePreferencesLoaded Then
                Return
            End If

            UserPreferences.AspectRatioTolerance = Me.toleranceNumericUpDown.Value
            UserPreferences.Save()
        End Sub

        ''' <summary>
        ''' Persists the minimum height preference.
        ''' </summary>
        Private Sub MinimumHeightNumericUpDown_ValueChanged(sender As Object, e As EventArgs) Handles minimumHeightNumericUpDown.ValueChanged
            If Not Me.arePreferencesLoaded Then
                Return
            End If

            UserPreferences.MinimumHeight = Me.minimumHeightNumericUpDown.Value
            UserPreferences.Save()
        End Sub

#End Region

#Region " Event Handlers (Execution) "

        ''' <summary>
        ''' Handles the Start Processing button.
        ''' </summary>
        Private Async Sub StartButton_Click(sender As Object, e As EventArgs) Handles startButton.Click
            Await Me.RunGroupingOperationAsync().ConfigureAwait(False)
        End Sub

        ''' <summary>
        ''' Handles the Cancel / Abort button.
        ''' </summary>
        Private Sub AbortButton_Click(sender As Object, e As EventArgs) Handles abortButton.Click
            Me.RequestCancellation()
        End Sub

        ''' <summary>
        ''' Periodically drains the queued progress reports into the UI (one batch per tick).
        ''' </summary>
        Private Sub ProgressTimer_Tick(sender As Object, e As EventArgs) Handles progressTimer.Tick
            Me.DrainProgress(showOutcome:=False)
        End Sub

#End Region

#Region " Event Handlers (Drag & Drop) "

        ''' <summary>
        ''' Accepts the drag operation only when it contains exactly one existing directory.
        ''' </summary>
        Private Sub AnyControl_DragEnter(sender As Object, e As DragEventArgs)
            Dim directoryPath As String = Nothing
            e.Effect = If(Me.TryGetSingleDroppedDirectory(e.Data, directoryPath), DragDropEffects.Copy, DragDropEffects.None)
        End Sub

        ''' <summary>
        ''' Sets the dropped directory as the target directory.
        ''' <para></para>
        ''' The work is deferred with <see cref="Control.BeginInvoke(System.Delegate)"/> because exceptions thrown inside OLE
        ''' drag and drop callbacks are swallowed by the OLE runtime. Running outside the callback keeps the fail-fast
        ''' behavior (exceptions reach the application's unhandled exception handler) and releases the drag source
        ''' (e.g. Explorer) immediately.
        ''' </summary>
        Private Sub AnyControl_DragDrop(sender As Object, e As DragEventArgs)
            Dim directoryPath As String = Nothing
            Dim isValid As Boolean = Me.TryGetSingleDroppedDirectory(e.Data, directoryPath)

            Me.BeginInvoke(New Action(
                Sub()
                    If Not isValid Then
                        Throw New InvalidOperationException("The dropped data is not a single, existing directory (it was validated on DragEnter).")
                    End If

                    Me.SetTargetDirectory(directoryPath)
                    Me.Activate()
                End Sub))
        End Sub

#End Region

#Region " Private Methods (Operation) "

        ''' <summary>
        ''' Validates the input, asks for confirmation and runs the grouping operation on a background thread.
        ''' </summary>
        ''' <returns>A task that completes when the operation has finished and the UI has been updated.</returns>
        Private Async Function RunGroupingOperationAsync() As Task

            ' ---- UI thread (until the first Await) ----

            If Me.isProcessing Then
                Throw New InvalidOperationException("An operation is already in progress.")
            End If

            ' Always start from a clean progress bar, even if the validation below fails.
            Me.ResetProgress()

            Dim directoryPath As String = Me.directoryTextBox.Text
            If String.IsNullOrWhiteSpace(directoryPath) Then
                MessageBox.Show(Me, "Select a target directory first.", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If Not Directory.Exists(directoryPath) Then
                Me.logConsole.AppendLine($"[ERROR] The target directory does not exist: {directoryPath}", LogSeverity.Error)
                Me.statusLabel.Text = "The target directory does not exist."
                MessageBox.Show(Me, $"The target directory does not exist:{Environment.NewLine}{directoryPath}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim aspectRatio As AspectRatio = Me.GetSelectedAspectRatio()
            Dim searchRecursively As Boolean = Me.recursiveToggle.Checked
            Dim tolerancePixels As Integer = Me.toleranceNumericUpDown.Value
            Dim minimumHeight As Integer = Me.minimumHeightNumericUpDown.Value
            Dim scopeDescription As String = If(searchRecursively, "and all its subdirectories", "(top directory only)")

            Dim answer As DialogResult =
                MessageBox.Show(Me,
                                $"Every image inside:{Environment.NewLine}{directoryPath}{Environment.NewLine}{scopeDescription}" & Environment.NewLine & Environment.NewLine &
                                $"will be MOVED into a subfolder according to the {aspectRatio.Key} aspect ratio." & Environment.NewLine & Environment.NewLine &
                                "Do you want to start the processing operation?",
                                My.Application.Info.Title,
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question)

            If answer <> DialogResult.Yes Then
                Me.logConsole.AppendLine("Operation aborted by the user.", LogSeverity.Warning)
                Return
            End If

            Dim options As New ImageGroupingOptions(directoryPath, searchRecursively, aspectRatio, tolerancePixels, minimumHeight)

            Me.cancellationSource = New CancellationTokenSource()
            Me.progressSink = New BufferedProgress(Of ImageGroupingProgress)()
            Me.SetProcessingState(True)

            Me.logConsole.AppendLine(String.Empty, LogSeverity.Information)
            Me.logConsole.AppendLine(ImageGroupingReport.Rule, LogSeverity.Muted)
            Me.logConsole.AppendLine($"[START] {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", MainForm.FormatCulture)}", LogSeverity.Information)
            Me.logConsole.AppendLine(ImageGroupingReport.Rule, LogSeverity.Muted)
            Me.progressTimer.Start()

            Dim progress As IProgress(Of ImageGroupingProgress) = Me.progressSink
            Dim cancellationToken As CancellationToken = Me.cancellationSource.Token

            ' ---- From here on, continuations run on the thread pool (ConfigureAwait(False)) ----
            ' ---- and every UI access is explicitly marshaled through InvokeOnUiThread.        ----

            Try
                Dim summary As ImageGroupingSummary =
                    Await Me.groupingService.RunAsync(options, progress, cancellationToken).ConfigureAwait(False)

                Me.InvokeOnUiThread(Sub() Me.OnGroupingCompleted(summary))

            Catch ex As OperationCanceledException
                Me.InvokeOnUiThread(AddressOf Me.OnGroupingCancelled)

            Catch ex As Exception
                Me.InvokeOnUiThread(Sub() Me.OnGroupingFailed(ex))

            Finally
                Me.InvokeOnUiThread(AddressOf Me.OnGroupingFinished)

            End Try
        End Function

        ''' <summary>
        ''' Moves every queued progress report into the console and refreshes the progress bar once.
        ''' Must be called from the UI thread while an operation is running.
        ''' </summary>
        ''' <param name="showOutcome">Whether the operation has already ended (see <see cref="RefreshProgressBar"/>).</param>
        ''' <exception cref="InvalidOperationException">Thrown when there is no operation in progress.</exception>
        Private Sub DrainProgress(showOutcome As Boolean)
            If Me.progressSink Is Nothing Then
                Throw New InvalidOperationException("There is no progress sink to drain: no operation is in progress.")
            End If

            Me.drainedReports.Clear()
            If Me.progressSink.DrainTo(Me.drainedReports) = 0 Then
                Return
            End If

            For Each report As ImageGroupingProgress In Me.drainedReports
                Me.logConsole.AppendLine(report.Message, report.Severity)

                Me.reportedTotalCount = report.TotalCount
                Me.highestProcessedCount = Math.Max(Me.highestProcessedCount, report.ProcessedCount)

                Select Case report.Severity
                    Case LogSeverity.Error
                        Me.hasErrors = True
                    Case LogSeverity.Warning
                        Me.hasWarnings = True
                End Select
            Next

            Me.drainedReports.Clear()
            Me.logConsole.Flush()
            Me.RefreshProgressBar(showOutcome)
        End Sub

        ''' <summary>
        ''' Reports the final statistics of a successful operation.
        ''' </summary>
        ''' <param name="summary">The operation summary.</param>
        Private Sub OnGroupingCompleted(summary As ImageGroupingSummary)
            If summary Is Nothing Then
                Throw New ArgumentNullException(NameOf(summary))
            End If

            Me.DrainProgress(showOutcome:=False)

            For Each line As LogLine In ImageGroupingReport.GetSummaryLines(summary)
                Me.logConsole.AppendLine(line.Text, line.Severity)
            Next

            Dim elapsed As String = ImageGroupingReport.FormatElapsed(summary.Elapsed)
            If summary.FailedCount > 0 Then
                Me.statusLabel.Text = $"Completed with {ImageGroupingReport.FormatCount(summary.FailedCount)} failure(s) in {elapsed}."
            ElseIf summary.MovedToIncompatibleCount > 0 Then
                Me.statusLabel.Text = $"Completed in {elapsed}, with {ImageGroupingReport.FormatCount(summary.MovedToIncompatibleCount)} incompatible image(s)."
            Else
                Me.statusLabel.Text = $"Completed successfully in {elapsed}."
            End If

            Me.RefreshProgressBar(showOutcome:=True)
        End Sub

        ''' <summary>
        ''' Reports a cancelled operation.
        ''' </summary>
        Private Sub OnGroupingCancelled()
            Me.DrainProgress(showOutcome:=False)

            Dim processed As String = Me.highestProcessedCount.ToString("N0", MainForm.FormatCulture)
            Dim total As String = Me.reportedTotalCount.ToString("N0", MainForm.FormatCulture)

            Me.logConsole.AppendLine($"[ABORTED] Operation cancelled by the user after {processed} of {total} image(s). Already moved files were left in place.", LogSeverity.Warning)
            Me.statusLabel.Text = $"Cancelled by the user ({processed} of {total} processed)."

            Me.hasWarnings = True
            Me.RefreshProgressBar(showOutcome:=True)
        End Sub

        ''' <summary>
        ''' Reports a fatal error that stopped the whole operation.
        ''' </summary>
        ''' <param name="exception">The exception.</param>
        Private Sub OnGroupingFailed(exception As Exception)
            If exception Is Nothing Then
                Throw New ArgumentNullException(NameOf(exception))
            End If

            Me.DrainProgress(showOutcome:=False)

            Me.logConsole.AppendLine($"[FATAL ERROR] {exception.GetType().Name}: {exception.Message}", LogSeverity.Error)
            Me.statusLabel.Text = "The operation failed. See the output log for details."

            Me.hasErrors = True
            Me.RefreshProgressBar(showOutcome:=True)

            MessageBox.Show(Me, exception.ToString(), $"{My.Application.Info.Title} - Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Sub

        ''' <summary>
        ''' Releases the operation resources and restores the idle UI state. Always runs, whatever the outcome.
        ''' </summary>
        Private Sub OnGroupingFinished()
            If Me.cancellationSource Is Nothing Then
                Throw New InvalidOperationException("The cancellation source was released before the operation finished.")
            End If

            Me.progressTimer.Stop()
            Me.DrainProgress(showOutcome:=True)
            Me.progressSink = Nothing

            Me.cancellationSource.Dispose()
            Me.cancellationSource = Nothing
            Me.SetProcessingState(False)
            Me.logConsole.Flush()

            If Me.isCloseRequested Then
                Me.Close()
            End If
        End Sub

        ''' <summary>
        ''' Requests the cooperative cancellation of the running operation.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when no operation is running.</exception>
        Private Sub RequestCancellation()
            If Me.cancellationSource Is Nothing Then
                Throw New InvalidOperationException("There is no operation in progress to cancel.")
            End If

            If Me.cancellationSource.IsCancellationRequested Then
                Return ' Idempotent: cancellation was already requested.
            End If

            Me.cancellationSource.Cancel()
            Me.logConsole.AppendLine("[CANCEL] Cancellation requested. Waiting for the in-flight images to finish...", LogSeverity.Warning)
            Me.statusLabel.Text = "Cancelling..."
            Me.UpdateUiState()
        End Sub

        ''' <summary>
        ''' Executes an action on the UI thread, synchronously.
        ''' </summary>
        ''' <param name="action">The action to execute.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is <see langword="Nothing"/>.</exception>
        Private Sub InvokeOnUiThread(action As Action)
            If action Is Nothing Then
                Throw New ArgumentNullException(NameOf(action))
            End If

            If Me.InvokeRequired Then
                Me.Invoke(action)
            Else
                action()
            End If
        End Sub

#End Region

#Region " Private Methods (Progress) "

        ''' <summary>
        ''' Resets the progress counters, the progress bar (value and color) and its label.
        ''' </summary>
        Private Sub ResetProgress()
            Me.highestProcessedCount = 0
            Me.reportedTotalCount = 0
            Me.hasErrors = False
            Me.hasWarnings = False
            Me.RefreshProgressBar(showOutcome:=False)
        End Sub

        ''' <summary>
        ''' Pushes the current progress counters into the progress bar and its label.
        ''' </summary>
        ''' <param name="showOutcome">
        ''' <see langword="False"/> while the operation is running: the bar keeps its normal color, whatever happens.
        ''' <see langword="True"/> once the operation has ended: the bar turns red if there were errors,
        ''' or yellow if there were warnings (it stays like that until the next operation starts).
        ''' </param>
        Private Sub RefreshProgressBar(showOutcome As Boolean)
            ' Lower the value before the maximum, so the bar never holds a value above its range.
            Me.progressBar.Value = 0
            Me.progressBar.Maximum = Me.reportedTotalCount
            Me.progressBar.Value = Math.Min(Me.highestProcessedCount, Me.reportedTotalCount)

            If Not showOutcome Then
                Me.progressBar.State = ProgressBarState.Normal
            ElseIf Me.hasErrors Then
                Me.progressBar.State = ProgressBarState.Error
            ElseIf Me.hasWarnings Then
                Me.progressBar.State = ProgressBarState.Warning
            Else
                Me.progressBar.State = ProgressBarState.Normal
            End If

            Me.progressLabel.Text = $"{Me.highestProcessedCount.ToString("N0", MainForm.FormatCulture)} / {Me.reportedTotalCount.ToString("N0", MainForm.FormatCulture)}"
        End Sub

#End Region

#Region " Private Methods (UI State) "

        ''' <summary>
        ''' Loads the persisted user preferences into the controls.
        ''' </summary>
        Private Sub LoadPreferences()
            If Me.arePreferencesLoaded Then
                Throw New InvalidOperationException("The preferences have already been loaded.")
            End If

            Me.aspectRatioComboBox.Items.Clear()
            For Each ratio As AspectRatio In AspectRatio.StandardRatios
                Me.aspectRatioComboBox.Items.Add(ratio)
            Next
            Me.aspectRatioComboBox.SelectedItem = UserPreferences.AspectRatio

            Me.toleranceNumericUpDown.Minimum = UserPreferences.MinimumTolerancePixels
            Me.toleranceNumericUpDown.Maximum = UserPreferences.MaximumTolerancePixels
            Me.toleranceNumericUpDown.Value = UserPreferences.AspectRatioTolerance

            Me.minimumHeightNumericUpDown.Maximum = UserPreferences.MinimumHeightUpperBound
            Me.minimumHeightNumericUpDown.Minimum = UserPreferences.MinimumHeightLowerBound
            Me.minimumHeightNumericUpDown.Value = UserPreferences.MinimumHeight

            Me.recursiveToggle.Checked = UserPreferences.SearchRecursively
            Me.themeToggleItem.Checked = (ThemeManager.CurrentTheme = AppTheme.Dark)
            Me.UpdateThemeCaption()

            Me.arePreferencesLoaded = True
        End Sub

        ''' <summary>
        ''' Gets the aspect ratio selected in the combo box.
        ''' </summary>
        ''' <returns>The selected aspect ratio.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when nothing (or an unexpected item) is selected.</exception>
        Private Function GetSelectedAspectRatio() As AspectRatio
            Dim ratio As AspectRatio = TryCast(Me.aspectRatioComboBox.SelectedItem, AspectRatio)
            If ratio Is Nothing Then
                Throw New InvalidOperationException("No aspect ratio is selected.")
            End If

            Return ratio
        End Function

        ''' <summary>
        ''' Switches the UI between the idle and the processing states.
        ''' </summary>
        ''' <param name="processing"><see langword="True"/> when an operation starts.</param>
        Private Sub SetProcessingState(processing As Boolean)
            Me.isProcessing = processing
            Me.statusLabel.Text = If(processing, "Processing...", Me.statusLabel.Text)
            Me.UpdateUiState()
        End Sub

        ''' <summary>
        ''' Enables or disables the controls according to the current state.
        ''' </summary>
        Private Sub UpdateUiState()
            Dim isIdle As Boolean = Not Me.isProcessing
            Dim hasDirectory As Boolean = Not String.IsNullOrEmpty(Me.directoryTextBox.Text)
            Dim canCancel As Boolean = Me.isProcessing AndAlso
                                       Me.cancellationSource IsNot Nothing AndAlso
                                       Not Me.cancellationSource.IsCancellationRequested

            Me.openDirectoryMenuItem.Enabled = isIdle
            Me.browseButton.Enabled = isIdle
            Me.recursiveToggle.Enabled = isIdle
            Me.aspectRatioComboBox.Enabled = isIdle
            Me.toleranceNumericUpDown.Enabled = isIdle
            Me.minimumHeightNumericUpDown.Enabled = isIdle
            Me.startButton.Enabled = isIdle AndAlso hasDirectory
            Me.abortButton.Enabled = canCancel

            If isIdle AndAlso Not hasDirectory Then
                Me.statusLabel.Text = "Select a target directory (Browse, Ctrl+O, or drag and drop a folder)."
            End If
        End Sub

        ''' <summary>
        ''' Shows the Ookii (Vista-style) folder browser dialog.
        ''' </summary>
        Private Sub BrowseForDirectory()
            Using dialog As New Ookii.Dialogs.WinForms.VistaFolderBrowserDialog()
                dialog.Description = "Select the directory that contains the images to group"
                dialog.UseDescriptionForTitle = True
                dialog.ShowNewFolderButton = False

                Dim currentDirectory As String = Me.directoryTextBox.Text
                If Not String.IsNullOrEmpty(currentDirectory) AndAlso Directory.Exists(currentDirectory) Then
                    ' A trailing separator makes the dialog open *inside* the folder instead of selecting it in its parent.
                    dialog.SelectedPath = $"{currentDirectory.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}"
                End If

                If dialog.ShowDialog(Me) = DialogResult.OK Then
                    Me.SetTargetDirectory(dialog.SelectedPath)
                End If
            End Using
        End Sub

        ''' <summary>
        ''' Sets and displays the target directory (it is intentionally never persisted).
        ''' </summary>
        ''' <param name="directoryPath">The directory path.</param>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="directoryPath"/> is empty.</exception>
        ''' <exception cref="DirectoryNotFoundException">Thrown when the directory does not exist.</exception>
        Private Sub SetTargetDirectory(directoryPath As String)
            If String.IsNullOrWhiteSpace(directoryPath) Then
                Throw New ArgumentException("The directory path cannot be empty.", NameOf(directoryPath))
            End If

            If Not Directory.Exists(directoryPath) Then
                Throw New DirectoryNotFoundException($"The directory does not exist: {directoryPath}")
            End If

            Me.directoryTextBox.Text = directoryPath
            Me.directoryTextBox.MoveCaretToEnd()
            Me.SetToolTipIncludingChildControls(Me.directoryTextBox, directoryPath)
            Me.logConsole.AppendLine($"Target directory set to '{directoryPath}'.", LogSeverity.Muted)

            Me.statusLabel.Text = "Ready."
            Me.UpdateUiState()
        End Sub

        ''' <summary>
        ''' Shows "Dark mode" or "Light mode" as the theme toggle caption, according to the active theme.
        ''' </summary>
        Private Sub UpdateThemeCaption()
            Me.themeToggleItem.Text = If(ThemeManager.CurrentTheme = AppTheme.Dark, "Dark mode", "Light mode")
        End Sub

        ''' <summary>
        ''' Copies the tool tip of every control to all its descendants that have no tool tip of their own.
        ''' <para></para>
        ''' A <see cref="ToolTip"/> only reacts to the control directly under the mouse pointer, so a tool tip assigned
        ''' to a composite <see cref="UserControl"/> (themed text box, numeric box...) would never show, because the
        ''' pointer is always over one of its inner native controls.
        ''' </summary>
        Private Sub PropagateToolTipsToChildControls()
            Dim pending As New Stack(Of Control)()
            pending.Push(Me)

            While pending.Count > 0
                Dim control As Control = pending.Pop()
                Dim toolTipText As String = Me.toolTip.GetToolTip(control)

                If Not String.IsNullOrEmpty(toolTipText) Then
                    Me.SetToolTipIncludingChildControls(control, toolTipText)
                    Continue While ' The whole subtree has just been handled.
                End If

                For Each child As Control In control.Controls
                    pending.Push(child)
                Next
            End While
        End Sub

        ''' <summary>
        ''' Assigns a tool tip to a control and to all its descendants (iteratively).
        ''' </summary>
        ''' <param name="root">The control.</param>
        ''' <param name="toolTipText">The tool tip text.</param>
        ''' <exception cref="ArgumentNullException">Thrown when any argument is <see langword="Nothing"/>.</exception>
        Private Sub SetToolTipIncludingChildControls(root As Control, toolTipText As String)
            If root Is Nothing Then
                Throw New ArgumentNullException(NameOf(root))
            End If

            If toolTipText Is Nothing Then
                Throw New ArgumentNullException(NameOf(toolTipText))
            End If

            Dim pending As New Stack(Of Control)()
            pending.Push(root)

            While pending.Count > 0
                Dim control As Control = pending.Pop()
                Me.toolTip.SetToolTip(control, toolTipText)

                For Each child As Control In control.Controls
                    pending.Push(child)
                Next
            End While
        End Sub

        ''' <summary>
        ''' Enables drag and drop on the form and every descendant control (iteratively),
        ''' so a folder can be dropped anywhere on the window.
        ''' </summary>
        Private Sub AttachDragDropHandlers()
            Dim pending As New Stack(Of Control)()
            pending.Push(Me)

            While pending.Count > 0
                Dim control As Control = pending.Pop()
                control.AllowDrop = True
                AddHandler control.DragEnter, AddressOf Me.AnyControl_DragEnter
                AddHandler control.DragDrop, AddressOf Me.AnyControl_DragDrop

                For Each child As Control In control.Controls
                    pending.Push(child)
                Next
            End While
        End Sub

        ''' <summary>
        ''' Determines whether the drag data contains exactly one existing directory (and nothing else).
        ''' </summary>
        ''' <param name="data">The drag data.</param>
        ''' <param name="directoryPath">When this method returns <see langword="True"/>, contains the directory path.</param>
        ''' <returns><see langword="True"/> if the data is acceptable; otherwise <see langword="False"/>.</returns>
        Private Function TryGetSingleDroppedDirectory(data As IDataObject, ByRef directoryPath As String) As Boolean
            directoryPath = Nothing

            If Me.isProcessing OrElse data Is Nothing OrElse Not data.GetDataPresent(DataFormats.FileDrop) Then
                Return False
            End If

            Dim droppedPaths As String() = TryCast(data.GetData(DataFormats.FileDrop), String())
            If droppedPaths Is Nothing OrElse droppedPaths.Length <> 1 OrElse Not Directory.Exists(droppedPaths(0)) Then
                Return False
            End If

            directoryPath = droppedPaths(0)
            Return True
        End Function

        ''' <summary>
        ''' Writes the welcome text (program purpose and examples) to the virtual console.
        ''' </summary>
        Private Sub WriteWelcomeBanner()
            Dim console As LogConsoleBox = Me.logConsole
            Dim exampleRatio As AspectRatio = AspectRatio.Widescreen16x9
            Dim alreadyFolder As String = ImageGroupingOptions.GetAlreadyConformingFolderName(exampleRatio)
            Dim incompatibleFolder As String = ImageGroupingOptions.GetIncompatibleFolderName(exampleRatio)

            console.AppendLine("PURPOSE:", LogSeverity.Warning)
            console.AppendLine("Scans a directory for image files and groups them by the selected target aspect ratio:", LogSeverity.Muted)
            console.AppendLine("- Images that already conform (within the tolerance) go to the '# Already <ratio>' folder.", LogSeverity.Muted)
            console.AppendLine("- Taller images go to a folder named after their optimal downwards crop height.", LogSeverity.Muted)
            console.AppendLine("- Images wider than the ratio (a downwards crop cannot fix them), or whose resulting", LogSeverity.Muted)
            console.AppendLine("  height would be below the minimum, go to the '# Incompatible <ratio>' folder.", LogSeverity.Muted)
            console.AppendLine(String.Empty, LogSeverity.Muted)
            console.AppendLine($"EXAMPLES ({exampleRatio.Key}, tolerance {ImageGroupingOptions.DefaultTolerancePixels} px, minimum height {ImageGroupingOptions.DefaultMinimumHeight} px):", LogSeverity.Warning)
            console.AppendLine("- 1990x1323 -> '1118' (crop the height down to 1118 px).", LogSeverity.Muted)
            console.AppendLine($"- 2948x1660 -> '{alreadyFolder}' (within tolerance).", LogSeverity.Muted)
            console.AppendLine($"- 1280x28   -> '{incompatibleFolder}' (wider than {exampleRatio.Key}).", LogSeverity.Muted)
            console.AppendLine($"- 400x600   -> '{incompatibleFolder}' (would be 224 px tall).", LogSeverity.Muted)
            console.AppendLine(String.Empty, LogSeverity.Muted)
            console.AppendLine("Tip: drag and drop a single folder anywhere on this window to select it.", LogSeverity.Muted)
            console.AppendLine(ImageGroupingReport.Rule, LogSeverity.Muted)
        End Sub

#End Region

    End Class

End Namespace
