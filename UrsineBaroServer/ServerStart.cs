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
                    //https://stackoverflow.com/questions/13385633/serving-large-files-with-c-sharp-httplistener meebs
                    Console.WriteLine("Listening...");

                    HttpListenerContext context = listener.GetContext();
                    var Response = context.Response;
                    HttpListenerRequest request = context.Request;
                    if (context.Request.QueryString["download"] == "1")
                    {
                        var file = @".\fakeBaro.zip";
                        using (FileStream fs = File.OpenRead(file))
                        {
                            Response.ContentLength64 = fs.Length;
                            Response.SendChunked = false;
                            //Response.ContentType
                            Response.AddHeader("Content-Disposition", "attachment; filename=baro.zip");
                            var fLen = fs.Length;
                            byte[] buffer = new byte[fLen];
                            int read;
                            using (BinaryWriter bw = new BinaryWriter(Response.OutputStream))
                            {
                                while ((read = fs.Read(buffer,0,buffer.Length)) > 0)
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
                Console.ReadKey();
            }
        }


    }
}
