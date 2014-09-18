package snippet07;

// Calling a method on null

public class Foo {
	public static void doSomething() {
		System.out.print("Foo ");
	}
	
	public static void main(String[] args) {
		((Foo)null).doSomething();
		
		Derived derived = new Derived();
		Foo foo = derived;
		derived.doSomething();
		foo.doSomething();
	}
}

class Derived extends Foo {
	public static void doSomething() {
		System.out.print("Derived ");		
	}
}
