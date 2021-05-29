using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace serverWEB {
    struct usersJson {
        public int    id;
        public string name;
        public string lastName;
        public string email;
        public string password;
        public string token;
        public int    position;
    }
    class token {
        private string tok; //tik
        public token() {
            tok = "";
        }
        private string genRandomPart() {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[5];
            var random = new Random();
            for (int i = 0; i < stringChars.Length - 1; i++) {
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
            return filler + pos.ToString();
        }
        public bool genToken(int pos) {
            if (pos > 0 && pos < 1000) {
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
            return email + ";" + password + ";" + token;
        }
    }

    class server {
        private List<voter> voters;
        //public List<string> usersForLogin;
        private usersJson presidente;
        private List<usersJson> usersFromJson;
        //azioni del presidente
        private bool runServer = true; //non penso che, per il modo in cui é costrituo, abbia senso dare questo potere al presidente
        private bool openLogin = true;
        private bool openVote = false;
        //stringhe per le pagine
        private string loginPage;
        private string loginPageForPresident;
        private string presidentArea;
        private string notAllowed;
        private string votePage;
        private string monitorPage;
        //for webserver
        private string url = "http://localhost:8000/";
        private HttpListener listener;
        
        //carica i senatori e il presidente dal json
        private void loadUsers() {
            //carica i senatori
            StreamReader sr = new StreamReader(@"config\senatore.json");
            string json = sr.ReadToEnd();
            usersFromJson = JsonConvert.DeserializeObject<List<usersJson>>(json);
            sr.Close();

            //carica il presidente
            sr = new StreamReader(@"config\presidente.json");
            json = sr.ReadToEnd();
            presidente = JsonConvert.DeserializeObject<usersJson>(json);
            sr.Close();
        }
        //il problema con i byte c'é ancora ma almeno ora utilizzo solo usersFromJson
        private bool checkLogin(string log) {//log = email;password;token
            foreach (var user in usersFromJson) {
                string check = "";
                
                check = user.email + ";" + user.password + ";" + user.token;
                byte[] logb = Encoding.UTF8.GetBytes(log.Trim());
                byte[] checkb = Encoding.UTF8.GetBytes(check.Trim());
                byte[] supp = new byte[checkb.Length - 1];

                for (int i = 0; i < checkb.Length - 1; i++) {
                    supp[i] = checkb[i];
                }
                if (logb.SequenceEqual(supp)) {
                    return true;
                }
            }
            return false;
        }
        private bool checkLoginPresidente(string log) {
            var toCheck = presidente.email + ";" + presidente.password + ";" + presidente.token;
            byte[] logb = Encoding.UTF8.GetBytes(log.Trim());
            byte[] userb = Encoding.UTF8.GetBytes(toCheck.Trim());
            byte[] userbmin1 = new byte[userb.Length - 1];
            for (int i = 0; i < userb.Length - 1; i++) {
                userbmin1[i] = userb[i];
            }
            return logb.SequenceEqual(userbmin1);
        }

        //costruttore
        public server() {
            listener = new HttpListener();
            voters = new List<voter>();
            //usersForLogin = new List<string>();
            usersFromJson = new List<usersJson>();
        }
        public void genLoginsCode() {
            loadUsers();
            int i = 0;
            for (; i < usersFromJson.Count; i++) {
                token t = new token();
                t.genToken(i + 1);
                usersJson a;
                a.id = usersFromJson[i].id;
                a.name = usersFromJson[i].name;
                a.lastName = usersFromJson[i].lastName;
                a.email = usersFromJson[i].email;
                a.password = usersFromJson[i].password;
                a.token = t.getToken();
                a.position = i + 1;
                usersFromJson[i] = a;
            }
            token pres = new token();
            pres.genToken(i + 1);
            presidente.token = pres.getToken();
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
            //carica la pagina di login per i votanti
            StreamReader sr = new StreamReader(@"pages\login.html");
            loginPage = sr.ReadToEnd();
            sr.Close();

            //carica la pagina di login per il presidente
            sr = new StreamReader(@"pages\loginForPresident.html");
            loginPageForPresident = sr.ReadToEnd();
            sr.Close();

            //carica la pagina di errore
            sr = new StreamReader(@"pages\notAllowed.html");
            notAllowed = sr.ReadToEnd();
            sr.Close();

            //carica la pagina di comando del presidente
            sr = new StreamReader(@"pages\presidentArea.html");
            presidentArea = sr.ReadToEnd();
            sr.Close();
        }
        public void addVoter(string login) {
            var i = 0;
            foreach(var user in usersFromJson) {
                var element = login.Split(";");
                if (element[0] == user.email && element[1] == user.password && element[2] == user.token) {
                    voter v = new voter(user.id, user.token, user.name, user.lastName, user.email, user.password, user.position) ;
                }
                i++;
            }
        }
        public void listen() {
            try {
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();
            } catch (Exception e) {
                Console.WriteLine(e);
            }
        }
        public string printUsers(int index) {
            if (index > 0 || index < usersFromJson.Count) {
                return usersFromJson[index].email + ";" + usersFromJson[index].password + ";" + usersFromJson[index].token;
            }
            return "index out of range";
        }
        public string printUsers() {
            string toReturn = "";
            foreach (var u in usersFromJson) {
                toReturn += u.email + ";" + u.password + ";" + u.token + "\n";
            }
            return toReturn;
        }
        public string printPreidente() {
            return presidente.email + ";" + presidente.password + ";" + presidente.token;
        }
        public async Task HandleIncomingConnections() {
            while (runServer) {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                if (req.Url.AbsolutePath != "/favicon.ico") {
                    //GESTIONE LOGIN VOTANTI
                    //GET request all'endpoint di login
                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/")) {
                        if (openLogin) {
                            init();
                            byte[] data = Encoding.UTF8.GetBytes(loginPage);
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = data.LongLength;

                            await resp.OutputStream.WriteAsync(data, 0, data.Length);
                            resp.Close();
                        } else {
                            resp.StatusCode = 405; //method not allowed
                            byte[] data = Encoding.UTF8.GetBytes(notAllowed);

                            //risposta al client
                            resp.ContentType = "text/html";
                            resp.ContentEncoding = Encoding.UTF8;
                            resp.ContentLength64 = data.LongLength;

                            await resp.OutputStream.WriteAsync(data, 0, data.Length);
                            resp.Close();
                        }
                    }
                    //POST request, controlla il login e, se autorizzato, ritorna l'endpoint con l'area di voto
                    if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/")) {
                        //legge la richiesta di un client
                        Stream stream = req.InputStream;
                        StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                        string content = sr.ReadToEnd().Trim();//.Replace("\n", "").Replace("\r", "").Trim();
                        string endPattern = Regex.Escape(content);
                        Console.WriteLine("richiesta del client:" + endPattern);
                        //controllo del login
                        byte[] data;
                        if (checkLogin(content)) {
                            data = Encoding.UTF8.GetBytes("{\"message\": \"Login accettato correttamente\",\"accepted\": true}");
                            addVoter(content);
                        } else {
                            data = Encoding.UTF8.GetBytes("{\"message\": \"Credenziali scorrette, utente non riconosciuto\", \"accepted\": false}");
                        }
                        //risposta al client

                        resp.ContentType = "application/json";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;

                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                    }

                    //GESTIONE DELLE VOTAZIONI
                    if (openVote) {

                    }

                    //GESTIONE LOGIN PRESIDENTE
                    //GET per l'endpoint del presidente, ritorna il login del presidente
                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/presidente")) {
                        init();
                        byte[] data = Encoding.UTF8.GetBytes(loginPageForPresident);
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;

                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                    }

                    //POST controlla se il login del presidente é corretto
                    if((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/presidente")) {
                        Stream stream = req.InputStream;
                        StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                        string content = sr.ReadToEnd().Trim();
                        Console.WriteLine("richiesta del client (presidente):" + content);
                        //controllo del login
                        byte[] data;
                        if (checkLoginPresidente(content)) {
                            data = Encoding.UTF8.GetBytes(presidentArea);
                            resp.ContentType = "text/html";
                        } else {
                            data = Encoding.UTF8.GetBytes("{\"message\": \"Credenziali scorrette, utente non riconosciuto\", \"accepted\": false}");
                            resp.ContentType = "application/json";
                        }
                        //risposta al client
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;

                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                    }
                }
            }
        }
    }

    class program {
        public static void Main(string[] args) {
            server s = new server();
            s.init();
            s.genLoginsCode();
            Console.WriteLine("login per presidente: "+ s.printPreidente());
            Console.WriteLine("login di un votante generico: " + s.printUsers(2));
            Console.WriteLine("ascolto sulla porta 8000");
            s.start();
            s.listen();
        }
    }
}