package main

import (
	"errors"
	"log"
	"math/rand"
	"net/http"
	"os"
	"strconv"
)

//token
type Token struct {
	token string
}

func (t Token) genRandomPart(dim int) string {
	chars := "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
	b := make([]byte, dim)
	for i := range b {
		b[i] = chars[rand.Intn(len(chars))]
	}
	return string(b)
}

//generate token (postion + 5 random letters)
func (t *Token) GenToken(pos int) error {
	if pos > 0 && pos < 1000 {
		var filler string
		if pos < 100 && pos >= 10 {
			filler += "0"
		} else if pos < 10 && pos >= 1 {
			filler += "00"
		}
		t.token = filler + strconv.Itoa(pos) + t.genRandomPart(5)
		return nil
	}
	return errors.New("invalid position")
}

type Voter struct {
	Id       int
	Token    string
	Name     string
	LastName string
	Email    string
	Password string
	Voted    bool
	Vote     int
	Position int
}

type Server struct {
	Voters        []Voter
	UsersForLogin []string
}

//start the server and listen on localhost:8080
func (s Server) Start() error {
	http.HandleFunc("/", s.loginHandler)
	return http.ListenAndServe(":8080", nil)
}

//handle the clients that connect to "/" and respond with the login page
func (s Server) loginHandler(w http.ResponseWriter, r *http.Request) {
	//return the login page if get method
	if r.Method == "GET" {
		w.Header().Add("Content Type", "text/html")
		content, err := os.ReadFile("pages/login.html")
		if err != nil {
			log.Fatalln(err)
			return
		}
		w.Write(content)
		return
	}

	//check if the login is correct, if so open a websocket connection with client
	if r.Method == "POST" {
		
	}
}

func main() {
	var s Server
	log.Println("inizio server sulla porta 8080")
	log.Fatal(s.Start())
}
