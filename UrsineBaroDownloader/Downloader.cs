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

namespace UrsineBaroDownloader
{

    class Downloader
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://localhost:12345/?download=1");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var client = new HttpClient(new HttpClientHandler {
                                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("http://localhost:12345");
                HttpResponseMessage response = client.GetAsync("?download=1").Result;
                var responseBytes = response.Content.ReadAsByteArrayAsync().Result;
                FileStream zipFile = new FileStream(@".\downloadedZip.zip", FileMode.Create);
                zipFile.Write(responseBytes, 0, responseBytes.Length);
                zipFile.Close();
                ZipFile.ExtractToDirectory(@".\downloadedZip.zip",@".")
                ;

            }
        }
    }
}
