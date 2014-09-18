package snippet04;

import java.util.NoSuchElementException;

// Declaring new exceptions on subclasses

interface Foo {
	void baz();
	void bat();
}

class Bar implements Foo {

	@Override
	public void baz() /*throws FileNotFoundException*/ {
	}

	@Override
	public void bat() throws NullPointerException, NoSuchElementException, UnsupportedOperationException  {
		
	}
	
	public static void main(String[] args) {
		new Bar().baz();
		new Bar().bat();
	}
	
}