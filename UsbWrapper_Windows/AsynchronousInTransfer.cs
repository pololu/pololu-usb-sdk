using System;
using System.Collections.Generic;
using System.Text;

namespace Pololu.UsbWrapper
{
    /// <summary>
    /// A class whose instances represent an asynchronous transfer of
    /// data to the host from the device on a bulk or interrupt endpoint.
    /// This class allows you to queue up hundreds of transfers at a low
    /// level in the USB system so that you application can do other 
    /// things while the transfers happen.
    /// Instances of this class can be reused to execute many such
    /// transfers.
    /// </summary>
    public class AsynchronousInTransfer
    {
        private UsbDevice device;
        byte endpoint;
        uint size;

        internal AsynchronousInTransfer(UsbDevice device, byte endpoint, uint size)
        {
            this.device = device;
            this.endpoint = endpoint;
            this.size = size;
        }

        /// <summary>
        /// Queues the transfer for execution in a low-level USB system queue.
        /// </summary>
        public void start()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the status of the transfer.  See the
        /// enum for details.
        /// </summary>
        public TransferStatus status
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the number of bytes transferred.
        /// This value is not valid while the transfer is
        /// still pending.
        /// </summary>
        public uint lengthTransferred
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The data received from the device.
        /// </summary>
        public byte[] buffer
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Represents the current status of the request.
    /// </summary>
    public enum TransferStatus
    {
        /// <summary>
        /// The request has not been processed yet.
        /// </summary>
        Pending,

        /// <summary>
        /// The request was successfully completed.
        /// This does NOT mean that the host received all the data. 
        /// </summary>
        Completed,

        /// <summary>
        /// There was an error completing this request.
        /// </summary>
        Error,

        /// <summary>
        /// The device took too long to send data, so the request timed out.
        /// </summary>
        TimedOut,

        /// <summary>
        /// The request was cancelled by the application.
        /// </summary>
        Cancelled,

        /// <summary>
        /// The device responded to the request with a STALL packet
        /// (halt condition).
        /// </summary>
        Stall,

        /// <summary>
        /// The device was disconnected.
        /// </summary>
        NoDevice,

        /// <summary>
        /// The device sent more data than was requested.
        /// </summary>
        Overflow
    }
}
