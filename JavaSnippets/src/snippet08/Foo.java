package snippet08;

// Calling new on an object

class Foo {
	class Bar {
		
	}
}

class Baz {
	public static void main(String[] args) {
		Foo foo = new Foo();
		//Foo.Bar bar = new Foo.Bar();
		Foo.Bar bar = foo.new Bar();
	}
}
