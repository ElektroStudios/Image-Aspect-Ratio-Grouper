Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

Imports ImageAspectRatioGrouper.Core

#End Region

Namespace ImageAspectRatioGrouper.Tests.Core

    ''' <summary>
    ''' Tests for the immutable model types: <see cref="ImageGroupingOptions"/>,
    ''' <see cref="ImageGroupingProgress"/> and <see cref="ImageGroupingSummary"/>.
    ''' </summary>
    Public NotInheritable Class ImageGroupingModelTests

        ''' <summary>
        ''' A rooted path that is valid on any platform.
        ''' </summary>
        Private Shared ReadOnly RootedPath As String = Path.GetTempPath()

        ''' <summary>
        ''' The options expose the special folder names of their ratio.
        ''' </summary>
        <Fact>
        Public Sub Options_ExposeRatioSpecificFolderNames()
            Dim options As New ImageGroupingOptions(RootedPath, False, AspectRatio.Phone9x16, 5, 100)

            Assert.Equal($"# Already 9{ChrW(&H2215)}16", options.AlreadyConformingFolderName)
            Assert.Equal($"# Incompatible 9{ChrW(&H2215)}16", options.IncompatibleFolderName)
            Assert.Equal(RootedPath, options.TargetDirectory)
            Assert.False(options.SearchRecursively)
            Assert.Equal(5, options.TolerancePixels)
            Assert.Equal(100, options.MinimumHeight)
            Assert.Contains(".jpg", options.AllowedExtensions)
            Assert.Contains(".png", options.AllowedExtensions)
        End Sub

        ''' <summary>
        ''' The allowed extensions cannot be modified by callers.
        ''' </summary>
        <Fact>
        Public Sub Options_AllowedExtensions_AreReadOnly()
            Dim options As New ImageGroupingOptions(RootedPath, False, AspectRatio.Default, 3, 480)
            Dim extensions As IList(Of String) = options.AllowedExtensions

            Assert.True(extensions.IsReadOnly)
            Assert.Throws(Of NotSupportedException)(Sub() extensions.Add(".gif"))
        End Sub

        ''' <summary>
        ''' Invalid option arguments must fail fast.
        ''' </summary>
        <Fact>
        Public Sub Options_InvalidArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingOptions(Nothing, True, AspectRatio.Default, 3, 480))
            Assert.Throws(Of ArgumentException)(Function() New ImageGroupingOptions("   ", True, AspectRatio.Default, 3, 480))
            Assert.Throws(Of ArgumentException)(Function() New ImageGroupingOptions("relative\path", True, AspectRatio.Default, 3, 480))
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingOptions(RootedPath, True, Nothing, 3, 480))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedPath, True, AspectRatio.Default, -1, 480))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingOptions(RootedPath, True, AspectRatio.Default, 3, -1))
        End Sub

        ''' <summary>
        ''' The static folder name helpers must fail fast on null.
        ''' </summary>
        <Fact>
        Public Sub Options_FolderNameHelpers_NullRatio_Throw()
            Assert.Throws(Of ArgumentNullException)(Sub() ImageGroupingOptions.GetAlreadyConformingFolderName(Nothing))
            Assert.Throws(Of ArgumentNullException)(Sub() ImageGroupingOptions.GetIncompatibleFolderName(Nothing))
        End Sub

        ''' <summary>
        ''' Progress reports keep their values.
        ''' </summary>
        <Fact>
        Public Sub Progress_KeepsValues()
            Dim report As New ImageGroupingProgress(2, 5, "message", LogSeverity.Moved)

            Assert.Equal(2, report.ProcessedCount)
            Assert.Equal(5, report.TotalCount)
            Assert.Equal("message", report.Message)
            Assert.Equal(LogSeverity.Moved, report.Severity)
        End Sub

        ''' <summary>
        ''' Inconsistent progress counters must fail fast.
        ''' </summary>
        <Fact>
        Public Sub Progress_InvalidArguments_Throw()
            Assert.Throws(Of ArgumentNullException)(Function() New ImageGroupingProgress(0, 0, Nothing, LogSeverity.Information))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingProgress(0, -1, "m", LogSeverity.Information))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingProgress(-1, 5, "m", LogSeverity.Information))
            Assert.Throws(Of ArgumentOutOfRangeException)(Function() New ImageGroupingProgress(6, 5, "m", LogSeverity.Information))
        End Sub

        ''' <summary>
        ''' The summary validates that its partial counters add up to the total.
        ''' </summary>
        <Fact>
        Public Sub Summary_ValidatesCounters()
            Dim summary As New ImageGroupingSummary(10, 1, 2, 3, 4, 0, TimeSpan.FromSeconds(1))
            Assert.Equal(10, summary.TotalFiles)
            Assert.Equal(1, summary.MovedToCropFolderCount)
            Assert.Equal(2, summary.MovedToAlreadyConformingCount)
            Assert.Equal(3, summary.MovedToIncompatibleCount)
            Assert.Equal(4, summary.SkippedCount)
            Assert.Equal(0, summary.FailedCount)

            Assert.Throws(Of InvalidOperationException)(Function() New ImageGroupingSummary(10, 1, 1, 1, 1, 1, TimeSpan.Zero))
        End Sub

    End Class

End Namespace
