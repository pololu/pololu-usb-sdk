Pololu USB Software Development Kit

Release Date: YYYY-MM-DD
http://www.pololu.com/


== Summary ==

This package contains the code you need for making your own
applications that control Pololu USB Devices.  Most of the code is
written in C#.  Most of the code uses the devices' native USB
interfaces (as opposed to their virtual COM ports).  The supported
devices are:

* USB AVR Programmer (#1300)
* Micro Maestro 6-Channel USB Servo Controller (#1350, #1351)
* Mini Maestro 12-Channel USB Servo Controller (#1352, #1353)
* Mini Maestro 18-Channel USB Servo Controller (#1354, #1355)
* Mini Maestro 24-Channel USB Servo Controller (#1356, #1357)
* Jrk 21v3 USB Motor Controller with Feedback (#1392)
* Jrk 12v12 USB Motor Controller with Feedback (#1393)

For more information, please contact Pololu.


== Directories ==

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

* Jrk: Code for communicating with the Jrk Motor Controller.
  - JrkCmd: Command-line status and control utility.
  - JrkExample: An example graphical client.
  - Jrk: Class library for USB commmunication.

* UsbWrapper_Windows: This directory contains low-level (binary-
  only) code for communicating with Pololu USB Devices in Windows.
  The code uses winusb, a driver that comes with Windows.

* UsbWrapper_Linux: Low-level code for communicating with Pololu USB
  Devices in Linux.  The code uses libusb-1.0.


== Getting Started in Windows ==

Our C# source code can be compiled with Visual Studio C# Express, a
free C# development environment from Microsoft, available at:

   http://www.microsoft.com/express/vcsharp/

Our code will also work with full, paid versions of Visual Studio, if
that is an option for you.

The Visual Studio files in this SDK were built using Visual Studio 2008.
If you open any of the projects or solutions with a newer version of
Visual Studio, it will walk you through the simple steps needed to
migrate that project or solution to work with the new version.


== Getting Started with the USB AVR Programmer in Windows ==

1) Open UsbAvrProgrammer\UsbAvrProgrammer.sln with Visual Studio.

2) In the Build menu, select "Build Solution" (or press F7).  This
   will compile UsbAvrProgrammer\PgmCmd\bin\Debug\PgmCmd.exe, which
   you can run from the Command Prompt.  If the program works for you,
   you have succeeded in building it from source.  You can now modify
   it to suit your needs.


== Getting Started with the Maestro in Windows ==

1) Open Maestro\Usc.sln with Visual Studio.

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


== Getting Started with the Jrk in Windows ==

1) Open Jrk\Jrk.sln with Visual Studio.

2) In the Solution Explorer, right click on "JrkExample" and
   select "Set as Startup Project".  This will make compiling and
   debugging this project more convenient; you may change the startup
   project when you are working on a different project or adding your
   own project to the solution.

3) In the debug menu, select "Start Debugging" (or press F5).  If you
   see the example start up and open a window, this means that you
   have succeeded in building the example from source.  You can now
   modify it to suit your needs.

4) If you want to make a command line utility instead of a graphical
   application, set JrkCmd as the startup project and modify that.

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

     ./UsbAvrProgrammer/PgmCmd/PgmCmd
     ./Maestro/UscCmd/UscCmd
     ./Maestro/MaestroExample/MaestroExample
     ./Jrk/JrkCmd/JrkCmd
     ./Jrk/JrkExample/JrkExample

   If you get an error message that says "cannot execute binary file",
   then try running the program with the mono runtime, for example:

     mono ./Maestro/UscCmd/UscCmd

   You can now modify these programs or create your own programs.


== Text Display Problem in Ubuntu 9.10 ==

If you have compiled and run the example graphical programs in Linux,
and some of the UI text is missing, your problem might be caused by a
bug in a graphics driver that comes with Ubuntu 9.10 (Karmic Koala).

The driver is in the package xserver-xorg-video-intel.  The driver is
for the Intel i8xx and i9xx family of chipsets, including i810, i815,
i830, i845, i855, i865, i915, i945 and i965 series chips.  You can see
if your computer has one of those chipsets by running `lspci` and
finding your graphics card.

Version 2.9.0 of the driver is known to have a bug.
Upgrading to Version 2.9.1 seems to fix the bug.

You can determine what version of the driver you have by running:

  dpkg -l | grep xserver-xorg-video-intel

You can determine what version of the driver your X.org server is
actually using by looking in /var/log/Xorg.0.log for a message like:

  (II) Module intel: vendor="X.Org Foundation"
          compiled for 1.6.4, module version = 2.9.1
          Module class: X.Org Video Driver
          ABI class: X.Org Video Driver, version 5.0


One way to fix the problem is to compile the new version of the driver
(2.9.1) from source and use it.  Here are the instructions for doing
that:

1) Go to http://xorg.freedesktop.org/archive/individual/driver/
   and get the latest version of the xf86-video-intel driver.
   At the time of this writing (2009-12-23), the latest version
   was xf86-video-intel-2.9.1.tar.gz.

2) Unzip the archive and install the archive by running:

     tar -xzvf xf86-video-intel-2.9.1.tar.gz

3) Install the required dev packages:

     sudo apt-get install xserver-xorg-dev libdrm-dev x11proto-gl-dev x11proto-xf86dri-dev

4) Install the driver by running:

     cd xf86-video-intel-2.9.1
     ./configure
     make
     sudo make install

   This installs the drivers to /usr/local/lib/xorg/modules/drivers,
   but that is not the location where the X.org server searches for
   drivers.

5) (Optional) Make a backup up the existing drivers so that if
   something goes wrong you can revert to them:
     cp -Ri /usr/lib/xorg/modules/drivers ~/backup_xorg_drivers
 
6) Close all of your graphical applications and log out because we must
   temporarily shut down the Gnome Desktop Manager (gdm).

7) Go to a text console by pressing Ctrl+Alt+F1 or Ctrl+Alt+F2.

8) Run these commands:
     sudo service gdm stop
     sudo cp /usr/local/lib/xorg/modules/drivers/* /usr/lib/xorg/modules/drivers
     sudo service gdm start

9) You should see the Ubuntu log-in screen appear.  If it does not, try
   pressing Ctrl+Alt+F7.  The graphic driver bug should now be fixed!

For more information, see:
https://bugzilla.novell.com/show_bug.cgi?id=549882
https://bugs.launchpad.net/ubuntu/+source/xserver-xorg-video-intel/+bug/462349
http://intellinuxgraphics.org/


