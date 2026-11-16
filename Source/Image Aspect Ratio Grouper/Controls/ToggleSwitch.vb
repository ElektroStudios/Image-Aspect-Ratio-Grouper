Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Imports ImageAspectRatioGrouper.Theming

#End Region

Namespace ImageAspectRatioGrouper.Controls

    ''' <summary>
    ''' A Windows 11 style, fully custom-painted on/off toggle switch.
    ''' <para></para>
    ''' Supports mouse, keyboard (Space/Enter), a subtle accent-border focus cue and theming.
    ''' </summary>
    <DefaultEvent(NameOf(ToggleSwitch.CheckedChanged))>
    <DefaultProperty(NameOf(ToggleSwitch.Checked))>
    <ToolboxItem(True)>
    Public Class ToggleSwitch : Inherits Control : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Gap between the track edge and the thumb.
        ''' </summary>
        Private Const ThumbInset As Single = 4.0F

#End Region

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="Checked"/>.
        ''' </summary>
        Private checkedValue As Boolean

        ''' <summary>
        ''' Whether the mouse pointer is over the control.
        ''' </summary>
        Private isHovered As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Events "

        ''' <summary>
        ''' Occurs when the value of the <see cref="Checked"/> property changes.
        ''' </summary>
        <Category("Property Changed")>
        <Description("Occurs when the value of the Checked property changes.")>
        Public Event CheckedChanged As EventHandler

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ToggleSwitch"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw Or
                        ControlStyles.Selectable, True)
            Me.SetStyle(ControlStyles.StandardDoubleClick, False)

            Me.Cursor = Cursors.Hand
            Me.TabStop = True
            Me.AccessibleRole = AccessibleRole.CheckButton
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets a value indicating whether the toggle is in the "on" state.
        ''' </summary>
        <Category("Appearance")>
        <Description("Indicates whether the toggle is in the 'on' state.")>
        <DefaultValue(False)>
        <Bindable(True)>
        Public Property Checked As Boolean
            Get
                Return Me.checkedValue
            End Get
            Set(value As Boolean)
                If Me.checkedValue = value Then
                    Return
                End If

                Me.checkedValue = value
                Me.AccessibleDescription = If(value, "On", "Off")
                Me.Invalidate()
                Me.OnCheckedChanged(EventArgs.Empty)
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(40, 20)
            End Get
        End Property

        ''' <summary>
        ''' This property is not relevant for this control.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Overrides Property Text As String
            Get
                Return MyBase.Text
            End Get
            Set(value As String)
                MyBase.Text = value
            End Set
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette
            Me.BackColor = palette.WindowBackground
            Me.ForeColor = palette.Foreground
            Me.Invalidate()
        End Sub

#End Region

#Region " Event Invocators "

        ''' <summary>
        ''' Raises the <see cref="CheckedChanged"/> event.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overridable Sub OnCheckedChanged(e As EventArgs)
            RaiseEvent CheckedChanged(Me, e)
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.Clear(Me.BackColor)

            ' The track fills the whole client area, so it is exactly centered within the control bounds
            ' (no hidden inner margin that would misalign it against neighboring labels).
            Dim trackBounds As New RectangleF(0.5F, 0.5F, Me.ClientSize.Width - 1.0F, Me.ClientSize.Height - 1.0F)

            If trackBounds.Width <= 0.0F OrElse trackBounds.Height <= 0.0F Then
                Return ' Too small to draw anything meaningful (e.g. while being resized in the designer).
            End If

            Dim trackRadius As Single = trackBounds.Height / 2.0F
            Dim trackFill As Color
            Dim trackBorder As Color
            Dim thumbColor As Color

            If Not Me.Enabled Then
                trackFill = If(Me.checkedValue, Me.palette.DisabledForeground, Me.palette.DisabledBackground)
                trackBorder = Me.palette.DisabledForeground
                thumbColor = If(Me.checkedValue, Me.palette.DisabledBackground, Me.palette.DisabledForeground)

            ElseIf Me.checkedValue Then
                trackFill = If(Me.isHovered, Me.palette.AccentHover, Me.palette.Accent)
                trackBorder = trackFill
                thumbColor = Me.palette.ToggleOnThumb

            Else
                trackFill = If(Me.isHovered, Me.palette.ButtonHover, Me.palette.ToggleOffTrack)
                trackBorder = If(Me.Focused AndAlso Me.ShowFocusCues, Me.palette.Accent, Me.palette.ToggleOffThumb) ' Subtle focus cue, consistent with the text boxes.
                thumbColor = Me.palette.ToggleOffThumb
            End If

            Using trackPath As GraphicsPath = DrawingHelper.CreateRoundedRectangle(trackBounds, trackRadius),
                  fillBrush As New SolidBrush(trackFill),
                  borderPen As New Pen(trackBorder, 1.0F)

                g.FillPath(fillBrush, trackPath)
                g.DrawPath(borderPen, trackPath)
            End Using

            Dim thumbDiameter As Single = trackBounds.Height - (ThumbInset * 2.0F)
            If Me.isHovered AndAlso Me.Enabled Then
                thumbDiameter += 1.0F
            End If

            Dim thumbX As Single = If(Me.checkedValue,
                                      trackBounds.Right - ThumbInset - thumbDiameter,
                                      trackBounds.Left + ThumbInset)
            Dim thumbY As Single = trackBounds.Top + ((trackBounds.Height - thumbDiameter) / 2.0F)

            Using thumbBrush As New SolidBrush(thumbColor)
                g.FillEllipse(thumbBrush, thumbX, thumbY, thumbDiameter, thumbDiameter)
            End Using
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnClick(e As EventArgs)
            Me.Focus()
            Me.Checked = Not Me.Checked
            MyBase.OnClick(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            If e.KeyCode = Keys.Space OrElse e.KeyCode = Keys.Enter Then
                Me.Checked = Not Me.Checked
                e.Handled = True
            End If

            MyBase.OnKeyDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
            MyBase.OnMouseEnter(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            Me.isHovered = False
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnGotFocus(e As EventArgs)
            Me.Invalidate()
            MyBase.OnGotFocus(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLostFocus(e As EventArgs)
            Me.Invalidate()
            MyBase.OnLostFocus(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            Me.Invalidate()
            MyBase.OnEnabledChanged(e)
        End Sub

#End Region

    End Class

End Namespace
