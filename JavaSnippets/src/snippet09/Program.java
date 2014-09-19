package snippet09;

import java.io.Serializable;

// Providing a generic constructor

class Foo {
	public <T> Foo(T t) {
		
	}
}

class Bar {
	public <T extends Comparable<T> & Serializable> Bar(T param) {
		
	}
	
	public <T> Bar(Node<T> parent, T value) {
		
	}
}

class Baz {
	
}

class Node<T> {
	public Node(T value) {
		
	}
}

public class Program {

	public static void main(String[] args) {
		new Foo("Test");
		new Foo(42);
		
		new Bar("Hello World!");
		//new Bar(new Baz());
		
		new Bar(new Node<String>("Hello World!"), "Test");
		//new Bar(new Node<String>("Hello World!"), 42);
	}
}
