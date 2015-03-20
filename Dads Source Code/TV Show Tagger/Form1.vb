Public Class TVTagger

    Public Structure oTVShow
        Dim ssSeasonEpisode As String
        Dim ssTitle As String
        Dim ssDesc As String
    End Structure


    ' call this to add more text into the status box
    Private Sub subAddStatus(sStatus As String)
        txtStatus.AppendText(sStatus & Environment.NewLine)
    End Sub

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click

        OpenFolderDialog.SelectedPath = "v:\TV Shows"
        OpenFolderDialog.Description = "Select the folder to process"
        If OpenFolderDialog.ShowDialog = DialogResult.OK Then
            ' Display the selected folder if the user clicked on the OK button.
            txtDirName.Text = OpenFolderDialog.SelectedPath
        End If
    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        ' this was done to create M4V meta tags based on the names of TV shows
        Dim i, fhtemp, iMetaCount, iFileCount As Integer
        Dim a As String
        Dim sdir, fn, cmdstr, sbatchfile, sDesc, sCoverArtFile As String
        Dim iSeason, iEpisode As Integer
        Dim sTitle, sTVShowName, sSeasonEpisode As String
        Dim aTVShow(50) As oTVShow

        sdir = txtDirName.Text
        iFileCount = 0
        subAddStatus("Scanning for TV shows in: " & sdir)

        'Dim fso
        'fso = CreateObject("scripting.filesystemobject")
        ' Dim fso As New FileSystemObject
        'Dim fil As File
        'For Each fil In fso.GetFolder("C:\").Files
        '   Debug.Print(fil.Name)
        'Next

        '
        ' **** LOAD in the META data if the file is available - store all the info we find into an array so we can reference it later
        ' 
        fhtemp = FreeFile()
        If My.Computer.FileSystem.FileExists(sdir & "\meta.txt") Then ' if the file exists, open it up and read it in
            'If Dir(sdir & "\meta.txt") <> "" Then ' if the file exists, open it up and read it in
            FileOpen(fhtemp, sdir & "\meta.txt", OpenMode.Input, OpenAccess.Read)

            iMetaCount = -1 ' the first increment will take the array to position 0
            Do While Not EOF(fhtemp)
                a = LineInput(fhtemp)

                If Mid(a, 1, 1) = "[" Then
                    iMetaCount += 1
                    aTVShow(iMetaCount).ssSeasonEpisode = Mid(a, 2, Len(a) - 2)
                End If

                If Mid(a, 1, 4).ToUpper = "DESC" Then
                    aTVShow(iMetaCount).ssDesc = Mid(a, InStr(a, "=") + 1).Trim ' pull out everything after the Desc=
                End If

                If Mid(a, 1, 5).ToUpper = "TITLE" Then
                    aTVShow(iMetaCount).ssTitle = Mid(a, InStr(a, "=") + 1).Trim ' pull out everything after the Desc=
                End If

            Loop
            iMetaCount += 1

            ' close out the meta.txt file
            FileClose(fhtemp)
            subAddStatus("   META.TXT file found, " & iMetaCount & " episodes loaded")

        Else

            subAddStatus("   No META.TXT file found!")

        End If

        '
        ' **** LOOP through all the movie files we find and process each one
        ' 
        sTVShowName = ""
        sTitle = ""
        sSeasonEpisode = ""

        For Each fn In My.Computer.FileSystem.GetFiles(sdir, FileIO.SearchOption.SearchTopLevelOnly, "*.m4v")

            'fn = Dir(txtDirName.Text & "\*.m4v")
            ' Do While fn <> ""

            ' format the status update
            iFileCount += 1
            subAddStatus(" ") ' blank line
            subAddStatus("Scanning File #" & iFileCount)
            subAddStatus("   " & fn)  ' c\ab sxxeyy - d

            ' find where the title & season info starts by searching for SxxEyy where xx is season and yy is episode
            For i = 1 To Len(fn)
                If UCase(Mid(fn, i, 1)) = "S" And UCase(Mid(fn, i + 3, 1)) = "E" And Mid(fn, i + 1, 1) >= "0" And Mid(fn, i + 1, 1) <= "9" Then
                    sTitle = Mid(fn, i + 9, Len(fn) - i - 9 - 3)
                    sTVShowName = Mid(fn, InStrRev(fn, "\") + 1, i - InStrRev(fn, "\") - 2)
                    iSeason = Val(Mid(fn, i + 1, 2))
                    iEpisode = Val(Mid(fn, i + 4, 2))
                    sSeasonEpisode = Mid(fn, i, 6)
                    Exit For
                End If
            Next

            ' Post a status update based on the parsing of the seasons and episodes
            subAddStatus("   Show name: " & sTVShowName)
            subAddStatus("   Title: " & sTitle)
            subAddStatus("   Season: " & iSeason & "   Episode: " & iEpisode)

            ' if there is meta data, find a matching description string
            sDesc = ""
            i = 0
            Do
                If aTVShow(i).ssSeasonEpisode = sSeasonEpisode Then
                    sDesc = aTVShow(i).ssDesc
                End If
                i += 1
            Loop While i < iMetaCount And sDesc = ""

            ' see if there is specific cover art we want to attach
            '   this will be in a subdir called "Cover Art" and it will be in the form of SXXEYY.jpg
            '   if no cover art is found, use the default file, folder.jpg
            sCoverArtFile = sdir & "\folder.jpg"
            If My.Computer.FileSystem.FileExists(sdir & "\Cover Art\" & sSeasonEpisode & ".jpg") Then
                sCoverArtFile = sdir & "\Cover Art\" & sSeasonEpisode & ".jpg"
                subAddStatus("   Found Cover Art")
            End If

            ' create a batch file to run AtomicParsley that also provides information on the file being process
            sbatchfile = "Echo Processing TV Show: " & fn & Chr(13) & Chr(10)

            sbatchfile &= Chr(34) & "c:\Users\Bill\Desktop\atomicparsley\atomicparsley.exe" & Chr(34) & " " ' need quotes surround everything after CMD /K
            sbatchfile &= Chr(34) & fn & Chr(34) & " "                          ' file name surrounded by quotes
            sbatchfile &= "--title " & Chr(34) & sTitle & Chr(34) & " "                           ' the cleaned up movie name
            sbatchfile &= "--TVShowName " & Chr(34) & sTVShowName & Chr(34) & " "                           ' the cleaned up movie name
            sbatchfile &= "--TVSeasonNum " & Str(iSeason) & " "                           ' the cleaned up movie name
            sbatchfile &= "--TVEpisodeNum " & Str(iEpisode) & " "                           ' the cleaned up movie name
            sbatchfile &= "--stik " & Chr(34) & "TV Show" & Chr(34) & " "                           ' the cleaned up movie name
            If sDesc <> "" Then
                sbatchfile &= "--description " & Chr(34) & sDesc & Chr(34) & " "                           ' the cleaned up movie name
            End If
            If cbAddCoverArt.Checked Then
                sbatchfile &= "--artwork " & Chr(34) & sCoverArtFile & Chr(34) & " "                 ' art work file name - only load if the checkbox is checked
            End If
            sbatchfile &= "--overWrite "                  ' tell it to overwrite the original file name rather than making a copy

            ' write the batch file so we can run it
            fhtemp = FreeFile()
            FileOpen(fhtemp, sdir & "\proctemp.bat", OpenMode.Output, OpenAccess.Write, OpenShare.Default)
            PrintLine(fhtemp, sbatchfile)
            FileClose(fhtemp)



            'FileOpen(6, sDir & "\dump.log", OpenMode.Output, OpenAccess.Write, OpenShare.Default)
            'PrintLine(6, cmdstr)
            'FileClose(6)

            cmdstr = "cmd /C " & Chr(34) & sdir & "\proctemp.bat " & Chr(34)

            Application.DoEvents() ' let some other events run before we call the CMD program

            ' updating the way Atomic Parsley in run 12-29-2013, from http://msdn.microsoft.com/en-us/library/microsoft.visualbasic.interaction.shell(v=vs.110).aspx
            'i = Shell(cmdstr, AppWinStyle.NormalFocus, True)
            Dim newProc As Diagnostics.Process
            Dim procID As Integer

            subAddStatus("   Calling AtomicParsley ")
            newProc = Diagnostics.Process.Start("cmd.exe", "/C" & Chr(34) & sdir & "\proctemp.bat" & Chr(34))
            procID = newProc.Id

            subAddStatus("   AtomicParsley Processing")
            newProc.WaitForExit()
            subAddStatus("   AtomicParsley Complete")

            ' Dim procEC As Integer = -1
            ' If newProc.HasExited Then
            '   procEC = newProc.ExitCode
            ' End If

            Kill(sdir & "\proctemp.bat") ' delete the temp batch file
            'Module2.LogWrite("Finished file")

            'fn = Dir()

            'Loop
        Next ' loop through each file name and assign it to the variable "fn"

    End Sub
End Class
