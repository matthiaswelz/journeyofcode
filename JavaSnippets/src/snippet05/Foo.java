package snippet05;

// Fun with enums

public enum Foo {
	A("C") {
		
		void doSomething() {
			System.out.println("A: " + this.var);
		}		
	},
	B("D") {
		void doSomething() {
			System.out.println("B: " + this.var);			
		}		
	};
	
	protected String var;
	
	Foo(String var) {
		this.var = var;
	}
	
	abstract void doSomething();
	
	public static void main(String[] args) {
		method(Foo.A);
		method(Foo.B);
	}
	
	static void method(Foo foo) {
		foo.doSomething();
	}
}