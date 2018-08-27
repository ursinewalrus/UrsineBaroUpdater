using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace UrsineBaroUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = GatherFilesInDir(@".\fakeBaro");
            var uploads = UploadFiles(files);
        }

        static async Task UploadFiles(List<string> files)
        {
            using (var dbx = new DropboxClient(ConfigurationManager.AppSettings["accessToken"]))
            {
                foreach(var file in files)
                {
                    var memory = new MemoryStream();
                    var fileOpen = File.Open(file, FileMode.Open);
                    fileOpen.CopyTo(memory);
                    using (memory)
                    {
                        var dirs = file.Split(new[] { @"\" }, StringSplitOptions.None).Skip(1);
                        var fileName = dirs.Last();
                        var dirString = String.Join(@"/", dirs.Take(dirs.Count() - 1).ToList());

                        var finalPath = "/" + dirString + "/" + fileName;
                        ;
                        var updated = await dbx.Files.UploadAsync(
                                finalPath,
                                WriteMode.Overwrite.Instance,
                                body: memory
                            );
                        Console.WriteLine(updated.Rev);
                        ;
                    }
                }
            }
        }

        public static List<string> GatherFilesInDir(string dir)
        {
            List<string> filesInDir = new List<string>();
            foreach (string d in Directory.GetDirectories(dir))
            {
                filesInDir.AddRange(GatherFilesInDir(d));
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                filesInDir.Add(file);
            }
            return filesInDir;
        }
    }
}
