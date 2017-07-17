using System;
using System.Collections.Generic;

namespace FolderSync
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var settings = new Settings();

            if (settings.Parse(args))
            {
                var sync = new Sync();

                sync.Perform(settings);
            }
            else
            {
                Console.WriteLine(@"
Usage: FolderSync <sourceFolder> <destinationFolder> [/xf <excludeFiles>] [/xd <excludeDirs>]

Example: FolderSync myFolder toFolder /xf *.csproj /xd obj
");
            }
        }
    }
}