using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Venus.Usb {
    /// <summary>
    /// A generic base class for physical devices.
    /// </summary>
    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class Device : IComparable {
        private readonly string path;
        private readonly DeviceClass deviceClass;
        private string description;
        private string @class;
        private string classGuid;
        private Device parent;
        private readonly int index;
        private DeviceCapabilities capabilities = DeviceCapabilities.Unknown;
        private List<Device> removableDevices;
        private string friendlyName;
        private readonly Native.SP_DEVINFO_DATA deviceInfoData;

        internal Device(DeviceClass deviceClass, Native.SP_DEVINFO_DATA deviceInfoData, string path, int index) {
            if (deviceClass == null)
                throw new ArgumentNullException("deviceClass");

            if (deviceInfoData == null)
                throw new ArgumentNullException("deviceInfoData");

            this.deviceClass = deviceClass;
            this.path = path; // may be null
            this.deviceInfoData = deviceInfoData;
            this.index = index;
        }

        /// <summary>
        /// Gets the device's index.
        /// </summary>
        public int Index {
            get { return index; }
        }

        /// <summary>
        /// Gets the device's class instance.
        /// </summary>
        [Browsable(false)]
        public DeviceClass DeviceClass {
            get { return deviceClass; }
        }

        /// <summary>
        /// Gets the device's path.
        /// </summary>
        public string Path {
            get {
                if (path == null) {}
                return path;
            }
        }

        /// <summary>
        /// Gets the device's instance handle.
        /// </summary>
        public int InstanceHandle {
            get { return deviceInfoData.devInst; }
        }

        /// <summary>
        /// Gets the device's class name.
        /// </summary>
        public string Class {
            get {
                if (@class == null)
                    @class = deviceClass.GetProperty(deviceInfoData, Native.SPDRP_CLASS, null);
                return @class;
            }
        }

        /// <summary>
        /// Gets the device's class Guid as a string.
        /// </summary>
        public string ClassGuid {
            get {
                if (classGuid == null)
                    classGuid = deviceClass.GetProperty(deviceInfoData, Native.SPDRP_CLASSGUID, null);
                return classGuid;
            }
        }

        /// <summary>
        /// Gets the device's description.
        /// </summary>
        public string Description {
            get {
                if (description == null)
                    description = deviceClass.GetProperty(deviceInfoData, Native.SPDRP_DEVICEDESC, null);
                return description;
            }
        }

        /// <summary>
        /// Gets the device's friendly name.
        /// </summary>
        public string FriendlyName {
            get {
                if (friendlyName == null)
                    friendlyName = deviceClass.GetProperty(deviceInfoData, Native.SPDRP_FRIENDLYNAME, null);
                return friendlyName;
            }
        }

        /// <summary>
        /// Gets the device's capabilities.
        /// </summary>
        public DeviceCapabilities Capabilities {
            get {
                if (capabilities == DeviceCapabilities.Unknown)
                    capabilities = (DeviceCapabilities) deviceClass.GetProperty(deviceInfoData, Native.SPDRP_CAPABILITIES, 0);
                return capabilities;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this device is a USB device.
        /// </summary>
        public virtual bool IsUsb {
            get {
                if (Class == "USB")
                    return true;

                if (Parent == null)
                    return false;

                return Parent.IsUsb;
            }
        }

        /// <summary>
        /// Gets the device's parent device or null if this device has not parent.
        /// </summary>
        public Device Parent {
            get {
                if (parent == null) {
                    int parentDevInst = 0;
                    int hr = Native.CM_Get_Parent(ref parentDevInst, deviceInfoData.devInst, 0);
                    if (hr == 0)
                        parent = new Device(deviceClass, deviceClass.GetInfo(parentDevInst), null, -1);
                }
                return parent;
            }
        }

        /// <summary>
        /// Gets this device's list of removable devices.
        /// Removable devices are parent devices that can be removed.
        /// </summary>
        public virtual List<Device> RemovableDevices {
            get {
                if (removableDevices == null) {
                    removableDevices = new List<Device>();

                    if ((Capabilities & DeviceCapabilities.Removable) != 0)
                        removableDevices.Add(this);
                    else {
                        if (Parent != null) {
                            foreach (var device in Parent.RemovableDevices)
                                removableDevices.Add(device);
                        }
                    }
                }
                return removableDevices;
            }
        }

        /// <summary>
        /// Ejects the device.
        /// </summary>
        /// <param name="allowUI">Pass true to allow the Windows shell to display any related UI element, false otherwise.</param>
        /// <returns>null if no error occured, otherwise a contextual text.</returns>
        public string Eject(bool allowUI) {
            foreach (var device in RemovableDevices) {
                if (allowUI) {
                    Native.CM_Request_Device_Eject_NoUi(device.InstanceHandle, IntPtr.Zero, null, 0, 0);
                }
                else {
                    var sb = new StringBuilder(1024);
                    Native.PNP_VETO_TYPE veto;
                    int hr = Native.CM_Request_Device_Eject(device.InstanceHandle, out veto, sb, sb.Capacity, 0);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    if (veto != Native.PNP_VETO_TYPE.Ok)
                        return veto.ToString();
                }
            }
            return null;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the comparands.</returns>
        public virtual int CompareTo(object obj) {
            var device = obj as Device;
            if (device == null)
                throw new ArgumentException();

            return Index.CompareTo(device.Index);
        }
    }
}