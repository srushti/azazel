using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Venus.Usb {
    /// <summary>
    /// A volume device.
    /// </summary>
    public class Volume : Device {
        private string volumeName;
        private string logicalDrive;
        private int[] diskNumbers;
        private List<Device> disks;
        private List<Device> removableDevices;

        internal Volume(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index)
            : base(deviceClass, deviceInfoData, path, index) {}

        /// <summary>
        /// Gets the volume's name.
        /// </summary>
        public string VolumeName {
            get {
                if (volumeName == null) {
                    var sb = new StringBuilder(1024);
                    if (!Native.GetVolumeNameForVolumeMountPoint(Path + "\\", sb, sb.Capacity)) {
                        // throw new Win32Exception(Marshal.GetLastWin32Error());
                    }

                    if (sb.Length > 0)
                        volumeName = sb.ToString();
                }
                return volumeName;
            }
        }

        /// <summary>
        /// Gets the volume's logical drive in the form [letter]:\
        /// </summary>
        public string LogicalDrive {
            get {
                if ((logicalDrive == null) && (VolumeName != null))
                    ((VolumeDeviceClass) DeviceClass).logicalDrives.TryGetValue(VolumeName, out logicalDrive);
                return logicalDrive;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this volume is a based on USB devices.
        /// </summary>
        public override bool IsUsb {
            get {
                if (Disks != null) {
                    foreach (var disk in Disks) {
                        if (disk.IsUsb)
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Gets a list of underlying disks for this volume.
        /// </summary>
        public List<Device> Disks {
            get {
                if (disks == null) {
                    disks = new List<Device>();

                    if (DiskNumbers != null) {
                        var diskDevice = new DiskDeviceClass();
                        foreach (int index in DiskNumbers) {
                            if (index < diskDevice.Devices.Count)
                                disks.Add(diskDevice.Devices[index]);
                        }
                    }
                }
                return disks;
            }
        }

        private int[] DiskNumbers {
            get {
                if (diskNumbers == null) {
                    var numbers = new List<int>();
                    if (LogicalDrive != null) {
                        IntPtr hFile = Native.CreateFile(@"\\.\" + LogicalDrive, Native.GENERIC_READ, Native.FILE_SHARE_READ | Native.FILE_SHARE_WRITE,
                                                         IntPtr.Zero, Native.OPEN_EXISTING, 0, IntPtr.Zero);
                        if (hFile.ToInt32() == Native.INVALID_HANDLE_VALUE)
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        int size = 0x400; // some big size
                        IntPtr buffer = Marshal.AllocHGlobal(size);
                        int bytesReturned = 0;
                        try {
                            if (
                                !Native.DeviceIoControl(hFile, Native.IOCTL_VOLUME_GET_VOLUME_DISK_EXTENTS, IntPtr.Zero, 0, buffer, size, out bytesReturned,
                                                        IntPtr.Zero)) {
                                // do nothing here on purpose
                            }
                        }
                        finally {
                            Native.CloseHandle(hFile);
                        }

                        if (bytesReturned > 0) {
                            var numberOfDiskExtents = (int) Marshal.PtrToStructure(buffer, typeof (int));
                            for (int i = 0; i < numberOfDiskExtents; i++) {
                                var extentPtr = new IntPtr(buffer.ToInt32() + Marshal.SizeOf(typeof (long)) + i*Marshal.SizeOf(typeof (Native.DISK_EXTENT)));
                                var extent = (Native.DISK_EXTENT) Marshal.PtrToStructure(extentPtr, typeof (Native.DISK_EXTENT));
                                numbers.Add(extent.DiskNumber);
                            }
                        }
                        Marshal.FreeHGlobal(buffer);
                    }

                    diskNumbers = new int[numbers.Count];
                    numbers.CopyTo(diskNumbers);
                }
                return diskNumbers;
            }
        }

        /// <summary>
        /// Gets a list of removable devices for this volume.
        /// </summary>
        public override List<Device> RemovableDevices {
            get {
                if (removableDevices == null) {
                    removableDevices = new List<Device>();
                    if (Disks == null)
                        removableDevices = base.RemovableDevices;
                    else {
                        foreach (var disk in Disks) {
                            foreach (var device in disk.RemovableDevices)
                                removableDevices.Add(device);
                        }
                    }
                }
                return removableDevices;
            }
        }

        public override int CompareTo(object obj) {
            var device = obj as Volume;
            if (device == null)
                throw new ArgumentException();

            if (LogicalDrive == null)
                return 1;

            if (device.LogicalDrive == null)
                return -1;

            return LogicalDrive.CompareTo(device.LogicalDrive);
        }

//
//        public override bool Equals(object obj) {
//            return Equals(((Volume) obj));
//        }
//
//        private bool Equals(Volume volume) {
//            return logicalDrive.Equals(volume.logicalDrive) && base.
//        }
//
//        public override int GetHashCode() {
//            return base.GetHashCode();
//        }
    }
}