package snippet10;

// Explicit specification of generic type parameters when calling a method 

class Foo {	
	public <T> void temp(T param) {
		System.out.println(param.getClass());
	}
	
	public static void main(String[] args) {
		Foo foo = new Foo();
		foo.temp(42);
		foo.<Integer>temp(42);
	}
}
