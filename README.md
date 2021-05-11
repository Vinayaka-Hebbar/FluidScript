# FluidScript 
Script parsing library for .Net 

## Installation

The Toolkit is available via NuGet, and should be installed into all of your projects (shared and individual platforms):

* NuGet Official Releases: [![NuGet](https://img.shields.io/nuget/vpre/FluidScript)](https://www.nuget.org/packages/FluidScript/)

Browse with the NuGet manager in your IDE to install them or run this command:

`Install-Package FluidScript`

## Getting Started

After installation, start using the features you're after.

```c#
var compiler = new RuntimeCompiler();
compiler["x"] = 10;
compiler["y"] = 20;
var statement = Parser.GetStatement("x+y");
var res = compiler.Invoke(statement);
Console.WriteLine("res = " + res);
```
