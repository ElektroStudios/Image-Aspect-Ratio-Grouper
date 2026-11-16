Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageAspectRatioGrouper.Core

#End Region

Namespace ImageAspectRatioGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="AspectRatioCalculator"/>.
    ''' </summary>
    Public NotInheritable Class AspectRatioCalculatorTests

        ''' <summary>
        ''' Verdicts and result heights for representative images (16:9, tolerance 3 px, minimum height 480 px).
        ''' </summary>
        ''' <param name="width">The image width.</param>
        ''' <param name="height">The image height.</param>
        ''' <param name="expectedVerdict">The expected verdict.</param>
        ''' <param name="expectedResultHeight">The expected result height.</param>
        <Theory>
        <InlineData(1990, 1323, AspectRatioVerdict.CropHeight, 1118)>
        <InlineData(2948, 1660, AspectRatioVerdict.AlreadyConforming, 1660)>
        <InlineData(1920, 1080, AspectRatioVerdict.AlreadyConforming, 1080)>
        <InlineData(3840, 2160, AspectRatioVerdict.AlreadyConforming, 2160)>
        <InlineData(1920, 1200, AspectRatioVerdict.CropHeight, 1080)>
        <InlineData(1280, 28, AspectRatioVerdict.IncompatibleTooWide, 0)>
        <InlineData(2560, 1080, AspectRatioVerdict.IncompatibleTooWide, 0)>
        <InlineData(1920, 1070, AspectRatioVerdict.IncompatibleTooWide, 0)>
        <InlineData(400, 600, AspectRatioVerdict.IncompatibleTooSmall, 224)>
        <InlineData(640, 360, AspectRatioVerdict.IncompatibleTooSmall, 360)>
        Public Sub Classify_16x9_ReturnsExpectedVerdict(width As Integer, height As Integer,
                                                        expectedVerdict As AspectRatioVerdict, expectedResultHeight As Integer)

            Dim classification As AspectRatioClassification =
                AspectRatioCalculator.Classify(width, height, AspectRatio.Widescreen16x9, tolerancePixels:=3, minimumHeight:=480)

            Assert.Equal(expectedVerdict, classification.Verdict)
            Assert.Equal(expectedResultHeight, classification.ResultHeight)
            Assert.Equal(width, classification.Width)
            Assert.Equal(height, classification.Height)
            Assert.Same(AspectRatio.Widescreen16x9, classification.Ratio)
        End Sub

        ''' <summary>
        ''' Portrait (mobile) ratios must work with the same rules.
        ''' </summary>
        ''' <param name="ratioKey">The ratio key.</param>
        ''' <param name="width">The image width.</param>
        ''' <param name="height">The image height.</param>
        ''' <param name="expectedVerdict">The expected verdict.</param>
        ''' <param name="expectedResultHeight">The expected result height.</param>
        <Theory>
        <InlineData("9:16", 1080, 2400, AspectRatioVerdict.CropHeight, 1920)>
        <InlineData("9:20", 1080, 2400, AspectRatioVerdict.AlreadyConforming, 2400)>
        <InlineData("9:19.5", 1170, 2532, AspectRatioVerdict.AlreadyConforming, 2532)>
        <InlineData("9:16", 1920, 1080, AspectRatioVerdict.IncompatibleTooWide, 0)>
        <InlineData("21:9", 2560, 1080, AspectRatioVerdict.AlreadyConforming, 1080)>
        <InlineData("1:1", 1000, 1200, AspectRatioVerdict.CropHeight, 1000)>
        Public Sub Classify_OtherRatios_ReturnsExpectedVerdict(ratioKey As String, width As Integer, height As Integer,
                                                               expectedVerdict As AspectRatioVerdict, expectedResultHeight As Integer)

            Dim classification As AspectRatioClassification =
                AspectRatioCalculator.Classify(width, height, AspectRatio.FromKey(ratioKey), tolerancePixels:=3, minimumHeight:=480)

            Assert.Equal(expectedVerdict, classification.Verdict)
            Assert.Equal(expectedResultHeight, classification.ResultHeight)
        End Sub

        ''' <summary>
        ''' The tolerance boundary is inclusive: a delta equal to the tolerance still conforms.
        ''' </summary>
        <Fact>
        Public Sub Classify_DeltaEqualToTolerance_IsAlreadyConforming()
            ' Exact height for 1920 is 1080: 1083 is exactly 3 px away.
            Assert.Equal(AspectRatioVerdict.AlreadyConforming,
                         AspectRatioCalculator.Classify(1920, 1083, AspectRatio.Widescreen16x9, 3, 0).Verdict)

            Assert.Equal(AspectRatioVerdict.CropHeight,
                         AspectRatioCalculator.Classify(1920, 1084, AspectRatio.Widescreen16x9, 3, 0).Verdict)
        End Sub

        ''' <summary>
        ''' A zero tolerance accepts only exact matches.
        ''' </summary>
        <Fact>
        Public Sub Classify_ZeroTolerance_AcceptsOnlyExactMatches()
            Assert.Equal(AspectRatioVerdict.AlreadyConforming,
                         AspectRatioCalculator.Classify(1920, 1080, AspectRatio.Widescreen16x9, 0, 0).Verdict)

            Assert.Equal(AspectRatioVerdict.CropHeight,
                         AspectRatioCalculator.Classify(1920, 1081, AspectRatio.Widescreen16x9, 0, 0).Verdict)
        End Sub

        ''' <summary>
        ''' Crop heights are always strictly even and never above the exact target height.
        ''' </summary>
        <Fact>
        Public Sub Classify_CropHeight_IsAlwaysEvenAndNotAboveExactHeight()
            For width As Integer = 100 To 3000 Step 7
                Dim classification As AspectRatioClassification =
                    AspectRatioCalculator.Classify(width, width * 2, AspectRatio.Widescreen16x9, 3, 0)

                Assert.Equal(AspectRatioVerdict.CropHeight, classification.Verdict)
                Assert.Equal(0, classification.ResultHeight Mod 2)
                Assert.True(classification.ResultHeight <= classification.ExactTargetHeight)
                Assert.True(classification.ExactTargetHeight - classification.ResultHeight < 2.0R)
            Next
        End Sub

        ''' <summary>
        ''' A minimum height below 2 px is raised to 2 px, so a result can never be 0 px.
        ''' </summary>
        <Fact>
        Public Sub Classify_MinimumHeightBelowAbsoluteMinimum_IsRaised()
            ' 3x10 -> exact height 1.6875 -> floor 1 -> even 0: must be "too small", never a "0" folder.
            Dim classification As AspectRatioClassification =
                AspectRatioCalculator.Classify(3, 10, AspectRatio.Widescreen16x9, 0, minimumHeight:=0)

            Assert.Equal(AspectRatioVerdict.IncompatibleTooSmall, classification.Verdict)
            Assert.Equal(AspectRatioCalculator.AbsoluteMinimumHeight, classification.MinimumHeight)
        End Sub

        ''' <summary>
        ''' Invalid numeric arguments must fail fast.
        ''' </summary>
        ''' <param name="width">The width.</param>
        ''' <param name="height">The height.</param>
        ''' <param name="tolerance">The tolerance.</param>
        ''' <param name="minimumHeight">The minimum height.</param>
        <Theory>
        <InlineData(0, 100, 3, 0)>
        <InlineData(-1, 100, 3, 0)>
        <InlineData(100, 0, 3, 0)>
        <InlineData(100, -5, 3, 0)>
        <InlineData(100, 100, -1, 0)>
        <InlineData(100, 100, 3, -1)>
        Public Sub Classify_InvalidArguments_Throw(width As Integer, height As Integer, tolerance As Integer, minimumHeight As Integer)
            Assert.Throws(Of ArgumentOutOfRangeException)(
                Sub() AspectRatioCalculator.Classify(width, height, AspectRatio.Widescreen16x9, tolerance, minimumHeight))
        End Sub

        ''' <summary>
        ''' A null ratio must fail fast.
        ''' </summary>
        <Fact>
        Public Sub Classify_NullRatio_Throws()
            Assert.Throws(Of ArgumentNullException)(Sub() AspectRatioCalculator.Classify(100, 100, Nothing, 3, 0))
        End Sub

        ''' <summary>
        ''' Folder names per verdict.
        ''' </summary>
        <Fact>
        Public Sub ResolveTargetFolderName_ReturnsFolderPerVerdict()
            Dim options As New ImageGroupingOptions(IO.Path.GetTempPath(), True, AspectRatio.Widescreen16x9, 3, 480)

            Assert.Equal("1118", AspectRatioCalculator.ResolveTargetFolderName(
                         AspectRatioCalculator.Classify(1990, 1323, AspectRatio.Widescreen16x9, 3, 480), options))

            Assert.Equal(options.AlreadyConformingFolderName, AspectRatioCalculator.ResolveTargetFolderName(
                         AspectRatioCalculator.Classify(1920, 1080, AspectRatio.Widescreen16x9, 3, 480), options))

            Assert.Equal(options.IncompatibleFolderName, AspectRatioCalculator.ResolveTargetFolderName(
                         AspectRatioCalculator.Classify(1280, 28, AspectRatio.Widescreen16x9, 3, 480), options))

            Assert.Equal(options.IncompatibleFolderName, AspectRatioCalculator.ResolveTargetFolderName(
                         AspectRatioCalculator.Classify(400, 600, AspectRatio.Widescreen16x9, 3, 480), options))
        End Sub

        ''' <summary>
        ''' Null arguments must fail fast.
        ''' </summary>
        <Fact>
        Public Sub ResolveTargetFolderName_NullArguments_Throw()
            Dim options As New ImageGroupingOptions(IO.Path.GetTempPath(), True, AspectRatio.Widescreen16x9, 3, 480)
            Dim classification As AspectRatioClassification = AspectRatioCalculator.Classify(1920, 1080, AspectRatio.Widescreen16x9, 3, 480)

            Assert.Throws(Of ArgumentNullException)(Sub() AspectRatioCalculator.ResolveTargetFolderName(Nothing, options))
            Assert.Throws(Of ArgumentNullException)(Sub() AspectRatioCalculator.ResolveTargetFolderName(classification, Nothing))
        End Sub

    End Class

End Namespace
