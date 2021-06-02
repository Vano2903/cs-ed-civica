package main

import (
	"sync"
)

type Connection struct {
	canali []chan int
	m      *sync.Mutex
}

func NewConnection() *Connection {
	return &Connection{}
}

func (c *Connection) AppendElement(element chan int) int {
	c.m.Lock()
	c.canali = append(c.canali, element)
	index := len(c.canali)
	c.m.Unlock()
	return index
}

func (c *Connection) RemoveElement(index int) {
	c.m.Lock()
	c.canali = append(c.canali[:index], c.canali[index+1:]...)
	c.m.Unlock()
}
