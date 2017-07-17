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
            this.Log.LogMessage(MessageImportance.Normal, $"Copying {this.SourceFolder} to {this.DestinationFolder}");
            
            var settings = new Settings
            {
                SourceFolder = this.SourceFolder,
                DestinationFolder = this.DestinationFolder,
                ExcludeFiles = (this.ExcludeFiles ?? "").Split(';'),
                ExcludeFolders = (this.ExcludeFolders ?? "").Split(';'),
                Mirror = this.Mirror
            };

            var sync = new Sync();

            sync.Perform(settings);

            return true;
        }
    }
}