Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Globalization

#End Region

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Pure, side-effect free aspect ratio math.
    ''' <para></para>
    ''' This class has no knowledge of files, folders or UI, which makes it trivially unit-testable.
    ''' </summary>
    Public NotInheritable Class AspectRatioCalculator

#Region " Constants "

        ''' <summary>
        ''' The absolute minimum result height: a strictly even height can never be lower than 2 pixels.
        ''' </summary>
        Public Const AbsoluteMinimumHeight As Integer = 2

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AspectRatioCalculator"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Evaluates the specified image dimensions against a target aspect ratio.
        ''' </summary>
        ''' <example>
        ''' With 16:9, a tolerance of 3 px and a minimum height of 480 px:
        ''' <list type="bullet">
        ''' <item><description>1990x1323 → exact height 1119.375 → floor 1119 → even 1118 → <see cref="AspectRatioVerdict.CropHeight"/>.</description></item>
        ''' <item><description>2948x1660 → exact height 1658.25 → |1660 - 1658.25| = 1.75 ≤ 3 → <see cref="AspectRatioVerdict.AlreadyConforming"/>.</description></item>
        ''' <item><description>1280x28 → exact height 720 → 28 is far below 720 (wider than 16:9) → <see cref="AspectRatioVerdict.IncompatibleTooWide"/>.</description></item>
        ''' <item><description>200x400 → exact height 112.5 → even 112 &lt; 480 → <see cref="AspectRatioVerdict.IncompatibleTooSmall"/>.</description></item>
        ''' </list>
        ''' </example>
        ''' <param name="width">The image width, in pixels. Must be greater than zero.</param>
        ''' <param name="height">The image height, in pixels. Must be greater than zero.</param>
        ''' <param name="ratio">The target aspect ratio.</param>
        ''' <param name="tolerancePixels">The allowed height delta to consider an image already conforming. Must be zero or greater.</param>
        ''' <param name="minimumHeight">
        ''' The minimum acceptable result height. Must be zero or greater.
        ''' Values below <see cref="AbsoluteMinimumHeight"/> are raised to it.
        ''' </param>
        ''' <returns>An immutable <see cref="AspectRatioClassification"/> describing the result.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="ratio"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when any numeric argument is out of its valid range.</exception>
        Public Shared Function Classify(width As Integer, height As Integer, ratio As AspectRatio,
                                        tolerancePixels As Integer, minimumHeight As Integer) As AspectRatioClassification

            If width <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(width), width, "Image width must be greater than zero.")
            End If

            If height <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(height), height, "Image height must be greater than zero.")
            End If

            If ratio Is Nothing Then
                Throw New ArgumentNullException(NameOf(ratio))
            End If

            If tolerancePixels < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(tolerancePixels), tolerancePixels, "Tolerance cannot be negative.")
            End If

            If minimumHeight < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(minimumHeight), minimumHeight, "Minimum height cannot be negative.")
            End If

            Dim effectiveMinimumHeight As Integer = Math.Max(minimumHeight, AbsoluteMinimumHeight)
            Dim exactTargetHeight As Double = ratio.GetExactHeight(width)
            Dim verdict As AspectRatioVerdict
            Dim resultHeight As Integer

            If Math.Abs(height - exactTargetHeight) <= tolerancePixels Then
                ' Already conforming: nothing to crop.
                verdict = AspectRatioVerdict.AlreadyConforming
                resultHeight = height

            ElseIf height > exactTargetHeight Then
                ' Taller than the ratio: crop the height downwards, enforcing strictly even numbers.
                verdict = AspectRatioVerdict.CropHeight
                resultHeight = CInt(Math.Floor(exactTargetHeight))
                If resultHeight Mod 2 <> 0 Then
                    resultHeight -= 1
                End If

            Else
                ' Wider than the ratio: reducing the height only moves the image further away from the ratio.
                Return New AspectRatioClassification(width, height, ratio, exactTargetHeight,
                                                     AspectRatioVerdict.IncompatibleTooWide, 0, effectiveMinimumHeight)
            End If

            If resultHeight < effectiveMinimumHeight Then
                verdict = AspectRatioVerdict.IncompatibleTooSmall
            End If

            Return New AspectRatioClassification(width, height, ratio, exactTargetHeight, verdict, resultHeight, effectiveMinimumHeight)
        End Function

        ''' <summary>
        ''' Resolves the name of the destination folder for the given classification.
        ''' </summary>
        ''' <param name="classification">The classification result.</param>
        ''' <param name="options">The run options (they provide the special folder names).</param>
        ''' <returns>The destination folder name (not a full path).</returns>
        ''' <exception cref="ArgumentNullException">Thrown when any argument is <see langword="Nothing"/>.</exception>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when the verdict is not a defined value.</exception>
        Public Shared Function ResolveTargetFolderName(classification As AspectRatioClassification, options As ImageGroupingOptions) As String

            If classification Is Nothing Then
                Throw New ArgumentNullException(NameOf(classification))
            End If

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            Select Case classification.Verdict
                Case AspectRatioVerdict.AlreadyConforming
                    Return options.AlreadyConformingFolderName

                Case AspectRatioVerdict.CropHeight
                    Return classification.ResultHeight.ToString(CultureInfo.InvariantCulture)

                Case AspectRatioVerdict.IncompatibleTooWide, AspectRatioVerdict.IncompatibleTooSmall
                    Return options.IncompatibleFolderName

                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(classification), CInt(classification.Verdict), GetType(AspectRatioVerdict))
            End Select
        End Function

#End Region

    End Class

End Namespace
