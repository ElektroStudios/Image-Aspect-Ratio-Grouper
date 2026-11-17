Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.IO

Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageAspectRatioGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="ImageDimensionReader"/>.
    ''' </summary>
    Public NotInheritable Class ImageDimensionReaderTests

        ''' <summary>
        ''' Every format that can be written for the tests is read with its real dimensions, through GDI+ or SkiaSharp.
        ''' </summary>
        ''' <param name="extension">The file extension.</param>
        ''' <param name="isSkiaDecoded">Whether the format is read with SkiaSharp.</param>
        <Theory>
        <InlineData(".jpg", False)>
        <InlineData(".jpeg", False)>
        <InlineData(".png", False)>
        <InlineData(".bmp", False)>
        <InlineData(".tif", False)>
        <InlineData(".tiff", False)>
        <InlineData(".webp", True)>
        <InlineData(".ico", True)>
        Public Sub ReadSize_ReturnsTheRealDimensions(extension As String, isSkiaDecoded As Boolean)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateImage(temp.Combine($"image{extension}"), 320, 181)

                Assert.Equal(isSkiaDecoded, ImageDimensionReader.IsSkiaDecoded(filePath))
                Assert.Equal(New Size(320, 181), ImageDimensionReader.ReadSize(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' An ICO image of 256 pixels or more (stored as 0 in the icon directory) is read with its real dimensions.
        ''' </summary>
        <Fact>
        Public Sub ReadSize_LargeIcon_ReturnsTheRealDimensions()
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateImage(temp.Combine("large.ico"), 512, 288)

                Assert.Equal(New Size(512, 288), ImageDimensionReader.ReadSize(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' A corrupt file read with SkiaSharp fails with an explicit error.
        ''' </summary>
        ''' <param name="fileName">The file name.</param>
        <Theory>
        <InlineData("corrupt.webp")>
        <InlineData("corrupt.avif")>
        <InlineData("corrupt.ico")>
        Public Sub ReadSize_CorruptSkiaFile_ThrowsInvalidOperation(fileName As String)
            Using temp As New TemporaryDirectory()
                Dim filePath As String = TestImageFactory.CreateCorruptImage(temp.Combine(fileName))

                Dim exception As InvalidOperationException = Assert.Throws(Of InvalidOperationException)(Function() ImageDimensionReader.ReadSize(filePath))
                Assert.Contains("SkiaSharp cannot read the file", exception.Message, StringComparison.Ordinal)
            End Using
        End Sub

        ''' <summary>
        ''' A file whose extension does not match its content (a PNG named .webp) is still read by SkiaSharp,
        ''' which detects the format from the content.
        ''' </summary>
        <Fact>
        Public Sub ReadSize_PngNamedWebP_IsRead()
            Using temp As New TemporaryDirectory()
                Dim pngPath As String = TestImageFactory.CreateImage(temp.Combine("image.png"), 64, 36)
                Dim webpPath As String = temp.Combine("image.webp")
                File.Move(pngPath, webpPath)

                Assert.Equal(New Size(64, 36), ImageDimensionReader.ReadSize(webpPath))
            End Using
        End Sub

        ''' <summary>
        ''' Invalid arguments fail fast.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_Throw()
            Assert.Throws(Of ArgumentException)(Function() ImageDimensionReader.ReadSize(Nothing))
            Assert.Throws(Of ArgumentException)(Function() ImageDimensionReader.ReadSize(" "))
            Assert.Throws(Of ArgumentNullException)(Function() ImageDimensionReader.IsSkiaDecoded(Nothing))
        End Sub

        ''' <summary>
        ''' The native SkiaSharp library of the current architecture is available.
        ''' </summary>
        <Fact>
        Public Sub EnsureSkiaSharpIsAvailable_DoesNotThrow()
            ImageDimensionReader.EnsureSkiaSharpIsAvailable()
        End Sub

        ''' <summary>
        ''' Only WebP, AVIF and ICO are read with SkiaSharp, case-insensitively.
        ''' </summary>
        ''' <param name="fileName">The file name.</param>
        ''' <param name="expected">Whether the file is read with SkiaSharp.</param>
        <Theory>
        <InlineData("a.webp", True)>
        <InlineData("a.WEBP", True)>
        <InlineData("a.avif", True)>
        <InlineData("a.ico", True)>
        <InlineData("a.png", False)>
        <InlineData("a.jpg", False)>
        <InlineData("a.tiff", False)>
        <InlineData("a", False)>
        Public Sub IsSkiaDecoded(fileName As String, expected As Boolean)
            Assert.Equal(expected, ImageDimensionReader.IsSkiaDecoded(fileName))
        End Sub

        ''' <summary>
        ''' The supported extensions are lower-case and dot-prefixed, include the formats read with SkiaSharp, and are
        ''' the extensions processed by the grouping.
        ''' </summary>
        <Fact>
        Public Sub SupportedExtensions_AreTheProcessedExtensions()
            Assert.All(ImageDimensionReader.SupportedExtensions,
                       Sub(extension As String)
                           Assert.StartsWith(".", extension, StringComparison.Ordinal)
                           Assert.Equal(extension.ToLowerInvariant(), extension)
                       End Sub)

            For Each extension As String In {".webp", ".avif", ".ico"}
                Assert.Contains(extension, ImageDimensionReader.SupportedExtensions)
            Next

            Dim options As New ImageGroupingOptions(Path.GetTempPath(), False, AspectRatio.Default, 0, 0)
            Assert.Equal(ImageDimensionReader.SupportedExtensions.ToList(), options.AllowedExtensions.ToList())
        End Sub

        ''' <summary>
        ''' End to end: WebP and ICO images are grouped like any other image.
        ''' </summary>
        ''' <returns>A task that represents the asynchronous test.</returns>
        <Fact>
        Public Async Function Grouping_WebPAndIco_AreGrouped() As Task
            Using temp As New TemporaryDirectory()
                TestImageFactory.CreateImage(temp.Combine("tall.webp"), 1990, 1323)
                TestImageFactory.CreateImage(temp.Combine("already.ico"), 256, 144)
                TestImageFactory.CreateCorruptImage(temp.Combine("corrupt.avif"))

                Dim options As New ImageGroupingOptions(temp.FullPath, False, AspectRatio.Default,
                                                        ImageGroupingOptions.DefaultTolerancePixels, minimumHeight:=100)
                Dim progress As New BufferedProgress(Of ImageGroupingProgress)()
                Dim summary As ImageGroupingSummary =
                    Await New ImageGroupingService().RunAsync(options, progress, TestContext.Current.CancellationToken).ConfigureAwait(False)

                Assert.Equal(3, summary.TotalFiles)
                Assert.Equal(1, summary.MovedToCropFolderCount)
                Assert.Equal(1, summary.MovedToAlreadyConformingCount)
                Assert.Equal(1, summary.FailedCount)
                Assert.True(File.Exists(temp.Combine("1118", "tall.webp")))
                Assert.True(File.Exists(temp.Combine(options.AlreadyConformingFolderName, "already.ico")))
                Assert.True(File.Exists(temp.Combine("corrupt.avif")))
            End Using
        End Function

    End Class

End Namespace
