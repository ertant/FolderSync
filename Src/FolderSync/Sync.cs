using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FolderSync
{
    public class Settings
    {
        public string SourceFolder { get; set; }

        public string DestinationFolder { get; set; }

        public string[] ExcludeFolders { get; set; } = { };

        public string[] ExcludeFiles { get; set; } = { };

        public bool Mirror { get; set; }

        internal Regex[] excludeFiles;

        internal Regex[] excludeFolders;

        static Regex ToRegex(string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }

        public void Compile()
        {
            this.excludeFiles = this.ExcludeFiles.Select(ToRegex).ToArray();
            this.excludeFolders = this.ExcludeFolders.Select(ToRegex).ToArray();
        }

        public bool Parse(string[] args)
        {
            if (args.Length < 2)
            {
                return false;
            }

            this.SourceFolder = args[0];
            this.DestinationFolder = args[1];

            if (args.Length > 2)
            {
                for (var i = 2; i < args.Length; i += 2)
                {
                    if (args.Length < i + 1)
                        return false;

                    if (string.Equals(args[i], "/xf", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ExcludeFiles = args[i + 1].Split(';');
                    }
                    else if (string.Equals(args[i], "/xd", StringComparison.OrdinalIgnoreCase))
                    {
                        this.ExcludeFolders = args[i + 1].Split(';');
                    }
                    else if (string.Equals(args[i], "/mirror", StringComparison.OrdinalIgnoreCase))
                    {
                        this.Mirror = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class Sync
    {
        public void Perform(Settings settings)
        {
            settings.Compile();

            if (!Directory.Exists(settings.SourceFolder))
            {
                Console.WriteLine($"Source folder {settings.SourceFolder} not found.");

                return;
            }

            var op = new FolderOperation
            {
                Source = Path.GetFullPath(settings.SourceFolder),
                Target = Path.GetFullPath(settings.DestinationFolder)
            };

            op.Perform(settings).Wait();
        }
    }

    interface IOperation
    {
        Task Perform(Settings settings);
    }

    class FolderOperation : IOperation
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public IEnumerable<IOperation> Feed(Settings settings)
        {
            foreach (var operation in this.CopyFiles(settings))
            {
                yield return operation;
            }

            foreach (var operation in this.CopyFolders(settings))
            {
                yield return operation;
            }
        }

        public async Task Perform(Settings settings)
        {
            if (!Directory.Exists(this.Target))
            {
                Console.WriteLine($"+{this.Target}");
                Directory.CreateDirectory(this.Target);
            }

            var tasks = this.Feed(settings).Select(x => x.Perform(settings));

            await Task.WhenAll(tasks);
        }

        private IDictionary<string, string> ToRelative(IEnumerable<string> items, Regex[] excludes)
        {
            return items
                .Where(x => !excludes.Any(r => r.IsMatch(Path.GetFileName(x))))
                .ToDictionary(Path.GetFileName, x => x);
        }

        private IEnumerable<IOperation> CopyFolders(Settings settings)
        {
            if (Directory.Exists(this.Source))
            {
                var sourceFolders = ToRelative(Directory.EnumerateDirectories(this.Source), settings.excludeFolders);
                var targetFolders = ToRelative(Directory.EnumerateDirectories(this.Target), settings.excludeFolders);

                foreach (var source in sourceFolders)
                {
                    var targetFullPath = Path.Combine(this.Target, source.Key);

                    yield return new FolderOperation
                    {
                        Source = source.Value,
                        Target = targetFullPath
                    };

                    targetFolders.Remove(source.Key);
                }

                if (settings.Mirror)
                {
                    foreach (var obsoleteFolderPath in targetFolders.Values)
                    {
                        yield return new FolderDelete
                        {
                            Path = obsoleteFolderPath
                        };
                    }
                }
            }
        }

        private IEnumerable<IOperation> CopyFiles(Settings settings)
        {
            var sourceFiles = ToRelative(Directory.EnumerateFiles(this.Source), settings.excludeFiles);
            var targetFiles = ToRelative(Directory.EnumerateFiles(this.Target), settings.excludeFiles);

            foreach (var source in sourceFiles)
            {
                var targetFullPath = Path.Combine(this.Target, source.Key);

                if (!targetFiles.ContainsKey(source.Key) || File.GetLastWriteTime(source.Value) > File.GetLastWriteTime(targetFullPath))
                {
                    yield return new FileCopy
                    {
                        Source = source.Value,
                        Target = targetFullPath
                    };
                }

                targetFiles.Remove(source.Key);
            }

            if (settings.Mirror)
            {
                foreach (var obsoleteFile in targetFiles.Values)
                {
                    yield return new FileDelete
                    {
                        Path = obsoleteFile
                    };
                }
            }
        }
    }

    class FileCopy : IOperation
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public async Task Perform(Settings settings)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"={this.Source} -> {this.Target}");

                File.Copy(this.Source, this.Target, true);
            });
        }
    }

    class FileDelete : IOperation
    {
        public string Path { get; set; }

        public async Task Perform(Settings settings)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"-{this.Path}");

                File.Delete(this.Path);
            });
        }
    }

    class FolderDelete : IOperation
    {
        public string Path { get; set; }

        public async Task Perform(Settings settings)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"-{this.Path}");

                Directory.Delete(this.Path, true);
            });
        }
    }
}