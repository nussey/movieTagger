Module Module2

    Public sAtomicParsleyLoc As String = "c:\Users\Bill\Desktop\atomicparsley\atomicparsley.exe"
    Public sLogFileDir As String = "C:\Users\Bill\Desktop\Dropbox\personal\Management Scripts\MP4 Tagger" ' can I use Application.StartupPath() here? Sure.
    Public MAXACTORS As Integer = 5


    Public Structure tM4VTags
        Dim sName As String
        Dim sGenre As String
        Dim sDesc As String
        Dim bCoverArt As Boolean
        Dim sHighDef As Boolean             ' added 6-2012 - matches strings for iTunes -> 0=sd, 1=720, 2=1080
        Dim sCoverArtFile As String
        Dim sRelease As String
        Dim sActors() As String
        Dim iActorCount As Integer             ' added 7-2012
        Dim sDirectors() As String             ' added 6-2012
        Dim iDirectorCount As Integer          ' added 7-2012
        Dim sRating As String
        Dim sCategories As String ' note categories are all concatenated together and separated by a vertical bar "|" (last category is followed by a bar, too)
    End Structure
    '
    ' Get a text file containing the list of files we need to tag
    Function ReadAndTagFiles(ByVal sFileList As String)
        Dim i, fhmain, fhtemp As Integer
        Dim iFileCount, iCurfile As Integer
        Dim cmdstr As String        ' the cmd we will pass to the shell function
        Dim sFile, sDir, sTemp As String
        Dim sActorList As String
        Dim iGenreStart, iGenreEnd As Integer
        Dim oMovie As tM4VTags
        Dim siTunesXMLAtom As String
        Dim sBatchFile As String
        Dim sTempHiDef As String

        ' create array space for actors and directors (this is a quirk of the newer VBs)
        ReDim oMovie.sActors(MAXACTORS + 1)
        ReDim oMovie.sDirectors(MAXACTORS + 1)

        ' open the file to get the number of entries so we can count down
        fhtemp = FreeFile()
        FileOpen(fhtemp, sFileList, OpenMode.Input, OpenAccess.Read)
        iFileCount = 0
        iCurfile = 1
        Do While Not EOF(fhtemp)
            sFile = LineInput(fhtemp)
            iFileCount += 1
        Loop
        FileClose(fhtemp)

        ' re-open the movie list file and iterate through each movie
        fhmain = FreeFile()
        FileOpen(fhmain, sFileList, OpenMode.Input, OpenAccess.Read)

        Do While Not EOF(fhmain)

            sFile = LineInput(fhmain)

            Module2.LogWrite("Working on file : " & sFile)
            MP4Tagger.WriteStatus("Working on file (" & Trim(Str(iCurfile)) & " of " & Trim(Str(iFileCount)) & "): " & sFile)
            iCurfile += 1

            ' let some other events run before we call the CMD program
            For i = 1 To 1000
                Application.DoEvents()
            Next

            ' load in the MyMovies.xml data as a starting point
            'i = ReadMyMoviesPropertiesFile(sFile, oMovie) ' used this temporarily while the then current version of MyMovies was encrypting the XML file
            i = ReadMyMoviesXML(sFile, oMovie)

            ' determine the directory we are working in
            sDir = Mid(sFile, 1, InStrRev(sFile, "\") - 1)

            ' GENRE - calc the Genre name - use the parent directory verbatim as the Genre name
            iGenreEnd = InStrRev(Mid(sDir, 1, Len(sDir) - 1), "\")
            iGenreStart = InStrRev(sDir, "\", iGenreEnd - 1)
            oMovie.sGenre = Mid(sDir, iGenreStart + 1, iGenreEnd - iGenreStart - 1)

            ' see if there is a category.txt in the directory that will override anythign returned from the XML or Properties file
            'If Dir(sDir & "category.txt") <> "" Then
            '   FileOpen(4, sDir & "category.txt", OpenMode.Input, OpenAccess.Read, OpenShare.Shared)
            '   oMovie.sCategories = LineInput(4)   ' assumes that the category.txt file is the same format we hand things to iTunes
            '   FileClose(4)
            'End If

            ' CATEGORY from genre directory - add a category from the directory name if it's not already there
            If Mid(oMovie.sCategories, 1, 1) <> "|" Then oMovie.sCategories = "|" ' always make sure there is an initial vertical bar
            If InStr(oMovie.sCategories.ToUpper, oMovie.sGenre.ToUpper) = 0 Then  ' does the genre name exist within the cateorgy string?
                oMovie.sCategories = oMovie.sCategories & oMovie.sGenre & "|"
            End If

            ' HD CATEGORY - add an HD category if the movie is hi-res
            oMovie.sHighDef = False   ' itunes interprets this as SD
            sTempHiDef = "false"     ' for some reason, this has to be lower case

            If InStr(sFile, "(720)") Then
                oMovie.sHighDef = True  ' iTunes interprets this as 720
                sTempHiDef = "true"     ' for some reason, this has to be lower case
                oMovie.sCategories = oMovie.sCategories & "HD|"
            ElseIf InStr(sFile, "(1080)") > 0 Then
                oMovie.sHighDef = True   ' iTunes interprets this as 1080
                oMovie.sCategories = oMovie.sCategories & "HD1080|HD|" 'add the extra category so I can create a smart list around the highest res movies
                sTempHiDef = "true"     ' for some reason, this has to be lower case
            End If

            ' COVER ART IMAGE - calc the cover art image file name
            oMovie.sCoverArtFile = sDir & "\folder.jpg"

            ' reduce the description size to ensure it'll fit into iTunes
            If Len(oMovie.sDesc) > 250 Then
                oMovie.sDesc = Mid(oMovie.sDesc, 1, 250)
            End If

            ' Translate the contents of oMovie into an iTunes compatible XML atom
            siTunesXMLAtom = MakeiTunesAtom(oMovie)

            ' TITLE - create a proper title that does not contain the resolution information (subtract -> "(xxx).m4v")
            ' *** CHECK THIS CODE DURING DEBUG - look for moviename (1965)(320) or moviename (1965) (320) or moviename (2007) (1080)
            sTemp = Mid(sFile, Len(sDir) + 2, Len(sFile) - Len(sDir) - 5) ' remove leading dir info and trailing extension
            sTemp = Trim(Mid(sTemp, 1, InStrRev(sTemp, "(") - 1))
            oMovie.sName = sTemp

            ' even though the XML Atom causes the actors to be displayed on the AppleTV, I'm adding them to the standard ID3 tag so you can see them in iTunes and other standard apps
            sActorList = ""
            For i = 0 To oMovie.iActorCount - 1
                sActorList &= oMovie.sActors(i) & ", "
            Next
            sActorList = Left(sActorList, Len(sActorList) - 2) ' remove the trailing comma from the list

            ' create a batch file to run AtomicParsley that also provides information on the file being process
            sBatchFile = "Echo Processing Movie: " & sFile & Chr(13) & Chr(10)

            sBatchFile &= Chr(34) & "c:\Users\Bill\Desktop\atomicparsley\atomicparsley.exe" & Chr(34) & " " ' need quotes surround everything after CMD /K
            sBatchFile &= Chr(34) & sFile & Chr(34) & " "                          ' file name surrounded by quotes
            sBatchFile &= "--title " & Chr(34) & oMovie.sName & Chr(34) & " "                           ' the cleaned up movie name
            sBatchFile &= "--genre " & Chr(34) & oMovie.sGenre & Chr(34) & " "                         ' genre
            sBatchFile &= "--composer " & Chr(34) & oMovie.sCategories & Chr(34) & " "                  ' categories
            sBatchFile &= "--description " & Chr(34) & oMovie.sDesc & Chr(34) & " "                  ' Desc
            sBatchFile &= "--artist " & Chr(34) & sActorList & Chr(34) & " "                  ' actors
            sBatchFile &= "--contentRating " & Chr(34) & oMovie.sRating & Chr(34) & " "                  ' rating
            sBatchFile &= "--hdvideo " & sTempHiDef & " "                  ' SD or HD
            sBatchFile &= "--stik " & Chr(34) & "Short Film" & Chr(34) & " "                           ' set the MediaKind to Movie. AtomicParsley has the stik id 9 as "Short Film" but the most recent iTunes uses 9 for Movies (https://code.google.com/p/mp4v2/wiki/iTunesMetadata)
            sBatchFile &= "--year " & Chr(34) & oMovie.sRelease & Chr(34) & " "                  ' release year
            If MP4Tagger.GetArtworkSaveValue() = True Then
                sBatchFile &= "--artwork " & Chr(34) & oMovie.sCoverArtFile & Chr(34) & " "                 ' art work file name
            End If
            sBatchFile &= "--overWrite "                  ' tell it to overwrite the original file name rather than making a copy
            sBatchFile &= "--rDNSatom " & Chr(34) & siTunesXMLAtom & Chr(34) & " name=iTunMOVI domain=com.apple.iTunes"    ' XML list of directors and actors !! for some reason, this NEEDS to be the last argument

            ' write the batch file so we can run it
            fhtemp = FreeFile()
            FileOpen(fhtemp, sDir & "\proctemp.bat", OpenMode.Output, OpenAccess.Write, OpenShare.Default)
            PrintLine(fhtemp, sBatchFile)
            FileClose(fhtemp)

            ' create the comd string - "quotes" are critical here - must have quotes surrounding the part passed to cmd
            '   and also surrounding the exe and movie file name so spaces don't get misinterpretted
            '   note that double-quotes "" are interpretted withing a string as a single quote
            'cmdstr = "cmd /C " & Chr(34) & Chr(34) & "c:\Users\Bill\Desktop\atomicparsley\atomicparsley.exe" & Chr(34) & " " ' need quotes surround everything after CMD /K
            'cmdstr &= Chr(34) & sFile & Chr(34) & " "                          ' file name surrounded by quotes
            'cmdstr &= "--title " & Chr(34) & oMovie.sName & Chr(34) & " "                           ' the cleaned up movie name
            'cmdstr &= "--genre " & Chr(34) & oMovie.sGenre & Chr(34) & " "                         ' genre
            'cmdstr &= "--composer " & Chr(34) & oMovie.sCategories & Chr(34) & " "                  ' categories
            'cmdstr &= "--description " & Chr(34) & oMovie.sDesc & Chr(34) & " "                  ' Desc
            'cmdstr &= "--artist " & Chr(34) & sActorList & Chr(34) & " "                  ' actors
            'cmdstr &= "--contentRating " & Chr(34) & oMovie.sRating & Chr(34) & " "                  ' rating
            'cmdstr &= "--hdvideo " & Chr(34) & oMovie.sHighDef & Chr(34) & " "                  ' SD or HD
            'cmdstr &= "--year " & Chr(34) & oMovie.sRelease & Chr(34) & " "                  ' release year
            'cmdstr &= "--artwork " & Chr(34) & oMovie.sCoverArtFile & Chr(34) & " "                 ' art work file name
            '   cmdstr &= "--rDNSatom " & Chr(39) & siTunesXMLAtom & Chr(34) & " name=iTunMOVI domain=com.apple.iTunes" & Chr(39) & " "    ' XML list of directors and actors
            'cmdstr &= "--rDNSatom " & Chr(34) & "hello there 'quote test' and """"test"""" " & Chr(34) & " name=iTunMOVI domain=com.apple.iTunes"    ' XML list of directors and actors
            'cmdstr &= "--overWrite "                  ' tell it to overwrite the original file name rather than making a copy
            'cmdstr &= "--rDNSatom " & Chr(34) & siTunesXMLAtom & Chr(34) & " name=iTunMOVI domain=com.apple.iTunes"    ' XML list of directors and actors !! for some reason, this NEEDS to be the last argument
            'cmdstr &= ">" & sDir & "\output.log"
            'cmdstr &= Chr(34)

            'FileOpen(6, sDir & "\dump.log", OpenMode.Output, OpenAccess.Write, OpenShare.Default)
            'PrintLine(6, cmdstr)
            'FileClose(6)

            cmdstr = "cmd /C " & Chr(34) & sDir & "\proctemp.bat " & Chr(34)

            Application.DoEvents() ' let some other events run before we call the CMD program
            i = Shell(cmdstr, AppWinStyle.NormalFocus, True)
            Kill(sDir & "\proctemp.bat") ' delete the temp batch file
            Module2.LogWrite("Finished file")

        Loop

        FileClose(fhmain)
        Return True

    End Function

    ' This takes the various data filled out in an M4VTag object and creates an iTunes/AppleTV compatible XML file
    ' This allows AppleTV to nicely display actors and directors
    '
    Function MakeiTunesAtom(ByVal oMovie As tM4VTags)
        Dim sTemp As String
        Dim sfs, scrlf As String
        Dim i As Integer

        sfs = Chr(47) & Chr(47)
        scrlf = Chr(13) ' Atomic Parsley can't seem to handle LFs or CRLFs on its command line

        ' create header string
        ' on the quad-quotes -- basic interprets a double quote as a single quote; AtomicParsley does the same thing; therefore
        ' I need to put four quotes here which means two quotes when VB outputs it; that means a single quote when AP reads it in
        ' I was shocked this work but it was a last resort so I'm really happy 7-2012
        sTemp = "<?xml version=""""1.0"""" encoding=""""UTF-8""""?>" & scrlf
        sTemp &= "<!DOCTYPE plist PUBLIC """"-" & sfs & "Apple" & sfs & "DTD PLIST 1.0" & sfs & "EN"""""
        sTemp &= " """"http://www.apple.com/DTDs/PropertyList-1.0.dtd""""> " & scrlf
        sTemp &= "<plist version=""""1.0"""">" & scrlf
        sTemp &= "<dict>" & scrlf

        ' add in the CAST
        sTemp &= Chr(9) & "<key>cast</key>" & scrlf
        sTemp &= Chr(9) & "<array>" & scrlf
        For i = 0 To oMovie.iActorCount - 1
            sTemp &= Chr(9) & Chr(9) & "<dict>" & scrlf
            sTemp &= Chr(9) & Chr(9) & Chr(9) & "<key>name</key>" & scrlf
            sTemp &= Chr(9) & Chr(9) & Chr(9) & "<string>" & oMovie.sActors(i) & "</string>" & scrlf
            sTemp &= Chr(9) & Chr(9) & "</dict>" & scrlf
        Next
        sTemp &= Chr(9) & "</array>" & scrlf

        ' add in the DIRECTORS
        sTemp &= Chr(9) & "<key>directors</key>" & scrlf
        sTemp &= Chr(9) & "<array>" & scrlf
        For i = 0 To oMovie.iDirectorCount - 1
            sTemp &= Chr(9) & Chr(9) & "<dict>" & scrlf
            sTemp &= Chr(9) & Chr(9) & Chr(9) & "<key>name</key>" & scrlf
            sTemp &= Chr(9) & Chr(9) & Chr(9) & "<string>" & oMovie.sDirectors(i) & "</string>" & scrlf
            sTemp &= Chr(9) & Chr(9) & "</dict>" & scrlf
        Next
        sTemp &= Chr(9) & "</array>" & scrlf

        ' add in the footer
        sTemp &= "</dict>" & scrlf
        sTemp &= "</plist>" & scrlf


        Return sTemp
    End Function


    ' check to see which tags are in the MV4 file
    ' returns a string with A (Artwork), G (Genre), R (Rating), D (Release date), S (Desc), C (Actors)
    Function MV4Check(ByVal sFile As String) As String

        Dim i
        Dim cmdstr As String
        Dim sline As String = ""
        Dim sAtoms As String

        ' initial error check - does the file even exist?
        If Not My.Computer.FileSystem.FileExists(sFile) Then Return ""

        ' create the comd string that runs a dump of the MP4 file
        ' (/C terminates after completion; /K leaves the window open)
        cmdstr = "cmd /k " & Chr(34) & Chr(34) & sAtomicParsleyLoc & Chr(34) & " " ' need quotes surround everything after CMD /K 
        cmdstr &= Chr(34) & sFile & Chr(34) & " "                          ' file name surrounded by quotes
        cmdstr &= "-t > " & Chr(34) & sLogFileDir & "\" & "AtomicParsleyOut.tmp"           ' send the output to a tmp file we can parse next
        cmdstr &= Chr(34)

        i = Shell(cmdstr, AppWinStyle.NormalFocus, True)

        FileOpen(1, sLogFileDir & "\" & "AtomicParsleyOut.tmp", OpenMode.Input, OpenAccess.Read, OpenShare.Default, )

        ' loop through each line and create one large searchable string for the atoms
        Do While Not EOF(1)
            sline &= LineInput(1) & Chr(13)
        Loop
        FileClose(1)
        sline = UCase(sline) ' make it upper case to simply comparisons

        ' check for each atom we are interested in
        If InStr(sline, Chr(169) & "GEN") > 0 Then sAtoms &= "G" ' Genre - it is preceeded by a copyright circle, not a "C"
        If InStr(sline, "COVR") > 0 Then sAtoms &= "A" ' Cover Art
        If InStr(sline, "[ITUNEXTC]") > 0 Then sAtoms &= "R" ' This precedes the description of the MPAA rating
        If InStr(sline, Chr(169) & "DAY") > 0 Then sAtoms &= "D" ' Release Date - it is preceeded by a copyright circle, not a "C"
        If InStr(sline, "DESC") > 0 Then sAtoms &= "S" ' Description
        If InStr(sline, Chr(169) & "ART") > 0 Then sAtoms &= "C" ' Actors (comma separated into a single string),  it is preceeded by a copyright circle, not a "C"

        Return sAtoms
    End Function

    ' read in the mymovies.xml file associated with the movie and fill in the data to oMovieData that is in there

    Function ReadMyMoviesXML(ByVal sMovieFile As String, ByRef oMovieData As tM4VTags) As Integer
        Dim sMyMovieData As String
        Dim sMyMovieFileName As String
        Dim sLine As String
        Dim i As Integer

        Dim sCategoryXML, sCategoryData As String
        Dim iCategoryCount As Integer

        Dim sPersonsXML, sPersonData, sPerson As String
        Dim sRatingXML, sRatingData As String
        Dim iRating As Integer


        ' first build the path/name for mymovies and load it in
        i = InStrRev(sMovieFile, "\")
        sMyMovieFileName = Mid(sMovieFile, 1, i) & "movie.xml" ' this is not a default MyMovies file - it needs to be turned on

        FileOpen(3, sMyMovieFileName, OpenMode.Input, OpenAccess.Read, OpenShare.Shared) ' make sure file handle "3" isn't used elsewhere
        sMyMovieData = ""
        Do While Not EOF(3)
            sLine = LineInput(3)
            sMyMovieData = sMyMovieData & sLine & Chr(13) & Chr(10)
        Loop
        FileClose(3)

        ' now parse out each individual piece of data from mymovies.xml

        ' get the year the movie was made
        oMovieData.sRelease = GetXMLTagVal(sMyMovieData, "ProductionYear")

        ' retrieve the description, revove the CDATA stuff, remove any quotes (so it won't confuse atomic parsely)
        oMovieData.sDesc = GetXMLTagVal(sMyMovieData, "Description")
        oMovieData.sDesc = Mid(oMovieData.sDesc, 10, Len(oMovieData.sDesc) - 12) ' remove leading and trailing CDATA set up
        oMovieData.sDesc = oMovieData.sDesc.Replace(Chr(13), "")
        oMovieData.sDesc = oMovieData.sDesc.Replace(Chr(10), "")
        oMovieData.sDesc = oMovieData.sDesc.Replace(Chr(34), "")

        ' retrieve the categories and build them into a single string
        sCategoryXML = GetXMLTagVal(sMyMovieData, "Categories")
        sCategoryData = ""
        iCategoryCount = 1
        oMovieData.sCategories = "|"
        Do
            sCategoryData = GetXMLTagVal(sCategoryXML, "Category", iCategoryCount)
            iCategoryCount += 1
            If sCategoryData <> "" Then oMovieData.sCategories = oMovieData.sCategories & sCategoryData & "|"
        Loop While sCategoryData <> ""

        ' retrieve the ACTOR list, up to five actors, and add them to an array
        ' note the structure looks like
        ' <PERSONS ...>
        '    <PERSON ...>
        '       <NAME>Bill Nussey</NAME>
        '       ...
        sPersonsXML = GetXMLTagVal(sMyMovieData, "Persons")
        sPersonData = ""
        oMovieData.iActorCount = 0
        i = 1
        Do
            sPersonData = GetXMLTagVal(sPersonsXML, "Person", i)
            sPerson = GetXMLTagVal(sPersonData, "Name", 1)

            ' only add it to the actor tag if it is of type "actor"
            If sPerson <> "" And GetXMLTagVal(sPersonData, "Type") = "Actor" Then
                oMovieData.sActors(oMovieData.iActorCount) = sPerson
                oMovieData.iActorCount += 1
            End If
            i += 1
        Loop While sPerson <> "" And oMovieData.iActorCount < MAXACTORS

        ' retrieve the DIRECTOR list, up to five directors, and add them to an array
        ' note the structure looks like
        ' <PERSONS ...>
        '    <PERSON ...>
        '       <NAME>Bill Nussey</NAME>
        '       ...
        sPersonsXML = GetXMLTagVal(sMyMovieData, "Persons")
        sPersonData = ""
        oMovieData.iDirectorCount = 0
        i = 1
        Do
            sPersonData = GetXMLTagVal(sPersonsXML, "Person", i)
            sPerson = GetXMLTagVal(sPersonData, "Name", 1)

            ' only add it to the actor tag if it is of type "director"
            If sPerson <> "" And GetXMLTagVal(sPersonData, "Type") = "Director" Then
                oMovieData.sDirectors(oMovieData.iDirectorCount) = sPerson
                oMovieData.iDirectorCount += 1
            End If
            i += 1
        Loop While sPerson <> "" And oMovieData.iDirectorCount < MAXACTORS

        ' Retrieve the ratings information
        sRatingXML = GetXMLTagVal(sMyMovieData, "PARENTALRATING")
        sRatingData = GetXMLTagVal(sRatingXML, "VALUE")
        iRating = Val(sRatingData)
        Select Case iRating
            Case 0
                oMovieData.sRating = "Unrated"
            Case 1
                oMovieData.sRating = "G"
            Case 3
                oMovieData.sRating = "PG"
            Case 4
                oMovieData.sRating = "PG-13"
            Case 6
                oMovieData.sRating = "R"
            Case Else
                oMovieData.sRating = ""
        End Select


        Return True

    End Function

    ' retrieve the specific data for an XML tag within an XML File (passed as a string)
    ' this will take into account that opening tags can have parameters not present in closing tags
    '   sXML is the XML file passed as a string
    '   sTag is the XML tag, without brackets around it
    '   index, which is optional, pulls out the nth instance of a tag, the first instance starts with a 1, not 0
    '
    ' note: using the XML parser is probably best but I don't know how to do it so I'm going to punt and just use string parsing
    Function GetXMLTagVal(ByVal sXML As String, ByVal sTag As String, Optional ByVal index As Integer = 1) As String
        Dim iopentagstart, iopentagend, iclosetagstart, ilaststart As Integer
        Dim sXMLuc As String
        Dim sTagVal As String

        ' first, make the source material all upper case so the tag comparisons will always  match regardless of case
        sXMLuc = sXML.ToUpper

        ' get the string locations for the opening and closing tag (return "" if no tag found)
        ' loop until index = 0 so we can get the nth instance of the tag
        ilaststart = 0
        sTag = sTag.ToUpper
        Do While index <> 0
            iopentagstart = InStr(ilaststart + 1, sXMLuc, "<" + sTag + ">")
            If iopentagstart = 0 Then ' no complete tag found, check for a tag with a parameter separated by a space
                iopentagstart = InStr(ilaststart + 1, sXMLuc, "<" & sTag & " ")
            End If
            ilaststart = iopentagstart
            index -= 1
        Loop
        If iopentagstart = 0 Then ' no tag of any kind found, return a blank
            Return ""
        End If

        iopentagend = InStr(iopentagstart, sXMLuc, ">")
        iclosetagstart = InStr(iopentagend, sXMLuc, "</" & sTag)

        If iclosetagstart = 0 Then Return "" ' there is no tag data - it's an empty tag
        ' pull out the specific tag content
        sTagVal = Mid(sXML, iopentagend + 1, iclosetagstart - iopentagend - 1)

        Return sTagVal
    End Function

    Function LogWrite(ByVal sLogEntry As String) As Integer

        ' write it to a file
        'FileOpen(5, sLogFileDir & "\Tagger.log", OpenMode.Append, OpenAccess.Write, OpenShare.Default)
        'PrintLine(5, Format(Now(), "yyyy-MM-dd HH:mm:ss ") & sLogEntry)
        'FileClose(5)

        Return True

    End Function
    '
    ' I created this during the version of MyMovies that was encrypting the movies.xml file. This is not very helpful at this point. 
    ' 
    ' The XML files are more useful because they contain the categories even though these property files are much easier to parse
    Function ReadMyMoviesPropertiesFile(ByVal sMovieFile As String, ByRef oMovieData As tM4VTags) As Integer
        Dim sMyPropertyFileName As String
        Dim sLine As String

        Dim sKey, sValue As String
        Dim iEqualSign As Integer

        ' first build the path/name for properties file and load it in
        sMyPropertyFileName = sMovieFile & ".properties"

        FileOpen(3, sMyPropertyFileName, OpenMode.Input, OpenAccess.Read, OpenShare.Shared) ' make sure file handle "3" isn't used elsewhere

        Do While Not EOF(3)
            sLine = LineInput(3)

            ' break into key and value pair
            iEqualSign = InStr(sLine, "=")
            sKey = UCase(Left(sLine, iEqualSign - 1))
            sValue = Mid(sLine, iEqualSign + 1, Len(sLine) - iEqualSign)

            If sKey = "DESCRIPTION" Then
                oMovieData.sDesc = sValue
            ElseIf sKey = "YEAR" Then
                oMovieData.sRelease = sValue
            ElseIf sKey = "RATED" Then
                oMovieData.sRating = sValue
            ElseIf sKey = "ACTOR" Then
                'oMovieData.sActors = sValue
            ElseIf sKey = "DIRECTOR" Then
                'oMovieData.sDirector = sValue
            End If

        Loop
        FileClose(3)

        Return True

    End Function

End Module

