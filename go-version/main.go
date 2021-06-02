package main

import "log"

var clientSocket *Connection

func init() {
	clientSocket = NewConnection()
}

func main() {
	var s Server
	log.Fatal(s.loadUsers())
	// s.PrintLogins()
	log.Println(s.Users[0].LoginString())
	log.Fatal(s.Start())
	log.Println("inizio server sulla porta 8080")
}