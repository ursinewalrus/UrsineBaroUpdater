using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;


namespace UrsineBaroDownloader
{

    class Downloader
    {
        public static string baseAddress = "http://18.208.175.131/api/baro/";
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
                    Application.Run(selectionForm);
                }
                catch(Exception e)
                {
                    Application.Run(new ShowError(e.Message));
                }
            }
        }
    }

    public class ZipSelectForm : Form
    {
        public List<string> FormOptions;
        public BackgroundWorker ProgressWorker = new BackgroundWorker();
        public ProgressBar ProgressBar = new ProgressBar();
        public Label ProgressText = new Label();
        public Label DownloadingText = new Label();

        public ZipSelectForm(List<string> zipFiles)
        {
            DownloadingText.Text = "Downloading Files";
            DownloadingText.Location = new Point(30, 10);
            DownloadingText.Size = new Size(200, 20);

            zipFiles = zipFiles.Select(s => s.Substring(1, s.Length - 2)).ToList();
            FormOptions = zipFiles;

            ProgressWorker.DoWork += (se, eventArgs) => { ExtractFiles(); };

            ProgressText.Location = new Point(30, 50);
            ProgressText.Size = new Size(200, 20);
            ProgressText.Text = "Extract Files Progress";
            this.Controls.Add(ProgressText);

            ProgressBar.Location = new Point(30, 70);
            ProgressBar.Size = new Size(200, 20);
            this.Controls.Add(ProgressBar);

            int yLoc = 70;
            foreach (string option in FormOptions)
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
        public void ProgressUpdater(object sender, ProgressChangedEventArgs e) {
            ProgressBar.Maximum = 100;
            ProgressBar.Minimum = 0;
            ProgressBar.Value = e.ProgressPercentage;
        }

        public void GetSelectedOption(string zipFile)
        {
            try
            {
                using (var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                }))
                {
                    client.BaseAddress = new Uri(Downloader.baseAddress);

                    this.Controls.Add(DownloadingText);
                    HttpResponseMessage response = client.GetAsync("downloadoption?download=" + zipFile).Result;
                    DownloadingText.Text = "Downloaded!";
                    this.Controls.Remove(DownloadingText);

                    var responseBytes = response.Content.ReadAsByteArrayAsync().Result;
                    FileStream downloadedZip = new FileStream(@".\tempFile.zip", FileMode.Create);
                    downloadedZip.Write(responseBytes, 0, responseBytes.Length);
                    downloadedZip.Close();

                    ExtractFiles();
                    ProgressText.Text = "Extraction Complete!";
                }

            }
            catch (Exception e)
            {
                Application.Run(new ShowError(e.Message));
            }
        }


        private void ExtractFiles()
        {
            ProgressWorker.WorkerReportsProgress = true;
            ProgressWorker.ProgressChanged += ProgressUpdater;

            using (ZipArchive archive = ZipFile.OpenRead(@".\tempFile.zip"))
            {
                int totalSize = archive.Entries.ToList().Sum(f => (int)f.Length);
                double totalDone = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    int percentage = (int)(((totalDone += entry.Length) / totalSize) * 100);

                    ProgressWorker.ReportProgress(percentage, "Working");

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

        public void OptionSelect(object sender, EventArgs e)
        {
            GetSelectedOption((sender as Button).Text);
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
