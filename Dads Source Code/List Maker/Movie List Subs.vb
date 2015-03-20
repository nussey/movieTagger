Module Module1
    Public sMovieDirs(2000) As String
    Public iMovieCount As Integer
    Public sMovieBaseDir As String = "V:\Movies\"

    ' TODO
    ' it currently checks every single file to make it is not a directory and it takes another 10 seconds or so doing this - see if I can remove this unncessesary check
    ' code can be found here: http://msdn.microsoft.com/en-us/library/t24c0thc(v=vs.90).aspx
    Function BuildDirList() As Integer
        Dim iWriteIndex, iReadIndex As Integer
        Dim sDir As String

        ' loop through all the directories and subdirectories to create an array of all subdirs
        iWriteIndex = 0
        iReadIndex = 0
        sMovieDirs(iWriteIndex) = sMovieBaseDir

        Do While (iReadIndex <= iWriteIndex)

            ' load in the next directory to be scanned
            sDir = Dir(sMovieDirs(iReadIndex), vbDirectory)

            ' look for subdirs in the directory and store any that it finds
            Do While sDir <> ""

                ' only store the file/dir if it is truly a directory
                If (GetAttr(sMovieDirs(iReadIndex) + "\" + sDir) And vbDirectory) = vbDirectory Then
 
                    ' don't add anything in the iTunes directory because it won't get organized properly (plus some material has non-movie M4Vs)
                    ' I can add in manually to a pre-made iTunes play list
                    If InStr(UCase(sDir), "ITUNES") = 0 Then
                        iWriteIndex += 1
                        sMovieDirs(iWriteIndex) = sMovieDirs(iReadIndex) + sDir + "\"
                    End If

                End If

                sDir = Dir() ' check for the next file/dir 
                MovieListMaker.WriteStatus("Scanning Directories: " & iWriteIndex.ToString)

            Loop

            iReadIndex += 1

        Loop

        iMovieCount = iWriteIndex

        Return True

    End Function

    ' this will create an output file name that the list is written to
    '       each file name starts with a time/date and then has the list type
    Function MakeListName(ByVal sListDesc As String) As String
        Dim sTimeDate, sPath As String

        sTimeDate = Format(Now(), "yyMMdd-HHmm")
        sPath = Application.StartupPath
        sPath = Mid(sPath, 1, InStrRev(sPath, "\") - 1) ' go up three directory levels to get the main directory
        sPath = Mid(sPath, 1, InStrRev(sPath, "\") - 1)
        sPath = Mid(sPath, 1, InStrRev(sPath, "\") - 1)

        Return sPath & "\" & sTimeDate & "-" & sListDesc & ".log"

    End Function

    ' Build a list of movies where the MyMovies movie.xml file is NEWER than the M4V movie file
    Function Build_Newer_MovieXML() As Integer
        Dim i, fh As Integer
        Dim sListName, sFile As String
        Dim dMovieFile, dXMLFile As Date

        sListName = MakeListName("Newer-MovieXML")

        ' start with a full dir list
        i = BuildDirList()

        ' open the output file so we can write matching files
        fh = FreeFile()
        FileOpen(fh, sListName, OpenMode.Output, OpenAccess.Write)
        sFile = ""

        ' loop through each directory entry and find the higest resolution file to store
        For i = 0 To iMovieCount
            ' write the status of what we are checking
            MovieListMaker.WriteStatus("scanning directory: " & sMovieDirs(i))


            ' no need to proceed unless we actually have a movie.xml file present
            If Dir(sMovieDirs(i) & "movie.xml") <> "" Then
                dXMLFile = FileDateTime(sMovieDirs(i) & "movie.xml")

                ' find each M4V movie in the directory and see if it's date is OLDER than the movie.xml
                sFile = Dir(sMovieDirs(i) & "*.m4v")
                Do While sFile <> ""
                    dMovieFile = FileDateTime(sMovieDirs(i) & sFile)
                    If dMovieFile < dXMLFile Then
                        PrintLine(fh, sMovieDirs(i) & sFile)
                    End If
                    sFile = Dir() ' get the nextm match file
                Loop


                ' find each M4V movie in the directory and see if it's date is OLDER than the movie.xml
                sFile = Dir(sMovieDirs(i) & "*.mp4")
                Do While sFile <> ""
                    dMovieFile = FileDateTime(sMovieDirs(i) & sFile)
                    If dMovieFile < dXMLFile Then
                        PrintLine(fh, sMovieDirs(i) & sFile)
                    End If
                    sFile = Dir() ' get the nextm match file
                Loop

            End If

            ' reset sFile so we don't accidentally re-use it
            sFile = ""
        Next i

        ' close out the list file
        FileClose(fh)

        MovieListMaker.WriteStatus("File Creation Complete")
        Return True

    End Function

    ' this creates a list of files that are raw from handbrake and not yet tagged
    ' this is very "hack-ish" - I just checked in the early ATOMs to see how AtomicParsley changed the ordering
    ' this does NOT actually read the atoms - it just looks for a consistent type of change made by Atomic Parsley
    Function Build_Untagged_M4V_wo_64bit() As Integer
        Dim i As Integer
        Dim sListName, sFile As String
        Dim sData As String = "1234"
        Dim i32or64bit As Int32

        sListName = MakeListName("Untagged-M4V")

        ' start with a full dir list
        i = BuildDirList()

        ' open the output file so we can write matching files
        FileOpen(1, sListName, OpenMode.Output, OpenAccess.Write)
        sFile = ""

        ' loop through each directory entry and find the higest resolution file to store
        For i = 0 To iMovieCount
            ' write the status of what we are checking
            MovieListMaker.WriteStatus("scanning directory: " & sMovieDirs(i))

            sFile = Dir(sMovieDirs(i) & "*.m4v")
            Do While sFile <> ""

                ' open the file and load the ATOMs (4 characters): 64bit or 32bit at 0xA0 (161 in decimal) and at 0xA4 (165 in decimal) 
                FileOpen(2, sMovieDirs(i) & sFile, OpenMode.Binary, OpenAccess.Read, OpenShare.Shared)
                FileGet(2, i32or64bit, 161)
                FileGet(2, sData, 165)
                FileClose(2)

                ' if the atom @A4 = mdat and the atom @A0 <> 1, then it's a raw 32bit movie out of handbrank 
                ' if the atom @A4 = moov, then it's a 32 bit file processed by Atomic Parsley
                ' if the atom @A0 = 1 then it's a 64 bit file that can not be processed by Atomic Parsley
                ' (note the byte orders are reversed so the Atom @a0 will get read as 0x01000000 for 64but files
                If sData.ToUpper = "MDAT" Then
                    'If sData.ToUpper = "MDAT" And i32or64bit <> &H1000000 Then  // got rid of the 64-bit check since Atomic Parsley can handle these files now
                    ' write out the file name if it is unprocessed and it is not 64-bit
                    PrintLine(1, sMovieDirs(i) & sFile)
                End If

                ' get the next potential M4V file to check
                sFile = Dir()
            Loop

        Next i

        ' close out the list file
        FileClose(1)

        MovieListMaker.WriteStatus("File Creation Complete")
        Return True

    End Function

  

    ' this creates a list of the files to import into the itunes instance supporting the AppleTV movies
    ' TODO: include (iphone) versions of movies where we haven't yet converted an M4V with handbrake
    Function Build_AppleTV_Import() As Integer
        Dim i As Integer
        Dim flag720 As Boolean = False
        Dim fNoMatchFound As Boolean
        Dim sListName, sFile, sPossibleFile As String

        sListName = MakeListName("AppleTV-Import")

        ' start with a full dir list
        i = BuildDirList()

        ' open the output file so we can write matching files
        FileOpen(1, sListName, OpenMode.Output, OpenAccess.Write)
        sFile = ""

        ' loop through each directory entry and find the higest resolution file to store
        For i = 0 To iMovieCount
            ' write the status of what we are checking
            MovieListMaker.WriteStatus("scanning directory: " & sMovieDirs(i))

            ' find the best version of the movie we can import into AppleTV
            sPossibleFile = Dir(sMovieDirs(i) & "*(1080).m4v")
            fNoMatchFound = True
            If sPossibleFile <> "" Then
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 1080p file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

            If fNoMatchFound Then
                sPossibleFile = Dir(sMovieDirs(i) & "*(720).m4v")
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 720p file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

            If fNoMatchFound Then
                sPossibleFile = Dir(sMovieDirs(i) & "*(480).m4v")
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 480p M4V file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

            If fNoMatchFound Then
                sPossibleFile = Dir(sMovieDirs(i) & "*(480).mp4") ' can't look for a single extension using wildcards because too many other file types show up
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 480p MP4 file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

        Next i

        ' close out the list file
        FileClose(1)

        MovieListMaker.WriteStatus("File Creation Complete")
        Return True

    End Function

    ' this creates a list of the files to import into the family itunes we use for syncing ipods and ipads
    ' it will include mostly (320) res files because there is no reason to have more res on those devices
    Function Build_iPod_Import() As Integer
        Dim i As Integer
        Dim fNoMatchFound As Boolean = False
        Dim sListName, sFile, sPossibleFile As String

        sListName = MakeListName("iPod-import")

        ' start with a full dir list
        i = BuildDirList()

        ' open the output file so we can write matching files
        FileOpen(1, sListName, OpenMode.Output, OpenAccess.Write)
        sFile = ""

            ' loop through each directory entry and find the best file to store
        For i = 0 To iMovieCount
            ' write the status of what we are checking
            MovieListMaker.WriteStatus("scanning directory: " & sMovieDirs(i))

            ' find the best version of the movie we can import into the ipod
            sPossibleFile = Dir(sMovieDirs(i) & "*(320).mp4")
            fNoMatchFound = True
            If sPossibleFile <> "" Then
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 320 mp4 file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

            If fNoMatchFound Then
                sPossibleFile = Dir(sMovieDirs(i) & "*(320).m4v") ' can't look for a single extension using wildcards because too many other file types show up
                While sPossibleFile <> ""
                    fNoMatchFound = False
                    PrintLine(1, sMovieDirs(i) & sPossibleFile)
                    sPossibleFile = Dir()       ' loop through every 320 MP4 file we find and add it to the list (e.g. extended versions, two-part movies, etc)
                End While
            End If

        Next i
          


            ' close out the list file
            FileClose(1)

            MovieListMaker.WriteStatus("File Creation Complete")
            Return True

    End Function


    ' 2013-09 This is a greatly revised version that creates a smarter list of import files for both iphones and AppleTV at the same time
    '
    '   - grab multi-part files, e.g. (1 of 2) and (2 of 2)
    '   - grab extra files, e.g. "making of"
    '   - extra files often have one resolution so they are placed into both the iphone and appletv imports
    '
    Function Build_All_Import_Files() As Integer
        Dim i, j, k, iFileCount As Integer
        Dim fHighestRes, fLowestRes As Boolean
        Dim sAppleTVListName, siPhoneListName, sFile As String
        Dim asFileList(20), asJustName(20) As String
        Dim aiResolution(20) As Integer

        sAppleTVListName = MakeListName("AppleTV-Import")
        siPhoneListName = MakeListName("iPhone-Import")

        ' start with a full dir list
        i = BuildDirList()

        ' open the output file so we can write matching files
        FileOpen(1, sAppleTVListName, OpenMode.Output, OpenAccess.Write)
        FileOpen(2, siPhoneListName, OpenMode.Output, OpenAccess.Write)

        ' loop through each directory entry and find the higest resolution file to store
        For i = 0 To iMovieCount
            ' write the status of what we are checking
            MovieListMaker.WriteStatus("scanning directory: " & sMovieDirs(i))

            ' fill three arrays with the names of every MP4 movies in the directory as well as its resolution and its name without the resolution
            iFileCount = 0
            sFile = Dir(sMovieDirs(i) & "*.m4v")
            Do While sFile <> ""
                If sFile <> "" Then
                    asFileList(iFileCount) = sFile
                    ' some files don't have a resolution at the end (like ones purchased from iTunes) - fill in default values for them
                    If InStr(sFile, "(320)") > 0 Or InStr(sFile, "(480)") > 0 Or InStr(sFile, "(720)") > 0 Or InStr(sFile, "(1080)") > 0 Then
                        asJustName(iFileCount) = Trim(Mid(sFile, 1, InStrRev(sFile, "(") - 1))
                        aiResolution(iFileCount) = Val(Mid(sFile, InStrRev(sFile, "(") + 1, InStrRev(sFile, ")") - 1))
                    Else
                        asJustName(iFileCount) = sFile
                        aiResolution(iFileCount) = 720
                    End If
                    iFileCount += 1
                    sFile = Dir()
                End If
            Loop
            sFile = Dir(sMovieDirs(i) & "*.mp4")
            Do While sFile <> ""
                If sFile <> "" Then
                    asFileList(iFileCount) = sFile
                    asJustName(iFileCount) = Trim(Mid(sFile, 1, InStrRev(sFile, "(") - 1))
                    aiResolution(iFileCount) = Val(Mid(sFile, InStrRev(sFile, "(") + 1, InStrRev(sFile, ")") - 1))
                    iFileCount += 1
                End If
                sFile = Dir()
            Loop

            ' go through each file in the movie directory and determine whether it should be written into the AppleTV or iPhone Import list
            For j = 0 To iFileCount - 1

                ' first see if this file is the HIGHEST or LOWEST resolution and write it out accordingly
                fHighestRes = True
                fLowestRes = True
                For k = 0 To iFileCount - 1
                    If UCase(asJustName(k)) = UCase(asJustName(j)) Then ' only compare for the same file names
                        If aiResolution(k) < aiResolution(j) Then fLowestRes = False
                        If aiResolution(k) > aiResolution(j) Then fHighestRes = False
                    End If
                Next

                ' write out the appropriate each respective import file if the file is either the highest or lowest res
                If fLowestRes Then PrintLine(2, sMovieDirs(i) & asFileList(j))
                If fHighestRes Then PrintLine(1, sMovieDirs(i) & asFileList(j))
            Next

        Next ' go into next directory

        ' close out the list file
        FileClose(1)
        FileClose(2)

        MovieListMaker.WriteStatus("File Creation Complete")
        Return True

    End Function
End Module
