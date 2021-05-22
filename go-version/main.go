package main

import (
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
	http.HandleFunc("/", loginHandler)

	log.Println("inizio server sulla porta 8080")
	if err := http.ListenAndServe(":8080", nil); err != nil {
		log.Fatal(err)
	}
}
