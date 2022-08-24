# Backupex - Simple file watcher and uploader.
Lemme explain, you see there's Minecraft's mod **FTBUtilities** or **FTBBackups**.
This mod is creating backup of my minecraft server world and place it in Backups folder.

That's good but if my server would catch fire and burn to the ground so would backups do.

So I created this little dotnet program to listen for new backups, wait 'til for last 60 seconds there was no changes to the file and then upload it to Backblaze. So simple.

# How to use it
You can download for Windows and Linux binaries from releases.  
Linux binary is self-contained that's why it's so big.

You can compile it from sources, clone this repo and run
```sh
dotnet publish
```
to build debug release or
```sh
./build.sh
```
It will build release binaries for linux and windows and put them into `build` directory.

Then run application, it will generate you following config file:
```json
{
  "appId": "<put your backblaze application id here>",
  "appKey": "<put your backblaze application key here>",
  "bucketId": "<put your backblaze bucket id here>",
  "filter": "*",
  "directoriesToWatch": [
    {
      "path": "/path/to/directory",
      "filenamePrefix": "Filename/prefix"
    }
  ]
}
```
Fill it and run again. It will listen to directory list and backup every file created in those directories.

You can set filter to filter files, for example `*.zip` or `backup.zip`.

