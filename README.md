# RSADClassesParser

## What is this?

RSADClassesParser is a C# ConsoleApplication made by generating Java code from a RSAD .emx project file. It scans the .emx for software classes (starting with "SW") and interfaces and, after parsing the classes, attributes, operations with parameters etc., it outputs the generated code (operations will have an empty body for now).

## Getting started

You can get the latest .exe release and run `RSADClassesParser.exe --help` on cmd to get started (thanks to https://www.nuget.org/packages/CommandLineParser/).

### Current commands and usage

#### -i, --input
This command sets the input path to the .emx, it's optional and if not specified the program will try to find a "Blank Package.emx" in the current directory.
Example `RSADClassesParser.exe -i C:\Users\crist\Desktop\laboratorio-8-blackbox\TravelOn\\"Blank Package.emx"`

#### -o, --output
This one sets the output directory; the .java files won't be directly saved there, instead a FileJava directory will be created in the specified path. This command is optional too.
If no path is specified, the program will try to create a FileJava directory in the current directory, if for some reason it fails then it'll just print on the console the generated Java code.
Otherwise if a path is specified, the program will try to create all the directories and subdirectories to access that path; if it fails it'll retry using the current directory and then the behavior is the same as before.
When finally the path with the directory FileJava is available the execution will continue and, after parsing, it'll save to .java files every software class and interface found in the RSAD project.
As said before, if the path can't be accessed then it'll just print on the console.

#### Built-in commands: --help and --version
These commands were added by *CommandLineParser* and they are self explanatory.

## Thanks for checking this out!

Feel free to use and edit the source code and anything you want.
