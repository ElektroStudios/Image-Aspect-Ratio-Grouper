Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Specifies how an image relates to the target aspect ratio.
    ''' </summary>
    Public Enum AspectRatioVerdict As Integer

        ''' <summary>
        ''' The image already conforms to the target ratio, within the configured pixel tolerance.
        ''' </summary>
        AlreadyConforming = 0

        ''' <summary>
        ''' The image is taller than the target ratio: cropping its height downwards reaches the ratio.
        ''' </summary>
        CropHeight = 1

        ''' <summary>
        ''' The image is wider than the target ratio: no downwards (height) crop can reach the ratio.
        ''' </summary>
        IncompatibleTooWide = 2

        ''' <summary>
        ''' The resulting height would be below the configured minimum (e.g. thumbnails, icons, thin strips).
        ''' </summary>
        IncompatibleTooSmall = 3

    End Enum

End Namespace
