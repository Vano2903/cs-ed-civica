package main

import (
	"errors"
	"fmt"
	"io/ioutil"
	"log"
	"math/rand"
	"net/http"
	"strconv"
	"strings"
)

type Token struct {
	Token string
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
		t.Token = filler + strconv.Itoa(pos) + t.genRandomPart(5)
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

func (s *Server) init() error {
	data := ReadFile("config/senatore.txt")
	// datas := File.Read(data)
	datas := strings.Split(string(data), "\n")
	for pos, data := range datas {
		var t Token
		elements := strings.Split(data, ";")
		err := t.GenToken(pos + 1)
		if err != nil {
			return err
		}
		toAdd := elements[3] + ";" + elements[4] + ";" + t.Token
		s.UsersForLogin = append(s.UsersForLogin, toAdd)
	}
	return nil
}

//start the server and listen on localhost:8080
func (s Server) Start() error {

	http.HandleFunc("/", s.loginHandler)
	return http.ListenAndServe(":8080", nil)
}

func (s Server) PrintLogins() {
	fmt.Println(s.init())
	for _, a := range s.UsersForLogin {
		fmt.Println(a)
	}
}

//handle the clients that connect to "/" and respond with the login page
func (s Server) loginHandler(w http.ResponseWriter, r *http.Request) {
	switch r.Method {
	//return the login page
	case "GET":
		w.Header().Add("Content Type", "text/html")

		// content, err := os.Open("config/senatore.txt")
		content := ReadFile("pages/login.html")
		// fmt.Println(content)
		w.Write(content)
		return

	//check if the login is correct, if so open a websocket connection with client
	case "POST":
		reqBody, err := ioutil.ReadAll(r.Body)
		if err != nil {
			log.Fatal(err)
		}
		fmt.Printf("%s\n", reqBody)
		w.Write([]byte("Received a POST request\n"))

	default:
		w.WriteHeader(http.StatusNotImplemented)
		w.Write([]byte(http.StatusText(http.StatusNotImplemented)))
	}
}

func main() {
	var s Server

	log.Println("inizio server sulla porta 8080")
	s.PrintLogins()
	s.init()
	log.Fatal(s.Start())

}
