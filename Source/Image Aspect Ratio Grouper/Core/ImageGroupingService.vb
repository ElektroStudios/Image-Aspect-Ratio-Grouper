Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

#End Region

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' UI-agnostic service that scans a directory for images and groups each one by a target <see cref="AspectRatio"/>:
    ''' <list type="bullet">
    ''' <item><description>Already conforming images go to <see cref="ImageGroupingOptions.AlreadyConformingFolderName"/>.</description></item>
    ''' <item><description>Taller images go to a folder named after their optimal downwards crop height.</description></item>
    ''' <item><description>Incompatible images (too wide, or too small) go to <see cref="ImageGroupingOptions.IncompatibleFolderName"/>.</description></item>
    ''' </list>
    ''' All the heavy work runs on a thread-pool thread (<c>Task.Run</c>) and is parallelized with <c>Parallel.ForEach</c>.
    ''' Progress is reported through <see cref="IProgress(Of T)"/>, so callers never block.
    ''' </summary>
    Public NotInheritable Class ImageGroupingService

#Region " Private Types "

        ''' <summary>
        ''' Describes what happened with a single processed file.
        ''' </summary>
        Private Enum FileOutcome As Integer

            ''' <summary>
            ''' The file was moved into a crop-height folder.
            ''' </summary>
            MovedToCropFolder = 0

            ''' <summary>
            ''' The file was moved into the already-conforming folder.
            ''' </summary>
            MovedToAlreadyConforming = 1

            ''' <summary>
            ''' The file was moved into the incompatible folder.
            ''' </summary>
            MovedToIncompatible = 2

            ''' <summary>
            ''' The file already resides in its target folder (e.g. from a previous run).
            ''' </summary>
            Skipped = 3

        End Enum

        ''' <summary>
        ''' Thread-safe mutable counters shared by all the parallel iterations of a single run.
        ''' </summary>
        Private NotInheritable Class RunCounters

            ''' <summary>Number of files processed so far (any outcome).</summary>
            Public Processed As Integer

            ''' <summary>Number of files moved into a crop-height folder.</summary>
            Public MovedToCropFolder As Integer

            ''' <summary>Number of files moved into the already-conforming folder.</summary>
            Public MovedToAlreadyConforming As Integer

            ''' <summary>Number of files moved into the incompatible folder.</summary>
            Public MovedToIncompatible As Integer

            ''' <summary>Number of skipped files.</summary>
            Public Skipped As Integer

            ''' <summary>Number of failed files.</summary>
            Public Failed As Integer

        End Class

#End Region

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The fixed width of the log tags (e.g. <c>"[INCOMPATIBLE]"</c>), for column alignment.
        ''' </summary>
        Private Const TagWidth As Integer = 15

        ''' <summary>
        ''' The culture used for numeric formatting in log messages (thousands separators, etc).
        ''' </summary>
        Private Shared ReadOnly FormatCulture As CultureInfo = CultureInfo.GetCultureInfo("en-US")

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Asynchronously runs the grouping operation on a background thread.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">
        ''' The progress sink that receives log messages and counters.
        ''' <para></para>
        ''' It is invoked concurrently from several worker threads at a very high rate (once per file),
        ''' so it must be thread-safe and must not marshal every call to the UI thread.
        ''' </param>
        ''' <param name="cancellationToken">A token to cooperatively cancel the operation.</param>
        ''' <returns>
        ''' A task that completes with an <see cref="ImageGroupingSummary"/>, or faults with
        ''' <see cref="OperationCanceledException"/> when <paramref name="cancellationToken"/> is cancelled.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="progress"/> is <see langword="Nothing"/>.</exception>
        Public Function RunAsync(options As ImageGroupingOptions,
                                 progress As IProgress(Of ImageGroupingProgress),
                                 cancellationToken As CancellationToken) As Task(Of ImageGroupingSummary)

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            If progress Is Nothing Then
                Throw New ArgumentNullException(NameOf(progress))
            End If

            Return Task.Run(Function() ImageGroupingService.Run(options, progress, cancellationToken), cancellationToken)
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Synchronous core of <see cref="RunAsync"/>. Must never be called from the UI thread.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">The progress sink.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <returns>The final run statistics.</returns>
        ''' <exception cref="DirectoryNotFoundException">Thrown when the target directory does not exist.</exception>
        ''' <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        Private Shared Function Run(options As ImageGroupingOptions,
                                    progress As IProgress(Of ImageGroupingProgress),
                                    cancellationToken As CancellationToken) As ImageGroupingSummary

            Dim stopwatch As Stopwatch = Stopwatch.StartNew()

            If Not Directory.Exists(options.TargetDirectory) Then
                Throw New DirectoryNotFoundException($"The target directory does not exist: {options.TargetDirectory}")
            End If

            progress.Report(New ImageGroupingProgress(0, 0, $"Scanning '{options.TargetDirectory}' (include subdirectories: {options.SearchRecursively})...", LogSeverity.Information))
            progress.Report(New ImageGroupingProgress(0, 0, $"Target ratio: {options.AspectRatio.DisplayName} | Tolerance: ±{options.TolerancePixels} px | Minimum height: {options.MinimumHeight} px", LogSeverity.Information))

            Dim imageFiles As List(Of FileInfo) = ImageGroupingService.EnumerateImageFiles(options, progress, cancellationToken)
            Dim totalFiles As Integer = imageFiles.Count

            If totalFiles = 0 Then
                progress.Report(New ImageGroupingProgress(0, 0, "[INFO] No compatible image files were found in the target directory.", LogSeverity.Warning))
                stopwatch.Stop()
                Return New ImageGroupingSummary(0, 0, 0, 0, 0, 0, stopwatch.Elapsed)
            End If

            Dim totalFilesFormatted As String = totalFiles.ToString("N0", ImageGroupingService.FormatCulture)
            progress.Report(New ImageGroupingProgress(0, totalFiles, $"Found {totalFilesFormatted} image file(s).", LogSeverity.Information))

            Dim rootDirectoryWithSeparator As String = ImageGroupingService.NormalizeRootDirectory(options.TargetDirectory)
            Dim counters As New RunCounters()

            Dim parallelOptions As New ParallelOptions() With {
                .CancellationToken = cancellationToken,
                .MaxDegreeOfParallelism = Environment.ProcessorCount
            }

            ' When the token is cancelled, Parallel.ForEach stops scheduling new iterations
            ' and throws OperationCanceledException once the in-flight iterations finish.
            Parallel.ForEach(imageFiles, parallelOptions,
                             Sub(imageFile As FileInfo)
                                 ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                                 Thread.CurrentThread.Join(0)

                                 Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, imageFile.FullName)
                                 Dim message As String
                                 Dim severity As LogSeverity

                                 Try
                                     Dim classification As AspectRatioClassification = Nothing
                                     Dim targetFolderName As String = Nothing
                                     Dim outcome As FileOutcome = ImageGroupingService.ProcessImageFile(imageFile, options, classification, targetFolderName)
                                     Dim resolution As String = $"{classification.Width}x{classification.Height}"

                                     Select Case outcome
                                         Case FileOutcome.MovedToCropFolder
                                             Interlocked.Increment(counters.MovedToCropFolder)
                                             message = $"{FormatTag("MOVED")}'{relativePath}' ({resolution}) -> {targetFolderName}"
                                             severity = LogSeverity.Moved

                                         Case FileOutcome.MovedToAlreadyConforming
                                             Interlocked.Increment(counters.MovedToAlreadyConforming)
                                             message = $"{FormatTag("ALREADY")}'{relativePath}' ({resolution}) -> {targetFolderName}"
                                             severity = LogSeverity.Success

                                         Case FileOutcome.MovedToIncompatible
                                             Interlocked.Increment(counters.MovedToIncompatible)
                                             message = $"{FormatTag("INCOMPATIBLE")}'{relativePath}' ({resolution}) {ImageGroupingService.DescribeIncompatibility(classification)} -> {targetFolderName}"
                                             severity = LogSeverity.Warning

                                         Case FileOutcome.Skipped
                                             Interlocked.Increment(counters.Skipped)
                                             message = $"{FormatTag("SKIPPED")}'{relativePath}' ({resolution}) already resides in '{targetFolderName}'."
                                             severity = LogSeverity.Skipped

                                         Case Else
                                             Throw New InvalidOperationException($"Unhandled file outcome: {outcome}")
                                     End Select

                                 Catch ex As Exception When Not TypeOf ex Is OperationCanceledException
                                     ' A single corrupt/locked image must not abort the whole batch:
                                     ' the failure is explicitly counted and reported, never swallowed.
                                     Interlocked.Increment(counters.Failed)
                                     message = $"{FormatTag("ERROR")}'{relativePath}' -> {ex.GetType().Name}: {ex.Message}"
                                     severity = LogSeverity.Error
                                 End Try

                                 Dim currentIndex As Integer = Interlocked.Increment(counters.Processed)
                                 Dim currentIndexFormatted As String = currentIndex.ToString("N0", ImageGroupingService.FormatCulture).PadLeft(totalFilesFormatted.Length, " "c)

                                 progress.Report(New ImageGroupingProgress(currentIndex, totalFiles, $"[{currentIndexFormatted}/{totalFilesFormatted}] {message}", severity))
                             End Sub)

            stopwatch.Stop()

            Return New ImageGroupingSummary(totalFiles,
                                        counters.MovedToCropFolder,
                                        counters.MovedToAlreadyConforming,
                                        counters.MovedToIncompatible,
                                        counters.Skipped,
                                        counters.Failed,
                                        stopwatch.Elapsed)
        End Function

        ''' <summary>
        ''' Locates all the valid image files, strictly filtering by exact extension matches.
        ''' <para></para>
        ''' The directory tree is walked iteratively (never recursively). Unlike <c>EnumerateFiles(..., AllDirectories)</c>,
        ''' a single inaccessible subdirectory (e.g. "System Volume Information" when a drive root is selected) does not
        ''' abort the whole operation: it is explicitly reported as a warning and skipped. Junctions and symbolic links
        ''' are reported and not followed, which prevents infinite loops and processing the same files twice.
        ''' </summary>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="progress">The progress sink, used to report skipped directories.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <returns>
        ''' A materialized list of files (so that subsequent moves cannot disturb the enumeration),
        ''' sorted by full path for a deterministic processing order.
        ''' </returns>
        ''' <exception cref="UnauthorizedAccessException">Thrown when the target directory itself is not accessible.</exception>
        Private Shared Function EnumerateImageFiles(options As ImageGroupingOptions,
                                                    progress As IProgress(Of ImageGroupingProgress),
                                                    cancellationToken As CancellationToken) As List(Of FileInfo)

            Dim allowedExtensions As New HashSet(Of String)(options.AllowedExtensions, StringComparer.OrdinalIgnoreCase)
            Dim rootDirectory As New DirectoryInfo(options.TargetDirectory)
            Dim rootDirectoryWithSeparator As String = ImageGroupingService.NormalizeRootDirectory(options.TargetDirectory)
            Dim result As New List(Of FileInfo)()
            Dim pendingDirectories As New Stack(Of DirectoryInfo)()
            pendingDirectories.Push(rootDirectory)

            While pendingDirectories.Count > 0
                ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                Thread.CurrentThread.Join(0)
                cancellationToken.ThrowIfCancellationRequested()

                Dim currentDirectory As DirectoryInfo = pendingDirectories.Pop()
                Dim isRoot As Boolean = currentDirectory Is rootDirectory
                Dim files As FileInfo()
                Dim subdirectories As DirectoryInfo()

                Try
                    files = currentDirectory.GetFiles("*", SearchOption.TopDirectoryOnly)
                    subdirectories = If(options.SearchRecursively,
                                        currentDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly),
                                        Array.Empty(Of DirectoryInfo)())

                Catch ex As UnauthorizedAccessException When Not isRoot
                    Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, currentDirectory.FullName)
                    progress.Report(New ImageGroupingProgress(0, 0, $"[WARNING] Skipped inaccessible directory '{relativePath}': {ex.Message}", LogSeverity.Warning))
                    Continue While
                End Try

                For Each fileInfo As FileInfo In files
                    ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                    Thread.CurrentThread.Join(0)

                    ' Strictly filter by extension to avoid Windows 8.3 legacy pattern matching anomalies (e.g., matching .jpg2).
                    If allowedExtensions.Contains(fileInfo.Extension) Then
                        result.Add(fileInfo)
                    End If
                Next

                For Each subdirectory As DirectoryInfo In subdirectories
                    ' Prevent ContextSwitchDeadlock in long-running/concurrent COM-bound contexts.
                    Thread.CurrentThread.Join(0)

                    If (subdirectory.Attributes And FileAttributes.ReparsePoint) = FileAttributes.ReparsePoint Then
                        Dim relativePath As String = ImageGroupingService.GetRelativePath(rootDirectoryWithSeparator, subdirectory.FullName)
                        progress.Report(New ImageGroupingProgress(0, 0, $"[SKIPPED] Junction or symbolic link not followed: '{relativePath}'.", LogSeverity.Skipped))
                        Continue For
                    End If

                    pendingDirectories.Push(subdirectory)
                Next
            End While

            result.Sort(Function(left As FileInfo, right As FileInfo) StringComparer.OrdinalIgnoreCase.Compare(left.FullName, right.FullName))
            Return result
        End Function

        ''' <summary>
        ''' Processes a single image file: reads its dimensions, classifies it and moves it to its target folder.
        ''' </summary>
        ''' <param name="imageFile">The image file to process.</param>
        ''' <param name="options">The options that drive this run.</param>
        ''' <param name="classification">When this method returns, contains the classification of the image.</param>
        ''' <param name="targetFolderName">When this method returns, contains the resolved target folder name.</param>
        ''' <returns>The outcome of the operation.</returns>
        ''' <exception cref="FileNotFoundException">Thrown when the file no longer exists.</exception>
        ''' <exception cref="IOException">Thrown when a file with the same name already exists in the destination folder.</exception>
        Private Shared Function ProcessImageFile(imageFile As FileInfo, options As ImageGroupingOptions,
                                                 ByRef classification As AspectRatioClassification,
                                                 ByRef targetFolderName As String) As FileOutcome

            imageFile.Refresh()
            If Not imageFile.Exists Then
                Throw New FileNotFoundException($"The target file does not exist or was moved: {imageFile.FullName}", imageFile.FullName)
            End If

            Using fileStream As New FileStream(imageFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                ' validateImageData:=False reads only the header (dimensions) instead of decoding the full bitmap.
                Using img As Image = Image.FromStream(fileStream, useEmbeddedColorManagement:=False, validateImageData:=False)
                    classification = AspectRatioCalculator.Classify(img.Width, img.Height, options.AspectRatio,
                                                                    options.TolerancePixels, options.MinimumHeight)
                End Using
            End Using

            targetFolderName = AspectRatioCalculator.ResolveTargetFolderName(classification, options)

            ' Re-running the tool over an already grouped tree must not nest folders (e.g. "1118\1118\").
            If String.Equals(imageFile.Directory.Name, targetFolderName, StringComparison.OrdinalIgnoreCase) Then
                Return FileOutcome.Skipped
            End If

            Dim targetFolderPath As String = Path.Combine(imageFile.DirectoryName, targetFolderName)

            ' CreateDirectory is idempotent and safe when several threads target the same folder.
            Directory.CreateDirectory(targetFolderPath)

            Dim destinationFilePath As String = Path.Combine(targetFolderPath, imageFile.Name)
            If File.Exists(destinationFilePath) Then
                Throw New IOException($"A file with the same name already exists in the destination folder: {destinationFilePath}")
            End If

            File.Move(imageFile.FullName, destinationFilePath)

            Select Case classification.Verdict
                Case AspectRatioVerdict.AlreadyConforming
                    Return FileOutcome.MovedToAlreadyConforming
                Case AspectRatioVerdict.CropHeight
                    Return FileOutcome.MovedToCropFolder
                Case AspectRatioVerdict.IncompatibleTooWide, AspectRatioVerdict.IncompatibleTooSmall
                    Return FileOutcome.MovedToIncompatible
                Case Else
                    Throw New InvalidOperationException($"Unhandled verdict: {classification.Verdict}")
            End Select
        End Function

        ''' <summary>
        ''' Builds a human-readable explanation of why an image is incompatible.
        ''' </summary>
        ''' <param name="classification">The (incompatible) classification.</param>
        ''' <returns>The explanation.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the classification is not incompatible.</exception>
        Private Shared Function DescribeIncompatibility(classification As AspectRatioClassification) As String
            Select Case classification.Verdict
                Case AspectRatioVerdict.IncompatibleTooWide
                    Return $"is wider than {classification.Ratio.Key}: no downwards crop can reach it"

                Case AspectRatioVerdict.IncompatibleTooSmall
                    Return $"would be {classification.ResultHeight} px tall, below the {classification.MinimumHeight} px minimum"

                Case Else
                    Throw New InvalidOperationException($"The verdict '{classification.Verdict}' is not an incompatibility.")
            End Select
        End Function

        ''' <summary>
        ''' Formats a log tag padded to a fixed width (e.g. <c>"[MOVED]        "</c>).
        ''' </summary>
        ''' <param name="tag">The tag text, without brackets.</param>
        ''' <returns>The padded tag.</returns>
        Private Shared Function FormatTag(tag As String) As String
            Return $"[{tag}]".PadRight(TagWidth)
        End Function

        ''' <summary>
        ''' Normalizes the root directory to an absolute path that ends with a directory separator.
        ''' </summary>
        ''' <param name="directoryPath">The directory path.</param>
        ''' <returns>The normalized directory path.</returns>
        Private Shared Function NormalizeRootDirectory(directoryPath As String) As String
            Dim fullPath As String = Path.GetFullPath(directoryPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            Return $"{fullPath}{Path.DirectorySeparatorChar}"
        End Function

        ''' <summary>
        ''' Gets the path of a file relative to the root directory, for compact log output.
        ''' </summary>
        ''' <param name="rootDirectoryWithSeparator">The normalized root directory (see <see cref="NormalizeRootDirectory"/>).</param>
        ''' <param name="fullPath">The full file path.</param>
        ''' <returns>The relative path.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when the file is not located under the root directory.</exception>
        Private Shared Function GetRelativePath(rootDirectoryWithSeparator As String, fullPath As String) As String
            If Not fullPath.StartsWith(rootDirectoryWithSeparator, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException($"The file '{fullPath}' is not located under '{rootDirectoryWithSeparator}'.")
            End If

            Return fullPath.Substring(rootDirectoryWithSeparator.Length)
        End Function

#End Region

    End Class

End Namespace
