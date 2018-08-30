using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace UrsineBaroUpdater
{
    class Updater
    {
        static void Main(string[] args)
        {
            foreach (string d in Directory.GetDirectories(@".\"))
            {
                if (File.Exists(d+".zip"))
                {
                    File.Delete(d + ".zip");
                }
                ZipFile.CreateFromDirectory(d, d+".zip");
            }
        }
    }
}
