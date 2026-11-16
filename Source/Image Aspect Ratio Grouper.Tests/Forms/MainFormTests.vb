Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Forms
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageAspectRatioGrouper.Tests.Forms

    ''' <summary>
    ''' Tests for <see cref="MainForm"/> and <see cref="AboutBox"/>. They need the real Windows Forms implementation
    ''' (DWM, uxtheme, native NumericUpDown internals), so they only run on .NET Framework on Windows.
    ''' </summary>
    <Collection(SharedStateCollection.Name)>
    Public NotInheritable Class MainFormTests

        ''' <summary>
        ''' Regression: the designer assigns default values to the numeric boxes inside InitializeComponent,
        ''' which raises ValueChanged. That must never overwrite the persisted preferences.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Constructor_DoesNotOverwritePersistedPreferences()
            Using New SettingsSnapshot()
                UserPreferences.AspectRatioTolerance = 17
                UserPreferences.MinimumHeight = 999

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                        End Using
                    End Sub)

                Assert.Equal(17, UserPreferences.AspectRatioTolerance)
                Assert.Equal(999, UserPreferences.MinimumHeight)
            End Using
        End Sub

        ''' <summary>
        ''' Loading the form restores every persisted preference into its control,
        ''' and closing it keeps (persists) the same values.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_RestoresPreferencesIntoControls_AndCloseKeepsThem()
            Using New SettingsSnapshot()
                UserPreferences.AspectRatioTolerance = 17
                UserPreferences.MinimumHeight = 999
                UserPreferences.AspectRatio = AspectRatio.Ultrawide21x9
                UserPreferences.SearchRecursively = False

                Dim restoredTolerance As Integer
                Dim restoredMinimumHeight As Integer
                Dim restoredRatio As Object = Nothing
                Dim restoredRecursive As Boolean = True

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()

                            restoredTolerance = form.toleranceNumericUpDown.Value
                            restoredMinimumHeight = form.minimumHeightNumericUpDown.Value
                            restoredRatio = form.aspectRatioComboBox.SelectedItem
                            restoredRecursive = form.recursiveToggle.Checked

                            form.Close()
                        End Using
                    End Sub)

                Assert.Equal(17, restoredTolerance)
                Assert.Equal(999, restoredMinimumHeight)
                Assert.Same(AspectRatio.Ultrawide21x9, restoredRatio)
                Assert.False(restoredRecursive)

                Assert.Equal(17, UserPreferences.AspectRatioTolerance)
                Assert.Equal(999, UserPreferences.MinimumHeight)
                Assert.Same(AspectRatio.Ultrawide21x9, UserPreferences.AspectRatio)
                Assert.False(UserPreferences.SearchRecursively)
            End Using
        End Sub

        ''' <summary>
        ''' Changing a numeric box after loading persists the new value immediately.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ChangingNumericBoxes_PersistsTheNewValues()
            Using New SettingsSnapshot()
                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()
                            form.toleranceNumericUpDown.Value = 9
                            form.minimumHeightNumericUpDown.Value = 720

                            Assert.Equal(9, UserPreferences.AspectRatioTolerance)
                            Assert.Equal(720, UserPreferences.MinimumHeight)
                            form.Close()
                        End Using
                    End Sub)
            End Using
        End Sub

        ''' <summary>
        ''' Tool tips are split into short lines, so they never span a large part of the screen.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub ToolTips_AreSplitIntoShortLines()
            Const MaximumLineLength As Integer = 60

            StaThreadRunner.Run(
                Sub()
                    Using form As New MainForm()
                        Dim pending As New Stack(Of Windows.Forms.Control)()
                        pending.Push(form)

                        While pending.Count > 0
                            Dim control As Windows.Forms.Control = pending.Pop()
                            Dim toolTipText As String = form.toolTip.GetToolTip(control)

                            For Each line As String In toolTipText.Split({Environment.NewLine}, StringSplitOptions.None)
                                Assert.True(line.Length <= MaximumLineLength, $"The tool tip of '{control.Name}' has a {line.Length}-character line: {line}")
                            Next

                            For Each child As Windows.Forms.Control In control.Controls
                                pending.Push(child)
                            Next
                        End While
                    End Using
                End Sub)
        End Sub

        ''' <summary>
        ''' The target directory is never restored at startup.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Load_DoesNotRestoreTheTargetDirectory()
            Using New SettingsSnapshot()
                Dim directoryText As String = Nothing

                StaThreadRunner.Run(
                    Sub()
                        Using form As New MainForm()
                            form.Show()
                            directoryText = form.directoryTextBox.Text
                            form.Close()
                        End Using
                    End Sub)

                Assert.Equal(String.Empty, directoryText)
            End Using
        End Sub

        ''' <summary>
        ''' The window title follows the required format, and the about box reads the assembly information.
        ''' </summary>
        <Fact(Skip:=TestEnvironment.WindowsOnlyReason, SkipUnless:=NameOf(TestEnvironment.IsWindowsDesktopFramework), SkipType:=GetType(TestEnvironment))>
        Public Sub Titles_FollowTheRequiredFormat()
            StaThreadRunner.Run(
                Sub()
                    Using form As New MainForm()
                        Dim expectedTitle As String = $"{My.Application.Info.Title} v{My.Application.Info.Version} ({UtilApplication.ProcessArchitecture.ToString().ToUpper()})"
                        Assert.Equal(expectedTitle, form.Text)
                    End Using

                    Using about As New AboutBox()
                        Assert.Equal($"About {My.Application.Info.Title}", about.Text)
                        Assert.NotNull(about.logoPictureBox.Image)
                    End Using
                End Sub)
        End Sub

    End Class

End Namespace
