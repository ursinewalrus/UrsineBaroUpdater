using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UrsineBaroDownloader
{

    //maybe
    //https://www.dropbox.com/developers-v1/core/docs#oauth2-methods
    //https://www.dropbox.com/1/oauth2/authorize?response_type=code&client_id=[]&redirect_uri=http://localhost:12345
    class Program
    {
        static void Main(string[] args)
        {
            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:12345/");

                listener.Start();

                for (; ; )
                {

                    Console.WriteLine("Listening...");

                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    var qs = context.Request.QueryString["code"];
                    ;
                    using (HttpListenerResponse response = context.Response)
                    {
                        
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
