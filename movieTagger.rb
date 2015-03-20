require 'rubygems'
require 'JSON'
require 'RestClient'
require 'Date'
require 'open-uri'

#DIRECTORS VS ACTORS FIELD?!?!?!

$apiKey = "4fa3511c0531a0ee30a0bb4ace6c485f"

#List of tags that must be present in the tag file before we can begin
$requiredTags = ["tagged","id","genre","composer","title"]
#List of tags that MAY be present in the tag file, otherwise just download them
$optionalTags = ["description","artist","year","contentRating","directors"]
#File address of the Atomic Parsley on the system
$atomicParsley = "/Users/Alex/Developer/ruby/movieTagger/atomicParsley/AtomicParsley"
#Define a constant to later hold the base URL for accessing cover art
$imageBase
#How many actors do you want to include in the tag
$numActors = 3
#This defiines the type of media in iTunes. There is a mismatch between the
#current atomic parsley and iTunes, so we use Short Film rather than movie
$stik = "Short Film"

#List of characters which needs to have escape characters in the final command
#Line calls
$eChars = ["(",")"," ","|"]

#Get information about a movie from The Movie DB using its ID
def fetchMovieData(movieID)
    puts "HITTING THE NET"
    #Construct the URL used to access the API
    url = "http://api.themoviedb.org/3/movie/#{movieID}?api_key=#{$apiKey}"
    #Preform a restful request to get the information
    #Uses the RestClient gem to preform the request
    data = RestClient.get(url)
    #Parse the JSON to get a simple hash (key value pairs) of info
    #Uses the JSON gem to avoid manual parsing
    data = JSON.parse(data)

    #Reformat some data to make it easier to work with later
    data["description"] = data["overview"][0..249]
    
    #Get the information that was not included in the first API call
    #Rating and date info
    ratingAndDate = fetchMovieRatingAndYear(movieID)
    #Add this data to the hash before returning it
    data["contentRating"] = ratingAndDate["certification"]
    data["year"] = Date.parse(ratingAndDate["release_date"]).year.to_s

    #Fetch the movie directors and actors and add them to the hash
    data["artist"] = fetchMovieCast(movieID)
    data["directors"] = fetchMovieDirectors(movieID)
    return data
end

#Gets the rating and year of publication of the movie based on its ID
def fetchMovieRatingAndYear(movieID)
    #Construct the URL used to access the API
    url = "http://api.themoviedb.org/3/movie/#{movieID}/releases?api_key=#{$apiKey}"
    #RESTful request
    data = JSON.parse(RestClient.get(url))
    #Pull out the list of releases from the parsed request
    countries = data["countries"]
    #Get just the US release (including date and rating info) and return it
    us = countries.select{ |country| country["iso_3166_1"]=="US"}
    return us[0]
end

#Get the cast of the movie based on its ID
def fetchMovieCast(movieID)
    #Construct the URL used to access the API
    url = "http://api.themoviedb.org/3/movie/#{movieID}/credits?api_key=#{$apiKey}"
    #RESTful request
    data = JSON.parse(RestClient.get(url))
    #Extract the cast data from the returned results
    castData = data["cast"]
    #Remove everything except the names of the actors, maintaining the order
    #This line is converting an array of hashes into an array of strings
    #by mapping the values that correspond to the key "name" for each hash in 
    #the array to the output array
    castNames = castData.map{ |castMember| castMember["name"]}
    #Grab just the first three of the actors and then join them on the string ", "
    return castNames[0..$numActors-1].join(", ")
end

def fetchMovieDirectors(movieID)
    #Construct the URL used to access the API
    url = "http://api.themoviedb.org/3/movie/#{movieID}/credits?api_key=#{$apiKey}"
    #RESTful request
    data = JSON.parse(RestClient.get(url))
    #Extract the crew data from the returned results
    crewData = data["crew"]
    directors = crewData.select{ |crewMember| crewMember["job"] == "Director"}
    directorNames = directors.map {|director| director["name"]}
    return directorNames.join(", ")
end

#Get the API server configuration information
def fetchServerConfig()
    #Construct the URL for the request
    url = "http://api.themoviedb.org/3/configuration?api_key=#{$apiKey}"
    #Preform a restful reqeust for the data
    data = RestClient.get(url)
    #Parse the returned JSON and return it as a hash
    return JSON.parse(data)
end

#Extracts the resolution of a file from its name
def stripResolution(nameOfFile)
    #Split the string on the open paren
    resolution = nameOfFile.split("(").last
    #Remove the close paren, leaving just the resolution
    return resolution.tr(")","")
end

#Adds escape characters at relevant positions in a string
def esc(inString)
    #For each of the characters in the list of characters to be escaped
    $eChars.each do |thisChar|
        #Replace every instance of thisChar with thisChar appended to an
        #escape character. We have to escape the escape to use it
        inString = inString.gsub(thisChar, "\\"+thisChar)
    end
    return inString
end

#Configure the URL paths to get images off the MovieDB Server
def configureImages()
    #Fetch the server configuration
    config = fetchServerConfig()
    #Used the gathered configuration information to build a URL for accessing images
    $imageBase = config['images']['base_url']+config['images']['poster_sizes'][5]
end

def buildStupidXML(actors,directors)
    #actors = fetchMovieCast(movieID).split(", ")
    #directors = fetchMovieDirectors(movieID).split(", ")
    xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
    xml += "<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\""
    xml += " \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n"
    xml += "<plist version=\"1.0\">\n"
    xml += "<dict>\n"
    xml += "\t<key>cast</key>\n"
    xml += "\t<array>\n"
    
    actors.each do |actor|
        xml+="\t\t<dict>\n"
        xml+="\t\t\t<key>name</key>\n"
        xml+="\t\t\t<string>#{actor}</string>\n"
        xml+="\t\t</dict>\n"
    end

    xml += "\t</array>\n"

    xml += "\t<key>directors</key>\n"
    xml += "\t<array>\n"
    
    directors.each do |director|
        xml+="\t\t<dict>\n"
        xml+="\t\t\t<key>name</key>\n"
        xml+="\t\t\t<string>#{director}</string>\n"
        xml+="\t\t</dict>\n"
    end

    xml += "\t</array>\n"

    xml += "</dict>\n"
    xml += "</plist>\n"

    #I was kind of expecting to see a </xml> tag here, but there is not one in the source you gave me, and I am far from an XML expert, so clearly it is not needed
end


#----------------------------------------------------------------------
#----------------------------------------------------------------------

#Start with the actual code

#Make sure that AtomicParsley exists at the specified location
if(File.exists?($atomicParsley))
    puts "Atomic Parlsey exe found! Ready to go"
else
    puts "No Atomic Parsley exe found! aborting"
    abort
end

#Grab current image grabbing configuration, store it in global variables for later use
configureImages()
Dir.glob("/Users/Alex/Desktop/**/*.{tag}") do |tagPath|
    puts "------------------------------"
    #Let the user know that we found a Tag File
    puts "Found tag file: #{tagPath}"
    #Create a new hash to hold all of the data we load from the file
    tagData = Hash.new
    #Open the file that we are currently working on
    tagFile = File.new(tagPath)
    #Loop through each line of the file
    while(line = tagFile.gets)
        #Get rid of the pesky new line characters
        line.delete!("\n")
        if(!line.empty?)
            #Split the line on the equals
            line = line.split("=")
            #Make the left half the key, and the right half the value in the hash
            tagData[line[0]] = line[1]
        end
    end
    #Create a default case
    skip = false
    #Loop through the list of tags that must be present in each tag file
    $requiredTags.each do |tag|
        #If the tag is not present in the loaded data
        if(!tagData.has_key?(tag))
            #Let the user know
            puts "ERROR: tag file #{tagPath} does not contain the required tag #{tag}; skipping it"
            #Prepare to skip this tag file
            skip = true
        end
    end

    if(tagData["tagged"] == "true")
        puts "No update needed! Skipping"
        skip=true;
    end


    #If any tags were found to be missing, skip over this tag file
    if(skip)
        next
    end

    #If everyting is valid and ready to go, we are going to fetch the data
    #About the movie form The Movie DB online
    newData = fetchMovieData(tagData["id"])

    #Iterate through each of the optional tags in the list
    $optionalTags.each do |tag|
        #If it did not exist in the loaded tag file
        if(!tagData.has_key?(tag))
            #Then use the data we got from TMDB
            tagData[tag] = newData[tag]
        end
    end

    #Time to handle the artwork for the movie
    #If the loaded data did not already contain a path to a desired cover image
    if(!tagData.has_key?("artwork"))
        #Generate paths to two possible images. 
        #One with the name of the tag file
        #And the other with the standard "folder.jpg"
        fileName = File.basename(tagPath,".tag")
        namedPath = File.dirname(tagPath)+"/"+fileName+".jpg"
        defaultPath = File.dirname(tagPath)+"/folder.jpg"
        #If either file exists, use that path
        if(File.exists?(namedPath))
            tagData["artwork"] = namedPath
        elsif(File.exists?(defaultPath))
            tagData["artwork"] = defaultPath
        else
            #If we are unable to find images at either of the default locations
            #We are going to have to download it

            #Create a file in the same directory as the tag file 
            #with the same name
            File.open(namedPath, 'wb') do |fo|
                fo.write open($imageBase+newData["poster_path"]).read 
            end

            puts "Downoading image from online"
        end
    #If there is a defined path to an image, and it doesnt exist,    
    elsif(!File.exists?(tagData["artwork"]))
        #let the user know and skip this tag file
        puts("ERROR: Cannot find tagged artwork file; skipping")
        next
    end

    #Handle setting the media type operator
    #If the media type does not already exist in the tag file
    if(!tagData.has_key?("stik"))
        tagData["stik"] = $stik
    end
    

    #The tag data is now prepared, time to find any correspoinding movies
    Dir.glob(File.dirname(tagPath)+"/*.{mp4}") do |moviePath|

        #Create strings that are just the names of the movie and tag files
        movieName = File.basename(moviePath,".mp4")
        tagFileName = File.basename(tagPath,".tag")

        #If the name of the tag file appears within hte name of the movie file
        if(movieName.include?(tagFileName))
            puts "Found movie file: #{movieName}"
            
            #Create the XML data that will be injected for Apple TV
            xml = buildStupidXML(tagData["actors"],tagData["directors"])

            #Remove the key (and its value) "id" from the hash because it does
            #not need to be passed to atomic parsley. Also, clone the hash
            #so that the modifications don't effec other movie files
            #Do the same with the "tagged" and "directors" tags
            thisTagData = tagData.clone.tap { |hs| hs.delete("id"); 
                hs.delete("tagged"); hs.delete("directors")}

            #Get the resolution of the movie file
            resolution = stripResolution(movieName)

            #If the movie is HD
            if(resolution=="1080"||resolution=="720")
                #Set the tag for HD to true, and add the HD lable to the
                #composer field
                thisTagData["hdvideo"] = "true"
                thisTagData["composer"] += "HD|"
            else
                thisTagData["hdvideo"] = "false"
            end

            #Create a string to hold the command we are going to execute
            cmd = esc{$atomicParsley)+" "+esc(moviePath)+" --overWrite"

            #Loop thorugh each one of the keys in the tag file and add it
            #to the command we are going to execute
            thisTagData.each do |k,v|
                cmd+=(" --"+k+" "+esc(v))
            end

            #For some reason, this has to be the last argument
            cmd+=(" --rDNSatom "+esc(xml)+" name=iTunMOVI domain=com.apple.iTunes")

            puts "Tagger ready, executing on current movie file"

            #Execute the command!
            system cmd 
            puts "Done!"
        end

    end

    #We are all done tagging, change the tagged attribute
    tagData["tagged"] = "true"

    #Create a string to hold the data we will write back to the file
    finalData = ""

    #Add each tag on to the string
    tagData.each do |k,v|
        finalData+=(k+"="+v+"\n")
    end

    #Write the data back to the text file, erasing anyting that was there before
    File.open(tagPath,'w') { |file| file.write(finalData)}
end
