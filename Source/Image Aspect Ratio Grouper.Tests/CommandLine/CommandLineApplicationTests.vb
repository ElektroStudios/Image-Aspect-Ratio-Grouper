Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO
Imports System.Threading

Imports ImageAspectRatioGrouper.CommandLine
Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageAspectRatioGrouper.Tests.CommandLine

    ''' <summary>
    ''' End-to-end tests of the command-line application (<see cref="CommandLineApplication"/>), in process, with real images.
    ''' </summary>
    Public NotInheritable Class CommandLineApplicationTests

#Region " Helpers "

        ''' <summary>
        ''' The title written first.
        ''' </summary>
        Private Const Title As String = "Image Aspect Ratio Grouper CLI vTest (X64)"

        ''' <summary>
        ''' Runs a command line.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        ''' <param name="currentDirectory">The current directory, used for relative paths.</param>
        ''' <param name="cancellationToken">The cancellation token.</param>
        ''' <param name="args">The command-line arguments.</param>
        ''' <returns>The exit code.</returns>
        Private Shared Function RunCommandLine(writer As RecordingConsoleWriter, currentDirectory As String,
                                               cancellationToken As CancellationToken, ParamArray args As String()) As CommandLineExitCode

            Return CommandLineApplication.Run(args, writer, Title, New ImageGroupingService(), currentDirectory, cancellationToken)
        End Function

        ''' <summary>
        ''' Runs a command line with the test cancellation token.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        ''' <param name="currentDirectory">The current directory.</param>
        ''' <param name="args">The command-line arguments.</param>
        ''' <returns>The exit code.</returns>
        Private Shared Function RunCommandLine(writer As RecordingConsoleWriter, currentDirectory As String, ParamArray args As String()) As CommandLineExitCode
            Return RunCommandLine(writer, currentDirectory, TestContext.Current.CancellationToken, args)
        End Function

        ''' <summary>
        ''' Asserts that the output starts with the header: title, repository address and an empty line.
        ''' </summary>
        ''' <param name="writer">The output.</param>
        Private Shared Sub AssertHeader(writer As RecordingConsoleWriter)
            Assert.True(writer.Output.Count >= 3)
            Assert.Equal(Title, writer.Output(0).Text)
            Assert.Equal(ProductInfo.RepositoryUrl, writer.Output(1).Text)
            Assert.Equal(String.Empty, writer.Output(2).Text)
        End Sub

        ''' <summary>
        ''' Gets the path that an image gets once it is grouped.
        ''' </summary>
        ''' <param name="directoryPath">The directory of the original image.</param>
        ''' <param name="folderName">The name of the destination folder.</param>
        ''' <param name="fileName">The file name.</param>
        ''' <returns>The path.</returns>
        Friend Shared Function GroupedPath(directoryPath As String, folderName As String, fileName As String) As String
            Return Path.Combine(directoryPath, folderName, fileName)
        End Function

#End Region

#Region " Header and help "

        ''' <summary>
        ''' No arguments and the help switch show the header and the help, with exit code 0.
        ''' </summary>
        <Fact>
        Public Sub Help_ShowsTheHeaderAndTheHelp()
            For Each args As String() In New String()() {New String() {}, New String() {"/?"}, New String() {"C:\x", "--help"}}
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(writer, Path.GetTempPath(), args))
                AssertHeader(writer)
                Assert.Equal(CommandLineHelp.GetUsageText(), String.Join(Environment.NewLine, writer.Output.Skip(3).Select(Function(line As LogLine) line.Text)))
                Assert.Empty(writer.Errors)
            Next
        End Sub

        ''' <summary>
        ''' Invalid arguments: the header, then the error and a hint about the help on the error output, with exit code 2.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_ShowTheHeaderAndTheError()
            Dim writer As New RecordingConsoleWriter()

            Assert.Equal(CommandLineExitCode.InvalidArguments, RunCommandLine(writer, Path.GetTempPath(), "x", "/ratio:16x9"))
            AssertHeader(writer)
            Assert.Equal(3, writer.Output.Count)
            Assert.StartsWith("[ERROR]", writer.Errors(0), StringComparison.Ordinal)
            Assert.Equal(CommandLineApplication.GetHelpHint(), writer.Errors(1))
        End Sub

#End Region

#Region " Success "

        ''' <summary>
        ''' Every kind of image goes to its folder; the output shows the header, every image and the summary; the exit
        ''' code is 0 (incompatible images are set apart, they are not failures).
        ''' </summary>
        <Fact>
        Public Sub Group_MovesEachImageToItsFolder_AndReturnsSuccess()
            Using temp As New TemporaryDirectory()
                Dim ratio As AspectRatio = AspectRatio.Default
                TestImageFactory.CreateImage(temp.Combine("already.png"), 1920, 1080)
                TestImageFactory.CreateImage(temp.Combine("tall.png"), 1990, 1323)
                TestImageFactory.CreateImage(temp.Combine("wide.png"), 1280, 28)
                TestImageFactory.CreateImage(temp.Combine("small.png"), 400, 600)
                Dim writer As New RecordingConsoleWriter()

                Dim exitCode As CommandLineExitCode = RunCommandLine(writer, Path.GetTempPath(), temp.FullPath)

                Assert.Equal(CommandLineExitCode.Success, exitCode)
                Assert.True(File.Exists(GroupedPath(temp.FullPath, ImageGroupingOptions.GetAlreadyConformingFolderName(ratio), "already.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, "1118", "tall.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, ImageGroupingOptions.GetIncompatibleFolderName(ratio), "wide.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, ImageGroupingOptions.GetIncompatibleFolderName(ratio), "small.png")))
                AssertHeader(writer)
                Assert.Contains(writer.Output, Function(line As LogLine) line.Text.StartsWith("[START] ", StringComparison.Ordinal))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Severity = LogSeverity.Moved AndAlso line.Text.Contains("'tall.png' (1990x1323) -> 1118"))
                Assert.Empty(writer.Errors)

                ' The summary is exactly the shared summary report (the same as in the main window).
                Dim summaryLines As List(Of LogLine) = writer.Output.Skip(writer.Output.Count - 11).ToList()
                Assert.Equal(ImageGroupingReport.Rule, summaryLines(1).Text)
                Assert.Contains(summaryLines, Function(line As LogLine) line.Text.StartsWith("Incompatible ", StringComparison.Ordinal) AndAlso line.Text.EndsWith(": 2", StringComparison.Ordinal))
                Assert.Equal("[SUCCESS] Operation completed. 2 incompatible image(s) were set apart.", summaryLines.Last().Text)
            End Using
        End Sub

        ''' <summary>
        ''' Every option reaches the grouping: recursion, aspect ratio, tolerance and minimum height.
        ''' </summary>
        <Fact>
        Public Sub Group_AppliesEveryOption()
            Using temp As New TemporaryDirectory()
                Dim ratio As AspectRatio = AspectRatio.Standard4x3
                TestImageFactory.CreateImage(temp.Combine("sub", "within.png"), 400, 305)        ' 4:3 = 300 px, +5 px.
                TestImageFactory.CreateImage(temp.Combine("tall.png"), 400, 400)                 ' Crop to 300 px.
                TestImageFactory.CreateImage(temp.Combine("too small.png"), 200, 200)            ' Crop to 150 px < 160 px.
                Dim writer As New RecordingConsoleWriter()

                Dim exitCode As CommandLineExitCode =
                    RunCommandLine(writer, Path.GetTempPath(), temp.FullPath, "/r", $"/a:{ratio.Key}", "/t:5", "/m:160")

                Assert.Equal(CommandLineExitCode.Success, exitCode)
                Assert.True(File.Exists(GroupedPath(temp.Combine("sub"), ImageGroupingOptions.GetAlreadyConformingFolderName(ratio), "within.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, "300", "tall.png")))
                Assert.True(File.Exists(GroupedPath(temp.FullPath, ImageGroupingOptions.GetIncompatibleFolderName(ratio), "too small.png")))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Text.Contains("Tolerance: ±5 px | Minimum height: 160 px"))
            End Using
        End Sub

        ''' <summary>
        ''' Without the recursive switch, the subdirectories are not processed.
        ''' </summary>
        <Fact>
        Public Sub Group_WithoutRecursive_IgnoresTheSubdirectories()
            Using temp As New TemporaryDirectory()
                Dim nested As String = TestImageFactory.CreateImage(temp.Combine("sub", "nested.png"), 1920, 1080)

                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), Path.GetTempPath(), temp.FullPath))
                Assert.True(File.Exists(nested))
            End Using
        End Sub

        ''' <summary>
        ''' Running again over an already grouped directory skips the images (never nests "1118\1118"), with exit code 0.
        ''' </summary>
        <Fact>
        Public Sub Group_RunningAgain_SkipsTheGroupedImages()
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("tall.png"), 1990, 1323)
                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), Path.GetTempPath(), temp.FullPath))

                Dim writer As New RecordingConsoleWriter()
                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(writer, Path.GetTempPath(), temp.FullPath, "/r"))

                Assert.True(File.Exists(GroupedPath(temp.FullPath, "1118", "tall.png")))
                Assert.False(Directory.Exists(temp.Combine("1118", "1118")))
                Assert.Contains(writer.Output, Function(line As LogLine) line.Severity = LogSeverity.Skipped AndAlso line.Text.Contains("tall.png"))
            End Using
        End Sub

        ''' <summary>
        ''' A relative directory is resolved against the current directory; a directory without images succeeds.
        ''' </summary>
        <Fact>
        Public Sub Group_RelativeAndEmptyDirectories()
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("images", "already.png"), 1920, 1080)
                Directory.CreateDirectory(temp.Combine("empty"))

                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), temp.FullPath, "images"))
                Assert.True(File.Exists(GroupedPath(temp.Combine("images"), ImageGroupingOptions.GetAlreadyConformingFolderName(AspectRatio.Default), "already.png")))
                Assert.Equal(CommandLineExitCode.Success, RunCommandLine(New RecordingConsoleWriter(), temp.FullPath, "empty"))
            End Using
        End Sub

#End Region

#Region " Problems "

        ''' <summary>
        ''' Failed images (corrupt, or name conflicts) make the exit code 1, and they stay in place.
        ''' </summary>
        <Fact>
        Public Sub Group_Failures_ReturnCompletedWithProblems()
            Using temp As New TemporaryDirectory()
                Dim corrupt As String = TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.png"))

                Assert.Equal(CommandLineExitCode.CompletedWithProblems, RunCommandLine(New RecordingConsoleWriter(), Path.GetTempPath(), temp.FullPath))
                Assert.True(File.Exists(corrupt))
            End Using

            Using temp As New TemporaryDirectory()
                Dim source As String = TestImageFactory.CreateImage(temp.Combine("same.png"), 1990, 1323)
                TestImageFactory.CreateTextFile(GroupedPath(temp.FullPath, "1118", "same.png"))
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.CompletedWithProblems, RunCommandLine(writer, Path.GetTempPath(), temp.FullPath))
                Assert.True(File.Exists(source))
                Assert.Equal(LogSeverity.Error, writer.Output.Last().Severity)
            End Using
        End Sub

        ''' <summary>
        ''' A missing directory, or a file, is reported on the error output with exit code 3, without moving anything.
        ''' </summary>
        <Fact>
        Public Sub Group_InvalidDirectory_ReturnsInvalidDirectory()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateImage(temp.Combine("image.png"), 1920, 1080)

                For Each target As String In {temp.Combine("missing"), filePath}
                    Dim writer As New RecordingConsoleWriter()

                    Assert.Equal(CommandLineExitCode.InvalidDirectory, RunCommandLine(writer, Path.GetTempPath(), target))
                    AssertHeader(writer)
                    Assert.StartsWith("[ERROR]", writer.Errors.Single(), StringComparison.Ordinal)
                Next

                Assert.True(File.Exists(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' A cancelled operation (Ctrl+C) returns exit code 5 and moves nothing.
        ''' </summary>
        <Fact>
        Public Sub Group_Cancelled_ReturnsCancelled()
            Using temp As New TemporaryDirectory(),
                  cancellationSource As New CancellationTokenSource()

                Dim image As String = TestImageFactory.CreateImage(temp.Combine("image.png"), 1920, 1080)
                cancellationSource.Cancel()
                Dim writer As New RecordingConsoleWriter()

                Assert.Equal(CommandLineExitCode.Cancelled, RunCommandLine(writer, Path.GetTempPath(), cancellationSource.Token, temp.FullPath))
                Assert.Contains("[CANCELLED]", writer.ErrorText, StringComparison.Ordinal)
                Assert.True(File.Exists(image))
            End Using
        End Sub

        ''' <summary>
        ''' Invalid calls fail fast.
        ''' </summary>
        <Fact>
        Public Sub Run_InvalidCalls_Throw()
            Dim writer As New RecordingConsoleWriter()
            Dim service As New ImageGroupingService()
            Dim token As CancellationToken = CancellationToken.None

            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run(Nothing, writer, Title, service, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, Nothing, Title, service, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, writer, Title, Nothing, Path.GetTempPath(), token))
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineApplication.Run({}, writer, Title, service, Nothing, token))
            Assert.Throws(Of ArgumentException)(Function() CommandLineApplication.Run({}, writer, " ", service, Path.GetTempPath(), token))
        End Sub

#End Region

    End Class

End Namespace
