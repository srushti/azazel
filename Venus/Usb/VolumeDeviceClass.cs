using System;
using System.Collections.Generic;
using System.Text;

namespace Venus.Usb {
    /// <summary>
    /// The device class for volume devices.
    /// </summary>
    public class VolumeDeviceClass : DeviceClass {
        internal SortedDictionary<string, string> logicalDrives = new SortedDictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the VolumeDeviceClass class.
        /// </summary>
        public VolumeDeviceClass() : base(new Guid(Native.GUID_DEVINTERFACE_VOLUME)) {
            foreach (string drive in Environment.GetLogicalDrives()) {
                var sb = new StringBuilder(1024);
                if (Native.GetVolumeNameForVolumeMountPoint(drive, sb, sb.Capacity))
                    logicalDrives[sb.ToString()] = drive.Replace("\\", "");
            }
        }

        internal override Device CreateDevice(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index) {
            return new Volume(deviceClass, deviceInfoData, path, index);
        }
    }
}