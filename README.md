# Polyglot

A Gamified .NET Interactive Framework for Teaching and Learning Multi-Language Programming for High School and Undergraduate Students

Currently, Polyglot works with C# and [SysML V2](https://www.omgsysml.org/SysML-2.htm) but we're working on the support for the other .NET Interactive languages (F#, Javascript, ...) as well.

See it in action in the video below!

## Polyglot Requirements

1. .NET 5 - [download it here](https://dotnet.microsoft.com/download)
2. .NET Interactive Notebooks extension for Visual Studio Code

## SysML Kernel requirements

1. JDK for Java >= 11
2. Graphviz - [download it here](https://graphviz.org/download/)

## How to run the demo

1. Clone the repo
2. Unzip the compressed folder ```out.zip``` from ```resources```
3. Set the environment variable ```SYSML_JAR_PATH``` to the path where you extracted the folder (e.g. if you unzipped the folder in ```C:\Users\myUser\Documents```, then ```SYSML_JAR_PATH``` will be ```C:\Users\myUser\Documents\out```)
4. Open ```demo sysml.ipynb``` from the folder ```notebooks``` using Visual Studio Code
5. Run the first cell to import POLYGLOT from NuGet
6. Run the second cell to setup the POLYGLOT engine with the ```#!start-game``` command
7. Follow the instructions in the notebook to complete the exercise!

> **_NOTE:_**  If you want to execute the exercise another time you need to change the player-id in the start-game command and re-execute the cell

## Contributors

- **Antonio Bucchiarone** - Fondazione Bruno Kessler (FBK), Trento - Italy

- **Diego Colombo** - Microsoft Research, Cambridge, United Kingdom

- **Tommaso Martorella** - Microsoft Learn Student Ambassador

For any information, you can contact us by writing an email to bucchiarone@fbk.eu
