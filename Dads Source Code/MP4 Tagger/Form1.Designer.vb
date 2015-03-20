<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MP4Tagger
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtDirName = New System.Windows.Forms.TextBox()
        Me.TextBox2 = New System.Windows.Forms.TextBox()
        Me.butSelectDir = New System.Windows.Forms.Button()
        Me.butRunTag = New System.Windows.Forms.Button()
        Me.txtStatus = New System.Windows.Forms.TextBox()
        Me.OpenFileDialog = New System.Windows.Forms.OpenFileDialog()
        Me.chkCoverArt = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'txtDirName
        '
        Me.txtDirName.Location = New System.Drawing.Point(24, 35)
        Me.txtDirName.Name = "txtDirName"
        Me.txtDirName.Size = New System.Drawing.Size(434, 20)
        Me.txtDirName.TabIndex = 0
        '
        'TextBox2
        '
        Me.TextBox2.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.TextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.TextBox2.Font = New System.Drawing.Font("Microsoft Sans Serif", 7.471698!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.TextBox2.Location = New System.Drawing.Point(24, 16)
        Me.TextBox2.Name = "TextBox2"
        Me.TextBox2.Size = New System.Drawing.Size(194, 12)
        Me.TextBox2.TabIndex = 1
        Me.TextBox2.Text = "Select Movie List File"
        '
        'butSelectDir
        '
        Me.butSelectDir.Location = New System.Drawing.Point(464, 32)
        Me.butSelectDir.Name = "butSelectDir"
        Me.butSelectDir.Size = New System.Drawing.Size(75, 23)
        Me.butSelectDir.TabIndex = 2
        Me.butSelectDir.Text = "Select File"
        Me.butSelectDir.UseVisualStyleBackColor = True
        '
        'butRunTag
        '
        Me.butRunTag.Location = New System.Drawing.Point(435, 208)
        Me.butRunTag.Name = "butRunTag"
        Me.butRunTag.Size = New System.Drawing.Size(125, 45)
        Me.butRunTag.TabIndex = 3
        Me.butRunTag.Text = "Apply Tagging"
        Me.butRunTag.UseVisualStyleBackColor = True
        '
        'txtStatus
        '
        Me.txtStatus.BackColor = System.Drawing.SystemColors.ButtonFace
        Me.txtStatus.Location = New System.Drawing.Point(28, 274)
        Me.txtStatus.Name = "txtStatus"
        Me.txtStatus.Size = New System.Drawing.Size(532, 20)
        Me.txtStatus.TabIndex = 6
        '
        'OpenFileDialog
        '
        Me.OpenFileDialog.FileName = "OpenFileDialog1"
        '
        'chkCoverArt
        '
        Me.chkCoverArt.AutoSize = True
        Me.chkCoverArt.Location = New System.Drawing.Point(24, 72)
        Me.chkCoverArt.Name = "chkCoverArt"
        Me.chkCoverArt.Size = New System.Drawing.Size(92, 17)
        Me.chkCoverArt.TabIndex = 7
        Me.chkCoverArt.Text = "Add Cover Art"
        Me.chkCoverArt.UseVisualStyleBackColor = True
        '
        'MP4Tagger
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(567, 322)
        Me.Controls.Add(Me.chkCoverArt)
        Me.Controls.Add(Me.txtStatus)
        Me.Controls.Add(Me.butRunTag)
        Me.Controls.Add(Me.butSelectDir)
        Me.Controls.Add(Me.TextBox2)
        Me.Controls.Add(Me.txtDirName)
        Me.Name = "MP4Tagger"
        Me.Text = "MP4 Tagger"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtDirName As System.Windows.Forms.TextBox
    Friend WithEvents TextBox2 As System.Windows.Forms.TextBox
    Friend WithEvents butSelectDir As System.Windows.Forms.Button
    Friend WithEvents butRunTag As System.Windows.Forms.Button
    Friend WithEvents txtStatus As System.Windows.Forms.TextBox
    Friend WithEvents OpenFileDialog As System.Windows.Forms.OpenFileDialog
    Friend WithEvents chkCoverArt As System.Windows.Forms.CheckBox

End Class
