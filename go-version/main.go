package main

import "log"

var clientSocket *Connection

func init() {
	clientSocket = NewConnection()
}

func main() {
	var s Server
	// s.LoadUsers()
	log.Println(s.LoadUsers())
	// s.PrintLogins()
	// s.PrintLogins()
	log.Println("user1: ", s.Users[0].LoginString())
	log.Println("presidente: ", s.Presidente.LoginString())
	log.Fatal(s.Start())
	log.Println("inizio server sulla porta 8080")
}
