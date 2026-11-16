Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Globalization

#End Region

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Immutable target aspect ratio (e.g. 16:9), with the catalog of standard desktop and mobile ratios.
    ''' </summary>
    Public NotInheritable Class AspectRatio

#Region " Constants "

        ''' <summary>
        ''' The Unicode Division Slash (U+2215), used instead of ':' or '/' to keep folder names filesystem-safe.
        ''' </summary>
        Private Const FolderTokenSeparator As Char = ChrW(&H2215)

#End Region

#Region " Standard Ratios (desktop / landscape) "

        ''' <summary>16:9 widescreen (the most common desktop and TV ratio).</summary>
        Public Shared ReadOnly Widescreen16x9 As New AspectRatio("16:9", 16, 9, "Widescreen", "1920x1080, 2560x1440, 3840x2160")

        ''' <summary>16:10 widescreen (common on laptops and productivity monitors).</summary>
        Public Shared ReadOnly Widescreen16x10 As New AspectRatio("16:10", 16, 10, "Widescreen", "1920x1200, 2560x1600")

        ''' <summary>21:9 ultrawide (the exact ratio of 2560x1080 is 64:27).</summary>
        Public Shared ReadOnly Ultrawide21x9 As New AspectRatio("21:9", 64, 27, "Ultrawide", "2560x1080, 5120x2160")

        ''' <summary>32:9 super ultrawide.</summary>
        Public Shared ReadOnly SuperUltrawide32x9 As New AspectRatio("32:9", 32, 9, "Super ultrawide", "3840x1080, 5120x1440")

        ''' <summary>3:2 (Surface and other productivity laptops).</summary>
        Public Shared ReadOnly Laptop3x2 As New AspectRatio("3:2", 3, 2, "Laptop", "2256x1504, 2880x1920")

        ''' <summary>4:3 standard.</summary>
        Public Shared ReadOnly Standard4x3 As New AspectRatio("4:3", 4, 3, "Standard", "1024x768, 1600x1200")

        ''' <summary>5:4 classic monitor.</summary>
        Public Shared ReadOnly Classic5x4 As New AspectRatio("5:4", 5, 4, "Classic monitor", "1280x1024")

        ''' <summary>1:1 square.</summary>
        Public Shared ReadOnly Square1x1 As New AspectRatio("1:1", 1, 1, "Square", "1080x1080, 2048x2048")

#End Region

#Region " Standard Ratios (mobile / portrait) "

        ''' <summary>9:16 classic phone portrait.</summary>
        Public Shared ReadOnly Phone9x16 As New AspectRatio("9:16", 9, 16, "Phone (classic)", "720x1280, 1080x1920")

        ''' <summary>9:19.5 modern iPhone portrait.</summary>
        Public Shared ReadOnly IPhone9x19_5 As New AspectRatio("9:19.5", 9, 19.5R, "Phone (iPhone)", "1170x2532, 1179x2556, 1290x2796")

        ''' <summary>9:20 modern Android portrait.</summary>
        Public Shared ReadOnly Android9x20 As New AspectRatio("9:20", 9, 20, "Phone (Android)", "720x1600, 1080x2400, 1440x3200")

        ''' <summary>9:21 tall Android portrait.</summary>
        Public Shared ReadOnly TallPhone9x21 As New AspectRatio("9:21", 9, 21, "Phone (tall)", "1080x2520, 1440x3360")

        ''' <summary>10:16 Android tablet portrait.</summary>
        Public Shared ReadOnly AndroidTablet10x16 As New AspectRatio("10:16", 10, 16, "Tablet (Android)", "1200x1920, 1600x2560")

        ''' <summary>3:4 iPad portrait.</summary>
        Public Shared ReadOnly IPad3x4 As New AspectRatio("3:4", 3, 4, "Tablet (iPad)", "1536x2048, 2048x2732")

        ''' <summary>
        ''' All the standard ratios, in display order. Declared after the individual ratios (shared field initialization is textual).
        ''' </summary>
        Private Shared ReadOnly AllRatios As AspectRatio() = {
            Widescreen16x9, Widescreen16x10, Ultrawide21x9, SuperUltrawide32x9, Laptop3x2, Standard4x3, Classic5x4, Square1x1,
            Phone9x16, IPhone9x19_5, Android9x20, TallPhone9x21, AndroidTablet10x16, IPad3x4
        }

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the catalog of standard aspect ratios (desktop first, then mobile).
        ''' </summary>
        Public Shared ReadOnly Property StandardRatios As ReadOnlyCollection(Of AspectRatio) = Array.AsReadOnly(AspectRatio.AllRatios)

        ''' <summary>
        ''' Gets the default aspect ratio (16:9).
        ''' </summary>
        Public Shared ReadOnly Property [Default] As AspectRatio
            Get
                Return AspectRatio.Widescreen16x9
            End Get
        End Property

        ''' <summary>
        ''' Gets the unique, human-readable key of the ratio (e.g. <c>"16:9"</c>). Also used for persistence.
        ''' </summary>
        Public ReadOnly Property Key As String

        ''' <summary>
        ''' Gets the horizontal component used in the math (e.g. 16, or 64 for the 21:9 preset).
        ''' </summary>
        Public ReadOnly Property Horizontal As Double

        ''' <summary>
        ''' Gets the vertical component used in the math (e.g. 9, or 27 for the 21:9 preset).
        ''' </summary>
        Public ReadOnly Property Vertical As Double

        ''' <summary>
        ''' Gets the short descriptive name (e.g. <c>"Widescreen"</c>).
        ''' </summary>
        Public ReadOnly Property Name As String

        ''' <summary>
        ''' Gets a few example resolutions that match this ratio.
        ''' </summary>
        Public ReadOnly Property ExampleResolutions As String

        ''' <summary>
        ''' Gets a value indicating whether this is a portrait (taller than wide) ratio.
        ''' </summary>
        Public ReadOnly Property IsPortrait As Boolean
            Get
                Return Me.Vertical > Me.Horizontal
            End Get
        End Property

        ''' <summary>
        ''' Gets the filesystem-safe representation of <see cref="Key"/> (e.g. <c>"16∕9"</c>).
        ''' </summary>
        Public ReadOnly Property FolderToken As String
            Get
                Return Me.Key.Replace(":"c, FolderTokenSeparator)
            End Get
        End Property

        ''' <summary>
        ''' Gets the display text (e.g. <c>"16:9 · Widescreen"</c>).
        ''' </summary>
        Public ReadOnly Property DisplayName As String
            Get
                Return $"{Me.Key}  ·  {Me.Name}"
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AspectRatio"/> class.
        ''' </summary>
        ''' <param name="key">The unique key (e.g. <c>"16:9"</c>).</param>
        ''' <param name="horizontal">The horizontal component used in the math.</param>
        ''' <param name="vertical">The vertical component used in the math.</param>
        ''' <param name="name">The short descriptive name.</param>
        ''' <param name="exampleResolutions">A few example resolutions.</param>
        ''' <exception cref="ArgumentException">Thrown when a string argument is empty.</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when a component is not positive.</exception>
        Private Sub New(key As String, horizontal As Double, vertical As Double, name As String, exampleResolutions As String)

            If String.IsNullOrWhiteSpace(key) Then
                Throw New ArgumentException("The key cannot be empty.", NameOf(key))
            End If

            If horizontal <= 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(horizontal), horizontal, "The horizontal component must be positive.")
            End If

            If vertical <= 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(vertical), vertical, "The vertical component must be positive.")
            End If

            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("The name cannot be empty.", NameOf(name))
            End If

            If String.IsNullOrWhiteSpace(exampleResolutions) Then
                Throw New ArgumentException("The example resolutions cannot be empty.", NameOf(exampleResolutions))
            End If

            Me.Key = key
            Me.Horizontal = horizontal
            Me.Vertical = vertical
            Me.Name = name
            Me.ExampleResolutions = exampleResolutions
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the standard ratio that has the specified key.
        ''' </summary>
        ''' <param name="key">The ratio key (e.g. <c>"16:9"</c>).</param>
        ''' <returns>The matching standard ratio.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="Nothing"/>.</exception>
        ''' <exception cref="ArgumentException">Thrown when no standard ratio has the specified key.</exception>
        Public Shared Function FromKey(key As String) As AspectRatio
            If key Is Nothing Then
                Throw New ArgumentNullException(NameOf(key))
            End If

            For Each ratio As AspectRatio In AspectRatio.AllRatios
                If String.Equals(ratio.Key, key, StringComparison.Ordinal) Then
                    Return ratio
                End If
            Next

            Throw New ArgumentException($"There is no standard aspect ratio with the key '{key}'.", NameOf(key))
        End Function

        ''' <summary>
        ''' Calculates the exact height that the specified width needs to match this ratio.
        ''' </summary>
        ''' <param name="width">The width, in pixels.</param>
        ''' <returns>The exact (fractional) height.</returns>
        Public Function GetExactHeight(width As Integer) As Double
            Return width * Me.Vertical / Me.Horizontal
        End Function

        ''' <summary>
        ''' Returns <see cref="DisplayName"/>. This is the text shown by list controls.
        ''' </summary>
        ''' <returns>The display name.</returns>
        Public Overrides Function ToString() As String
            Return Me.DisplayName
        End Function

#End Region

    End Class

End Namespace
