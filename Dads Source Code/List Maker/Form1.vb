Public Class MovieListMaker
    ' write the status onto the main screen

    Public Sub WriteStatus(ByVal sStatus As String)

        txtStatus.Text = sStatus
        Application.DoEvents()

    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles butMakeList.Click
        Dim iListType As Integer
        Dim sListSelect As String
        Dim i As Integer

        ' check to see what list type has been selected
        sListSelect = lstListType.Text
        sListSelect = Mid(sListSelect, 1, InStr(sListSelect, ".") - 1)
        iListType = Convert.ToInt32(sListSelect)

        Select Case iListType
            Case 1
                i = Build_Untagged_M4V_wo_64bit()
            Case 2
                i = Build_Newer_MovieXML()
            Case 3
                i = Build_AppleTV_Import()
            Case 4
                i = Build_iPod_Import()
            Case 5
                i = Build_All_Import_Files()
        End Select
    End Sub

    Private Sub MovieListMaker_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' load up the list box with the options for types of lists
        lstListType.Items.Add("1. Untagged M4Vs w/o 64bit files")
        lstListType.Items.Add("2. Movies with newer movie.xml files")
        lstListType.Items.Add("3. AppleTV Import")
        lstListType.Items.Add("4. iPod Library Import")
        lstListType.Items.Add("5. iPhone+AppleTV Import v2")

    End Sub


End Class
