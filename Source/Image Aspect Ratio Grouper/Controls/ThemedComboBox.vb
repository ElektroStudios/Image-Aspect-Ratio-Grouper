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
    ''' A themed, drop-down-list-only combo box.
    ''' <para></para>
    ''' It hosts a border-less, owner-drawn <see cref="ComboBox"/> and paints its own 1px border, exactly like
    ''' <see cref="ThemedTextBox"/> and <see cref="ThemedNumericUpDown"/>. Hosting the native combo box is what makes the
    ''' height deterministic: the height of a native drop-down combo box cannot be set (Windows computes it from the item
    ''' height plus a frame that depends on the OS, the visual styles and the DPI), whereas the height of this container
    ''' is always <see cref="ControlMetrics.GetInputHeight"/>, and the hosted combo box is centered and clipped inside it.
    ''' <para></para>
    ''' The drop-down items are owner-drawn with the palette colors, the native drop-down list and scroll bar use the
    ''' <c>DarkMode_CFD</c> visual style in dark mode, and each item can show a muted, right-aligned secondary text
    ''' taken from <see cref="SecondaryTextMember"/>.
    ''' </summary>
    <DefaultEvent(NameOf(ThemedComboBox.SelectedIndexChanged))>
    <DefaultProperty(NameOf(ThemedComboBox.SelectedItem))>
    <ToolboxItem(True)>
    Public Class ThemedComboBox : Inherits UserControl : Implements IThemeable

#Region " Constants "

        ''' <summary>
        ''' Horizontal padding between the border and the text.
        ''' </summary>
        Private Const TextPadding As Integer = 6

        ''' <summary>
        ''' Width of the area that hosts the chevron.
        ''' </summary>
        Private Const ArrowAreaWidth As Integer = 24

        ''' <summary>
        ''' Width of the border painted by the container.
        ''' </summary>
        Private Const BorderWidth As Integer = 1

#End Region

#Region " Fields "

        ''' <summary>
        ''' The hosted, owner-drawn combo box.
        ''' </summary>
        Private ReadOnly hostedComboBox As OwnerDrawnComboBox

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

#Region " Events "

        ''' <summary>
        ''' Occurs when the <see cref="SelectedIndex"/> property changes.
        ''' </summary>
        <Category("Behavior")>
        <Description("Occurs when the SelectedIndex property changes.")>
        Public Event SelectedIndexChanged As EventHandler

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

            Me.hostedComboBox = New OwnerDrawnComboBox(Me) With {
                .Margin = Padding.Empty
            }

            AddHandler Me.hostedComboBox.SelectedIndexChanged, AddressOf Me.HostedComboBox_SelectedIndexChanged
            AddHandler Me.hostedComboBox.GotFocus, AddressOf Me.HostedComboBox_StateChanged
            AddHandler Me.hostedComboBox.LostFocus, AddressOf Me.HostedComboBox_StateChanged
            AddHandler Me.hostedComboBox.DropDown, AddressOf Me.HostedComboBox_StateChanged
            AddHandler Me.hostedComboBox.DropDownClosed, AddressOf Me.HostedComboBox_StateChanged
            AddHandler Me.hostedComboBox.MouseEnter, AddressOf Me.HostedComboBox_MouseEnter
            AddHandler Me.hostedComboBox.MouseLeave, AddressOf Me.HostedComboBox_MouseLeave
            AddHandler Me.hostedComboBox.SizeChanged, AddressOf Me.HostedComboBox_SizeChanged

            Me.Controls.Add(Me.hostedComboBox)

            ' The base constructor sets the initial size from DefaultSize without calling SetBoundsCore,
            ' so the height rule must be enforced explicitly once (later changes go through SetBoundsCore).
            Me.Height = ControlMetrics.GetInputHeight(Me.Font)
            Me.ApplyTheme(Me.palette)
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the items of the combo box.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Items As ComboBox.ObjectCollection
            Get
                Return Me.hostedComboBox.Items
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the selected item.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectedItem As Object
            Get
                Return Me.hostedComboBox.SelectedItem
            End Get
            Set(value As Object)
                Me.hostedComboBox.SelectedItem = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the index of the selected item (-1 when there is no selection).
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property SelectedIndex As Integer
            Get
                Return Me.hostedComboBox.SelectedIndex
            End Get
            Set(value As Integer)
                Me.hostedComboBox.SelectedIndex = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the width of the drop-down list, in pixels.
        ''' </summary>
        <Category("Behavior")>
        <Description("The width of the drop-down list, in pixels.")>
        Public Property DropDownWidth As Integer
            Get
                Return Me.hostedComboBox.DropDownWidth
            End Get
            Set(value As Integer)
                Me.hostedComboBox.DropDownWidth = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the maximum number of items shown in the drop-down list.
        ''' </summary>
        <Category("Behavior")>
        <Description("The maximum number of items shown in the drop-down list.")>
        <DefaultValue(16)>
        Public Property MaxDropDownItems As Integer
            Get
                Return Me.hostedComboBox.MaxDropDownItems
            End Get
            Set(value As Integer)
                Me.hostedComboBox.MaxDropDownItems = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the drop-down list is sized to show only complete items.
        ''' </summary>
        <Category("Behavior")>
        <Description("Indicates whether the drop-down list is sized to show only complete items.")>
        <DefaultValue(False)>
        Public Property IntegralHeight As Boolean
            Get
                Return Me.hostedComboBox.IntegralHeight
            End Get
            Set(value As Boolean)
                Me.hostedComboBox.IntegralHeight = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the drop-down list is shown.
        ''' </summary>
        <Browsable(False)>
        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property DroppedDown As Boolean
            Get
                Return Me.hostedComboBox.DroppedDown
            End Get
            Set(value As Boolean)
                Me.hostedComboBox.DroppedDown = value
            End Set
        End Property

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
                Me.hostedComboBox.Invalidate()
            End Set
        End Property

        ''' <inheritdoc/>
        Protected Overrides ReadOnly Property DefaultSize As Size
            Get
                Return New Size(160, 28)
            End Get
        End Property

        ''' <summary>
        ''' Gets the area inside the border, where the hosted combo box is shown.
        ''' </summary>
        Private ReadOnly Property ContentBounds As Rectangle
            Get
                Return New Rectangle(BorderWidth, BorderWidth,
                                     Math.Max(1, Me.ClientSize.Width - (BorderWidth * 2)),
                                     Math.Max(1, Me.ClientSize.Height - (BorderWidth * 2)))
            End Get
        End Property

#End Region

#Region " IThemeable "

        ''' <inheritdoc/>
        Public Sub ApplyTheme(palette As ThemePalette) Implements IThemeable.ApplyTheme
            If palette Is Nothing Then
                Throw New ArgumentNullException(NameOf(palette))
            End If

            Me.palette = palette

            Dim surface As Color = If(Me.Enabled, palette.SurfaceBackground, palette.DisabledBackground)
            Me.BackColor = surface
            Me.ForeColor = palette.Foreground
            Me.hostedComboBox.BackColor = surface
            Me.hostedComboBox.ForeColor = palette.Foreground

            If Me.hostedComboBox.IsHandleCreated AndAlso Not Me.DesignMode Then
                WindowChrome.ApplyComboBoxTheme(Me.hostedComboBox, palette.IsDark)
            End If

            Me.hostedComboBox.Invalidate()
            Me.Invalidate()
        End Sub

#End Region

#Region " Overridden Methods "

        ''' <inheritdoc/>
        Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer, width As Integer, height As Integer, specified As BoundsSpecified)
            ' The height is always the common input height, whatever the native combo box would like to be.
            MyBase.SetBoundsCore(x, y, width, ControlMetrics.GetInputHeight(Me.Font), specified)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnLayout(e As LayoutEventArgs)
            MyBase.OnLayout(e)

            If Me.hostedComboBox Is Nothing Then
                Return ' Construction-order guard: the base constructor can lay out before the hosted control exists.
            End If

            Me.EnforceInputHeight()

            Dim content As Rectangle = Me.ContentBounds
            Dim innerHeight As Integer = Me.hostedComboBox.Height
            Dim innerTop As Integer = content.Top + ((content.Height - innerHeight) \ 2)

            ' The width is settable; the height of a drop-down combo box is not (it only follows the item height).
            Me.hostedComboBox.SetBounds(content.Left, innerTop, content.Width, innerHeight)

            ' If the native combo box is taller than the content area, it is clipped, so it never covers the border.
            Dim previousRegion As Region = Me.hostedComboBox.Region
            Me.hostedComboBox.Region = New Region(New Rectangle(0, content.Top - innerTop, content.Width, content.Height))
            If previousRegion IsNot Nothing Then
                previousRegion.Dispose()
            End If
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnHandleCreated(e As EventArgs)
            MyBase.OnHandleCreated(e)
            Me.EnforceInputHeight()
            Me.hostedComboBox.AccessibleName = Me.AccessibleName
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnFontChanged(e As EventArgs)
            MyBase.OnFontChanged(e)

            If Me.hostedComboBox Is Nothing Then
                Return ' Construction-order guard (see OnLayout). The constructor picks up the current font.
            End If

            Me.hostedComboBox.Font = Me.Font
            Me.EnforceInputHeight()
            Me.PerformLayout()
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnEnabledChanged(e As EventArgs)
            MyBase.OnEnabledChanged(e)
            Me.ApplyTheme(Me.palette)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            ' Clicking the border opens the list, like clicking anywhere on a native drop-down list.
            If e.Button = MouseButtons.Left AndAlso Me.Enabled Then
                Me.hostedComboBox.Focus()
                Me.hostedComboBox.DroppedDown = Not Me.hostedComboBox.DroppedDown
            End If

            MyBase.OnMouseDown(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
            MyBase.OnMouseEnter(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            Me.isHovered = Me.ClientRectangle.Contains(Me.PointToClient(Control.MousePosition))
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        ''' <inheritdoc/>
        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Dim g As Graphics = e.Graphics
            g.Clear(Me.BackColor)

            Dim borderColor As Color
            If Not Me.Enabled Then
                borderColor = Me.palette.Border
            ElseIf Me.hostedComboBox.Focused OrElse Me.hostedComboBox.DroppedDown Then
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

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Applies the common input height for the current font, if the control does not have it yet.
        ''' <para></para>
        ''' The current height must not be re-assigned to itself to re-evaluate the constraint: WinForms skips
        ''' <c>SetBoundsCore</c> when the requested bounds equal the current ones, so the height computed with the
        ''' previous font (e.g. the default font, before the parent font is inherited) would remain.
        ''' </summary>
        Private Sub EnforceInputHeight()
            Dim requiredHeight As Integer = ControlMetrics.GetInputHeight(Me.Font)
            If Me.Height <> requiredHeight Then
                Me.Height = requiredHeight
            End If
        End Sub

#End Region

#Region " Event Invocators "

        ''' <summary>
        ''' Raises the <see cref="SelectedIndexChanged"/> event.
        ''' </summary>
        ''' <param name="e">The event data.</param>
        Protected Overridable Sub OnSelectedIndexChanged(e As EventArgs)
            RaiseEvent SelectedIndexChanged(Me, e)
        End Sub

#End Region

#Region " Event Handlers "

        ''' <summary>
        ''' Forwards the hosted combo box <see cref="ComboBox.SelectedIndexChanged"/> event.
        ''' </summary>
        Private Sub HostedComboBox_SelectedIndexChanged(sender As Object, e As EventArgs)
            Me.hostedComboBox.Invalidate()
            Me.OnSelectedIndexChanged(e)
        End Sub

        ''' <summary>
        ''' Repaints the border when the focus or the drop-down state changes.
        ''' </summary>
        Private Sub HostedComboBox_StateChanged(sender As Object, e As EventArgs)
            Me.hostedComboBox.Invalidate()
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Shows the hover border while the pointer is over the hosted combo box.
        ''' </summary>
        Private Sub HostedComboBox_MouseEnter(sender As Object, e As EventArgs)
            Me.isHovered = True
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Removes the hover border when the pointer leaves the whole control.
        ''' </summary>
        Private Sub HostedComboBox_MouseLeave(sender As Object, e As EventArgs)
            Me.isHovered = Me.ClientRectangle.Contains(Me.PointToClient(Control.MousePosition))
            Me.Invalidate()
        End Sub

        ''' <summary>
        ''' Re-centers the hosted combo box when its native height changes (e.g. after a font change).
        ''' </summary>
        Private Sub HostedComboBox_SizeChanged(sender As Object, e As EventArgs)
            Me.PerformLayout()
        End Sub

#End Region

#Region " OwnerDrawnComboBox "

        ''' <summary>
        ''' The hosted combo box: drop-down list only, owner-drawn items, user-painted closed state without border
        ''' (the border is painted by the container).
        ''' </summary>
        Private NotInheritable Class OwnerDrawnComboBox : Inherits ComboBox

            ''' <summary>
            ''' The container that owns this combo box (palette and secondary text member).
            ''' </summary>
            Private ReadOnly owner As ThemedComboBox

            ''' <summary>
            ''' Initializes a new instance of the <see cref="OwnerDrawnComboBox"/> class.
            ''' </summary>
            ''' <param name="owner">The container that owns this combo box.</param>
            Public Sub New(owner As ThemedComboBox)
                If owner Is Nothing Then
                    Throw New ArgumentNullException(NameOf(owner))
                End If

                Me.owner = owner

                Me.SetStyle(ControlStyles.UserPaint Or
                            ControlStyles.AllPaintingInWmPaint Or
                            ControlStyles.OptimizedDoubleBuffer Or
                            ControlStyles.ResizeRedraw, True)

                Me.DrawMode = DrawMode.OwnerDrawFixed
                Me.DropDownStyle = ComboBoxStyle.DropDownList
                Me.FlatStyle = FlatStyle.Flat
                Me.IntegralHeight = False
                Me.MaxDropDownItems = 16
            End Sub

            ''' <inheritdoc/>
            Protected Overrides Sub OnHandleCreated(e As EventArgs)
                MyBase.OnHandleCreated(e)

                If Not Me.DesignMode Then
                    WindowChrome.ApplyComboBoxTheme(Me, Me.owner.palette.IsDark)
                End If
            End Sub

            ''' <inheritdoc/>
            Protected Overrides Sub OnPaint(e As PaintEventArgs)
                Dim palette As ThemePalette = Me.owner.palette
                Dim g As Graphics = e.Graphics
                Dim surface As Color = If(Me.Enabled, palette.SurfaceBackground, palette.DisabledBackground)
                Dim textColor As Color = If(Me.Enabled, palette.Foreground, palette.DisabledForeground)

                g.Clear(surface)

                ' The text and the chevron are centered on the visible area of the container (not on this control,
                ' which can be slightly taller and clipped), so they stay centered within the painted border.
                Dim visibleBounds As Rectangle = Me.GetVisibleBounds()

                If Me.SelectedIndex >= 0 Then
                    Dim textBounds As New Rectangle(TextPadding - BorderWidth, visibleBounds.Top,
                                                    Math.Max(1, Me.ClientSize.Width - TextPadding - ArrowAreaWidth), visibleBounds.Height)
                    TextRenderer.DrawText(g, Me.GetItemText(Me.SelectedItem), Me.Font, textBounds, textColor,
                                          TextFormatFlags.Left Or TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                          TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix)
                End If

                Dim centerX As Single = Me.ClientSize.Width - (ArrowAreaWidth / 2.0F)
                Dim centerY As Single = visibleBounds.Top + (visibleBounds.Height / 2.0F)
                g.SmoothingMode = SmoothingMode.AntiAlias
                Using chevronPen As New Pen(textColor, 1.5F)
                    g.DrawLines(chevronPen, {New PointF(centerX - 4.0F, centerY - 2.0F),
                                             New PointF(centerX, centerY + 2.0F),
                                             New PointF(centerX + 4.0F, centerY - 2.0F)})
                End Using
                g.SmoothingMode = SmoothingMode.None
            End Sub

            ''' <inheritdoc/>
            Protected Overrides Sub OnDrawItem(e As DrawItemEventArgs)
                If e.Index < 0 Then
                    Return ' Nothing to draw: the list is empty or there is no current item.
                End If

                Dim palette As ThemePalette = Me.owner.palette
                Dim secondaryTextMember As String = Me.owner.secondaryTextMemberValue
                Dim item As Object = Me.Items(e.Index)
                Dim isSelected As Boolean = (e.State And DrawItemState.Selected) = DrawItemState.Selected

                Using backgroundBrush As New SolidBrush(If(isSelected, palette.MenuHighlight, palette.SurfaceBackground))
                    e.Graphics.FillRectangle(backgroundBrush, e.Bounds)
                End Using

                Dim flags As TextFormatFlags = TextFormatFlags.VerticalCenter Or TextFormatFlags.SingleLine Or
                                               TextFormatFlags.EndEllipsis Or TextFormatFlags.NoPrefix

                Dim primaryBounds As New Rectangle(e.Bounds.Left + TextPadding, e.Bounds.Top, e.Bounds.Width - (TextPadding * 2), e.Bounds.Height)
                TextRenderer.DrawText(e.Graphics, Me.GetItemText(item), Me.Font, primaryBounds, palette.Foreground, flags Or TextFormatFlags.Left)

                If Not String.IsNullOrEmpty(secondaryTextMember) Then
                    Dim secondaryValue As Object = Me.FilterItemOnProperty(item, secondaryTextMember)
                    If secondaryValue Is Nothing OrElse secondaryValue Is item Then
                        Throw New InvalidOperationException($"The item type '{item.GetType().Name}' has no '{secondaryTextMember}' property (see {NameOf(ThemedComboBox.SecondaryTextMember)}).")
                    End If

                    TextRenderer.DrawText(e.Graphics, Convert.ToString(secondaryValue, Globalization.CultureInfo.InvariantCulture),
                                          Me.Font, primaryBounds, palette.MutedForeground, flags Or TextFormatFlags.Right)
                End If

                MyBase.OnDrawItem(e)
            End Sub

            ''' <inheritdoc/>
            Protected Overrides Sub OnEnabledChanged(e As EventArgs)
                Me.Invalidate()
                MyBase.OnEnabledChanged(e)
            End Sub

            ''' <summary>
            ''' Gets the part of this control that is visible inside the border of the container, in client coordinates.
            ''' </summary>
            ''' <returns>The visible bounds.</returns>
            Private Function GetVisibleBounds() As Rectangle
                If Me.Parent Is Nothing Then
                    Return Me.ClientRectangle
                End If

                Dim contentHeight As Integer = Math.Max(1, Me.Parent.ClientSize.Height - (BorderWidth * 2))
                Return New Rectangle(0, BorderWidth - Me.Top, Me.ClientSize.Width, contentHeight)
            End Function

        End Class

#End Region

    End Class

End Namespace
