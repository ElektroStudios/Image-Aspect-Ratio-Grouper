Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Globalization
Imports System.IO

Imports ImageAspectRatioGrouper.Core

#End Region

Namespace ImageAspectRatioGrouper.Tests.Core

    ''' <summary>
    ''' Tests for <see cref="AspectRatio"/>.
    ''' </summary>
    Public NotInheritable Class AspectRatioTests

        ''' <summary>
        ''' The default ratio must be 16:9.
        ''' </summary>
        <Fact>
        Public Sub Default_Is16x9()
            Assert.Same(AspectRatio.Widescreen16x9, AspectRatio.Default)
            Assert.Equal("16:9", AspectRatio.Default.Key)
        End Sub

        ''' <summary>
        ''' The catalog must contain both desktop and mobile ratios, without duplicated keys.
        ''' </summary>
        <Fact>
        Public Sub StandardRatios_HaveUniqueKeys_AndIncludeDesktopAndMobile()
            Dim keys As List(Of String) = AspectRatio.StandardRatios.Select(Function(ratio As AspectRatio) ratio.Key).ToList()

            Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count())
            Assert.Contains(AspectRatio.StandardRatios, Function(ratio As AspectRatio) Not ratio.IsPortrait)
            Assert.Contains(AspectRatio.StandardRatios, Function(ratio As AspectRatio) ratio.IsPortrait)
            Assert.Same(AspectRatio.Default, AspectRatio.StandardRatios(0))
        End Sub

        ''' <summary>
        ''' Every standard ratio must be retrievable by its own key.
        ''' </summary>
        <Fact>
        Public Sub FromKey_RoundTripsEveryStandardRatio()
            For Each ratio As AspectRatio In AspectRatio.StandardRatios
                Assert.Same(ratio, AspectRatio.FromKey(ratio.Key))
            Next
        End Sub

        ''' <summary>
        ''' Unknown keys must fail fast.
        ''' </summary>
        ''' <param name="key">The unknown key.</param>
        <Theory>
        <InlineData("7:3")>
        <InlineData("")>
        <InlineData("16/9")>
        <InlineData("16:9 ")>
        Public Sub FromKey_UnknownKey_Throws(key As String)
            Assert.Throws(Of ArgumentException)(Sub() AspectRatio.FromKey(key))
        End Sub

        ''' <summary>
        ''' A null key must fail fast.
        ''' </summary>
        <Fact>
        Public Sub FromKey_NullKey_Throws()
            Assert.Throws(Of ArgumentNullException)(Sub() AspectRatio.FromKey(Nothing))
        End Sub

        ''' <summary>
        ''' Folder tokens are used in folder names, so they must be valid file name characters
        ''' and must not contain the ASCII ':' or '/'.
        ''' </summary>
        <Fact>
        Public Sub FolderToken_IsFileSystemSafe()
            Dim invalidCharacters As Char() = Path.GetInvalidFileNameChars().Concat({":"c, "/"c, "\"c}).ToArray()

            For Each ratio As AspectRatio In AspectRatio.StandardRatios
                Assert.True(ratio.FolderToken.IndexOfAny(invalidCharacters) < 0, $"Invalid folder token for {ratio.Key}: {ratio.FolderToken}")
            Next

            Assert.Equal($"16{ChrW(&H2215)}9", AspectRatio.Widescreen16x9.FolderToken)
        End Sub

        ''' <summary>
        ''' The exact height must follow width * vertical / horizontal.
        ''' </summary>
        <Fact>
        Public Sub GetExactHeight_ReturnsWidthScaledByRatio()
            Assert.Equal(1080.0R, AspectRatio.Widescreen16x9.GetExactHeight(1920))
            Assert.Equal(1119.375R, AspectRatio.Widescreen16x9.GetExactHeight(1990))
            Assert.Equal(1080.0R, AspectRatio.Ultrawide21x9.GetExactHeight(2560))
            Assert.Equal(1920.0R, AspectRatio.Phone9x16.GetExactHeight(1080))
        End Sub

        ''' <summary>
        ''' List controls display <see cref="AspectRatio.ToString"/>.
        ''' </summary>
        <Fact>
        Public Sub ToString_ReturnsDisplayName()
            Assert.Equal(AspectRatio.Widescreen16x9.DisplayName, AspectRatio.Widescreen16x9.ToString())
            Assert.StartsWith("16:9", AspectRatio.Widescreen16x9.ToString(), StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' Every example resolution advertised in the UI must actually conform to its ratio with the default tolerance.
        ''' A wrong example would mislead the user into expecting a different result.
        ''' </summary>
        <Fact>
        Public Sub ExampleResolutions_ConformToTheirRatio_WithDefaultTolerance()
            For Each ratio As AspectRatio In AspectRatio.StandardRatios
                For Each resolution As String In ratio.ExampleResolutions.Split(","c)
                    Dim parts As String() = resolution.Trim().Split("x"c)
                    Dim width As Integer = Integer.Parse(parts(0), CultureInfo.InvariantCulture)
                    Dim height As Integer = Integer.Parse(parts(1), CultureInfo.InvariantCulture)

                    Dim classification As AspectRatioClassification =
                        AspectRatioCalculator.Classify(width, height, ratio, ImageGroupingOptions.DefaultTolerancePixels, minimumHeight:=0)

                    Assert.True(classification.Verdict = AspectRatioVerdict.AlreadyConforming,
                                $"{width}x{height} is advertised as {ratio.Key} but classifies as {classification.Verdict} (exact height {classification.ExactTargetHeight}).")
                Next
            Next
        End Sub

    End Class

End Namespace
