package main

import (
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
)

var LoginPage string

func init() {
	data, err := ioutil.ReadFile("./pages/login.html")
	if err != nil {
		panic(err)
	}
	LoginPage = string(data)
}

func login(w http.ResponseWriter, r *http.Request) {
	w.Header().Add("Content Type", "text/html")
	fmt.Fprintf(w, LoginPage)
}

func main() {
	// baseLink := http.FileServer(http.Dir("./pages/login"))
	http.HandleFunc("/", login)
	// http.HandleFunc("/api/user/signup", handleSingUp)

	fmt.Println("inizio server sulla porta 8080")
	if err := http.ListenAndServe(":8080", nil); err != nil {
		log.Fatal(err)
	}
}
