Pololu USB Software Development Kit

Release Date: 2009-12-14
http://www.pololu.com/


== Summary ==

This package contains the code you need for making your own
applications that control Pololu USB Devices.  Most of the code is
written in C#.  Most of the code uses the devices' native USB
interfaces (as opposed to their virtual COM ports).  The supported
devices are:

* USB AVR Programmer (#1300)
* Micro Maestro 6-channel USB Servo Controller (#1350, #1351)

Support for other Pololu USB devices is planned.  For more
information, please contact Pololu.


== Directories ==

* UsbWrapper_Windows: This directory contains low-level code for
  communicating with Pololu USB Devices in Windows (binary only).

* UsbWrapper_Linux: Low-level code for communicating with Pololu USB
  Devices in Linux.

* UsbAvrProgrammer: Code for communicating with the USB AVR Programmer.
  - PgmCmd: Command-line status and configuration utility.
  - Programmer: Class library for USB communication.

* Maestro: Code for communicating with the Maestro Servo Controller.
  - UscCmd: Command-line status and control utility.
  - MaestroExample: An example graphical client.
  - Usc: Class library for USB communication.
  - Sequencer: Class library for sequences of servo movements.
  - Bytecode: Class library for compiling and representing scripts
    (binary only).

== Getting Started in Windows ==

Our C# source code can be compiled with Visual Studio C# Express, a
free C# development environment from Microsoft, available at:

   http://www.microsoft.com/express/vcsharp/

Our code will also work with full, paid versions of Visual Studio, if
that is an option for you.


== Getting Started with the USB AVR Programmer in Windows ==

1) Open UsbAvrProgrammer\UsbAvrProgrammer.sln with Visual Studio.

2) In the Build menu, select "Build Solution" (or press F7).  This
   will compile UsbAvrProgrammer\PgmCmd\bin\Debug\PgmCmd.exe, which
   you can run from the Command Prompt.  If the program works for you,
   you have succeeded in building it from source.  You can now modify
   it to suit your needs.


== Getting Started with the Maestro in Windows ==

1) Open Maestro/Usc.sln with Visual Studio.

2) In the Solution Explorer, right click on "MaestroExample" and
   select "Set as Startup Project".  This will make compiling and
   debugging this project more convenient; you may change the startup
   project when you are working on a different project or adding your
   own project to the solution.

3) In the debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.

4) If you want to make a command line utility instead of a graphical
   application, set UscCmd as the startup project and modify that.

== Getting Started in Linux ==

1) Copy the file 99-pololu.rules to /etc/udev/rules.d/ in order to grant
   permission for all users to use Pololu USB devices.  If you already
   plugged in a Pololu USB device, you should unplug it at this point so
   the new permissions will get applied later when you plug it back in.

2) Download and install these packages:

     libusb-1.0-0-dev mono-gmcs mono-devel libmono-winforms2.0-cil

   In Ubuntu, you can do this with the command:

     sudo apt-get install libusb-1.0-0-dev mono-gmcs mono-devel libmono-winforms2.0-cil

3) In the top level directory of this SDK, type "make".  This will
   build all the programs and libraries in the SDK.  Now you should be
   able to run the programs using:

     ./Maestro/MaestroExample/MaestroExample.sh
     ./Maestro/UscCmd/UscCmd.sh
     ./UsbAvrProgrammer/PgmCmd/PgmCmd.sh

   You can now modify these programs or create your own programs.
