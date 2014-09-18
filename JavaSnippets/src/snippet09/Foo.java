package snippet09;

// Providing a generic constructor

class Foo {
	public <T> Foo(T t) {
		System.out.println(t);
	}
	
	public static void main(String[] args) {
		Foo foo1 = new Foo("Test");
		Foo foo2 = new Foo(42);
	}
}