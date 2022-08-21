using Backupex;
using Backupex.Configuration;

Console.WriteLine("Backupex version Alpha 1.0.\n");

try {
    ConfigManager.Instance.LoadConfig();
}
catch(FileNotFoundException) {
    Console.WriteLine("Config not found, generating new file.");
    ConfigManager.Instance.GenerateConfigTemplate();
    Console.WriteLine("Created file 'config.json', please fill it with your configuration and run it again.");
    System.Environment.Exit(0);
} 

var b2Client = new Backupex.Backblaze.Client(
    ConfigManager.Instance.Config!.AppId!,
    ConfigManager.Instance.Config!.AppKey!,
    ConfigManager.Instance.Config!.BucketId!
);

b2Client.Authorize();

long GetUnixNow() {
    return DateTimeOffset.Now.ToUnixTimeSeconds();
}

var backuper = new BackupToB2(b2Client);

var filesToBackup = new Dictionary<string, FileToBackup>();

List<FileSystemWatcher> watchers = ConfigManager.Instance.Config!.DirectoriesToWatch!.Select(dir => {
    var watcher = new FileSystemWatcher(dir.Path);

    watcher.NotifyFilter = NotifyFilters.CreationTime
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Security
                        | NotifyFilters.Size;
    watcher.Created += (sender, e) => OnCreated(e, dir.FilenamePrefix);
    watcher.Changed += OnChanged;

    watcher.Filter = ConfigManager.Instance.Config!.Filter;
    watcher.IncludeSubdirectories = true;
    watcher.EnableRaisingEvents = true;

    return watcher;
}).ToList();


void OnCreated(FileSystemEventArgs e, string filenamePrefix) {
    filesToBackup.Add(e.FullPath, new FileToBackup(e.FullPath, filenamePrefix, GetUnixNow()));
    Logger.Info("Found new backup file {1}", DateTime.Now.ToString(), e.FullPath);
}
void OnChanged(object sender, FileSystemEventArgs e) {
    if (e.ChangeType != WatcherChangeTypes.Changed)
    {
        return;
    }
    
    if (filesToBackup.ContainsKey(e.FullPath)) {
        filesToBackup[e.FullPath].LastChanged = GetUnixNow();
    }
}

while (true) {
    Thread.Sleep(10_000);
    foreach(var (k, v) in filesToBackup) {
        if((GetUnixNow() - v.LastChanged) > 60) {
            backuper.SendBackup(v.FullPath!, v.FilenamePrefix! + Path.GetFileName(v.FullPath!));
            filesToBackup.Remove(k);
        }
    }
}