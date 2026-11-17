Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Immutable final statistics of a completed <see cref="ImageGroupingService"/> run.
    ''' </summary>
    Public NotInheritable Class ImageGroupingSummary

#Region " Properties "

        ''' <summary>
        ''' Gets the total number of compatible image files found.
        ''' </summary>
        Public ReadOnly Property TotalFiles As Integer

        ''' <summary>
        ''' Gets the number of files moved into a crop-height folder.
        ''' </summary>
        Public ReadOnly Property MovedToCropFolderCount As Integer

        ''' <summary>
        ''' Gets the number of files moved into the already-conforming folder.
        ''' </summary>
        Public ReadOnly Property MovedToAlreadyConformingCount As Integer

        ''' <summary>
        ''' Gets the number of files moved into the incompatible folder.
        ''' </summary>
        Public ReadOnly Property MovedToIncompatibleCount As Integer

        ''' <summary>
        ''' Gets the number of files skipped because they already reside in their target folder.
        ''' </summary>
        Public ReadOnly Property SkippedCount As Integer

        ''' <summary>
        ''' Gets the number of files that failed to be processed.
        ''' </summary>
        Public ReadOnly Property FailedCount As Integer

        ''' <summary>
        ''' Gets the total elapsed processing time.
        ''' </summary>
        Public ReadOnly Property Elapsed As TimeSpan

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingSummary"/> class.
        ''' </summary>
        ''' <param name="totalFiles">The total number of compatible image files found.</param>
        ''' <param name="movedToCropFolderCount">The number of files moved into a crop-height folder.</param>
        ''' <param name="movedToAlreadyConformingCount">The number of files moved into the already-conforming folder.</param>
        ''' <param name="movedToIncompatibleCount">The number of files moved into the incompatible folder.</param>
        ''' <param name="skippedCount">The number of skipped files.</param>
        ''' <param name="failedCount">The number of failed files.</param>
        ''' <param name="elapsed">The total elapsed processing time.</param>
        ''' <exception cref="InvalidOperationException">Thrown when the partial counters do not add up to <paramref name="totalFiles"/>.</exception>
        Friend Sub New(totalFiles As Integer, movedToCropFolderCount As Integer, movedToAlreadyConformingCount As Integer,
                       movedToIncompatibleCount As Integer, skippedCount As Integer, failedCount As Integer, elapsed As TimeSpan)

            Dim accountedFiles As Integer = movedToCropFolderCount + movedToAlreadyConformingCount + movedToIncompatibleCount + skippedCount + failedCount
            If accountedFiles <> totalFiles Then
                Throw New InvalidOperationException($"Summary counters are inconsistent: {accountedFiles} accounted file(s) vs {totalFiles} total file(s).")
            End If

            Me.TotalFiles = totalFiles
            Me.MovedToCropFolderCount = movedToCropFolderCount
            Me.MovedToAlreadyConformingCount = movedToAlreadyConformingCount
            Me.MovedToIncompatibleCount = movedToIncompatibleCount
            Me.SkippedCount = skippedCount
            Me.FailedCount = failedCount
            Me.Elapsed = elapsed
        End Sub

#End Region

    End Class

End Namespace
