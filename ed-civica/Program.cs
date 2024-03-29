﻿using System;
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
        public bool setVote(string vote) {
            if (!voted) {
                voted = true;
                if (vote == "true") {
                    this.vote = 1;
                    return true;
                }
                this.vote = 2;
                return true;
            }
            return false;
        }
        public string getToken() {
            return token;
        }
        public string jsonElement() {
            //{"name":"ciao", "vote": 1, "position": 2}
            return "{\"name\":\"" + lastName + " " + name + "\", \"vote\":" + vote + ", \"position\":" + position + "}";
        }
    }
    class server {
        //gestinone utenti
        private List<voter> voters;
        private usersJson presidente;
        private List<usersJson> usersFromJson;
        //azioni del presidente
        private bool runServer = true; //non penso che, per il modo in cui é costruito, abbia senso dare questo potere al presidente
        private bool openLogin = true; 
        private bool openVote = true;
        //stringhe per le pagine
        private string loginPage;
        private string loginPageForPresident;
        private string presidentArea;
        private string notAllowed;
        private string votePage;
        private string monitorPage;
        private string broadcast;
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
                    voter v = new voter(user.id, user.token, user.name, user.lastName, user.email, user.password, user.position);
                    voters.Add(v);
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
        private bool checkPresidentToken(string tok) {
            byte[] tokb = Encoding.UTF8.GetBytes(tok.Trim());
            byte[] actualb = Encoding.UTF8.GetBytes(presidente.token.Trim());
            byte[] supp = new byte[actualb.Length - 1];

            for (int i = 0; i < actualb.Length - 1; i++) {
                supp[i] = actualb[i];
            }
            return tokb.SequenceEqual(supp) ;
        }
        private int validVoterToken(string tok) {
            int index = 0;
            foreach (var user in voters) {
                byte[] tokb = Encoding.UTF8.GetBytes(tok.Trim());
                byte[] tokenb = Encoding.UTF8.GetBytes(user.getToken());
                byte[] supp = new byte[tokenb.Length - 1];
                /*
                Console.WriteLine("tok: " + tok);
                Console.WriteLine("user.getToken(): " + user.getToken());
                Console.Write("tokb: ");
                for(int i = 0; i < tokb.Length - 1; i++) {
                    Console.Write(tokb[i]);
                }
                Console.WriteLine();
                Console.Write("tokenb: ");
                for (int i = 0; i < tokenb.Length - 1; i++) {
                    Console.Write(tokenb[i]);
                }
                Console.WriteLine();
                */
                for (int i = 0; i < tokenb.Length - 1; i++) {
                    supp[i] = tokenb[i];
                }
                if (tokb.SequenceEqual(supp)) {
                    return index;
                }
            }
            return -1;
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
            
            //carica la pagina di voto per i votanti
            sr = new StreamReader(@"pages\vote.html");
            votePage = sr.ReadToEnd();
            sr.Close();

            sr = new StreamReader(@"pages\monitor.html");
            monitorPage = sr.ReadToEnd();
            sr.Close();
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

                Console.WriteLine(req.Url.AbsolutePath);
                Console.WriteLine(req.HttpMethod);

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
                        if (openLogin) {
                            //legge la richiesta di un client
                            Stream stream = req.InputStream;
                            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                            string content = sr.ReadToEnd().Trim();//.Replace("\n", "").Replace("\r", "").Trim();
                            string endPattern = Regex.Escape(content);
                            Console.WriteLine("richiesta del client:" + endPattern);
                            //controllo del login
                            byte[] data;
                            if (checkLogin(content)) {
                                //data = Encoding.UTF8.GetBytes("{\"message\": \"Login accettato correttamente\",\"accepted\": true}");
                                data = Encoding.UTF8.GetBytes(votePage);
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
                        } else {
                            resp.StatusCode = 405; //method not allowed
                            resp.Close();
                        }
                    }

                    //GESTIONE DELLE VOTAZIONI
                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/vote")) {
                        init();
                        byte[] data;
                        if (openVote) {
                            broadcast = "MESSAGGIOOOO :D";
                            //data = Encoding.UTF8.GetBytes("{\"message\": \"Login accettato correttamente\",\"accepted\": true}");
                            data = Encoding.UTF8.GetBytes("{\"canVote\": true, \"broadcast\": \"" + broadcast + "\"}");
                            resp.ContentType = "text/html";
                        } else {
                            data = Encoding.UTF8.GetBytes("{\"canVote\": false}");
                            resp.ContentType = "application/json";
                        }
                        Console.WriteLine("open vote:" + openVote.ToString());
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;

                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                    }

                    if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/vote")) {
                        if (openVote) {
                            Stream stream = req.InputStream;
                            StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                            string content = sr.ReadToEnd().Trim();
                            Console.WriteLine("voto del client:" + content);
                            var elements = content.Split(";");
                            var index = validVoterToken(elements[1]);
                            bool accepted;
                            if (index >= 0) {
                                accepted = voters[index].setVote(elements[0]);
                                byte[] data;
                                if (accepted) {
                                    data = Encoding.UTF8.GetBytes("{\"message\": \"Voto accettato correttamente\", \"alreadyVoted\":\"false\"}");
                                } else {
                                    data = Encoding.UTF8.GetBytes("{\"message\": \"L'utente ha giá votato\", \"alreadyVoted\":\"true\"}");
                                }
                                resp.ContentType = "application/json";
                                resp.StatusCode = 202; //accettato
                                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                                resp.Close();

                            } else {
                                resp.StatusCode = 406; //not acceptable
                                resp.Close();
                            }
                        }
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
                    if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/presidente")) {
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

                    //area di controllo per i votanti (per il presidente)
                    if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/admin")) {
                        Stream stream = req.InputStream;
                        StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                        string content = sr.ReadToEnd().Trim();
                        var elements = content.Split(";");
                        if (checkPresidentToken(elements[0])) {
                            if (elements[1] == "voteOn") {
                                openVote = true;
                                Console.Write("richiesta di voto on: ");
                                //Console.WriteLine("il valore di openVote é: " + openVote.ToString());
                            } else if (elements[1] == "voteOff") {
                                openVote = false;
                                Console.Write("richiesta di voto off: ");
                                //Console.WriteLine("il valore di openVote é: " + openVote.ToString());
                            } else if (elements[1] == "loginOn") {
                                openLogin = true;
                                Console.Write("richiesta di login on: ");
                                //Console.WriteLine("il valore di openLogin é: " + openLogin.ToString());
                            } else if (elements[1] == "loginOff") {
                                openLogin = false;
                                Console.Write("richiesta di login off: ");
                                //Console.WriteLine("il valore di openLogin é: " + openLogin.ToString());
                            } else {
                                resp.StatusCode = 406; //not acceptable
                                resp.Close();
                            }
                        } else {
                            resp.StatusCode = 401; //non autorizzato
                            resp.Close();
                        }
                    }

                    //monitor endpoint
                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/monitor")) {
                        init();
                        byte[] data = Encoding.UTF8.GetBytes(monitorPage);
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;

                        await resp.OutputStream.WriteAsync(data, 0, data.Length);
                        resp.Close();
                    }

                    //non so come é gestito il multi utente in questo programma, se ogni utente occupa un thread diverso o se tutti gli utenti usano le stesse risorse
                    //anche perché con 3 utenti ritengo quasi impossibili ottenere dei problemi ma con molti utenti come dovrebbe essere effettivamente
                    //non so valutare la gestione di questa funzione
                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/results")) {
                        string jsonToSend = "[";
                        for (int i = 0; i < voters.Count; i++) {
                            if(i != 0) {
                                jsonToSend += ",";
                            }
                            jsonToSend += voters[i].jsonElement();
                        }
                        jsonToSend += "]";
                        //invia il json
                        byte[] data = Encoding.UTF8.GetBytes(jsonToSend);
                        resp.ContentType = "application/json";
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
            Console.WriteLine("login di un votante generico 1: " + s.printUsers(0));
            Console.WriteLine("login di un votante generico 2: " + s.printUsers(1));
            Console.WriteLine("login di un votante generico 3: " + s.printUsers(2));
            Console.WriteLine("ascolto sulla porta 8000");
            s.start();
            s.listen();
        }
    }
}