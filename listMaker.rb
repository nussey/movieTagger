#If no arguments are given, or if a question mark is passed as the first
#argument, then display the help information
#if ARGV.empty? || ARGV[0] == "?"
    puts "Welcome to the super awesome list generator!"
    puts "Pass arguments by calling ruby /path/to/script \"arg1\" \"arg2\""
    puts "Argument 1: "
    #abort
#end


#Open up a new file to write the list to
output = File.open("/Users/Alex/Desktop/out.txt", 'w')

puts ARGV[0]
puts ARGV[1]

#Iterate through each file with the extension .m4v in the given directory
#In each iteration, the string path of the file will be assigned to moviePath
Dir.glob("/Volumes/Videos/Movies/**/*.{m4v}") do |moviePath|
    #Get just the directory of the movie in question
    directory = File.dirname(moviePath)
    #Get the path to the xml file associated with the movie
    xmlPath = directory+"/movie.xml"
    #Get just the name of the actual movie file
    movieName = File.basename(moviePath)
    #If the XML file doesn't exist (they don't all for some reason)
    if !File.exist?(xmlPath)
        #Write a quaint little ouptut message to the command line alerting the
        #User of the application that the file is missing
        puts "No XML data exists for "+movieName
    #If the file does exist, find out if it is newer than the actual movie
    #File.mtime(filePath) returns a Time object with the time and date of the
    #Last edit to the file
    elsif File.mtime(xmlPath) > File.mtime(moviePath)
        #Print that the tags for that movie need to be updated
        #This was mainly for debugging, it can easily be removed
        puts "UPDATE TAGS: "+moviePath
        #Write the path to the file to the output text file
        output.puts(moviePath+"\n")
    end

    #If the file name of the movie starts with raw
    if movieName.slice(0..2) == "raw"
        #Output that this is a newly encoded movie. Also not needed
        puts "RAW MOVIE: "+moviePath
        #Output the file path to the text file
        output.puts(moviePath+"\n")
    end
end

#It is good practice to close the open text file
output.close
