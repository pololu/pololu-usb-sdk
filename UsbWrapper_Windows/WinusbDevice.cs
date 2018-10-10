/* Changelog:
 *   2011-04-21: Switched from asynchronous to synchronous control transfers.
 *     One side effect is that this fixes the CreateEvent handle leak.    
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Pololu.WinusbHelper
{
    internal abstract class WinUsbDevice
    {
        private WinUsbDeviceHandles handles;

        protected internal WinUsbDevice(WinUsbDeviceHandles handles)
        {
            this.handles.winusbHandle = handles.winusbHandle;
            this.handles.deviceHandle = handles.deviceHandle;
            this.handles.deviceInstance = handles.deviceInstance;
        }

        /// <summary>
        /// Gets the device instance (DEVINST) of the device.  This is a number
        /// that windows uses to uniquely identify devices that are connected to
        /// the computer.
        /// </summary>
        internal Int32 deviceInstance
        {
            get
            {
                return handles.deviceInstance;
            }
        }

        /// <summary>
        /// Disconnects from the device.  This allows other programs to connect to the
        /// device and frees all the memory used by Winusb.  After calling disconnect()
        /// you should not try to call any other functions on this object.
        /// 
        /// Example usage:
        ///     device.disconnect();
        ///     device = null;
        /// </summary>
        internal void disconnect()
        {
            Winusb.disconnect(this.handles);
        }

        /// <summary>
        /// Performs a control transfer that has no data stage.
        /// </summary>
        /// <param name="setupPacket">The SETUP packet to send to the device.</param>
        protected unsafe void controlTransfer(WINUSB_SETUP_PACKET setupPacket)
        {
            UInt32 lengthTransferred = controlTransfer(setupPacket, (Byte*)null);
            if (lengthTransferred != 0)
            {
                throw new Exception("The control transfer was expected to have no data stage, but " + lengthTransferred + " bytes were transferred.");
            }
        }

        internal UInt16 getProductID()
        {
            return Winusb.getProductID(deviceInstance);
        }

        internal UInt16 getVendorID()
        {
            return Winusb.getVendorID(deviceInstance);
        }

        /// <summary>
        /// Performs a control transfer with a data stage.  Calling this function
        /// is slightly safer than calling controlTransfer(WINUSB_SETUP_PACKET, Byte *)
        /// because this function can verify that buffer is not null and that buffer
        /// is long enough.
        /// </summary>
        /// <param name="buffer">
        /// If this is a Read transfer, this is the buffer that the data from the
        /// device will be read in to during the data stage.  If this is a Write
        /// transfer, this is the buffer that will be written to the device during
        /// the data stage.
        /// </param>
        /// <param name="setupPacket">The SETUP packet to send to the device.</param>
        /// <returns>
        ///   The number of bytes transferred in the data stage.
        ///   This is usually equal to setupPacket.wLength.
        /// </returns>
        protected unsafe UInt32 controlTransfer(WINUSB_SETUP_PACKET setupPacket, Byte[] buffer)
        {
            if (setupPacket.Length != 0)
            {
                if (buffer == null)
                {
                    throw new ArgumentException("The setupPacket length field is non-zero, but no buffer was provided.", "buffer");
                }

                if (buffer.Length < setupPacket.Length)
                {
                    throw new ArgumentException("The setupPacket length field is " + setupPacket.Length + ", but the buffer provided is only " + buffer.Length + " bytes.", "buffer");
                }
            }

            fixed (Byte* pointer = &buffer[0]) return controlTransfer(setupPacket, pointer);
        }

        /// <summary>
        /// Performs a control transfer, a series of USB transactions that
        /// consist of a SETUP packet sent to the device, an optional data
        /// phase for transferring bytes to the host or the device, and an
        /// acknowledgement packet form the party that did not send the
        /// data phase.
        /// 
        /// A control transfer with no data phase is considered to be a
        /// control write transfer, so the acknowledgement packet comes
        /// from the device.
        /// </summary>
        /// <param name="buffer">
        /// If this is a Read transfer, this is the buffer that the data from the
        /// device will be read in to during the data stage.  If this is a Write
        /// transfer, this is the buffer that will be written to the device during
        /// the data stage.
        /// </param>
        /// <param name="setupPacket">The SETUP packet to send to the device.</param>
        /// <returns>
        ///   The number of bytes transferred in the data stage.
        ///   This is usually equal to setupPacket.wLength.
        /// </returns>
        protected unsafe UInt32 controlTransfer(WINUSB_SETUP_PACKET setupPacket, void * buffer)
        {
            UInt32 lengthTransferred = 0;
            Boolean success = WinUsb_ControlTransfer(handles.winusbHandle, setupPacket, buffer, setupPacket.Length, &lengthTransferred, (OVERLAPPED*)0);
            if (!success)
            {
                throw new Win32Exception("Control transfer failed.");
            }
            return lengthTransferred;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct OVERLAPPED
        {
            internal IntPtr Internal;
            internal IntPtr InternalHigh;
            internal Int32 Offset;
            internal Int32 OffsetHigh;
            internal IntPtr hEvent;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        protected struct WINUSB_SETUP_PACKET
        {
            internal Byte RequestType;
            internal Byte Request;
            internal UInt16 Value;
            internal UInt16 Index;
            internal UInt16 Length;
        }

        [DllImport("winusb.dll", SetLastError = true)]
        static extern unsafe Boolean WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, void* Buffer, UInt32 BufferLength, UInt32* LengthTransferred, OVERLAPPED* Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, Byte PipeID, ref Byte Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_SetPipePolicy(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref Byte Value);

        [DllImport("winusb.dll", SetLastError = true)]
        static extern Boolean WinUsb_WritePipe(IntPtr InterfaceHandle, Byte PipeID, ref Byte Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

    }
}
