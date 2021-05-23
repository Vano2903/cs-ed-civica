using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace serverWEB {
    class token {
        private int position;
        private string tok; //tik

        private string genRandomPart() {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[5];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++) {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
        private string genNumPart(int pos) {
            string filler = "";
            if (pos < 100 && pos >= 10) {
                filler += "0";
            } else if (pos < 10 && pos >= 1) {
                filler += "00";
            }
            return pos.ToString() + filler;
        }

        public token() {
            position = 0;
            tok = "";
        }
        public bool genToken(int pos) {
            if(pos > 0 && pos < 1000) {

                tok += genNumPart(pos) + genRandomPart();
                return true;
            }
            return false;
        }
        public string getToken() {
            return tok;
        }
    }

    class voter {
        private int id;
        private token token;
        private string email;
        private string password;
        private bool voted;
        private int vote;
        private int position;
    }

    class server {
        private List<voter> voters;
        //for webserver
        private HttpListener listener;
        private string url = "http://localhost:8000/";
        private int pageViews = 0;
        private int favorevoli = 0;
        private int requestCount = 0;
        private string loginPage;
        private string votePage;
        private string monitorPage;
        private bool runServer = true;

        public server() {
            listener = new HttpListener();
            voters = new List<voter>();
        }
        public void start() {
            listener.Prefixes.Add(url);
            listener.Start();
        }
        public void stop() {
            runServer = false;
        }
        public void restart() {
            runServer = true;
        }
        public void kill() {
            listener.Close();
        }
        public void init() {
            StreamReader sr = new StreamReader(@"pages\login.html");
            loginPage = sr.ReadToEnd();
            sr.Close();
        }
        public async Task HandleIncomingConnections() {
            while (runServer) {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/")) {
                    byte[] data = Encoding.UTF8.GetBytes(loginPage);
                    resp.ContentType = "text/html";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;

                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/favorevole")) {
                    Console.WriteLine("Favorevole requested");
                    favorevoli++;
                }
                
                /*Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();
                byte[] data = Encoding.UTF8.GetBytes(loginPage);
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();*/
            }
        }
        public void listen() {
            try {
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();
            } catch(Exception e) {
                Console.WriteLine(e);
            }
        }
    }
    
    class program {
        public static void Main(string[] args) {
            token t = new token();
            t.genToken(100);
            Console.WriteLine(t.getToken());
            server s = new server();
            s.init();
            s.start();
            s.listen();
        }
    }
}