Pololu USB Software Development Kit

Release Date: 2014-06-04
http://www.pololu.com/


== Summary ==

This package contains the code you need for making your own
applications that control Pololu USB Devices.  All of the code
uses the devices' native USB interfaces (as opposed to their
virtual COM ports).  The supported devices are:

* Jrk USB Motor Controllers with Feedback:
  - Jrk 21v3 USB Motor Controller with Feedback (#1392)
  - Jrk 12v12 USB Motor Controller with Feedback (#1393)
* Maestro USB Servo Controllers:
  - Micro Maestro 6-Channel USB Servo Controller (#1350, #1351)
  - Mini Maestro 12-Channel USB Servo Controller (#1352, #1353)
  - Mini Maestro 18-Channel USB Servo Controller (#1354, #1355)
  - Mini Maestro 24-Channel USB Servo Controller (#1356, #1357)
* Simple Motor Controllers:
  - Simple Motor Controller 18v7 (#1372, #1373)
  - Simple High-Power Motor Controller 18v15 (#1376, #1377)
  - Simple High-Power Motor Controller 24v12 (#1378, #1379)
  - Simple High-Power Motor Controller 18v25 (#1381)
  - Simple High-Power Motor Controller 24v23 (#1383)
* USB AVR Programmer (#1300)

For more information, please contact us at www.pololu.com/contact


== Languages in this SDK ==

Most of the code here is written in C#, but there are some example
programs in Visual Basic .NET and in Visual C++.  All Visual Basic .NET
examples have a name ending in "Vb".  All Visual C++ examples have a
name ending in "Cpp".

All the source code and precompiled binaries here target .NET Framework
3.5, but they should work with later versions of the framework.

The Visual Studio files in this SDK were built using Visual Studio 2008.
If you open any of the projects or solutions with a newer version of
Visual Studio, it will walk you through the simple steps needed to
migrate that project or solution to work with the new version.


== Choosing a Programming Language ==

If you are not sure what language you want to use, we recommend C#.
It is modern and powerful, but relatively simple to understand.  Most
of the code in this SDK is written in C#.  All of the products
supported by this SDK come with example code written in C#.  All of
the C# code here runs under Windows and Linux.

The next best choices are Visual Basic .NET and Visual C++.  These
languages are part of the .NET Framework, so you can incorporate our
class libraries into your project the same way you would add any other
.NET assembly (see "Incorporating Class Libraries" below).  Some of our
products have VB and C++ examples, so you can compile them and then
modify them to suit your needs.

If you are using a language that is not part of .NET, then you will
not be able to directly run the code in this SDK, but you can use it
as a guide to figure out which USB commands you need to send to your
device.  You can then use WinUSB, libusb, or IOKit to send those
commands to your device.

If the options above sound challenging to you, we recommend that
you use your device's virtual USB COM port.  This SDK does not contain
any code to help you do that, but here is what you would do:  First,
find the documentation of your device's serial protocol by reading the
product's user's guide.  Next, make sure you have put your device in
the correct serial mode so it can receive commands from the COM port.
Finally, search Google for "serial port example [your language name]"
to find out how to send and receive bytes on the serial port.


== Directories ==

* Jrk: Code for communicating with the Jrk Motor Controller.
  - JrkExample: An example graphical client (C#).
  - JrkCmd: Command-line status and control utility (C#).
  - Jrk: Class library for USB commmunication (C#).

* Maestro: Code for communicating with the Maestro Servo Controller.
  - MaestroEasyExample: Example GUI with three buttons (C#).
  - MaestroEasyExampleVb: Example GUI with three buttons (VB).
  - MaestroEasyExampleCpp: Example GUI with three buttons (C++).
  - MaestroAdvancedExample: An example graphical client (C#).
  - UscCmd: Command-line status and control utility (C#).
  - Usc: Class library for USB communication (C#).
  - Sequencer: Class library for sequences of servo movements (C#).
  - Bytecode: Class library for compiling and representing scripts
    (binary only).

* SimpleMotorController: Code for communicating with the Simple Motor
  Controller.
  - SmcExample1: Example GUI with three buttons (C#).
  - SmcExample1Vb: Example GUI with three buttons (VB).
  - SmcExample1Cpp: Example GUI with three buttons (C++).
  - SmcExample2: Example GUI with a scroll bar (C#).
  - SmcCmd: Command-line status and control utility (C#).
  - Smc: Class library for USB communication (C#).

* UsbAvrProgrammer: Code for communicating with the USB AVR Programmer.
  - PgmCmd: Command-line status and configuration utility (C#).
  - Programmer: Class library for USB communication (C#).

* UsbWrapper_Windows: This directory contains low-level binary-only
  code for communicating with Pololu USB Devices in Windows.
  The code uses WinUSB, a driver that comes with Windows.  All example
  code for Windows depends on this library.

* UsbWrapper_Linux: Low-level code for communicating with Pololu USB
  Devices in Linux.  The code uses libusb-1.0.  All example code for
  Linux depends on this library.


== Getting Started with C#, C++ or Visual Basic in Windows ==

The C#, C++ and Visual Basic source code in this SDK can be compiled
in Windows using free development tools from Microsoft.  The SDK was
designed to be used with Visual C# 2008 Express, Visual C++ 2008
Express, or Visual Basic C# express, but it can also be used with
later versions of Visual Studio, such as Visual Studio Express 2013
for Windows Desktop.

To get started, download and install the version you want from
Microsoft's website:

   http://www.microsoft.com/express/Windows/

The code will also work with full, paid versions of Visual Studio.


== Compiling Jrk C# code in Windows ==

1) Open Jrk\Jrk.sln with Visual Studio.

2) In the Solution Explorer, right click on "JrkExample" and select
   "Set as Startup Project".  This setting will make compiling and
   debugging this project more convenient, and you can change it later.

3) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.

4) If you want to make a command line utility instead of a graphical
   application, set JrkCmd as the startup project and modify that.


== Compiling Maestro C# code in Windows ==

1) Open Maestro\Usc.sln with Visual Studio.

2) In the Solution Explorer, right click on "MaestroEasyExample" and
   select "Set as Startup Project".  This setting will make compiling
   and debugging this project more convenient, and you can change it
   later.

3) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.

You can also compile UscCmd (a command-line utility) and
MaestroAdvancedExample (a more advanced GUI) using the same procedure.


== Compiling Maestro Visual Basic code in Windows ==

1) Open Maestro\MaestroEasyExampleVb\MaestroEasyExampleVb.vbproj with
   Visual Studio.

2) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.


== Compiling Maestro Visual C++ code in Windows ==

1) Open Maestro\MaestroEasyExampleCpp\MaestroEasyExampleCpp.vcproj
   with Visual Studio.

2) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.


== Compiling USB AVR Programmer C# code in Windows ==

1) Open UsbAvrProgrammer\UsbAvrProgrammer.sln with Visual Studio.

2) In the Build menu, select "Build Solution" (or press F7).  This
   will compile UsbAvrProgrammer\PgmCmd\bin\Debug\PgmCmd.exe, which
   you can run from the Command Prompt.  If the program works for you,
   you have succeeded in building it from source.  You can now modify
   it to suit your needs.


== Compiling Simple Motor Controller C# code in Windows ==

1) Open SimpleMotorController\Smc.sln with Visual Studio.

2) In the Solution Explorer, right click on "SmcExample1" and select
   "Set as Startup Project".  This setting will make compiling and
   debugging this project more convenient, and you can change it later.

3) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.

You can also compile SmcCmd (a command-line utility) and SmcExample2
(a more advanced GUI with a scroll bar) using the same procedure.


== Compiling Simple Motor Controller Visual Basic code in Windows ==

1) Open SimpleMotorController\SmcExample1Vb\SmcExample1Vb.vbproj with
   Visual Studio.

2) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.


== Compiling Simple Motor Controller Visual C++ code in Windows ==

1) Open SimpleMotorController\SmcExample1Cpp\SmcExample1Cpp.vcproj
   with Visual Studio.

2) In the Debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.


== Compiling C# code in Linux ==

1) Copy the file 99-pololu.rules to /etc/udev/rules.d/ in order to
   grant permission for all users to use Pololu USB devices.  Run

     sudo udevadm control --reload-rules

   to make sure the rules get reloaded.  If you already plugged in
   a Pololu USB device, you should unplug it at this point so the new
   permissions will get applied later when you plug it back in.

2) Download and install these packages:

     libusb-1.0-0-dev mono-gmcs mono-devel libmono-winforms2.0-cil

   In Ubuntu, you can do this with the command:

     sudo apt-get install libusb-1.0-0-dev mono-gmcs mono-devel libmono-winforms2.0-cil

3) In the top-level directory of this SDK, type "make".  This will
   build all the programs and libraries in the SDK.  Now you should be
   able to run the programs using commands like:

     ./Maestro/MaestroEasyExample/MaestroEasyExample
     ./SimpleMotorController/SmcExample1/SmcExample1

   If you get an error message that says "cannot execute binary file",
   then try running the program with the mono runtime, for example:

     mono ./Maestro/UscCmd/UscCmd

   You can now modify these programs or create your own programs.



== Incorporating Class Libraries ==

The .NET Framework supports many languages, including C#, Visual Basic
.NET, Visual C++, and F#.  Any .NET project can call code from compiled
.NET assemblies (DLL files), regardless of what language the assembly
was written with.  This means that if you are writing a program in a
.NET language, you can incorporate our code into your project and use
it to communicate with your device, the same way you would integrate
any other .NET assembly.

First, find the DLL files you will need.  You will need UsbWrapper.dll
and also all the class libraries for your device.  You may need to use
Visual Studio to compile the DLLs if they are not available in a
precompiled form (look for a precompiled_obj folder).

Next, in your project's properties, add references to those DLL files.
You will need to add the library and all of its dependencies (e.g.
UsbWrapper.dll).

In your source files, it is also a good idea to add "Imports Pololu..."
(VB), or "using Pololu..." (C#), or "using namespace Pololu::..." (C++)
statements so that you can import the portions of the Pololu namespace
that you need.  (The ellipses above should be replaced with the name of
a namespace in your code.)

When you are writing your code, you can use auto-complete to discover
which functions are available and what they do.  You can also look at
the example code in this SDK to figure out which functions you need to
call, even if it is in a different language than the one you are using.
