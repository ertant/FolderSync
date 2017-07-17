
# FolderSync

Folder synchronisation as command line tool and msbuild task.

## Command Line

    FolderSync <sourceFolder> <destinationFolder> [/mirror] [/xf <excludeFiles>] [/xd <excludeDirs>] 

### Parameters

	sourceFolder
		Path to source folder.
	destinationFolder
		Path to destination folder.
    /mirror
	    Mirror the directory tree. Removes files on target folder if not exists in source.		
    /xf
	    List of file names to exclude from copy.
    /xd
	    List of folder names to exclude from copy.

### Example

    FolderSync myFolder toFolder /mirror /xf *.csproj;web.config /xd obj;config

Command line can be executed as below for Mac OS.

    mono FolderSync.exe myFolder toFolder

## MSBuild Task

```
    <UsingTask TaskName="SyncTask" AssemblyFile="Tools\Build\FolderSync.exe" />
    <SyncTask 
        SourceFolder="MyWebApp" 
        DestinationFolder="$(OutputDir)\$(AppName)" 
        ExcludeFolders="obj;configuration"
        ExcludeFiles="*.csproj;*.user;*.debug.config;*.release.config"
        Mirror="true" />
```        
   
# Notes

* Excluded file and folder names performed as case insensitive.

## Release History

 - 1.0.0 Initial release

## License

Written by Ertan Tike

Licensed under the Apache 2.0 license.