# AtomicParsley Shield Fork

This is a fork of atomic parsley to add some new features and handle new atoms added to the MP4 format by iTunes. For uptodate information vist my [blog] [9]. Belowe is a list of features added to this fork.

## Features

Version 0.9.6  
  - CMake Build scripts.  
  - Option to output MP4 atoms as XML.  
  - Added support for 1080p iTunes atoms (--hdvideo option also takes a number)  
  - Added --flavor option for setting flvr atom.  
  - Allow sort order fields to be set in the format of --sortOrder name=value   
  - Added missing sort albumn option  
  - Added option for setting plID atom  

## Building Instructions

Building the sources can be done on windows, linux and Mac OSX. You will need 
cmake (www.cmake.org) on these platforms to perform the build:

      % cmake
      % make

Use the program in situ or place it somewhere in your $PATH by using:

      % sudo make install

### Dependencies:

zlib  - used to compress ID3 frames & expand already compressed frames
        available from http://www.zlib.net

## Stable Binaries

Version 0.9.5 binaries for different platforms can be found at these locations:

  - [Linux static x86-64] [5]  
  - [Linux static x86-32] [6]  
  - [Windows static x86-32] [7]  
  - [Mac OSX x86-64] [8]  

## Prebuilt Development Binaries

There are pre-built binaries for different avaliable. The binaries are uploaded everytime
their is a change to the source code. The binaries for various platforms can be downloaded from
the following locations:

  - [Linux static x86-64] [1]  
  - [Linux static x86-32] [2]  
  - [Windows static x86-32] [3]  
  - [Mac OSX x86-64] [4]  

[1]: http://dl.dropbox.com/u/33732973/AtomicParsley/build_x86_64/AtomicParsley
[2]: http://dl.dropbox.com/u/33732973/AtomicParsley/build_x86_32/AtomicParsley
[3]: http://dl.dropbox.com/u/33732973/AtomicParsley/build_win32/AtomicParsley.exe
[4]: http://dl.dropbox.com/u/33732973/AtomicParsley/build_mac_x86_64/AtomicParsley)
[5]: https://bitbucket.org/shield007/atomicparsley/src/68337c0c05ec/downloads/build_linux_x86_64/AtomicParsley
[6]: https://bitbucket.org/shield007/atomicparsley/src/68337c0c05ec/downloads/build_linux_x86_32/AtomicParsley
[7]: https://bitbucket.org/shield007/atomicparsley/src/68337c0c05ec/downloads/build_win32/AtomicParsley.exe
[8]: https://bitbucket.org/shield007/atomicparsley/src/68337c0c05ec/downloads/build_mac_x86_64/AtomicParsley
[9]: http://shield008.blogspot.com/search/label/AtomicParsley
