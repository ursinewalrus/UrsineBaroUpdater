using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace UrsineBaroDownloader
{

    class Downloader
    {
        [STAThread]
        static void Main(string[] args)
        {
            GetUpdateOptions();
        }

        private static void GetUpdateOptions()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://localhost:12345/");

            using (var client = new HttpClient(new HttpClientHandler { }))
            {
                client.BaseAddress = new Uri("http://localhost:12345");
                HttpResponseMessage response = client.GetAsync("?download=getOptions").Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                List<string> possibleDownloads = responseString.Split('|').ToList();
                Application.EnableVisualStyles();
                Application.Run(new ZipSelectForm(possibleDownloads));
            }
        }

        public static void GetSelectedOption(string zipFile)
        {
            using (var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            }))
            {
                client.BaseAddress = new Uri("http://localhost:12345");
                HttpResponseMessage response = client.GetAsync("?download="+zipFile).Result;
                var responseBytes = response.Content.ReadAsByteArrayAsync().Result;
                FileStream downloadedZip = new FileStream(@".\tempFile.zip", FileMode.Create);
                downloadedZip.Write(responseBytes, 0, responseBytes.Length);
                downloadedZip.Close();

                using (ZipArchive archive = ZipFile.OpenRead(@".\tempFile.zip"))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        var splitPath = entry.FullName.Split('/');
                        var fileDir = string.Join("/", splitPath.Take(splitPath.Length - 1).ToList());
                        if (!Directory.Exists(fileDir) && fileDir != "")
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        fileDir = @".\" + fileDir + @"\";
                        var extractPath = fileDir + entry.Name;
                        entry.ExtractToFile(extractPath, true);
                    }
                }
                File.Delete(@".\tempFile.zip");
            }
        }

    }

    public class ZipSelectForm : Form
    {
        public List<string> FormOptions;
        public List<Button> Options;
        public static string SelectedOption;
        public ZipSelectForm(List<string> zipFiles)
        {
            FormOptions = zipFiles;
            int yLoc = 0;
            foreach(string option in FormOptions)
            {
                var button = new Button();
                button.Size = new Size(200, 60);
                button.Location = new Point(30, 30 + yLoc);
                button.Text = option;   
                this.Controls.Add(button);
                SelectedOption = option;
                button.Click += new EventHandler(OptionSelect);
                //Options.Add(button);
                yLoc += 90;
            }
        }

        public static void OptionSelect(object sender, EventArgs e)
        {
            Downloader.GetSelectedOption((sender as Button).Text);
        }

    }
}
