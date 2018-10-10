using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("UsbWrapper")]
[assembly: AssemblyDescription("Handles low-level USB communication in Windows.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Pololu")]
[assembly: AssemblyProduct("UsbWrapper")]
[assembly: AssemblyCopyright("Copyright © Pololu 2009-2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("de1c9e0b-35da-4c80-b7fa-52cc0396587f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.5.2")]
[assembly: AssemblyFileVersion("1.5.2")]


// Changelog:
// 2010-02-12: Changed from version 1.1.0.0 to 1.2.0.0.
// 2010-02-12: Made controlTransfer() return lengthTransferred for control transfers with a data stage!
// 2010-03-30: Changed from 1.2.0.0 to 1.2.0.1, made getPortNames(String deviceInstanceIdPrefix)
//    be case-insensitive.
// 2010-05-26: Fixed two bugs revealed by trying to run our code in Visual
//    Studio 2010 while targeting the .NET Framework 4.  See changelog in
//    WinusbHelper.cs for details.  Changed version from 1.2.2.0 to 1.3.0.0.
// 2010-09-20: Added support for longer serial numbers.  Changed version from 1.3.0.0 to 1.4.0.
// 2011-04-18: Fixed three memory leaks. Changed from version 1.4.0 to 1.4.1.
// 2011-04-21: Switched from asynchronous to synchronous control transfers.
//             One side effect is that this fixes the CreateEvent handle leak.
//             Changed version from 1.4.1 to 1.5.0.
// 2014-01-14: Fixed a bug that was preventing this from running in a 64-bit process.
//             We now call IntPtr.ToInt64() instead of IntPtr.ToInt32().
//             Changed version from 1.5.0 to 1.5.1.
// 2018-10-10, v1.5.2: Fixed the reporting of errors from CreateFile.