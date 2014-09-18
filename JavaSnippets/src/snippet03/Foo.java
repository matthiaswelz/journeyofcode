package snippet03;

// Using modifier keywords before the visibility specification

public class Foo {
	synchronized strictfp final public void bar() {
		System.out.println("Hello world");
	}
	
	public static void main(String[] args) {
		new Foo().bar();
	}
}
