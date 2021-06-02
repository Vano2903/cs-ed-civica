package main

import (
	"errors"
	"math/rand"
	"strconv"
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
