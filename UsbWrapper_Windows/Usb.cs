using System;
using System.Collections.Generic;
using Pololu.WinusbHelper;

namespace Pololu.UsbWrapper
{
    /// <summary>
    /// A static class that has some methods for interacting with the operating
    /// system's USB support.
    /// </summary>
    public static class Usb
    {
        /// <summary>
        /// Returns a list of port names (e.g. "COM2", "COM3") for all
        /// currently-connected devices in the Ports list in the Device Manager
        /// whose device instance ID begins with the given prefix string.
        /// 
        /// For example, to get the port names of the umc01a bootloader,
        /// give a prefix of = USB\PID_1FFB&amp;PID_0082.
        /// </summary>
        /// <param name="deviceInstanceIdPrefix">
        /// The string that we match against the device instance ID.  The device
        /// instance ID of the device you want must begin with this string.
        /// </param>
        /// <returns></returns>
        public static IList<String> getPortNames(String deviceInstanceIdPrefix)
        {
            return Winusb.getPortNames(deviceInstanceIdPrefix);
        }

        /// <summary>
        /// Return true if the operating system supports notifying forms
        /// when a device is connected or disconnect from the system.
        /// Currently returns true for Windows, false for Linux.
        /// See notificationRegister for details.
        /// </summary>
        public static bool supportsNotify { get { return true; } }

        /// <summary>
        /// A constant needed for processing device change messages in Windows.
        /// See notificationRegister for details.
        /// </summary>
        /// <value>0x219</value>
        /// <example>
        /// protected override void WndProc(ref Message m)
        /// {
        ///    if (m.Msg == Usb.WM_DEVICECHANGE)
        ///    {
        ///        // ... insert your own code here to deal with device changes ...
        ///    }
        ///
        ///    base.WndProc(ref m);
        /// }
        /// </example>
        public static uint WM_DEVICECHANGE = Winusb.WM_DEVICECHANGE;

        /// <summary>
        /// Registers your form to receive notifications from the OS when one
        /// of a particular class of devices is removed or attached.  See
        /// Usb.WM_DEVICECHANGE for example code for receiving notifications.
        /// You should only call this if supportsNotify returns true or your
        /// application will only be run on Windows.
        /// </summary>
        /// <param name="guid">The device interface GUID of the
        /// device you are interested in (from the INF file).</param>
        /// <param name="handle">The handle of the form that will receive
        /// notifications (form.Handle).</param>
        /// <returns>A handle representing this notification request.</returns>
        public static IntPtr notificationRegister(Guid guid, IntPtr handle)
        {
            return Winusb.notificationRegister(guid, handle);
        }
    }
}
