Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Immutable result of evaluating an image's dimensions against a target <see cref="AspectRatio"/>.
    ''' </summary>
    Public NotInheritable Class AspectRatioClassification

#Region " Properties "

        ''' <summary>
        ''' Gets the original image width, in pixels.
        ''' </summary>
        Public ReadOnly Property Width As Integer

        ''' <summary>
        ''' Gets the original image height, in pixels.
        ''' </summary>
        Public ReadOnly Property Height As Integer

        ''' <summary>
        ''' Gets the target aspect ratio used for the evaluation.
        ''' </summary>
        Public ReadOnly Property Ratio As AspectRatio

        ''' <summary>
        ''' Gets the mathematically exact target height for <see cref="Width"/>.
        ''' </summary>
        Public ReadOnly Property ExactTargetHeight As Double

        ''' <summary>
        ''' Gets the verdict.
        ''' </summary>
        Public ReadOnly Property Verdict As AspectRatioVerdict

        ''' <summary>
        ''' Gets the resulting height: the original height for <see cref="AspectRatioVerdict.AlreadyConforming"/>,
        ''' the strictly even downwards crop height for <see cref="AspectRatioVerdict.CropHeight"/>
        ''' and <see cref="AspectRatioVerdict.IncompatibleTooSmall"/>, or zero for <see cref="AspectRatioVerdict.IncompatibleTooWide"/>.
        ''' </summary>
        Public ReadOnly Property ResultHeight As Integer

        ''' <summary>
        ''' Gets the effective minimum height that was enforced.
        ''' </summary>
        Public ReadOnly Property MinimumHeight As Integer

        ''' <summary>
        ''' Gets a value indicating whether the image is incompatible with the target ratio.
        ''' </summary>
        Public ReadOnly Property IsIncompatible As Boolean
            Get
                Return Me.Verdict = AspectRatioVerdict.IncompatibleTooWide OrElse
                       Me.Verdict = AspectRatioVerdict.IncompatibleTooSmall
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AspectRatioClassification"/> class.
        ''' </summary>
        ''' <param name="width">The original image width.</param>
        ''' <param name="height">The original image height.</param>
        ''' <param name="ratio">The target aspect ratio.</param>
        ''' <param name="exactTargetHeight">The exact target height for the given width.</param>
        ''' <param name="verdict">The verdict.</param>
        ''' <param name="resultHeight">The resulting height.</param>
        ''' <param name="minimumHeight">The effective minimum height.</param>
        Friend Sub New(width As Integer, height As Integer, ratio As AspectRatio, exactTargetHeight As Double,
                       verdict As AspectRatioVerdict, resultHeight As Integer, minimumHeight As Integer)

            Me.Width = width
            Me.Height = height
            Me.Ratio = ratio
            Me.ExactTargetHeight = exactTargetHeight
            Me.Verdict = verdict
            Me.ResultHeight = resultHeight
            Me.MinimumHeight = minimumHeight
        End Sub

#End Region

    End Class

End Namespace
