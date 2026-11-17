Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageAspectRatioGrouper.Core

#End Region

Namespace ImageAspectRatioGrouper.CommandLine

    ''' <summary>
    ''' Immutable result of parsing the command-line arguments (see <see cref="CommandLineParser"/>).
    ''' <para></para>
    ''' The grouping options default to the documented defaults of the application, never to the preferences saved by
    ''' the main window, so a command line always does the same thing on any computer.
    ''' </summary>
    Friend NotInheritable Class CommandLineArguments

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether the help must be shown (instead of grouping the images).
        ''' </summary>
        Friend ReadOnly Property ShowHelp As Boolean

        ''' <summary>
        ''' Gets the directory, exactly as written in the command line (not validated, not resolved), or
        ''' <see langword="Nothing"/> when <see cref="ShowHelp"/> is <see langword="True"/>.
        ''' Use <see cref="DirectoryPathResolver.Resolve"/> to validate it.
        ''' </summary>
        Friend ReadOnly Property Directory As String

        ''' <summary>
        ''' Gets a value indicating whether the images inside the subdirectories are processed too.
        ''' </summary>
        Friend ReadOnly Property SearchRecursively As Boolean

        ''' <summary>
        ''' Gets the target aspect ratio.
        ''' </summary>
        Friend ReadOnly Property AspectRatio As AspectRatio

        ''' <summary>
        ''' Gets the tolerance, in pixels.
        ''' </summary>
        Friend ReadOnly Property TolerancePixels As Integer

        ''' <summary>
        ''' Gets the minimum result height, in pixels.
        ''' </summary>
        Friend ReadOnly Property MinimumHeight As Integer

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineArguments"/> class, to group the images of a directory.
        ''' </summary>
        ''' <param name="directory">The directory, as written in the command line.</param>
        ''' <param name="searchRecursively">Whether the subdirectories are processed too.</param>
        ''' <param name="aspectRatio">The target aspect ratio.</param>
        ''' <param name="tolerancePixels">The tolerance, in pixels.</param>
        ''' <param name="minimumHeight">The minimum result height, in pixels.</param>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="directory"/> or <paramref name="aspectRatio"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when a numeric argument is out of range.</exception>
        Friend Sub New(directory As String, searchRecursively As Boolean, aspectRatio As AspectRatio,
                       tolerancePixels As Integer, minimumHeight As Integer)

            If directory Is Nothing Then
                Throw New ArgumentNullException(NameOf(directory))
            End If

            If aspectRatio Is Nothing Then
                Throw New ArgumentNullException(NameOf(aspectRatio))
            End If

            If tolerancePixels < ImageGroupingOptions.MinimumTolerancePixels OrElse tolerancePixels > ImageGroupingOptions.MaximumTolerancePixels Then
                Throw New ArgumentOutOfRangeException(NameOf(tolerancePixels), tolerancePixels,
                                                      $"The tolerance must be from {ImageGroupingOptions.MinimumTolerancePixels} to {ImageGroupingOptions.MaximumTolerancePixels}.")
            End If

            If minimumHeight < ImageGroupingOptions.MinimumHeightLowerBound OrElse minimumHeight > ImageGroupingOptions.MinimumHeightUpperBound Then
                Throw New ArgumentOutOfRangeException(NameOf(minimumHeight), minimumHeight,
                                                      $"The minimum height must be from {ImageGroupingOptions.MinimumHeightLowerBound} to {ImageGroupingOptions.MinimumHeightUpperBound}.")
            End If

            Me.Directory = directory
            Me.SearchRecursively = searchRecursively
            Me.AspectRatio = aspectRatio
            Me.TolerancePixels = tolerancePixels
            Me.MinimumHeight = minimumHeight
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="CommandLineArguments"/> class, to show the help.
        ''' </summary>
        Private Sub New()
            Me.ShowHelp = True
            Me.AspectRatio = AspectRatio.Default
            Me.TolerancePixels = ImageGroupingOptions.DefaultTolerancePixels
            Me.MinimumHeight = ImageGroupingOptions.DefaultMinimumHeight
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Creates the arguments that show the help.
        ''' </summary>
        ''' <returns>The arguments.</returns>
        Friend Shared Function ForHelp() As CommandLineArguments
            Return New CommandLineArguments()
        End Function

#End Region

    End Class

End Namespace
