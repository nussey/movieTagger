Public Class MP4Tagger

    '
    ' write the status onto the main screen
    Public Sub WriteStatus(ByVal sStatus As String)

        txtStatus.Text = sStatus
        Application.DoEvents()

    End Sub

    '
    ' allow module to access the state of the art work checkbox
    Public Function GetArtworkSaveValue()
        Return chkCoverArt.Checked

    End Function


    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butSelectDir.Click

        With (OpenFileDialog)
            .InitialDirectory = "C:\Users\Bill\Desktop\My Dropbox\personal\Management Scripts"
            .Title = "Select the movie list file"
            If .ShowDialog = DialogResult.OK Then
                ' Display the selected folder if the user clicked on the OK button.
                txtDirName.Text = .FileName
            End If
        End With


    End Sub

    Private Sub butRunTag_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butRunTag.Click

        ' call the function that will read the list of file names and tag each one - use the file name from the Dialog box
        ReadAndTagFiles(txtDirName.Text)

    End Sub


    Private Sub TextBox2_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox2.TextChanged

    End Sub
    Private Sub txtDirName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtDirName.TextChanged

    End Sub
End Class
