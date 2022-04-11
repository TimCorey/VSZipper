# VSZipper App
This application zips up Visual Studio Projects and Solutions without unnecessary files/folders like the bin and obj directories.

## Installation Instructions
You will need to build the project to create an exe at this point. In the project directory, run this command at the command line:

    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true --self-contained true

Take the generated exe and place it in the directory where you want the Visual Studio solution zipped up. Double-click the file to execute it.

## Current Features
 * Zips up a directory and all sub-directories
 * Excludes hidden folders and files
 * Excludes files and folders I specify (bin/obj/etc.)

## Roadmap
 * Automated build of the self-contained exe file on commit.
 * Use of a new zip file name if the current one already exists.
 * Make use of the .gitignore file to exclude files and folders.

## Possible Ideas
 * Allow for rename of zip file during creation.
 * Show up on the right-click menu.
 * Visual Studio extension
