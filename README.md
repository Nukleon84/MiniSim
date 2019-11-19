# MiniSim
A Minimalistic chemical process simulator based on the IKCAPE thermodynamic specifications.

The MiniSim Library is a collection of C# libraries that can be used to simulate stationary chemical processes using mass- and energy balances. The library includes a basic implementation of the IKCAPE Thermodynamics and a handful of simple unit operations. The resulting equation system is solved simulatenously using a Newton-type solver using the L/U decomposition provided in CSPARSE.net.

The library is designed to be hosted inside Jupyter notebook. In contrast to the Open Flowsheeting and Modeling Library, this is not a standalone application. I have decided that I do not want to reinvent the wheel and use Jupyter Notebook for the actual applications.

This program was build for fun and the enjoyment of exploring process simulation tools. It was heavily inspired by the PhD thesis of K.Lau and G. Varma. These two doctoral thesises were written in the early 90ies and explore the possibility of an object-oriented flowsheet simulator. Now, nearly 25 years later modern software development tools make it possible for even a single person to write such an application.

Development of a process simulator using object oriented programming: Information modeling and program structure Gadiraju V. Varma https://lib.dr.iastate.edu/cgi/viewcontent.cgi?article=11354&context=rtd

Development of a process simulator using object oriented programming: Numerical procedures and convergence studies Kheng Hock Lau https://lib.dr.iastate.edu/cgi/viewcontent.cgi?article=11324&context=rtd

The thermodynamics methods implemented in this libary are part of the IKCAPE thermodynamics. I reimplemented the equations in my own modeling framework. I also use the neutral input format described in their user guide as an input format. http://dechema.de/en/IK_CAPE+THERMO-p-40.html

# Intended Use-Cases

MiniSim will probably never feature a GUI or graphical flowsheeting environment. Instead I want to focus on automation scenarios, and provide a simulator that is ready for digitalization applications. 

Although MiniSim is right now compiled with the .net Framework, it is fully compatible with .net Core and will remain so in the future. This would allow the project to be compiled with Mono on Linux systems. 

From there, many opportunities arise, from deployed soft-sensors running in Docker containers, over simulation micro-service architectures and thermodynamic property calculation REST APIs to Jupyter Notebooks combining process data with simple supplementary models, which would not neccessitate the use of a full-blown process simulator.


# Quick Start

In the \doc folder you will find some Jupyter Notebooks that show how the library is to be used in order to simulate chemical processes.

# Prerequisites

* Visual Studio (Community edition is free to use for open-source software)   
* MiniSim integrates with the ChemSep database. The pure component database is included in the repository. If you want to use the binary interaction parameters, you need to copy the *.ipd files from your local copy of ChemSep to the \bin\PropertyDatabase\ChemSep folder

# Set up

I reduced the complexity of the solution as much as possible and now the entire simulator fits in one solution. After cloning the repository, you should be able to modify & build the code after restoring the nuget packages.    
    
