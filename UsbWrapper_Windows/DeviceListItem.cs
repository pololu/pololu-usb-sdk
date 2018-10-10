using System;

namespace Pololu.UsbWrapper
{
    /// <summary>
    /// A class that represents a device connected to the computer.  This
    /// class can be used as an item in the device list dropdown box.
    /// </summary>
    public class DeviceListItem
    {
        /// <summary>
        /// Gets the device instance (DEVINST) for this device.  This can be
        /// passed in WinusbHelper.connect() to connect the device.
        /// </summary>
        internal readonly Int32 deviceInstance;

        /// <summary>
        /// The guid of the Windows device interface class of this device.
        /// </summary>
        public readonly Guid guid;

        private String privateText;

        /// <summary>
        /// The text to display to the user in the list to represent this
        /// device.  By default, this text is "#" + serialNumberString,
        /// but it can be changed to suit the application's needs
        /// (for example, adding model information to it).
        /// </summary>
        public String text
        {
            get
            {
                return privateText;
            }
            set
            {
                privateText = value;
            }
        }

        /// <summary>
        /// The USB serial number string of the device.
        /// </summary>
        /// <remarks>
        /// Since the serial number is needed by most applications, and used to
        /// generate deviceListItem.text, it is not a big waste to always fetch
        /// it when creating the device list.
        /// </remarks>
        public readonly string serialNumber;

        /// <summary>
        /// The USB product ID of the device.
        /// </summary>
        public readonly UInt16 productId;

        /// <summary>
        /// Constructs a new device list item to represent a device connected to
        /// this computer.
        /// </summary>
        /// <param name="deviceInstance">
        /// The device instance (DEVINST) of the device, typically returned
        /// from Winusb.listGetDeviceInstance.
        /// </param>
        /// <param name="text">Text that identifies the device to the user.</param>
        /// <param name="guid">The device instance GUID (from the INF file).</param>
        /// <param name="serialNumber">The serial number of the device (from the string descriptor).</param>
        internal DeviceListItem(Int32 deviceInstance, Guid guid, String text, String serialNumber, UInt16 productId)
        {
            this.deviceInstance = deviceInstance;
            this.guid = guid;
            this.privateText = text;
            this.serialNumber = serialNumber;
            this.productId = productId;
        }

        /// <summary>
        /// Creates an item that doesn't actually refer to a device; just for populating the list with things like "Disconnected"
        /// </summary>
        public static DeviceListItem CreateDummyItem(String text)
        {
            return new DeviceListItem(0, new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), text, "", 0);
        }

        /// <summary>
        /// Return true if the two devices are the same.
        /// </summary>
        public bool isSameDeviceAs(DeviceListItem deviceListItem)
        {
            return (deviceInstance == deviceListItem.deviceInstance);
        }
    }
}
