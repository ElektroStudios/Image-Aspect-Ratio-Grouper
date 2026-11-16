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
    ''' A themed, drop-down-list-only <see cref="ComboBox"/>.
    ''' <para></para>
    ''' The closed state is fully user-painted (1px border, chevron, focus underline), the drop-down items
    ''' are owner-drawn with the palette colors, and the native drop-down list and scroll bar use the
    ''' <c>DarkMode_CFD</c> visual style in dark mode. Optionally, each drop-down item can show a muted,
    ''' right-aligned secondary text taken from <see cref="SecondaryTextMember"/>.
    ''' </summary>
    <ToolboxItem(True)>
    Public Class ThemedComboBox : Inherits ComboBox : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Horizontal padding between the border and the text.
        ''' </summary>
        Private Const TextPadding As Integer = 6

        ''' <summary>
        ''' Width of the area that hosts the chevron.
        ''' </summary>
        Private Const ArrowAreaWidth As Integer = 24

#End Region

#Region " Fields "

        ''' <summary>
        ''' Backing field for <see cref="SecondaryTextMember"/>.
        ''' </summary>
        Private secondaryTextMemberValue As String = String.Empty

        ''' <summary>
        ''' Whether the mouse pointer is over the control.
        ''' </summary>
        Private isHovered As Boolean

        ''' <summary>
        ''' The palette currently applied.
        ''' </summary>
        Private palette As ThemePalette = ThemeManager.CurrentPalette

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ThemedComboBox"/> class.
        ''' </summary>
        Public Sub New()
            Me.SetStyle(ControlStyles.UserPaint Or
                        ControlStyles.AllPaintingInWmPaint Or
                        ControlStyles.OptimizedDoubleBuffer Or
                        ControlStyles.ResizeRedraw, True)

            MyBase.DrawMode = DrawMode.OwnerDrawFixed
            MyBase.DropDownStyle = ComboBoxStyle.DropDownList
            MyBase.FlatStyle = FlatStyle.Flat
            Me.IntegralHeight = False
            Me.MaxDropDownItems = 16
            ' Note: ItemHeight is NOT assigned here. Its setter calls CreateControl() when there is no handle yet,
            ' which would create the native window prematurely (before the control is parented). It is applied in OnHandleCreated.
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the name of the item property whose value is drawn as a muted, right-aligned
        ''' secondary text in the drop-down list. An empty string disables the secondary text.
        ''' </summary>
        <Category("Data")>
        <Description("The item property drawn as a muted, right-aligned secondary text in the drop-down list.")>
        <DefaultValue("")>
        Public Property SecondaryTextMember As String
            Get
                Return Me.secondaryTextMemberValue
            End Get
            Set(value As String)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If

                Me.secondaryTextMemberValue = value
                Me.Invalidate()
            End Set
        End Property

        ''' <summary>
        ''' This property is fixed to <see cref="DrawMode.OwnerDrawFixed"/>.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property DrawMode As DrawMode
            Get
                Return MyBase.DrawMode
            End Get
            Set(value As DrawMode)
                If value <> DrawMode.OwnerDrawFixed Then
                    Throw New NotSupportedException($"{NameOf(ThemedComboBox)} only supports {NameOf(DrawMode.OwnerDrawFixed)}.")
                End If

                MyBase.DrawMode = value
            End Set
        End Property

        ''' <summary>
        ''' This property is fixed to <see cref="ComboBoxStyle.DropDownList"/>.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property DropDownStyle As ComboBoxStyle
            Get
                Return MyBase.DropDownStyle
            End Get
            Set(value As ComboBoxStyle)
                If value <> ComboBoxStyle.DropDownList Then
                    Throw New NotSupportedException($"{NameOf(ThemedComboBox)} only supports {NameOf(ComboBoxStyle.DropDownList)}.")
                End If

                MyBase.DropDownStyle = value
            End Set
        End Property

        ''' <summary>
        ''' This property is fixed to <see cref="FlatStyle.Flat"/>.
        ''' </summary>
        <Browsable(False)>
        <EditorBrowsable(EditorBrowsableState.Never)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Shadows Property FlatStyle As FlatStyle
            Get
                Return MyBase.FlatStyle
            End Get
            Set(value As FlatStyle)
                If value <> FlatStyle.Flat Then
                    Throw New NotSupportedException($"{NameOf(ThemedComboBox)} only supports {NameOf(FlatStyle.Flat)}.")
                End If

                MyBase.FlatStyle = value
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
            Me.BackColor = palette.SurfaceBackground
            Me.ForeColor = palette.Foreground

            If Me.IsHandleCreated AndAlso Not Me.DesignMode Then
                WindowChrome.ApplyComboBoxTheme(Me, palette.IsDark)
            End If

            Me.Invalidate()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            Me.MatchInputHeight()

            If Not Me.DesignMode Then
                WindowChrome.ApplyComboBoxTheme(Me, Me.palette.IsDark)
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFontChanged(e As EventArgs)
            MyBase.OnFontChanged(e)

            If Me.IsHandleCreated Then
                Me.MatchInputHeight()
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            Dim surface As Color = If(Me.Enabled, Me.palette.SurfaceBackground, Me.palette.DisabledBackground)
            Dim textColor As Color = If(Me.Enabled, Me.palette.Foreground, Me.palette.DisabledForeground)
            Dim isActive As Boolean = Me.Focused OrElse Me.DroppedDown

            g.Clear(surface)

            ' Selected item text.
            If Me.SelectedIndex >= 0 Then
                Dim textBounds As New Rectangle(TextPadding, 0, Math.Max(1, Me.ClientSize.Width - TextPadding - ArrowAreaWidth), Me.ClientSize.Height)
                TextRenderer.DrawText(g, Me.GetItemText(Me.SelectedItem), Me.Font, textBounds, textColor,
                                      TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                      TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
            End If

            ' Chevron.
            Dim centerX As Single = Me.ClientSize.Width - (ArrowAreaWidth / 2.0F)
            Dim centerY As Single = Me.ClientSize.Height / 2.0F
            g.SmoothingMode = SmoothingMode.AntiAlias
            Using chevronPen As New Pen(textColor, 1.5F)
                g.DrawLines(chevronPen, {New PointF(centerX - 4.0F, centerY - 2.0F),
                                         New PointF(centerX, centerY + 2.0F),
                                         New PointF(centerX + 4.0F, centerY - 2.0F)})
            End Using
            g.SmoothingMode = SmoothingMode.None

            ' Border.
            Dim borderColor As Color
            If Not Me.Enabled Then
                borderColor = Me.palette.Border
            ElseIf isActive Then
                borderColor = Me.palette.Accent
            ElseIf Me.isHovered Then
                borderColor = Me.palette.BorderHover
            Else
                borderColor = Me.palette.Border
            End If

            Using borderPen As New Pen(borderColor, 1.0F)
                g.DrawRectangle(borderPen, 0, 0, Me.ClientSize.Width - 1, Me.ClientSize.Height - 1)
            End Using

        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
            If e.Index < 0 Then
                Return ' Nothing to draw: the list is empty or there is no current item.
            End If

            Dim item As Object = Me.Items(e.Index)
            Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected

            Using backgroundBrush As New SolidBrush(If(isSelected, Me.palette.MenuHighlight, Me.palette.SurfaceBackground))
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds)
            End Using

            Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                           TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix

            Dim primaryBounds As New Rectangle(e.Bounds.Left + TextPadding, e.Bounds.Top, e.Bounds.Width - (TextPadding * 2), e.Bounds.Height)
            TextRenderer.DrawText(e.Graphics, Me.GetItemText(item), Me.Font, primaryBounds, Me.palette.Foreground, flags Or TextFormatFlags.Left)

            If Not String.IsNullOrEmpty(Me.secondaryTextMemberValue) Then
                Dim secondaryValue As Object = Me.FilterItemOnProperty(item, Me.secondaryTextMemberValue)
                If secondaryValue Is Nothing OrElse secondaryValue Is item Then
                    Throw New InvalidOperationException($"The item type '{item.GetType().Name}' has no '{Me.secondaryTextMemberValue}' property (see {NameOf(Me.SecondaryTextMember)}).")
                End If

                TextRenderer.DrawText(e.Graphics, Convert.ToString(secondaryValue, Globalization.CultureInfo.InvariantCulture),
                                      Me.Font, primaryBounds, Me.palette.MutedForeground, flags Or TextFormatFlags.Right)
            End If

            MyBase.OnDrawItem(e)
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
        Protected Overrides Sub OnDropDown(e As EventArgs)
            Me.Invalidate()
            MyBase.OnDropDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnDropDownClosed(e As EventArgs)
            Me.Invalidate()
            MyBase.OnDropDownClosed(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnSelectedIndexChanged(e As EventArgs)
            Me.Invalidate()
            MyBase.OnSelectedIndexChanged(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            Me.Invalidate()
            MyBase.OnEnabledChanged(e)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Adjusts the item height so that the total control height equals <see cref="ControlMetrics.GetInputHeight"/>,
        ''' exactly like the other themed input controls.
        ''' <para></para>
        ''' The height of a drop-down-list combo box is not settable: it is its selection-field (item) height plus a
        ''' native frame whose size depends on the OS and visual style, so the frame is measured instead of assumed.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the handle has not been created, or the font is too small to fit the native frame.</exception>
        Private Sub MatchInputHeight()
            If Not Me.IsHandleCreated Then
                Throw New InvalidOperationException("The native frame can only be measured once the handle has been created.")
            End If

            Dim targetHeight As Integer = ControlMetrics.GetInputHeight(Me.Font)
            Dim frameHeight As Integer = Me.Height - Me.ItemHeight
            Dim newItemHeight As Integer = targetHeight - frameHeight

            If newItemHeight < 1 Then
                Throw New InvalidOperationException($"The native combo box frame ({frameHeight} px) does not fit in the target height ({targetHeight} px).")
            End If

            If Me.ItemHeight <> newItemHeight Then
                Me.ItemHeight = newItemHeight
            End If
        End Sub

#End Region

    End Class

End Namespace
