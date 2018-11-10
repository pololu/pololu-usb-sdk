/*  CHANGELOG:
 *  2010-05-26 for version 1.3.0.0:
 *      Fixed PInvoke signatures:
 *          SetupDiEnumDeviceInterfaces
 *              DeviceInfoData changed from Int32 to IntPtr
 *              MemberIndex changed from Int32 to UInt32
 *          SetupDiGetClassDevs
 *              Enumberator chagned from String to "[MarshalAs(UnmanagedType.LPTStr)] String"
 *              hwndParent changed from Int32 to IntPtr
 *              Flags changed from Int32 to UInt32
 *          CM_Get_Parent
 *              ulFlags changed from UInt64 to UInt32
 *          CM_Get_Device_IDW
 *              BufferLen changed from UInt64 to UInt32
 *              ulFlags changed from UInt64 to UInt32
 *      Removed the CloseHandle PInvoke function in favor of SafeFileHandle.Close().
 *      CloseHandle caused a problem because when the garbage collector finalizes
 *      a a SafeFileHandle that was previously closed with CloseHandle it raises
 *      an exception (SEHException or IOError).  This was not a problem prior to
 *      .NET Framework 4.
 *
 *   2010-09-20: Added support for longer serial numbers.
 *   2011-04-18: Fixed a memory leak in getDeviceInstanceId() by adding a call to Marshal.FreeHGlobal.
 *               Fixed a memory leak in connect().
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Pololu.WinusbHelper
{
    /// <summary>
    /// WinUsbDeviceHandles:  This struct holds all the handles we need for
    /// fully-featured use of a WinUsb device.  WinUsbHelper has static functions
    /// for creating these handles (connect) and destroying them (disconnect).
    /// </summary>
    internal struct WinUsbDeviceHandles
    {
        /// <summary>
        /// winusbHandle allows us to call function from the WinUsb api to read
        /// and write write data form the device.  It is created by
        /// WinUsb_Initialize and must be destoryed with WinUsb_Free.
        /// </summary>
        internal IntPtr winusbHandle;

        /// <summary>
        /// deviceHandle is created by CreateFile and destroyed with CloseHandle.
        /// The only reason we store it is so that we can close it with
        /// CloseHandle when we are done using the device, which allows other
        /// programs (or this program) to subsequently use the device.
        /// </summary>
        internal SafeFileHandle deviceHandle;

        /// <summary>
        /// The deviceInstance is a number that identifies the device in the
        /// configuration manager (CM) API functions that we use to get
        /// the serial number and traverse the device tree.
        /// </summary>
        internal Int32 deviceInstance;
    }

    /// <summary>
    /// WinUsbHelper contains functions for connecting and disconnecting from
    /// WinUsbDevices using WinUsbDeviceHandles structs.
    /// </summary>
    internal static class Winusb
    {
        /// <summary>
        /// Connects to a device that has the specified GUID.
        /// </summary>
        /// <param name="deviceInterfaceGuid">GUID from the INF file.</param>
        /// <returns>Handles needed to communicate with the device.</returns>
        internal static WinUsbDeviceHandles connect(Guid deviceInterfaceGuid)
        {
            IntPtr listHandle = listCreate(deviceInterfaceGuid);
            try
            {
                return listConnect(listHandle, 0, deviceInterfaceGuid);
            }
            finally
            {
                listDestroy(listHandle);
            }
        }

        /// <summary>
        /// Creates a list of devices with the specified GUID that are currently
        /// connected to the computer.  The handles returned by this function
        /// must be destroyed when it is no longer in use, using the
        /// listDestroy function.
        /// </summary>
        /// <param name="deviceInterfaceGuid">GUID from the INF file.</param>
        /// <returns>A list handle that can be used in listConnect and
        /// other list-oriented functions.</returns>
        internal static IntPtr listCreate(Guid deviceInterfaceGuid)
        {
            IntPtr listHandle = SetupDiGetClassDevs(ref deviceInterfaceGuid,
                null, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

            if (listHandle == new IntPtr(-1))  // INVALID_HANDLE_VALUE
            {
                throw new Win32Exception("Unable to create a list of connected devices.");
            }

            return listHandle;
        }

        /// <summary>
        /// Returns the number of devices in the given list.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        /// <returns>The number of devices in the list.</returns>
        internal static UInt32 listSize(IntPtr listHandle)
        {
            UInt32 index = 0;

            // We only iterate up to 255 because we don't want to get stuck in
            // a loop that takes a long time for reason.  This should not be a
            // problem, because the USB standard only allows for 127
            // simultaneously connected devices.
            for (index = 0; index < 255; index++)
            {
                SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
                deviceInfoData.cbSize = (UInt32)Marshal.SizeOf(deviceInfoData);
                Boolean result = SetupDiEnumDeviceInfo(listHandle, index, ref deviceInfoData);

                if (!result)
                {
                    if (Marshal.GetLastWin32Error() != ERROR_NO_MORE_ITEMS)
                    {
                        throw new Win32Exception("Unable to access device number " + index.ToString() + ".");
                    }

                    return index;
                }
            }

            return index;
        }

        /// <summary>
        /// A device registry property code from setupapi.h.
        /// </summary>
        const UInt32 SPDRP_DEVICEDESC = 0;

        /// <summary>
        /// Gets the name of the device as shown in the device manager.
        /// </summary>
        internal static unsafe String listGetName(IntPtr deviceListHandle, byte index)
        {
            // Assumption: The string will be less than 128 characters (256 bytes).
            // If this is not true in the future, we can use the RequiredSize parameter
            // to first determine the size of the string, then allocate the memory.

            SP_DEVINFO_DATA deviceInfoData = listGetDevinfoData(deviceListHandle, index);
            Byte[] buffer = new Byte[256];
            fixed (Byte * b = &buffer[0])
            {
                Boolean result = SetupDiGetDeviceRegistryProperty(deviceListHandle, ref deviceInfoData,
                    SPDRP_DEVICEDESC, IntPtr.Zero, b, 256, IntPtr.Zero);
                if (!result)
                {
                    throw new Win32Exception("There was an error getting the name.");
                }
                return new String((char*)b);
            }
        }

        /// <summary>
        /// Gets the serial number of the specified device in the list.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        /// <param name="index">The zero-based index of the device.</param>
        /// <returns>The serial number, as a string.</returns>
        internal static String listGetSerialNumber(IntPtr listHandle, UInt32 index)
        {
            return getSerialNumber(listGetDeviceInstance(listHandle, index));
        }

        /// <summary>
        /// Gets the SP_DEVINFO_DATA struct for the given entry in the list.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        /// <param name="index">The zero-based index of the device.</param>
        /// <returns></returns>
        private static SP_DEVINFO_DATA listGetDevinfoData(IntPtr listHandle, UInt32 index)
        {
            SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
            deviceInfoData.cbSize = (UInt32)Marshal.SizeOf(deviceInfoData);
            Boolean result = SetupDiEnumDeviceInfo(listHandle, index, ref deviceInfoData);
            if (!result)
            {
                throw new Win32Exception("Unable to access device info data for device number " + index.ToString() + ".");
            }
            return deviceInfoData;
        }

        /// <summary>
        /// Gets the device instance (DEVINST) for the specified device in the list.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        /// <param name="index">The zero-based index of the device.</param>
        /// <returns></returns>
        internal static Int32 listGetDeviceInstance(IntPtr listHandle, UInt32 index)
        {
            return listGetDevinfoData(listHandle, index).DevInst;
        }

        /// <summary>
        /// Gets the serial number of a device.  Works for composite and
        /// non-composite devices.
        /// </summary>
        /// <param name="deviceInstance">A device instance (DEVINST)</param>
        /// <returns></returns>
        internal static String getSerialNumber(Int32 deviceInstance)
        {
            // Determine whether this device is a composite child or if it is the
            // top-level device by looking at its device instance id.
            // deviceInstanceId will be like one of these:
            //   USB\VID_1FFB&PID_0081\12345678   (for the top-level device)
            //   USB\VID_1FFB&PID_0081&MI_04\6&304568CB&0&0004 (for a composite child)
            String deviceInstanceId = getDeviceInstanceId(deviceInstance);

            // Assumption: A real serial number will not contain "&MI_".
            if (deviceInstanceId.Contains("&MI_"))
            {
                // This device is a composite child.  We need to look at its
                // parent to get the serial number.
                Int32 parentDeviceInstance = getParentDeviceInstance(deviceInstance);
                deviceInstanceId = getDeviceInstanceId(parentDeviceInstance);
            }

            // Remove the first 22 characters to get the serial number.
            return deviceInstanceId.Substring(22);
        }

        /// <summary>
        /// Gets the product id of a device by reading its device instance ID.
        /// </summary>
        /// <param name="deviceInstance">A device instance (DEVINST)</param>
        /// <returns></returns>
        internal static UInt16 getProductID(Int32 deviceInstance)
        {
            String deviceInstanceId = getDeviceInstanceId(deviceInstance);

            // deviceInstanceId will be like this:
            // USB\VID_1FFB&PID_0081\...
            return UInt16.Parse(deviceInstanceId.Substring(17, 4), System.Globalization.NumberStyles.HexNumber);
        }

        internal static UInt16 listGetProductId(IntPtr deviceListHandle, UInt32 index)
        {
            return getProductID(listGetDeviceInstance(deviceListHandle, index));
        }

        /// <summary>
        /// Gets the vendor id of a device by reading its device instance ID.
        /// </summary>
        /// <param name="deviceInstance">A device instance (DEVINST)</param>
        /// <returns></returns>
        internal static UInt16 getVendorID(Int32 deviceInstance)
        {
            String deviceInstanceId = getDeviceInstanceId(deviceInstance);

            // deviceInstanceId will be like this:
            // USB\VID_1FFB&PID_0081\...
            return UInt16.Parse(deviceInstanceId.Substring(8, 4), System.Globalization.NumberStyles.HexNumber);
        }


        /// <summary>
        /// Gets the Device Instance Id string for the device
        /// (e.g. "USB\VID_1FFB&amp;PID_0081\00000000").
        /// </summary>
        /// <param name="deviceInstance">
        ///   The device instance (DEVINST) for the device.
        /// </param>
        /// <returns>The Device Instance Id string.</returns>
        private static String getDeviceInstanceId(Int32 deviceInstance)
        {
            UInt32 cresult;  // type CONFIGRET is UInt32

            // Allocate some memory to hold the parent's Device Instance ID string.
            // One hundred unicode characters ought to be enough.
            IntPtr buffer = Marshal.AllocHGlobal(100 * 2);
            try
            {
                if (buffer == IntPtr.Zero)
                {
                    throw new Win32Exception("Unable to allocate memory for the parent device's device instance ID string.");
                }

                // Get the parent's Device Instance ID string.
                cresult = CM_Get_Device_IDW(deviceInstance, buffer, 100, 0);
                if (cresult != CR_SUCCESS)
                {
                    throw new Win32Exception((Int32)cresult, "Unable to get the device instance id.");
                }

                // At this point, buffer is a 16-bit unicode string like this:
                // USB\VID_1FFB&PID_0081\00000000

                return Marshal.PtrToStringUni(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// Gets the device instance (DEVINST) of the parent device of the
        /// device specified by the given device instance.  This is useful for
        /// getting the parent device that represents a composite USB device,
        /// given the device instance one of the child devices.
        /// </summary>
        /// <param name="deviceInstance">The device instance of the child.</param>
        private static Int32 getParentDeviceInstance(Int32 deviceInstance)
        {
            UInt32 cresult;  // type CONFIGRET is UInt32
            Int32 parent;    // type DEVINST is Int32

            // Get the parent device's DEVINST handle.
            cresult = CM_Get_Parent(out parent, deviceInstance, 0);
            if (cresult != CR_SUCCESS)
            {
                throw new Win32Exception((Int32)cresult, "Unable to get the parent of the device.");
            }
            return parent;
        }

        /// <summary>
        /// Destroys the list of devices, freeing up the memory.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        internal static void listDestroy(IntPtr listHandle)
        {
            SetupDiDestroyDeviceInfoList(listHandle);
        }

        /// <summary>
        /// Connects to the device specified by the device instance
        /// and device interface guid.
        /// 
        /// This function throws Win32Exceptions but it also throws
        /// regular Exceptions, so be sure to catch both.
        /// </summary>
        /// <param name="deviceInterfaceGuid">
        /// The device interface GUID of the device (from the INF file).
        /// </param>
        /// <param name="deviceInstance">
        /// This is typically returned from listGetDeviceInstance.
        /// </param>
        /// <returns></returns>
        internal static WinUsbDeviceHandles connect(Guid deviceInterfaceGuid, Int32 deviceInstance)
        {
            IntPtr listHandle = listCreate(deviceInterfaceGuid);
            try
            {
                UInt32 size = listSize(listHandle);
                if (size == 0)
                {
                    throw new Exception("No devices were detected.");
                }

                for (Byte i = 0; i < size; i++)
                {
                    if (deviceInstance == listGetDeviceInstance(listHandle, i))
                    {
                        return listConnect(listHandle, i, deviceInterfaceGuid);
                    }
                }
                throw new Exception("None of the connected devices have the device instance id expected.");
            }
            finally
            {
                listDestroy(listHandle);
            }
        }

        /// <summary>
        ///   Connects to the specified device in the list.
        /// </summary>
        /// <param name="listHandle">
        ///   The handle of the list created with listCreate.
        /// </param>
        /// <param name="index">
        ///   The zero-based index of the device in the list to connect to.
        /// </param>
        /// <param name="deviceInterfaceGuid">GUID from the INF file.</param>
        /// <returns>Handles needed to communicate with the device.</returns>
        internal static unsafe WinUsbDeviceHandles listConnect(IntPtr listHandle, Byte index, Guid deviceInterfaceGuid)
        {
            WinUsbDeviceHandles handles = new WinUsbDeviceHandles();

            /* Get the device instance ****************************************/
            handles.deviceInstance = listGetDeviceInstance(listHandle, index);

            /* Get the DeviceInterfaceData struct *****************************/

            // DeviceInterfaceData is some info about the device in the list.
            SP_DEVICE_INTERFACE_DATA DeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();

            // Necessary according to http://msdn.microsoft.com/en-us/library/ms791242.aspx
            DeviceInterfaceData.cbSize = (UInt32)Marshal.SizeOf(DeviceInterfaceData);

            Boolean result = SetupDiEnumDeviceInterfaces(listHandle, IntPtr.Zero,
                ref deviceInterfaceGuid, index, ref DeviceInterfaceData);
            if (!result)
            {
                throw new Win32Exception("Unable to get the device interface data.");
            }

            /* Get the DeviceInterfaceDetailData struct ***********************/

            // RequiredSize is the size in bytes of the
            // DeviceInterfaceDetailData struct we want to get for the device.
            UInt32 RequiredSize = 0;
            result = SetupDiGetDeviceInterfaceDetail(listHandle, ref DeviceInterfaceData,
                IntPtr.Zero, 0, ref RequiredSize, IntPtr.Zero);
            if (!result && Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
            {
                throw new Win32Exception("Unable to get the size of the device interface detail.");
            }

            // Now that we know the size of the DeviceInterfaceDetailData struct,
            // we can allocate memory for it.
            IntPtr pDeviceInterfaceDetailData = Marshal.AllocHGlobal((Int32)RequiredSize);

            // Get the DeviceInterfaceDetailData.

            /* According to the MSDN, we must set cbSize (the first 4 bytes of
             * the array) to sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA).  On 32-bit
             * machines, this is 6, and on 64-bit machines, this is 8.  I'm not
             * sure why.
             */
              
            bool sixtyFourBit = (sizeof(IntPtr) == 8);

            Marshal.WriteInt32(pDeviceInterfaceDetailData, sixtyFourBit ? 8 : 6);

            result = SetupDiGetDeviceInterfaceDetail(listHandle, ref DeviceInterfaceData,
                pDeviceInterfaceDetailData, RequiredSize, ref RequiredSize, IntPtr.Zero);
            if (!result)
            {
                Marshal.FreeHGlobal(pDeviceInterfaceDetailData);
                throw new Win32Exception("Unable to get the device interface detail.");
            }

            /* Get the device handle ******************************************/

            // Get the address of the PDeviceInterfaceDetaildata->DevicePath.
            IntPtr pDevicePath = new IntPtr(pDeviceInterfaceDetailData.ToInt64() + 4);

            // Get a string object with the device path.
            // This is what a typical DevicePath looks like:
            // \\?\usb#vid_04d8&pid_da01#5&226425fa&0&2#{fe187157-e4cb-4c53-a1d6-e6040ff6896f}
            String devicePath = Marshal.PtrToStringAuto(pDevicePath);

            // Use the DevicePath to open a file handle.
            handles.deviceHandle = CreateFile(devicePath,
                GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            if (handles.deviceHandle.IsInvalid)
            {
                Int32 error = Marshal.GetLastWin32Error();
                Marshal.FreeHGlobal(pDeviceInterfaceDetailData);
                if (error == 5) // ERROR_ACCESS_DENIED
                {
                    throw new Win32Exception("Access denied when trying to open device.  Try closing all other programs that are using the device.");
                }
                else
                {
                    throw new Win32Exception(error, "Unable to create a handle for the device (" + devicePath + ").");
                }
            }
            Marshal.FreeHGlobal(pDeviceInterfaceDetailData);

            // Use DeviceHandle to make a Winusb Interface Handle.
            result = WinUsb_Initialize(handles.deviceHandle, ref handles.winusbHandle);
            if (!result)
            {
                Int32 error = Marshal.GetLastWin32Error();
                handles.deviceHandle.Close();
                throw new Win32Exception(error, "Unable to initialize WinUSB.");
            }

            // Set the timeout for control transfers to 350 ms.
            // It needs to be that long to support the control transfer that erases the entire script
            // on the Mini Maestros.
            // TODO: test to make sure this timeout is actually being applied correctly
            UInt32 timeout = 350;
            result = WinUsb_SetPipePolicy(handles.winusbHandle, 0, PIPE_TRANSFER_TIMEOUT, 4, &timeout);
            if (!result)
            {
                Int32 error = Marshal.GetLastWin32Error();
                Winusb.disconnect(handles);
                throw new Win32Exception(error, "Unable to set control transfer timeout.");
            }

            return handles;
        }

        /// <summary>
        ///     Frees the handles so that the device can be used by other
        ///     programs, or by this program later.
        /// </summary>
        /// <param name="handles">
        ///     Handles returned by some other WinUsbHelper function.
        /// </param>
        internal static void disconnect(WinUsbDeviceHandles handles)
        {
            WinUsb_Free(handles.winusbHandle);
            handles.deviceHandle.Close();
            // Device Instance does not need to be freed, its existence does
            // not imply a lock.
        }

        /// <summary>
        /// Registers your form to receive notifications from Windows when one
        /// of a particular class of devices is removed or attached.
        /// </summary>
        /// <param name="deviceInterfaceGuid">The device interface GUID from the INF file.</param>
        /// <param name="formHandle">The handle of the form that will receive notifications (form.Handle).</param>
        /// <returns>A handle representing this notification request.</returns>
        internal static unsafe IntPtr notificationRegister(Guid deviceInterfaceGuid, IntPtr formHandle)
        {
            DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();

            devBroadcastDeviceInterface.dbcc_size = (UInt32)sizeof(DEV_BROADCAST_DEVICEINTERFACE);
            devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            devBroadcastDeviceInterface.dbcc_classguid = deviceInterfaceGuid;

            IntPtr deviceNotificationHandle = RegisterDeviceNotification(formHandle,
                &devBroadcastDeviceInterface, DEVICE_NOTIFY_WINDOW_HANDLE);

            if (deviceNotificationHandle == IntPtr.Zero)
            {
                throw new Win32Exception("There was an error registering for device attachment/removal notifications.");
            }

            return deviceNotificationHandle;
        }

        /// <summary>
        /// Tells Windows to stop sending device notifications to your form.
        /// </summary>
        /// <param name="deviceNotificationHandle">The handle returned by notificationRegister.</param>
        internal static void notificationUnregister(IntPtr deviceNotificationHandle)
        {
            Boolean result = UnregisterDeviceNotification(deviceNotificationHandle);
            if (!result)
            {
                throw new Win32Exception("There was an error unregistering device attachment/removal notifications.");
            }
        }

        /// <summary>
        /// Returns a list of port names (e.g. "COM2", "COM3") for all
        /// currently-connected devices in the Ports list in the Device Manager
        /// whose device instance ID begins with the given prefix string.
        /// 
        /// For example, to get the port names of the umc01a bootloader,
        /// give a prefix of USB\PID_1FFB&amp;PID_0082
        /// </summary>
        /// <param name="deviceInstanceIdPrefix">
        /// The string that we match against the device instance ID.  The device
        /// instance ID of the device you want must begin with this string.
        /// </param>
        /// <returns></returns>
        internal static IList<String> getPortNames(String deviceInstanceIdPrefix)
        {
            deviceInstanceIdPrefix = deviceInstanceIdPrefix.ToUpperInvariant();

            // Get a list of all USB-to-serial adapter devices currently connected.
            IntPtr listHandle = SetupDiGetClassDevs(ref GUID_DEVCLASS_PORTS, "USB", IntPtr.Zero, DIGCF_PRESENT);
            if (listHandle == new IntPtr(-1))  // INVALID_HANDLE_VALUE
            {
                throw new Win32Exception("Unable to create a list of connected USB-to-serial adapters.");
            }

            UInt32 count = listSize(listHandle);
           
            IList<String> portNames = new List<string>();

            try
            {
                for (UInt32 i = 0; i < count; i++)
                {
                    SP_DEVINFO_DATA devinfoData = listGetDevinfoData(listHandle, i);
                    String deviceInstanceId = getDeviceInstanceId(devinfoData.DevInst);

                    if (!deviceInstanceId.ToUpperInvariant().StartsWith(deviceInstanceIdPrefix))
                    {
                        continue;
                    }

                    // We found a bootloader.

                    String portName = getCustomDeviceProperty(listHandle, devinfoData, "PortName");

                    portNames.Add(portName);
                }
            }
            finally
            {
                // Even if an exception happens above, we want to free the list
                // handle.
                SetupDiDestroyDeviceInfoList(listHandle);
            }
            
            return portNames;
        }

        /// <summary>
        /// Gets a custom property of a device from the registry
        /// using SetupDiGetCustomDeviceProperty.
        /// 
        /// For example, if the device instance ID of the device is
        /// USB\VID_1FFB&amp;PID_0081&amp;MI_02\6&amp;19484c5e&amp;0&amp;0002 then this function will read:
        ///     HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Enum\USB\VID_1FFB&amp;PID_0081&amp;MI_02\6&amp;19484c5e&amp;0&amp;0002\Device Parameters\PortName
        /// </summary>
        /// <returns></returns>
        private static unsafe string getCustomDeviceProperty(IntPtr listHandle, SP_DEVINFO_DATA devinfoData, String propertyName)
        {
            // usbser.sys stores the portname in the registry in the
            // "Device Parameters" folder.

            Byte[] buffer = new Byte[100];

            Boolean result;

            fixed (Byte* b = &buffer[0])
            {
                result = SetupDiGetCustomDeviceProperty(listHandle, ref devinfoData, propertyName, 0, (UInt32*)0, (Byte*)b, 100, (UInt32*)0);

                if (!result)
                {
                    throw new Win32Exception("There was an error getting the port name.");
                }

                return new String((char*)b);
            }
        }

        internal const UInt32 WM_DEVICECHANGE = 0x219; 

        // From Devguid.h:
        static Guid GUID_DEVCLASS_PORTS = new Guid(0x4D36E978, 0xE325, 0x11CE, 0xBF, 0xC1, 0x08, 0x00, 0x2B, 0xE1, 0x03, 0x18);

        const UInt32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        const UInt32 DBT_DEVTYP_DEVICEINTERFACE = 5;

        const UInt16 FILE_ATTRIBUTE_NORMAL = 0x80;
        const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        const UInt16 FILE_SHARE_READ = 0x1;
        const UInt16 FILE_SHARE_WRITE = 0x2;
        const UInt32 GENERIC_READ = 0x80000000;
        const UInt32 GENERIC_WRITE = 0x40000000;
        const UInt16 OPEN_EXISTING = 3;

        // From cfgmgr32.h:
        const UInt32 CR_SUCCESS = 0;

        // From setupapi.h:
        const UInt16 DIGCF_PRESENT = 0x2;
        const UInt16 DIGCF_DEVICEINTERFACE = 0x10;

        // From http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx :
        const UInt32 ERROR_INSUFFICIENT_BUFFER = 122;
        const UInt32 ERROR_NO_MORE_ITEMS = 0x103;

        // For WinUsb_SetPipePolicy, from http://msdn.microsoft.com/en-us/library/ff540304%28v=vs.85%29.aspx :
        const UInt32 PIPE_TRANSFER_TIMEOUT = 3;

        [StructLayout(LayoutKind.Sequential)]
        struct DEV_BROADCAST_DEVICEINTERFACE
        {
            internal UInt32 dbcc_size;
            internal UInt32 dbcc_devicetype;
            internal UInt32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        } 

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVINFO_DATA
        {
            internal UInt32 cbSize;
            internal Guid ClassGuid;
            internal Int32 DevInst;
            internal IntPtr Reserved; // WARNING: the size of this part might be dependent on the architecture!
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            internal UInt32 cbSize;
            internal Guid InterfaceClassGuid;
            internal UInt32 Flags;
            internal IntPtr Reserved;  // WARNING: the size of this part might be dependent on the architecture!
        }

        [StructLayout(LayoutKind.Sequential, Pack=1)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal UInt32 cbSize;
            internal UInt16 character; // this approximates a character
        }


        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, UInt32 index, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, UInt32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, [MarshalAs(UnmanagedType.LPTStr)] String Enumerator, IntPtr hwndParent, UInt32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, UInt32 DeviceInterfaceDetailDataSize, ref UInt32 RequiredSize, IntPtr DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern unsafe Boolean SetupDiGetCustomDeviceProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, String CustomPropertyName, UInt32 Flags, UInt32* PropertyRegDataType, Byte* PropertyBuffer, UInt32 PropertyBufferSize, UInt32* RequiredSize);

        /// <summary>
        /// This function returns error code 0x80004005 when you give it an incorrect Property argument.
        /// </summary>
        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern unsafe Boolean SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, IntPtr PropertyRegDataType, Byte* PropertyBuffer, UInt32 PropertyBufferSize, IntPtr RequiredSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_Initialize(SafeFileHandle DeviceHandle, ref IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern unsafe Boolean WinUsb_SetPipePolicy(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, void* value);

        [DllImport("cfgmgr32.dll", SetLastError = true)]
        static extern UInt32 CM_Get_Parent(out Int32 pdnDevInst, Int32 dnDevInst, UInt32 ulFlags);

        [DllImport("cfgmgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern UInt32 CM_Get_Device_IDW(Int32 dnDevInst, IntPtr Buffer, UInt32 BufferLen, UInt32 ulFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern unsafe IntPtr RegisterDeviceNotification(IntPtr hRecipient, DEV_BROADCAST_DEVICEINTERFACE* NotificationFilter, UInt32 Flags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern Boolean UnregisterDeviceNotification(IntPtr Handle);
    }

}