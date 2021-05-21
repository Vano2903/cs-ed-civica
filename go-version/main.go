package main

import (
	// "fmt"
	"log"
	"net/http"
	"os"
)

func loginHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Add("Content Type", "text/html")
	content, err := os.ReadFile("pages/login.html")
	if err != nil {
		log.Fatalln(err)
		return
	}

	w.Write(content)
}

func main() {
	// baseLink := http.FileServer(http.Dir("./pages/login"))
	http.HandleFunc("/", loginHandler)
	// http.HandleFunc("/api/user/signup", handleSingUp)

	log.Println("inizio server sulla porta 8080")
	if err := http.ListenAndServe(":8080", nil); err != nil {
		log.Fatal(err)
	}
}
