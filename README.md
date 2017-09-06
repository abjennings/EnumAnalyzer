# EnumAnalyzer
Static analysis for C# to indicate regions where you missed handling an enum value

## Motivation
Sometimes I code up an if/else block or a switch statement to handle all the different values
of an enum and I think, "It would be great if the compiler would warn me if I ever added
another value to that enum and forgot to handle it here."

So I wrote this compiler extension.  You have to opt-in to this check, and you do it with:

```C#
throw new net.ajennings.EnumNotExhaustedException<T>();
```
    
where T is the enum type that you want to make sure you handled all the values for.

## Installation

1. Download and double-click the VSIX file (in Releases) and it should install as a visual studio extension.
(I think Visual Studio 2017 is required.)

2. Include the definition of EnumNotExhaustedException in your project.  Either include this definition:

```C#
namespace net.ajennings
{
    class EnumNotExhaustedException<T> : System.Exception
    {
    }
}
````
    
or include the EnumNotExhausted.cs file (included with the release) in your project.

## Usage

Here is an example:

![EnumAnalyzer example code screenshot](https://raw.githubusercontent.com/abjennings/EnumAnalyzer/master/docs/EnumAnalyzer_screenshot.png)

## Details

If the analyzer sees you throw the `EnumNotExhaustedException`, it checks the surrounding code block to make sure
each possible enum value is mentioned somewhere.  If one or more enum values aren't ever mentioned, it raises a diagnostic
error.  I don't think it will prevent the project from compiling, though.

## Warning

If enums are cast from integers or if they are received from other assemblies, they can take values other than the named
constants in the enum definition.  Enums are really just integers with some syntactic sugar.  If you're doing that stuff,
this extension can't keep you safe at run-time.

## FAQ

### What if I want to handle the unexpected value without throwing an exception?

I would recommend putting the `throw new EnumNotExhaustedException` right after a `return` statement.  It will still
trigger the static analysis even if it is in unreachable code.