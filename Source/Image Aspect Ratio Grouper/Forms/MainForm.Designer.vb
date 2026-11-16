Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.Forms

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
    Partial Class MainForm
        Inherits ThemedForm

        'Form overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()>
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()>
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.appMenuStrip = New System.Windows.Forms.MenuStrip()
            Me.fileMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.openDirectoryMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.fileMenuSeparator = New System.Windows.Forms.ToolStripSeparator()
            Me.exitMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.helpMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.themeToggleItem = New ImageAspectRatioGrouper.Controls.ToolStripToggleSwitch()
            Me.rootLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.settingsLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.directoryLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.directoryTextBox = New ImageAspectRatioGrouper.Controls.ThemedTextBox()
            Me.browseButton = New ImageAspectRatioGrouper.Controls.ThemedButton()
            Me.recursiveToggle = New ImageAspectRatioGrouper.Controls.ToggleSwitch()
            Me.aspectRatioLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.ratioOptionsLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.aspectRatioComboBox = New ImageAspectRatioGrouper.Controls.ThemedComboBox()
            Me.toleranceLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.toleranceNumericUpDown = New ImageAspectRatioGrouper.Controls.ThemedNumericUpDown()
            Me.minimumHeightLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.minimumHeightNumericUpDown = New ImageAspectRatioGrouper.Controls.ThemedNumericUpDown()
            Me.executionLayout = New System.Windows.Forms.TableLayoutPanel()
            Me.startButton = New ImageAspectRatioGrouper.Controls.ThemedButton()
            Me.abortButton = New ImageAspectRatioGrouper.Controls.ThemedButton()
            Me.progressBar = New ImageAspectRatioGrouper.Controls.ThemedProgressBar()
            Me.progressLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.logLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.logConsole = New ImageAspectRatioGrouper.Controls.LogConsoleBox()
            Me.statusLabel = New ImageAspectRatioGrouper.Controls.ThemedLabel()
            Me.toolTip = New System.Windows.Forms.ToolTip(Me.components)
            Me.progressTimer = New System.Windows.Forms.Timer(Me.components)
            Me.appMenuStrip.SuspendLayout()
            Me.rootLayout.SuspendLayout()
            Me.settingsLayout.SuspendLayout()
            Me.ratioOptionsLayout.SuspendLayout()
            Me.executionLayout.SuspendLayout()
            Me.SuspendLayout()
            '
            'appMenuStrip
            '
            Me.appMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.fileMenuItem, Me.helpMenuItem, Me.themeToggleItem})
            Me.appMenuStrip.Location = New System.Drawing.Point(0, 0)
            Me.appMenuStrip.Name = "appMenuStrip"
            Me.appMenuStrip.Padding = New System.Windows.Forms.Padding(6, 3, 0, 3)
            Me.appMenuStrip.Size = New System.Drawing.Size(804, 28)
            Me.appMenuStrip.TabIndex = 0
            '
            'fileMenuItem
            '
            Me.fileMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.openDirectoryMenuItem, Me.fileMenuSeparator, Me.exitMenuItem})
            Me.fileMenuItem.Name = "fileMenuItem"
            Me.fileMenuItem.Size = New System.Drawing.Size(37, 22)
            Me.fileMenuItem.Text = "&File"
            '
            'openDirectoryMenuItem
            '
            Me.openDirectoryMenuItem.Name = "openDirectoryMenuItem"
            Me.openDirectoryMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O), System.Windows.Forms.Keys)
            Me.openDirectoryMenuItem.Size = New System.Drawing.Size(206, 22)
            Me.openDirectoryMenuItem.Text = "&Open Directory..."
            '
            'fileMenuSeparator
            '
            Me.fileMenuSeparator.Name = "fileMenuSeparator"
            Me.fileMenuSeparator.Size = New System.Drawing.Size(203, 6)
            '
            'exitMenuItem
            '
            Me.exitMenuItem.Name = "exitMenuItem"
            Me.exitMenuItem.ShortcutKeyDisplayString = "Alt+F4"
            Me.exitMenuItem.Size = New System.Drawing.Size(206, 22)
            Me.exitMenuItem.Text = "E&xit"
            '
            'helpMenuItem
            '
            Me.helpMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.aboutMenuItem})
            Me.helpMenuItem.Name = "helpMenuItem"
            Me.helpMenuItem.Size = New System.Drawing.Size(44, 22)
            Me.helpMenuItem.Text = "&Help"
            '
            'aboutMenuItem
            '
            Me.aboutMenuItem.Name = "aboutMenuItem"
            Me.aboutMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1
            Me.aboutMenuItem.Size = New System.Drawing.Size(135, 22)
            Me.aboutMenuItem.Text = "&About..."
            '
            'themeToggleItem
            '
            Me.themeToggleItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right
            Me.themeToggleItem.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.themeToggleItem.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.themeToggleItem.Name = "themeToggleItem"
            Me.themeToggleItem.Text = "Dark mode"
            Me.themeToggleItem.ToolTipText = "Switch between the dark and the light theme."
            '
            'rootLayout
            '
            Me.rootLayout.ColumnCount = 1
            Me.rootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.rootLayout.Controls.Add(Me.settingsLayout, 0, 0)
            Me.rootLayout.Controls.Add(Me.executionLayout, 0, 1)
            Me.rootLayout.Controls.Add(Me.logLabel, 0, 2)
            Me.rootLayout.Controls.Add(Me.logConsole, 0, 3)
            Me.rootLayout.Controls.Add(Me.statusLabel, 0, 4)
            Me.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.rootLayout.Location = New System.Drawing.Point(0, 28)
            Me.rootLayout.Name = "rootLayout"
            Me.rootLayout.Padding = New System.Windows.Forms.Padding(12, 10, 12, 8)
            Me.rootLayout.RowCount = 5
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.rootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.rootLayout.Size = New System.Drawing.Size(804, 473)
            Me.rootLayout.TabIndex = 1
            '
            'settingsLayout
            '
            Me.settingsLayout.AutoSize = True
            Me.settingsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.settingsLayout.ColumnCount = 3
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.settingsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.settingsLayout.Controls.Add(Me.directoryLabel, 0, 0)
            Me.settingsLayout.Controls.Add(Me.directoryTextBox, 1, 0)
            Me.settingsLayout.Controls.Add(Me.browseButton, 2, 0)
            Me.settingsLayout.Controls.Add(Me.recursiveToggle, 1, 1)
            Me.settingsLayout.Controls.Add(Me.aspectRatioLabel, 0, 2)
            Me.settingsLayout.Controls.Add(Me.ratioOptionsLayout, 1, 2)
            Me.settingsLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.settingsLayout.Location = New System.Drawing.Point(12, 10)
            Me.settingsLayout.Margin = New System.Windows.Forms.Padding(0, 0, 0, 10)
            Me.settingsLayout.Name = "settingsLayout"
            Me.settingsLayout.RowCount = 3
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34.0!))
            Me.settingsLayout.Size = New System.Drawing.Size(780, 102)
            Me.settingsLayout.TabIndex = 0
            '
            'directoryLabel
            '
            Me.directoryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.directoryLabel.AutoSize = True
            Me.directoryLabel.Location = New System.Drawing.Point(3, 9)
            Me.directoryLabel.Name = "directoryLabel"
            Me.directoryLabel.Size = New System.Drawing.Size(92, 15)
            Me.directoryLabel.TabIndex = 0
            Me.directoryLabel.Text = "Target directory:"
            '
            'directoryTextBox
            '
            Me.directoryTextBox.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.directoryTextBox.BackColor = System.Drawing.Color.White
            Me.directoryTextBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.directoryTextBox.Location = New System.Drawing.Point(101, 3)
            Me.directoryTextBox.Name = "directoryTextBox"
            Me.directoryTextBox.ReadOnly = True
            Me.directoryTextBox.Size = New System.Drawing.Size(572, 28)
            Me.directoryTextBox.TabIndex = 1
            Me.toolTip.SetToolTip(Me.directoryTextBox, "Tip: you can also drag and drop a single folder" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "anywhere on this window.")
            '
            'browseButton
            '
            Me.browseButton.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.browseButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.browseButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.browseButton.Location = New System.Drawing.Point(679, 3)
            Me.browseButton.Name = "browseButton"
            Me.browseButton.Size = New System.Drawing.Size(98, 28)
            Me.browseButton.TabIndex = 2
            Me.browseButton.Text = "&Browse..."
            Me.toolTip.SetToolTip(Me.browseButton, "Select the directory that contains" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "the images to group (Ctrl+O).")
            Me.browseButton.UseVisualStyleBackColor = False
            '
            'recursiveToggle
            '
            Me.recursiveToggle.AccessibleName = "Include subdirectories"
            Me.recursiveToggle.AccessibleRole = System.Windows.Forms.AccessibleRole.CheckButton
            Me.recursiveToggle.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.recursiveToggle.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.recursiveToggle.Cursor = System.Windows.Forms.Cursors.Hand
            Me.recursiveToggle.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.recursiveToggle.Location = New System.Drawing.Point(101, 40)
            Me.recursiveToggle.Name = "recursiveToggle"
            Me.recursiveToggle.Size = New System.Drawing.Size(171, 20)
            Me.recursiveToggle.TabIndex = 3
            Me.recursiveToggle.Text = "Include subdirectories"
            Me.toolTip.SetToolTip(Me.recursiveToggle, "When enabled, the images inside all the" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "subdirectories are processed too.")
            '
            'aspectRatioLabel
            '
            Me.aspectRatioLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.aspectRatioLabel.AutoSize = True
            Me.aspectRatioLabel.Location = New System.Drawing.Point(3, 77)
            Me.aspectRatioLabel.Name = "aspectRatioLabel"
            Me.aspectRatioLabel.Size = New System.Drawing.Size(73, 15)
            Me.aspectRatioLabel.TabIndex = 4
            Me.aspectRatioLabel.Text = "Aspect ratio:"
            Me.toolTip.SetToolTip(Me.aspectRatioLabel, "The target aspect ratio." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Common desktop (landscape) and mobile (portrait)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "ratio" &
        "s are available.")
            '
            'ratioOptionsLayout
            '
            Me.ratioOptionsLayout.AutoSize = True
            Me.ratioOptionsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.ratioOptionsLayout.ColumnCount = 6
            Me.settingsLayout.SetColumnSpan(Me.ratioOptionsLayout, 2)
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.ratioOptionsLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.ratioOptionsLayout.Controls.Add(Me.aspectRatioComboBox, 0, 0)
            Me.ratioOptionsLayout.Controls.Add(Me.toleranceLabel, 1, 0)
            Me.ratioOptionsLayout.Controls.Add(Me.toleranceNumericUpDown, 2, 0)
            Me.ratioOptionsLayout.Controls.Add(Me.minimumHeightLabel, 3, 0)
            Me.ratioOptionsLayout.Controls.Add(Me.minimumHeightNumericUpDown, 4, 0)
            Me.ratioOptionsLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.ratioOptionsLayout.Location = New System.Drawing.Point(98, 68)
            Me.ratioOptionsLayout.Margin = New System.Windows.Forms.Padding(0)
            Me.ratioOptionsLayout.Name = "ratioOptionsLayout"
            Me.ratioOptionsLayout.RowCount = 1
            Me.ratioOptionsLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.ratioOptionsLayout.Size = New System.Drawing.Size(682, 34)
            Me.ratioOptionsLayout.TabIndex = 5
            '
            'aspectRatioComboBox
            '
            Me.aspectRatioComboBox.AccessibleName = "Aspect ratio"
            Me.aspectRatioComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.aspectRatioComboBox.BackColor = System.Drawing.Color.White
            Me.aspectRatioComboBox.DropDownWidth = 440
            Me.aspectRatioComboBox.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.aspectRatioComboBox.IntegralHeight = False
            Me.aspectRatioComboBox.Location = New System.Drawing.Point(3, 6)
            Me.aspectRatioComboBox.MaxDropDownItems = 16
            Me.aspectRatioComboBox.Name = "aspectRatioComboBox"
            Me.aspectRatioComboBox.SecondaryTextMember = "ExampleResolutions"
            Me.aspectRatioComboBox.Size = New System.Drawing.Size(230, 28)
            Me.aspectRatioComboBox.TabIndex = 0
            Me.toolTip.SetToolTip(Me.aspectRatioComboBox, "The target aspect ratio." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Common desktop (landscape) and mobile (portrait)" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "ratio" &
        "s are available.")
            '
            'toleranceLabel
            '
            Me.toleranceLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.toleranceLabel.AutoSize = True
            Me.toleranceLabel.Location = New System.Drawing.Point(256, 9)
            Me.toleranceLabel.Margin = New System.Windows.Forms.Padding(20, 0, 3, 0)
            Me.toleranceLabel.Name = "toleranceLabel"
            Me.toleranceLabel.Size = New System.Drawing.Size(84, 15)
            Me.toleranceLabel.TabIndex = 1
            Me.toleranceLabel.Text = "Tolerance (px):"
            Me.toolTip.SetToolTip(Me.toleranceLabel, "Maximum difference, in pixels, between the image height" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "and the exact height its" &
        " width requires for the selected" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "ratio, for the image to count as already confo" &
        "rming." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 3 px.")
            '
            'toleranceNumericUpDown
            '
            Me.toleranceNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.toleranceNumericUpDown.BackColor = System.Drawing.Color.White
            Me.toleranceNumericUpDown.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.toleranceNumericUpDown.Location = New System.Drawing.Point(346, 3)
            Me.toleranceNumericUpDown.Name = "toleranceNumericUpDown"
            Me.toleranceNumericUpDown.Size = New System.Drawing.Size(70, 28)
            Me.toleranceNumericUpDown.TabIndex = 2
            Me.toolTip.SetToolTip(Me.toleranceNumericUpDown, "Maximum difference, in pixels, between the image height" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "and the exact height its" &
        " width requires for the selected" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "ratio, for the image to count as already confo" &
        "rming." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 3 px.")
            Me.toleranceNumericUpDown.Value = 3
            '
            'minimumHeightLabel
            '
            Me.minimumHeightLabel.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.minimumHeightLabel.AutoSize = True
            Me.minimumHeightLabel.Location = New System.Drawing.Point(439, 9)
            Me.minimumHeightLabel.Margin = New System.Windows.Forms.Padding(20, 0, 3, 0)
            Me.minimumHeightLabel.Name = "minimumHeightLabel"
            Me.minimumHeightLabel.Size = New System.Drawing.Size(95, 15)
            Me.minimumHeightLabel.TabIndex = 3
            Me.minimumHeightLabel.Text = "Min. height (px):"
            Me.toolTip.SetToolTip(Me.minimumHeightLabel, "Minimum resulting height, in pixels." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Images that would end up smaller than this" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(thumbnails, icons, thin strips...) are moved" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "to the '# Incompatible' folder." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 480 px.")
            '
            'minimumHeightNumericUpDown
            '
            Me.minimumHeightNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left
            Me.minimumHeightNumericUpDown.BackColor = System.Drawing.Color.White
            Me.minimumHeightNumericUpDown.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.minimumHeightNumericUpDown.Location = New System.Drawing.Point(540, 3)
            Me.minimumHeightNumericUpDown.Maximum = 10000
            Me.minimumHeightNumericUpDown.Name = "minimumHeightNumericUpDown"
            Me.minimumHeightNumericUpDown.Size = New System.Drawing.Size(80, 28)
            Me.minimumHeightNumericUpDown.TabIndex = 4
            Me.toolTip.SetToolTip(Me.minimumHeightNumericUpDown, "Minimum resulting height, in pixels." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Images that would end up smaller than this" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(thumbnails, icons, thin strips...) are moved" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "to the '# Incompatible' folder." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Default: 480 px.")
            Me.minimumHeightNumericUpDown.Value = 480
            '
            'executionLayout
            '
            Me.executionLayout.AutoSize = True
            Me.executionLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
            Me.executionLayout.ColumnCount = 4
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
            Me.executionLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle())
            Me.executionLayout.Controls.Add(Me.startButton, 0, 0)
            Me.executionLayout.Controls.Add(Me.abortButton, 1, 0)
            Me.executionLayout.Controls.Add(Me.progressBar, 2, 0)
            Me.executionLayout.Controls.Add(Me.progressLabel, 3, 0)
            Me.executionLayout.Dock = System.Windows.Forms.DockStyle.Fill
            Me.executionLayout.Location = New System.Drawing.Point(12, 122)
            Me.executionLayout.Margin = New System.Windows.Forms.Padding(0, 0, 0, 8)
            Me.executionLayout.Name = "executionLayout"
            Me.executionLayout.RowCount = 1
            Me.executionLayout.RowStyles.Add(New System.Windows.Forms.RowStyle())
            Me.executionLayout.Size = New System.Drawing.Size(780, 40)
            Me.executionLayout.TabIndex = 1
            '
            'startButton
            '
            Me.startButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.startButton.ButtonStyle = ImageAspectRatioGrouper.Controls.ThemedButtonStyle.Accent
            Me.startButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.startButton.Location = New System.Drawing.Point(3, 3)
            Me.startButton.Name = "startButton"
            Me.startButton.Size = New System.Drawing.Size(150, 34)
            Me.startButton.TabIndex = 0
            Me.startButton.Text = "&Start Processing"
            Me.toolTip.SetToolTip(Me.startButton, "Scan the target directory and group every image" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "by the selected aspect ratio.")
            Me.startButton.UseVisualStyleBackColor = False
            '
            'abortButton
            '
            Me.abortButton.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.abortButton.ButtonStyle = ImageAspectRatioGrouper.Controls.ThemedButtonStyle.Danger
            Me.abortButton.ForeColor = System.Drawing.Color.FromArgb(CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer), CType(CType(27, Byte), Integer))
            Me.abortButton.Location = New System.Drawing.Point(159, 3)
            Me.abortButton.Name = "abortButton"
            Me.abortButton.Size = New System.Drawing.Size(130, 34)
            Me.abortButton.TabIndex = 1
            Me.abortButton.Text = "&Cancel / Abort"
            Me.toolTip.SetToolTip(Me.abortButton, "Stop the operation cleanly." & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "Files already moved stay where they are.")
            Me.abortButton.UseVisualStyleBackColor = False
            '
            'progressBar
            '
            Me.progressBar.AccessibleRole = System.Windows.Forms.AccessibleRole.ProgressBar
            Me.progressBar.Anchor = CType((System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.progressBar.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.progressBar.Location = New System.Drawing.Point(307, 16)
            Me.progressBar.Margin = New System.Windows.Forms.Padding(15, 3, 9, 3)
            Me.progressBar.Name = "progressBar"
            Me.progressBar.Size = New System.Drawing.Size(370, 8)
            Me.progressBar.TabIndex = 2
            Me.progressBar.TabStop = False
            '
            'progressLabel
            '
            Me.progressLabel.Anchor = System.Windows.Forms.AnchorStyles.Right
            Me.progressLabel.AutoSize = True
            Me.progressLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.progressLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.progressLabel.IsMuted = True
            Me.progressLabel.Location = New System.Drawing.Point(689, 12)
            Me.progressLabel.MinimumSize = New System.Drawing.Size(88, 0)
            Me.progressLabel.Name = "progressLabel"
            Me.progressLabel.Size = New System.Drawing.Size(88, 15)
            Me.progressLabel.TabIndex = 3
            Me.progressLabel.Text = "0 / 0"
            Me.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
            '
            'logLabel
            '
            Me.logLabel.AutoSize = True
            Me.logLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.logLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.logLabel.IsMuted = True
            Me.logLabel.Location = New System.Drawing.Point(15, 170)
            Me.logLabel.Margin = New System.Windows.Forms.Padding(3, 0, 3, 4)
            Me.logLabel.Name = "logLabel"
            Me.logLabel.Size = New System.Drawing.Size(68, 15)
            Me.logLabel.TabIndex = 2
            Me.logLabel.Text = "Output log:"
            '
            'logConsole
            '
            Me.logConsole.BackColor = System.Drawing.Color.White
            Me.logConsole.Dock = System.Windows.Forms.DockStyle.Fill
            Me.logConsole.Location = New System.Drawing.Point(15, 192)
            Me.logConsole.Name = "logConsole"
            Me.logConsole.Padding = New System.Windows.Forms.Padding(1)
            Me.logConsole.Size = New System.Drawing.Size(774, 244)
            Me.logConsole.TabIndex = 3
            '
            'statusLabel
            '
            Me.statusLabel.AutoEllipsis = True
            Me.statusLabel.BackColor = System.Drawing.Color.FromArgb(CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer), CType(CType(243, Byte), Integer))
            Me.statusLabel.Dock = System.Windows.Forms.DockStyle.Fill
            Me.statusLabel.ForeColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer), CType(CType(96, Byte), Integer))
            Me.statusLabel.IsMuted = True
            Me.statusLabel.Location = New System.Drawing.Point(15, 445)
            Me.statusLabel.Margin = New System.Windows.Forms.Padding(3, 6, 3, 0)
            Me.statusLabel.Name = "statusLabel"
            Me.statusLabel.Size = New System.Drawing.Size(774, 20)
            Me.statusLabel.TabIndex = 4
            Me.statusLabel.Text = "Ready."
            '
            'progressTimer
            '
            'MainForm
            '
            Me.AllowDrop = True
            Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(804, 501)
            Me.Controls.Add(Me.rootLayout)
            Me.Controls.Add(Me.appMenuStrip)
            Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.MainMenuStrip = Me.appMenuStrip
            Me.MinimumSize = New System.Drawing.Size(820, 460)
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "MainForm"
            Me.appMenuStrip.ResumeLayout(False)
            Me.appMenuStrip.PerformLayout()
            Me.rootLayout.ResumeLayout(False)
            Me.rootLayout.PerformLayout()
            Me.settingsLayout.ResumeLayout(False)
            Me.settingsLayout.PerformLayout()
            Me.ratioOptionsLayout.ResumeLayout(False)
            Me.ratioOptionsLayout.PerformLayout()
            Me.executionLayout.ResumeLayout(False)
            Me.executionLayout.PerformLayout()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Friend WithEvents appMenuStrip As System.Windows.Forms.MenuStrip
        Friend WithEvents fileMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents openDirectoryMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents fileMenuSeparator As System.Windows.Forms.ToolStripSeparator
        Friend WithEvents exitMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents helpMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents aboutMenuItem As System.Windows.Forms.ToolStripMenuItem
        Friend WithEvents themeToggleItem As ImageAspectRatioGrouper.Controls.ToolStripToggleSwitch
        Friend WithEvents rootLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents settingsLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents directoryLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents directoryTextBox As ImageAspectRatioGrouper.Controls.ThemedTextBox
        Friend WithEvents browseButton As ImageAspectRatioGrouper.Controls.ThemedButton
        Friend WithEvents recursiveToggle As ImageAspectRatioGrouper.Controls.ToggleSwitch
        Friend WithEvents aspectRatioLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents ratioOptionsLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents aspectRatioComboBox As ImageAspectRatioGrouper.Controls.ThemedComboBox
        Friend WithEvents toleranceLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents toleranceNumericUpDown As ImageAspectRatioGrouper.Controls.ThemedNumericUpDown
        Friend WithEvents minimumHeightLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents minimumHeightNumericUpDown As ImageAspectRatioGrouper.Controls.ThemedNumericUpDown
        Friend WithEvents executionLayout As System.Windows.Forms.TableLayoutPanel
        Friend WithEvents startButton As ImageAspectRatioGrouper.Controls.ThemedButton
        Friend WithEvents abortButton As ImageAspectRatioGrouper.Controls.ThemedButton
        Friend WithEvents progressBar As ImageAspectRatioGrouper.Controls.ThemedProgressBar
        Friend WithEvents progressLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents logLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents logConsole As ImageAspectRatioGrouper.Controls.LogConsoleBox
        Friend WithEvents statusLabel As ImageAspectRatioGrouper.Controls.ThemedLabel
        Friend WithEvents toolTip As System.Windows.Forms.ToolTip
        Friend WithEvents progressTimer As System.Windows.Forms.Timer

    End Class

End Namespace
