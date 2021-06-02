package main

type Voter struct {
	Id       int    `json: "id"`
	Name     string `json: "name"`
	LastName string `json: "lastName"`
	Email    string `json: "email"`
	Password string `json: "password"`
	Voted    bool   `json: "voted"`
	Vote     int    `json: "vote"`
	Position int    `json: "position"`
	Logged   bool   `json: "logged"`
	Token    Token
}

func (t Voter) LoginString() string {
	return t.Email + ";" + t.Password + ";" + t.Token.Token
}
