Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO
Imports System.Threading

Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageAspectRatioGrouper.Tests.Core

    ''' <summary>
    ''' Integration tests for <see cref="ImageGroupingService"/>, using real image files in temporary directories.
    ''' </summary>
    Public NotInheritable Class ImageGroupingServiceTests

#Region " Helpers "

        ''' <summary>
        ''' Runs the service and returns its summary, collecting every progress report.
        ''' </summary>
        ''' <param name="options">The options.</param>
        ''' <param name="reports">Receives every progress report, in arrival order.</param>
        ''' <returns>The summary.</returns>
        Private Shared Async Function RunAsync(options As ImageGroupingOptions, reports As List(Of ImageGroupingProgress)) As Task(Of ImageGroupingSummary)
            Dim progress As New BufferedProgress(Of ImageGroupingProgress)()
            Dim service As New ImageGroupingService()

            Dim summary As ImageGroupingSummary =
                Await service.RunAsync(options, progress, TestContext.Current.CancellationToken).ConfigureAwait(False)

            progress.DrainTo(reports)
            Return summary
        End Function

        ''' <summary>
        ''' Creates default 16:9 options (tolerance 3 px, minimum height 100 px) for the specified directory.
        ''' </summary>
        ''' <param name="directoryPath">The target directory.</param>
        ''' <param name="recursive">Whether to search recursively.</param>
        ''' <returns>The options.</returns>
        Private Shared Function CreateOptions(directoryPath As String, Optional recursive As Boolean = True) As ImageGroupingOptions
            Return New ImageGroupingOptions(directoryPath, recursive, AspectRatio.Widescreen16x9, tolerancePixels:=3, minimumHeight:=100)
        End Function

#End Region

#Region " Tests "

        ''' <summary>
        ''' Every kind of image ends up in the right folder, and the summary adds up.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_MovesEachImageToItsFolder() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("crop.png"), 400, 300)        ' exact 225 -> crop to 224
                TestImageFactory.CreateImage(temp.Combine("already.jpg"), 640, 361)     ' exact 360 -> within tolerance
                TestImageFactory.CreateImage(temp.Combine("wide.png"), 1280, 28)        ' wider than 16:9
                TestImageFactory.CreateImage(temp.Combine("small.bmp"), 96, 200)        ' exact 54 -> 54 < 100
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(options, reports).ConfigureAwait(False)

                Assert.True(File.Exists(temp.Combine("224", "crop.png")))
                Assert.True(File.Exists(temp.Combine(options.AlreadyConformingFolderName, "already.jpg")))
                Assert.True(File.Exists(temp.Combine(options.IncompatibleFolderName, "wide.png")))
                Assert.True(File.Exists(temp.Combine(options.IncompatibleFolderName, "small.bmp")))
                Assert.Empty(Directory.GetFiles(temp.FullPath))

                Assert.Equal(4, summary.TotalFiles)
                Assert.Equal(1, summary.MovedToCropFolderCount)
                Assert.Equal(1, summary.MovedToAlreadyConformingCount)
                Assert.Equal(2, summary.MovedToIncompatibleCount)
                Assert.Equal(0, summary.SkippedCount)
                Assert.Equal(0, summary.FailedCount)
            End Using
        End Function

        ''' <summary>
        ''' Progress reports cover every file exactly once, and incompatible images are reported as warnings.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_ReportsEveryFileOnce() As Task
            Using temp As New TemporaryDirectory()
                For index As Integer = 1 To 12
                    TestImageFactory.CreateImage(temp.Combine($"image{index:D2}.png"), 320, 240)
                Next
                TestImageFactory.CreateImage(temp.Combine("wide.png"), 1280, 28)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Dim fileReports As List(Of ImageGroupingProgress) = reports.Where(Function(r As ImageGroupingProgress) r.ProcessedCount > 0).ToList()
                Assert.Equal(summary.TotalFiles, fileReports.Count)
                Assert.Equal(Enumerable.Range(1, summary.TotalFiles), fileReports.Select(Function(r As ImageGroupingProgress) r.ProcessedCount).OrderBy(Function(n As Integer) n))
                Assert.All(fileReports, Sub(r As ImageGroupingProgress) Assert.Equal(summary.TotalFiles, r.TotalCount))
                Assert.Single(fileReports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Warning AndAlso r.Message.Contains("wide.png"))
            End Using
        End Function

        ''' <summary>
        ''' Re-running over an already grouped tree must not nest folders: every file is skipped.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_SecondRun_SkipsEverythingWithoutNesting() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("crop.png"), 400, 300)
                TestImageFactory.CreateImage(temp.Combine("already.png"), 640, 360)
                TestImageFactory.CreateImage(temp.Combine("wide.png"), 1280, 28)
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath)

                Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)
                Dim second As ImageGroupingSummary = Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(3, second.TotalFiles)
                Assert.Equal(3, second.SkippedCount)
                Assert.False(Directory.Exists(temp.Combine("224", "224")))
                Assert.False(Directory.Exists(temp.Combine(options.AlreadyConformingFolderName, options.AlreadyConformingFolderName)))
                Assert.False(Directory.Exists(temp.Combine(options.IncompatibleFolderName, options.IncompatibleFolderName)))
            End Using
        End Function

        ''' <summary>
        ''' Without recursion, images in subdirectories are left untouched.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_NonRecursive_IgnoresSubdirectories() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("top.png"), 640, 360)
                Dim nested As String = TestImageFactory.CreateImage(temp.Combine("sub", "nested.png"), 640, 360)

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath, recursive:=False), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.True(File.Exists(nested))
            End Using
        End Function

        ''' <summary>
        ''' With recursion, each image is grouped inside its own directory.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_Recursive_GroupsInsideEachDirectory() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("a", "b", "deep.png"), 400, 300)
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath)

                Dim summary As ImageGroupingSummary = Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.MovedToCropFolderCount)
                Assert.True(File.Exists(temp.Combine("a", "b", "224", "deep.png")))
            End Using
        End Function

        ''' <summary>
        ''' Only the exact allowed extensions are processed (no 8.3 wildcard anomalies like ".jpg2"), case-insensitively.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_ProcessesOnlyAllowedExtensions() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("upper.PNG"), 640, 360)
                Dim notImage As String = TestImageFactory.CreateTextFile(temp.Combine("notes.txt"))
                Dim lookalike As String = TestImageFactory.CreateTextFile(temp.Combine("photo.jpg2"))

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.True(File.Exists(notImage))
                Assert.True(File.Exists(lookalike))
            End Using
        End Function

        ''' <summary>
        ''' A corrupt image is counted and reported as an error, and it does not stop the other images.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_CorruptImage_IsReportedAndDoesNotAbortTheBatch() As Task
            Using temp As New TemporaryDirectory()
                Dim corrupt As String = TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.jpg"))
                TestImageFactory.CreateImage(temp.Combine("good.png"), 640, 360)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(2, summary.TotalFiles)
                Assert.Equal(1, summary.FailedCount)
                Assert.Equal(1, summary.MovedToAlreadyConformingCount)
                Assert.True(File.Exists(corrupt))
                Assert.Single(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Error AndAlso r.Message.Contains("corrupt.jpg"))
            End Using
        End Function

        ''' <summary>
        ''' An existing file with the same name in the destination folder is never overwritten.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_DestinationCollision_IsReportedAndNothingIsOverwritten() As Task
            Using temp As New TemporaryDirectory()
                Dim options As ImageGroupingOptions = CreateOptions(temp.FullPath)
                Dim source As String = TestImageFactory.CreateImage(temp.Combine("same.png"), 640, 360)
                Dim existing As String = TestImageFactory.CreateTextFile(temp.Combine(options.AlreadyConformingFolderName, "same.png"))
                Dim existingContent As String = File.ReadAllText(existing)

                ' The pre-existing file inside the target folder is itself evaluated (and fails: not an image).
                Dim summary As ImageGroupingSummary = Await RunAsync(options, New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(2, summary.FailedCount)
                Assert.True(File.Exists(source))
                Assert.Equal(existingContent, File.ReadAllText(existing))
            End Using
        End Function

        ''' <summary>
        ''' An empty directory completes with an all-zero summary.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_EmptyDirectory_ReturnsEmptySummary() As Task
            Using temp As New TemporaryDirectory()
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(0, summary.TotalFiles)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Warning)
            End Using
        End Function

        ''' <summary>
        ''' A missing directory fails the whole operation.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_MissingDirectory_Throws() As Task
            Dim missing As String = Path.Combine(Path.GetTempPath(), $"IARG-Missing-{Guid.NewGuid():N}")
            Dim service As New ImageGroupingService()

            Await Assert.ThrowsAsync(Of DirectoryNotFoundException)(
                Function() service.RunAsync(CreateOptions(missing), New BufferedProgress(Of ImageGroupingProgress)(), TestContext.Current.CancellationToken)).ConfigureAwait(False)
        End Function

        ''' <summary>
        ''' A cancelled token cancels the operation without moving anything.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_CancelledToken_IsCancelled() As Task
            Using temp As New TemporaryDirectory(),
                  cancellationSource As New CancellationTokenSource()

                Dim image As String = TestImageFactory.CreateImage(temp.Combine("image.png"), 640, 360)
                cancellationSource.Cancel()
                Dim service As New ImageGroupingService()

                Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                    Function() service.RunAsync(CreateOptions(temp.FullPath), New BufferedProgress(Of ImageGroupingProgress)(), cancellationSource.Token)).ConfigureAwait(False)

                Assert.True(File.Exists(image))
            End Using
        End Function

        ''' <summary>
        ''' Null arguments fail fast, synchronously.
        ''' </summary>
        <Fact>
        Public Sub RunAsync_NullArguments_Throw()
            Dim service As New ImageGroupingService()
            Dim options As ImageGroupingOptions = CreateOptions(Path.GetTempPath())

            Assert.Throws(Of ArgumentNullException)(Sub() service.RunAsync(Nothing, New BufferedProgress(Of ImageGroupingProgress)(), CancellationToken.None))
            Assert.Throws(Of ArgumentNullException)(Sub() service.RunAsync(options, Nothing, CancellationToken.None))
        End Sub

        ''' <summary>
        ''' Directory links (junctions, symbolic links) are reported and never followed, so a link that points
        ''' back to an ancestor cannot cause an infinite loop nor process the same images twice.
        ''' <para></para>
        ''' The link is a junction on Windows (no elevation required) and a symbolic link on Unix.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_DoesNotFollowDirectoryLinks() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("real", "image.png"), 640, 360)
                TestEnvironment.CreateDirectoryLink(temp.Combine("real", "loop"), temp.FullPath)
                Dim reports As New List(Of ImageGroupingProgress)()

                Dim summary As ImageGroupingSummary = Await RunAsync(CreateOptions(temp.FullPath), reports).ConfigureAwait(False)

                Assert.Equal(1, summary.TotalFiles)
                Assert.Equal(1, summary.MovedToAlreadyConformingCount)
                Assert.Contains(reports, Function(r As ImageGroupingProgress) r.Severity = LogSeverity.Skipped AndAlso r.Message.Contains("loop"))
            End Using
        End Function

        ''' <summary>
        ''' A large batch is fully processed in parallel, with consistent counters.
        ''' </summary>
        <Fact>
        Public Async Function RunAsync_LargeBatch_IsConsistent() As Task
            Using temp As New TemporaryDirectory()
                For index As Integer = 1 To 120
                    Dim height As Integer = If(index Mod 3 = 0, 360, If(index Mod 3 = 1, 500, 28))
                    TestImageFactory.CreateImage(temp.Combine($"batch{index:D3}.png"), 640, height)
                Next

                Dim summary As ImageGroupingSummary =
                    Await RunAsync(CreateOptions(temp.FullPath), New List(Of ImageGroupingProgress)()).ConfigureAwait(False)

                Assert.Equal(120, summary.TotalFiles)
                Assert.Equal(40, summary.MovedToAlreadyConformingCount)
                Assert.Equal(40, summary.MovedToCropFolderCount)
                Assert.Equal(40, summary.MovedToIncompatibleCount)
                Assert.Equal(0, summary.FailedCount)
                Assert.Empty(Directory.GetFiles(temp.FullPath))
            End Using
        End Function

#End Region

    End Class

End Namespace
