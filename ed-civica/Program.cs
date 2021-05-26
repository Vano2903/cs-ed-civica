using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace serverWEB {
    struct usersJson{
        public int id;
        public string name;
        public string lastName;
        public string email;
        public string password;
        public string token;
    }
    class token {
        private string tok; //tik

        public token() {
            tok = "";
        }
        private string genRandomPart() {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[6];
            var random = new Random();
            for (int i = 0; i < stringChars.Length -1 ; i++) {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            stringChars[5] = '\0'; 
            return new String(stringChars);
        }
        private string genNumPart(int pos) {
            string filler = "";
            if (pos < 100 && pos >= 10) {
                filler += "0";
            } else if (pos < 10 && pos >= 1) {
                filler += "00";
            }
            return filler+ pos.ToString();
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
        private string token;
        private string name;
        private string lastName;
        private string email;
        private string password; //problema si sicurezza (possibile risolvere con funzione hash)
        private bool voted;
        private int vote; //0 nullo, 1 conferma, 2 non conferma
        private int position;

        public voter(int Id, string Token, string Name, string LastName, string Email, string Password, int Position) {
            id = Id;
            token = Token;
            name = Name;
            lastName = LastName;
            email = Email;
            password = Password;
            voted = false;
            vote = 0;
            position = Position;
        }
        
        public string loginString() {
            return email +";"+ password +";"+ token;
        }
    }

    class server {
        private List<voter> voters;
        public List<string> usersForLogin;
        private List<usersJson> usersFromJson;
        private int pageViews = 0;
        private int favorevoli = 0;
        private int requestCount = 0;
        private string loginPage;
        private string votePage;
        private string monitorPage;
        //for webserver
        private string url = "http://localhost:8000/";
        private HttpListener listener;
        private bool runServer = true;

        //carica i senatori dal json
        private void loadUsers() {
            StreamReader sr = new StreamReader(@"config\senatore.json");
            string json = sr.ReadToEnd();
            usersFromJson = JsonConvert.DeserializeObject<List<usersJson>>(json);
            sr.Close();
        }

        //costruttore
        public server() {
            listener = new HttpListener();
            voters = new List<voter>();
            usersForLogin = new List<string>();
            usersFromJson = new List<usersJson>();
        }

        //CONTROLLA 
        public void genLoginsCode() {
            //versione con json
            loadUsers();
            int pos = 1;
            foreach (var user in usersFromJson) {
                token t = new token();
                var toAdd = user.email + ";" + user.password + ";" + t.genToken(pos);
                usersForLogin.Add(toAdd);
                pos++;
            }

            //versione con il csv
            /*int pos = 1;
            StreamReader sr = new StreamReader(@"config/senatore.csv");
            string toSplit = sr.ReadToEnd();
            sr.Close();
            Console.WriteLine(toSplit);
            foreach(var user in toSplit.Split("\n").ToList()) {
                //var toAdd = new System.Text.StringBuilder();
                var toAdd = "";
                token t = new token();
                t.genToken(pos);
                var element = user.Split(";");
                toAdd = element[3] + ";" + element[4] + ";" + t.getToken();
                Console.WriteLine("toAdd:" + toAdd);
                usersForLogin.Add(toAdd);
                pos++;
            }*/
        }
        public bool checkLogin(string log) {//log = token;email;password
            return usersForLogin.Contains(log);
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
        public void printUsersForLogin() {
            foreach(var u in usersForLogin) {
                /*Console.WriteLine(u.id);
                Console.WriteLine(u.lastName);
                Console.WriteLine(u.name);
                Console.WriteLine(u.email);
                Console.WriteLine(u.password);*/

                Console.WriteLine(u);
                Console.ReadLine();
            }
        }
    }

    class program {
        public static void Main(string[] args) {
            token t = new token();
            t.genToken(1);
            Console.WriteLine(t.getToken());
            server s = new server();
            s.init();
            s.genLoginsCode();
            s.printUsersForLogin();
            //Console.WriteLine(s.usersForLogin[2]);
            //var a = s.usersForLogin[1].Split(";");
            //Console.WriteLine("ciaoo:" + a[0] + ";" + a[1] + ";" + a[2]);
            s.start();
            s.listen();
            
        }
    }
}