using Microsoft.Build.Framework;

namespace FolderSync
{
    public class SyncTask : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string SourceFolder { get; set; }

        [Required]
        public string DestinationFolder { get; set; }

        public bool Mirror { get; set; }

        public string ExcludeFolders { get; set; }

        public string ExcludeFiles { get; set; }

        public override bool Execute()
        {
            var settings = new Settings
            {
                SourceFolder = this.SourceFolder,
                DestinationFolder = this.DestinationFolder,
                ExcludeFiles = (this.ExcludeFiles ?? "").Split(';'),
                ExcludeFolders = (this.ExcludeFolders ?? "").Split(';'),
                Mirror = this.Mirror,
                Log = this.Log
            };

            this.Log.LogMessage(MessageImportance.Normal, $"Sync Folders {settings.SourceFolder} to {settings.DestinationFolder}");

            settings.Log?.LogMessage(MessageImportance.Normal, $"Ignoring folders {string.Join(";", settings.ExcludeFolders)}");
            settings.Log?.LogMessage(MessageImportance.Normal, $"Ignoring files {string.Join(";", settings.ExcludeFiles)}");

            var sync = new Sync();

            sync.Perform(settings);

            return true;
        }
    }
}