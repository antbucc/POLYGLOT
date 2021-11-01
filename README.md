# Polyglot

A Gamified .NET Interactive Framework for Teaching and Learning Multi-Language Programming for High School and Undergraduate Students

Currently, Polyglot only works with C# but we're working on the support for the other .NET Interactive languages (F#, Javascript, ...) and [SysML V2](https://www.omgsysml.org/SysML-2.htm)

See it in action in the videos below!




https://user-images.githubusercontent.com/41111850/139651588-cb775250-209f-4654-b811-9777544e136b.mp4




https://user-images.githubusercontent.com/41111850/139651436-ab99f0cc-312d-4cdd-b1b0-39f23cb80fe9.mp4



## How to contribute

You can clone the repo and open the solution with Visual Studio.  
The solution is composed of three projects (plus the test project):

- **Polyglot.Core**: defines the language engine structure and the metrics interface. Handles the communication with Polyglot backend
- **Polyglot.Interactive**: lets you install language engines and provides some common metrics
- **Polyglot.CSharp**: has language specific metrics and language engine

## How to test

To test what we have done, you'll need the latest version of Visual Studio Code.
You can create your new notebook or use the ones in ```/notebooks``` as a template.  

You can build Polyglot solution, pack it into a NuGet package and import it from a local folder as shown [in this notebook](https://github.com/antbucc/POLYGLOT/blob/main/notebooks/Sample1.ipynb).  
Alternatively you can skip the NuGet package part and directly use the .dll created by the build.
By doing that you'll need to manually configure Polyglot using this code in your first cell.

``` csharp
#r "path_to_your_polyglot_folder\src\Polyglot.Interactive\bin\Debug\net5.0\Polyglot.Interactive.dll"
using Polyglot.Interactive;
using Polyglot.Core;
using Microsoft.DotNet.Interactive;
var kernelExtension = new KernelExtension();
var root = Kernel.Current.ParentKernel;
await kernelExtension.OnLoadAsync(root);
```

## Contributors

- **Antonio Bucchiarone** - Fondazione Bruno Kessler (FBK), Trento - Italy

- **Diego Colombo** - Microsoft Research, Cambridge, United Kingdom

- **Tommaso Martorella** - Microsoft Learn Student Ambassador

For any information, you can contact us by writing an email to bucchiarone@fbk.eu

