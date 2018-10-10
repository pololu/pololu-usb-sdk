/**
 * Changelog:
 * 
 * 2011-04-18: Fixed a memory leak in getDeviceList by adding a call to Winusb.listDestroy.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Pololu.WinusbHelper;

namespace Pololu.UsbWrapper
{
    /// <summary>
    /// An abstract class whose instances represent a connection to
    /// a USB device.  When the connection is broken, the instance
    /// will stop functioning.  This class currently has mechanism to
    /// automatically re-connect, but you can implement it in your
    /// application.
    /// </summary>
    public abstract class UsbDevice : IDisposable
    {
        private class MyWinUsbDevice : WinUsbDevice
        {
            internal readonly string serialNumber;

            internal MyWinUsbDevice(WinUsbDeviceHandles handles, string serialNumber) : base(handles)
            {
                this.serialNumber = serialNumber;
            }

            internal void controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index)
            {
                WINUSB_SETUP_PACKET setupPacket = new WINUSB_SETUP_PACKET();
                setupPacket.RequestType = RequestType;
                setupPacket.Request = Request;
                setupPacket.Value = Value;
                setupPacket.Index = Index;
                setupPacket.Length = 0;
                controlTransfer(setupPacket);
            }

            internal uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, byte[] data)
            {
                WINUSB_SETUP_PACKET setupPacket = new WINUSB_SETUP_PACKET();
                setupPacket.RequestType = RequestType;
                setupPacket.Request = Request;
                setupPacket.Value = Value;
                setupPacket.Index = Index;
                setupPacket.Length = (ushort)data.Length;

                return controlTransfer(setupPacket, data);
            }

            internal unsafe uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, void * data, ushort Length)
            {
                WINUSB_SETUP_PACKET setupPacket = new WINUSB_SETUP_PACKET();
                setupPacket.RequestType = RequestType;
                setupPacket.Request = Request;
                setupPacket.Value = Value;
                setupPacket.Index = Index;
                setupPacket.Length = Length;
                return controlTransfer(setupPacket, data);
            }
        }
           
        private MyWinUsbDevice device;

        /// <summary>
        /// Returns the serial number of device.  It's a string because that is how they
        /// are transmitted over USB.
        /// For PIC18-based Pololu devices, this will be an 8-digit decimal number.
        /// For STM32-based Pololu devices, this will be a 24-digit hex number.
        /// </summary>
        public string getSerialNumber()
        {
            return device.serialNumber;
        }

        /// <summary>
        /// Returns the product ID of the device.  Every Pololu USB product that
        /// has a different product ID (except those that have identical firmware).
        /// Each product's bootloader has its own product ID as well.
        /// </summary>
        protected UInt16 getProductID()
        {
            return device.getProductID();
        }

        /// <summary>
        /// Performs a control transfer that has no data stage.
        /// Returns when the control transfer is complete.
        /// There is a 50 ms timeout so that a malfunctioning device
        /// will not cause your program to hang.
        /// </summary>
        /// <remarks>For more info, see section 9.3 of the USB Specification.</remarks>
        protected void controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index)
        {
            device.controlTransfer(RequestType, Request, Value, Index);
        }

        /// <summary>
        /// Performs a control transfer that has a data stage.
        /// The data either flows from the device to the host or the
        /// host to the device.  The direction is determined by RequestType.
        /// Returns when the control transfer is complete.
        /// There is a 50 ms timeout so that a malfunctioning device
        /// will not cause your program to hang.
        /// </summary>
        /// <remarks>For more info, see section 9.3 of the USB Specification.</remarks>
        protected uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, byte[] data)
        {
            return device.controlTransfer(RequestType, Request, Value, Index, data);
        }

        /// <summary>
        /// Performs a control transfer that has a data stage.
        /// The data either flows from the device to the host or the
        /// host to the device.  The direction is determined by RequestType.
        /// Returns when the control transfer is complete.
        /// There is a 50 ms timeout so that a malfunctioning device
        /// will not cause your program to hang.
        /// </summary>
        /// <remarks>For more info, see section 9.3 of the USB Specification.</remarks>
        protected unsafe uint controlTransfer(byte RequestType, byte Request, ushort Value, ushort Index, void * data, ushort Length)
        {
            return device.controlTransfer(RequestType, Request, Value, Index, data, Length);
        }

        /// <summary>
        /// Returns an integer uniquely identifying the device among devices currently available.
        /// </summary>
        internal Int32 deviceInstance
        {
            get
            {
                return device.deviceInstance;
            }
        }

        /// <summary>
        /// Connect to the USB device specified by the DeviceListItem.
        /// </summary>
        protected UsbDevice(DeviceListItem deviceListItem)
        {
            WinUsbDeviceHandles handles;

            try
            {
                handles = Winusb.connect(deviceListItem.guid, deviceListItem.deviceInstance);
            }
            catch (Exception exception)
            {
                if (exception is Win32Exception && ((Win32Exception)exception).NativeErrorCode == 5) // ERROR_ACCESS_DENIED
                {
                    throw new Exception("Access was denied when trying to connect to the device.  "
                        + "Try closing all programs using the device.");
                }
                else
                {
                    throw new Exception("There was an error connecting to the device.", exception);
                }
            }

            device = new MyWinUsbDevice(handles, deviceListItem.serialNumber);
        }

        /// <summary>
        /// Disconnects from the USB device, freeing all resources
        /// that were allocated when the connection was made.
        /// This is the same as Dispose().
        /// </summary>
        public void disconnect()
        {
            device.disconnect();
        }

        /// <summary>
        /// Disconnects from the USB device, freeing all resources
        /// that were allocated when the connection was made.  This is the
        /// same as disconnect().
        /// </summary>
        public void Dispose()
        {
            disconnect();
        }

        /// <summary>
        /// gets a list of devices
        /// </summary>
        /// <returns></returns>
        protected static List<DeviceListItem> getDeviceList(Guid deviceInterfaceGuid)
        {
            IntPtr deviceListHandle = Winusb.listCreate(deviceInterfaceGuid);
            try
            {
                UInt32 deviceListSize = Winusb.listSize(deviceListHandle);

                var deviceList = new List<DeviceListItem>();

                for (Byte i = 0; i < deviceListSize; i++)
                {
                    // Get all the needed info in an effecient way.  This is more efficient than
                    // using listGetSerialNumber and listGetProductId because we only need to
                    // get the device instance once instead of 3 times.
                    Int32 deviceInstance = Winusb.listGetDeviceInstance(deviceListHandle, i);
                    String serialNumber = Winusb.getSerialNumber(deviceInstance);
                    UInt16 productId = Winusb.getProductID(deviceInstance);

                    DeviceListItem item = new DeviceListItem(deviceInstance,
                        deviceInterfaceGuid,
                        "#" + serialNumber,
                        serialNumber,
                        productId);
                    deviceList.Add(item);
                }
                return deviceList;
            }
            finally
            {
                Winusb.listDestroy(deviceListHandle);
            }
        }

        /// <summary>
        /// This is used in the linux version to search by vendor and product ID
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="productIds"></param>
        /// <returns></returns>
        protected static List<DeviceListItem> getDeviceList(uint vendorId, ushort[] productIds)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// true if the devices are the same
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool isSameDeviceAs(DeviceListItem item)
        {
            return deviceInstance == item.deviceInstance;
        }

        //protected AsynchronousInTransfer newAsynchronousInTransfer(byte endpoint, uint size)
        //{
        //    return new AsynchronousInTransfer(this, endpoint, size);
        //}
    }
}
