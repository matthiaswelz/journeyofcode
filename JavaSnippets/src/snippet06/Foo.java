package snippet06;

// Using initializer blocks (static and non-static)

public class Foo {
	{
		System.out.print("1 ");
	}
	static {
		System.out.print("2 ");
	}
	Foo () {
		System.out.print("3 ");
		
	}
	{
		System.out.print("4 ");
	}
	static {
		System.out.print("5 ");
	}
	
	public static void main(String[] args) {
		new Foo();
	}
}
