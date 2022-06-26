# SharpLox

Hello there and welcome to SharpLox, a modified version of the lox language designed by Robert Nystrom implemented in C#.

## Running the interpreter

Simply clone and run the command
```
dotnet run <fully qualified path to .slox file>
```

Example 
```
➜ cat test.slox
       │ File: test.slox
   1   │ print("hi");

➜ dotnet run --project CommandLine/CommandLine.csproj /Users/jaymadden/programming/projects/SharpLox/test.slox 
hi

```

## The basics

Variable declarations

```
var i = 4;
var j = 4;

print(i + j);
```

Null handling
```
var a = nil;

if true {
    a = 4;
}

print(a)
```

### Control Flow

Standard control flow concepts are implemented

#### if statements
```
var state = true;

if state {
    print("Hello there");
}
else {
    print("Hi there");
}
```

#### while and for loops
```
var i = 0;

while i < 10 {
    print(i);
    i = i + 1;
}
```

```
for var i = 0; i < 10; i = i + 1 {
    print(i);
}
```

#### Break
```
var i = 0;

while true {
    print(i);
    i = i + 1;

    if i > 10 {
        break;
    }
}
```

### Functions

Top level functions are supported

```
func someMethod(k) {
    var i = 4;
    var j = 4;

    print(i + j + k);
}

someMethod(k);
```

### Anonymous functions

```

func someMethod(lambda) {
    lambda(1);
}

var someLambda = func(foo) {
    print(foo + 1);
}

someMethod(someLambda);

```

### Classes

```
class Foo {
    func init(baz) {
        this.bar = baz;
    }

    func printBar() {
        print(this.bar);
    }
}

var f = Foo(5);
f.printBar();
```

```
class Foo {
    func init(baz) {
        this.bar = baz;
    }

    func printBar() {
        print(this.bar);
    }
}

class Baz : Foo {

    func init() {
        super.init(5);
    }

    func something() {
        print("Hi from baz");
    }
}

var f = Baz();
f.printBar();
```
