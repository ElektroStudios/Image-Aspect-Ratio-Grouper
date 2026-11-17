Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.IO

#End Region

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Immutable set of options that drive a single <see cref="ImageGroupingService"/> run.
    ''' </summary>
    Public NotInheritable Class ImageGroupingOptions

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The default pixel tolerance delta allowed when determining if an image already conforms.
        ''' </summary>
        Public Const DefaultTolerancePixels As Integer = 3

        ''' <summary>
        ''' The default minimum result height, in pixels.
        ''' </summary>
        Public Const DefaultMinimumHeight As Integer = 480

        ''' <summary>
        ''' The smallest allowed tolerance, in pixels.
        ''' </summary>
        Public Const MinimumTolerancePixels As Integer = 0

        ''' <summary>
        ''' The largest allowed tolerance, in pixels.
        ''' </summary>
        Public Const MaximumTolerancePixels As Integer = 100

        ''' <summary>
        ''' The smallest allowed minimum result height, in pixels.
        ''' </summary>
        Public Const MinimumHeightLowerBound As Integer = 0

        ''' <summary>
        ''' The largest allowed minimum result height, in pixels.
        ''' </summary>
        Public Const MinimumHeightUpperBound As Integer = 10000


#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the absolute directory path containing the images to process.
        ''' </summary>
        Public ReadOnly Property TargetDirectory As String

        ''' <summary>
        ''' Gets a value indicating whether the search must include all subdirectories.
        ''' </summary>
        Public ReadOnly Property SearchRecursively As Boolean

        ''' <summary>
        ''' Gets the target aspect ratio.
        ''' </summary>
        Public ReadOnly Property AspectRatio As AspectRatio

        ''' <summary>
        ''' Gets the pixel tolerance delta allowed when determining if an image already conforms.
        ''' </summary>
        Public ReadOnly Property TolerancePixels As Integer

        ''' <summary>
        ''' Gets the minimum acceptable result height, in pixels.
        ''' </summary>
        Public ReadOnly Property MinimumHeight As Integer

        ''' <summary>
        ''' Gets the folder name used to store images that already conform (e.g. <c>"# Already 16∕9"</c>).
        ''' </summary>
        Public ReadOnly Property AlreadyConformingFolderName As String

        ''' <summary>
        ''' Gets the folder name used to store incompatible images (e.g. <c>"# Incompatible 16∕9"</c>).
        ''' </summary>
        Public ReadOnly Property IncompatibleFolderName As String

        ''' <summary>
        ''' Gets the read-only collection of allowed (lower-case, dot-prefixed) file extensions.
        ''' </summary>
        Public ReadOnly Property AllowedExtensions As ReadOnlyCollection(Of String)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ImageGroupingOptions"/> class using the default extension list.
        ''' </summary>
        ''' <param name="targetDirectory">The absolute directory path containing the images to process.</param>
        ''' <param name="searchRecursively">Whether to include all subdirectories.</param>
        ''' <param name="aspectRatio">The target aspect ratio.</param>
        ''' <param name="tolerancePixels">The pixel tolerance delta. Must be zero or greater.</param>
        ''' <param name="minimumHeight">The minimum acceptable result height. Must be zero or greater.</param>
        ''' <exception cref="ArgumentNullException">Thrown when a reference argument is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentException">Thrown when <paramref name="targetDirectory"/> is empty or not rooted.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when a numeric argument is negative.</exception>
        Public Sub New(targetDirectory As String, searchRecursively As Boolean, aspectRatio As AspectRatio,
                       tolerancePixels As Integer, minimumHeight As Integer)

            If targetDirectory Is Nothing Then
                Throw New ArgumentNullException(NameOf(targetDirectory))
            End If

            If String.IsNullOrWhiteSpace(targetDirectory) Then
                Throw New ArgumentException("The target directory cannot be empty.", NameOf(targetDirectory))
            End If

            If Not Path.IsPathRooted(targetDirectory) Then
                Throw New ArgumentException($"The target directory must be an absolute path: {targetDirectory}", NameOf(targetDirectory))
            End If

            If aspectRatio Is Nothing Then
                Throw New ArgumentNullException(NameOf(aspectRatio))
            End If

            If tolerancePixels < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(tolerancePixels), tolerancePixels, "Tolerance cannot be negative.")
            End If

            If minimumHeight < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(minimumHeight), minimumHeight, "Minimum height cannot be negative.")
            End If

            Me.TargetDirectory = targetDirectory
            Me.SearchRecursively = searchRecursively
            Me.AspectRatio = aspectRatio
            Me.TolerancePixels = tolerancePixels
            Me.MinimumHeight = minimumHeight
            Me.AlreadyConformingFolderName = ImageGroupingOptions.GetAlreadyConformingFolderName(aspectRatio)
            Me.IncompatibleFolderName = ImageGroupingOptions.GetIncompatibleFolderName(aspectRatio)
            Me.AllowedExtensions = ImageDimensionReader.SupportedExtensions
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the folder name used to store images that already conform to the specified ratio.
        ''' </summary>
        ''' <param name="aspectRatio">The aspect ratio.</param>
        ''' <returns>The folder name (e.g. <c>"# Already 16∕9"</c>).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="aspectRatio"/> is <see langword="Nothing"/>.</exception>
        Public Shared Function GetAlreadyConformingFolderName(aspectRatio As AspectRatio) As String
            If aspectRatio Is Nothing Then
                Throw New ArgumentNullException(NameOf(aspectRatio))
            End If

            Return $"# Already {aspectRatio.FolderToken}"
        End Function

        ''' <summary>
        ''' Gets the folder name used to store images that are incompatible with the specified ratio.
        ''' </summary>
        ''' <param name="aspectRatio">The aspect ratio.</param>
        ''' <returns>The folder name (e.g. <c>"# Incompatible 16∕9"</c>).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="aspectRatio"/> is <see langword="Nothing"/>.</exception>
        Public Shared Function GetIncompatibleFolderName(aspectRatio As AspectRatio) As String
            If aspectRatio Is Nothing Then
                Throw New ArgumentNullException(NameOf(aspectRatio))
            End If

            Return $"# Incompatible {aspectRatio.FolderToken}"
        End Function

#End Region

    End Class

End Namespace
