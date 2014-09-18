package snippet01;

// Using Class.this and Class.super

public class Foo {
	private int var;
	
	class Bar {
		void doSomething() {
			System.out.println(Foo.this.var);
			System.out.println(Foo.super.toString());
		}
	}
}
