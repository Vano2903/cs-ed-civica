package main

import (
	"fmt"
	"io/ioutil"
	"os"
)

//ReadFile return all the file as a byte array
//expect a relative path
func readFile(path string) []byte {
	data, err := ioutil.ReadFile(path)
	if err != nil {
		fmt.Println("Errore nel leggere il file")
		os.Exit(4)
	}
	return data
}

//WriteFile overwrite the whole file
//expect a relative path
func WriteFile(path string, stream []byte) error {
	err := ioutil.WriteFile(path, stream, 0644)
	return err
}

//AppendFile append an array of byte type to an existing file
//expect a relative path
func AppendFile(path string, stream []byte) error {
	fileContent := readFile(path)
	fileContent = append(fileContent, stream...)
	err := WriteFile(path, fileContent)
	return err
}

//questa parte serve solo per avere un programma piú pulito e cosí devo passare solo il file e il paccchetto modificato invece che tutta la cartella con i txt vuoti xD

//Exist check if a file exist (duhh ;-; )
//espect a relative path
func Exist(path string) bool {
	info, err := os.Stat(path)
	if os.IsNotExist(err) {
		return false
	}
	return !info.IsDir()
}

// func isError(err error) bool {
// 	if err != nil {
// 		fmt.Println(err.Error())
// 	}
// 	return (err != nil)
// }

// func createFile(path string) {
// 	// check if file exists
// 	var _, err = os.Stat(path)

// 	// create file if not exists
// 	if os.IsNotExist(err) {
// 		var file, err = os.Create(path)
// 		if isError(err) {
// 			return
// 		}
// 		defer file.Close()
// 	}
// }
