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
        private static string baseAddress = "http://18.208.175.131/api/baro/";
        [STAThread]
        static void Main(string[] args)
        {
            GetUpdateOptions();
        }

        private static void GetUpdateOptions()
        {
            using (var client = new HttpClient(new HttpClientHandler { }))
            {
                try
                {
                    client.BaseAddress = new Uri(baseAddress);
                    HttpResponseMessage response = client.GetAsync("download").Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    List<string> possibleDownloads = responseString.Split('|').ToList();
                    Application.EnableVisualStyles();
                    var selectionForm = new ZipSelectForm(possibleDownloads);
                    Application.Run(new ZipSelectForm(possibleDownloads));
                }
                catch(Exception e)
                {
                    Application.Run(new ShowError(e.Message));
                }
            }
        }

        public static void GetSelectedOption(string zipFile)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                }))
                {
                    client.BaseAddress = new Uri(baseAddress);
                    HttpResponseMessage response = client.GetAsync("downloadoption?download=" + zipFile).Result;
                    var responseBytes = response.Content.ReadAsByteArrayAsync().Result;
                    FileStream downloadedZip = new FileStream(@".\tempFile.zip", FileMode.Create);
                    downloadedZip.Write(responseBytes, 0, responseBytes.Length);
                    downloadedZip.Close();

                    using (ZipArchive archive = ZipFile.OpenRead(@".\tempFile.zip"))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.Name == "")
                                continue;
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
            catch(Exception e)
            {
                Application.Run(new ShowError(e.Message));
            }
        }

    }

    public class ZipSelectForm : Form
    {
        public List<string> FormOptions;
        public ZipSelectForm(List<string> zipFiles)
        {
            zipFiles = zipFiles.Select(s => s.Substring(1, s.Length - 2)).ToList();
            FormOptions = zipFiles;
            int yLoc = 0;
            foreach(string option in FormOptions)
            {
                var button = new Button();
                button.Size = new Size(200, 60);
                button.Location = new Point(30, 30 + yLoc);
                button.Text = option;   
                this.Controls.Add(button);
                button.Click += new EventHandler(OptionSelect);
                yLoc += 90;
            }
        }

        public static void OptionSelect(object sender, EventArgs e)
        {
            Downloader.GetSelectedOption((sender as Button).Text);
        }

    }

    public class ShowError : Form
    {
        public ShowError(string ErrorMsg)
        {
            var textDisplay = new TextBox();
            textDisplay.Text = ErrorMsg;
        }
    }
}
