using System;

namespace Venus.Usb {
    /// <summary>
    /// The device class for disk devices.
    /// </summary>
    public class DiskDeviceClass : DeviceClass {
        /// <summary>
        /// Initializes a new instance of the DiskDeviceClass class.
        /// </summary>
        public DiskDeviceClass() : base(new Guid(Native.GUID_DEVINTERFACE_DISK)) {}
    }
}