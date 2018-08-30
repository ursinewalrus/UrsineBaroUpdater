using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;

namespace UrsineBaroServer
{
    class ServerStart
    {

        static void Main(string[] args)
        {
            ;
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:12345/");

                listener.Start();

                for (; ; )
                {
                    Console.WriteLine("Listening...");

                    HttpListenerContext context = listener.GetContext();
                    var Response = context.Response;
                    HttpListenerRequest request = context.Request;
                    if (context.Request.QueryString["download"] != null)
                    {
                        string qsArg = context.Request.QueryString["download"];
                        if (qsArg == "getOptions")
                        {
                            var ext = new List<string> { ".zip" };
                            var files = Directory.GetFiles(@".\").Where(f => ext.Contains(Path.GetExtension(f))).ToList();
                            string returnOptions = string.Join("|", files);
                            SendSelectableOptions(Response, returnOptions);
                        }
                        else
                        {
                            SendSelectedZip(Response, qsArg);
                        }
                    }
                }
                Console.ReadKey();
            }
        }

        private static void SendSelectableOptions(HttpListenerResponse Response, string returnOptions)
        {
            Response.StatusCode = (int)HttpStatusCode.Accepted;
            Response.StatusDescription = "OK";
            Response.SendChunked = false;
            Response.AddHeader("Content-Disposition", "text/plain");

            byte[] buffer = Encoding.UTF8.GetBytes(returnOptions);
            Response.ContentLength64 = buffer.Length;
            Stream output = Response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        private static void SendSelectedZip(HttpListenerResponse Response, string zipFile)
        {
            using (FileStream fs = File.OpenRead(zipFile))
            {
                Response.ContentLength64 = fs.Length;
                Response.SendChunked = false;
                Response.AddHeader("Content-Disposition", "attachment; filename=baro.zip");

                var fLen = fs.Length;
                byte[] buffer = new byte[fLen];
                int read;

                using (BinaryWriter bw = new BinaryWriter(Response.OutputStream))
                {
                    while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        //bw.Flush(); -> this makes no sense tome
                    }
                    bw.Close();
                }
                Response.StatusCode = (int)HttpStatusCode.Accepted;
                Response.StatusDescription = "OK";
                Response.OutputStream.Close();
            }
        }
    }
}
