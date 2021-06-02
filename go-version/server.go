package main

import (
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"strings"

	"github.com/gorilla/websocket"
)

type Server struct {
	Users      []Voter
	Presidente Voter
}

//unmarshall users from file and fill Users slice
func (s *Server) LoadUsers() error {
	data := readFile("config/senatore.json")
	err := json.Unmarshal(data, &s.Users)
	if err != nil {
		return err
	}
	for i := 0; i < len(s.Users); i++ {
		pos := i + 1
		s.Users[i].Position = pos
		s.Users[i].Token.GenToken(pos)
		// fmt.Println(s.Users[i].LoginString())
	}
	data = readFile("config/presidente.json")
	err = json.Unmarshal(data, &s.Presidente)
	return err
}

//start the server and listen on localhost:8080
func (s Server) Start() error {
	http.HandleFunc("/", s.loginHandler)
	http.HandleFunc("/socketVote", s.SocketVote)
	http.HandleFunc("/presidente", s.presidenteHandler)
	return http.ListenAndServe(":8080", nil)
}

//print all logins codes, for now it's kinda just for debug
func (s Server) PrintLogins() {
	// fmt.Println(s.LoadUsers())
	for _, a := range s.Users {
		fmt.Println(a.LoginString())
	}
}

func (s *Server) PresidentLogin(login string) bool {
	if s.Presidente.LoginString() == login {
		s.Presidente.Logged = true
		return true
	}
	return false
}

//check if the login is correct, if so then add it to Voters
func (s *Server) CheckLoginAndAdd(login string) bool {
	for _, user := range s.Users {
		if user.LoginString() == login {
			user.Logged = true
			return true
		}
	}
	return false
}

func (s Server) SocketVote(w http.ResponseWriter, r *http.Request) {
	log.Println("soo quiiiiiii")
	can := make(chan int)
	//! Se non dovesse funzionare metti "&chan"
	_ = clientSocket.AppendElement(can)

	upgrader := websocket.Upgrader{}
	c, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("upgrade:", err)
		return
	}
	defer c.Close()

	for {
		mt, message, err := c.ReadMessage()
		if err != nil {
			log.Println("read:", err)
			break
		}
		log.Printf("ricezione di voto: %s", message)

		msgString := string(message)
		log.Println("hellooo")
		ele := strings.Split(msgString, ";")
		log.Println("anyone")
		
		switch ele[0] {
		case "verify":
			var verified bool
			for _, user := range s.Users {
				if user.LoginString() == ele[1]+";"+ele[2]+";"+ele[3] {
					verified = true
					break
				}
			}
			if verified {
				log.Println("verificato")
				w.Header().Add("Content Type", "application/json")
				content := "{\"scope\":\"verify\", \"approved\":true}"
				err = c.WriteMessage(mt, []byte(content))
				if err != nil {
					log.Println("errore:", err)
					break
				}
			} else {
				log.Println("non verificato")
				w.Header().Add("Content Type", "application/json")
				content := "{\"scope\":\"verify\", \"approved\":false}"
				err = c.WriteMessage(mt, []byte(content))
				if err != nil {
					log.Println("errore:", err)
					break
				}
			}
		case "vote":
		default:
			//probabilmente sta cercando di accedere da una sessione vuota
		}

		select {
		case <-can:
			//TODO: Scrivi al client che ora può votare
			content := "{\"scope\":\"verify\", \"approved\":false}"
			err = c.WriteMessage(mt, []byte(content))
			if err != nil {
				log.Println("errore:", err)
			}
		}
	}
}

//handle the clients that connect to "/" and respond with the login page
func (s Server) loginHandler(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	//return the login page
	case "GET":
		w.Header().Add("Content Type", "text/html")
		content := readFile("pages/login.html")
		w.Write(content)
		return

	//check if the login is correct, if so open a websocket connection with client
	case "POST":
		reqBody, err := ioutil.ReadAll(r.Body)
		if err != nil {
			log.Println("errore nella post a login handler", err)
		}
		log.Printf("%s\n", reqBody)
		mes := struct {
			Message  string `json:"message"`
			Accepted bool   `json:"accepted"`
		}{
			"Credenziali scorrette",
			false,
		}
		if s.CheckLoginAndAdd(string(reqBody)) {
			mes.Message = "Login corretto"
			mes.Accepted = true
			w.Header().Set("Content-Type", "application/json")
			toSend, _ := json.Marshal(mes)
			w.Write([]byte(toSend))
		} else {
			w.Header().Set("Content-Type", "application/json")
			toSend, _ := json.Marshal(mes)
			w.Write([]byte(toSend))
		}
	default:
		w.WriteHeader(http.StatusNotImplemented)
		w.Write([]byte(http.StatusText(http.StatusNotImplemented)))
	}
}

func (s Server) voteHandler(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case "GET":
		w.Header().Add("Content Type", "text/html")
		content := readFile("pages/vote.html")
		w.Write(content)
		return
	default:
		w.WriteHeader(http.StatusNotImplemented)
		w.Write([]byte(http.StatusText(http.StatusNotImplemented)))
	}
}

func (s Server) presidenteHandler(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	case "GET":
		w.Header().Add("Content Type", "text/html")
		content := readFile("pages/loginForPresident.html")
		w.Write(content)
		return

	case "POST":
		reqBody, err := ioutil.ReadAll(r.Body)
		if err != nil {
			log.Println("errore nella post a login handler", err)
		}
		log.Printf("%s\n", reqBody)
		mes := struct {
			Message  string `json:"message"`
			Accepted bool   `json:"accepted"`
		}{
			"Credenziali scorrette",
			false,
		}
		if s.PresidentLogin(string(reqBody)) {
			mes.Message = "Login corretto"
			mes.Accepted = true
			w.Header().Set("Content-Type", "application/json")
			toSend, _ := json.Marshal(mes)
			w.Write([]byte(toSend))
		} else {
			w.Header().Set("Content-Type", "application/json")
			toSend, _ := json.Marshal(mes)
			w.Write([]byte(toSend))
		}

	default:
		w.WriteHeader(http.StatusNotImplemented)
		w.Write([]byte(http.StatusText(http.StatusNotImplemented)))
	}
}
