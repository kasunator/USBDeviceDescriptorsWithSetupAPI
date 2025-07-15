using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SetupAPIDLLImports;
using System.Security.Cryptography.X509Certificates;

namespace USBDevDescriptorAccess
{
    class Win32DeviceMgmt
    {
        const int utf16terminatorSize_bytes = 2;

        public enum StringLength : uint
        {
            MAX_PATH = 260,
        }

        public struct DeviceInfo
        {
            public string name;
            public string description;
            public string bus_description;
        }

        public struct DeviceInfoAdvanced
        {
            public string DeviceInstanceID;
            public string DeviceDescription;
            public string HardwareIDs;
            public string BusRprtedDevDesc;
            public string DeviceManufacturer;
            public string DeviceFriendlyName;
            public string DeviceLocationInfo;
            public string ContainerID;
            public string VID;
            public string PID;
            public string MI;
            public string port_name;

        }


        static SetupAPI.DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc;
        static SetupAPI.DEVPROPKEY DEVPKEY_Device_Manufacturer;
        static SetupAPI.DEVPROPKEY DEVPKEY_Device_FriendlyName;
        static SetupAPI.DEVPROPKEY DEVPKEY_Device_LocationInfo;
        static SetupAPI.DEVPROPKEY DEVPKEY_Device_ContainerId;

        static SetupAPI.DEVPROPKEY GUID_DEVCLASS_HIDCLASS;
        /* these guids are defined inside devpkey.h at https://github.com/tpn/winsdk-10/blob/master/Include/10.0.16299.0/shared/devpkey.h */
        static Win32DeviceMgmt()
        {
            DEVPKEY_Device_BusReportedDeviceDesc = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_BusReportedDeviceDesc.fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2);
            DEVPKEY_Device_BusReportedDeviceDesc.pid = 4;
        }

        static void initDEVPKEY_Device_BusReportedDeviceDesc()
        {
            DEVPKEY_Device_BusReportedDeviceDesc = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_BusReportedDeviceDesc.fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2);
            DEVPKEY_Device_BusReportedDeviceDesc.pid = 4;
        }

        static void initDEVPKEY_Device_Manufacturer()
        {
            DEVPKEY_Device_Manufacturer = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_Manufacturer.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);
            DEVPKEY_Device_Manufacturer.pid = 13;
        }

        static void initDEVPKEY_Device_FriendlyName()
        {
            DEVPKEY_Device_FriendlyName = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_FriendlyName.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);
            DEVPKEY_Device_FriendlyName.pid = 14;
        }

        static void initDEVPKEY_Device_LocationInfo()
        {
            DEVPKEY_Device_LocationInfo = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_LocationInfo.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);
            DEVPKEY_Device_LocationInfo.pid = 15;
        }

        static void initDEVPKEY_Device_ContainerID()
        {
            DEVPKEY_Device_ContainerId = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_ContainerId.fmtid = new Guid(0x8c7ed206, 0x3f8a, 0x4827, 0xb3, 0xab, 0xae, 0x9e, 0x1f, 0xae, 0xfc, 0x6c);
            DEVPKEY_Device_ContainerId.pid = 2;
        }




        public static List<DeviceInfo> GetAllCOMPorts()
        {
            Guid[] guids = GetClassGUIDs("Ports");
            //Guid[] guids = GetClassGUIDs("COM");
            List<DeviceInfo> devices = new List<DeviceInfo>();
            for (int index = 0; index < guids.Length; index++)
            {
                IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(ref guids[index], 0, 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT);
                if (hDeviceInfoSet == IntPtr.Zero)
                {
                    throw new Exception("Failed to get device information set for the COM ports");
                }

                try
                {
                    UInt32 iMemberIndex = 0;
                    while (true)
                    {
                        SetupAPI.SP_DEVINFO_DATA deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
                        deviceInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));
                        bool success = SetupAPI.SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                        if (!success)
                        {
                            // No more devices in the device information set
                            break;
                        }

                        DeviceInfo deviceInfo = new DeviceInfo();

                        deviceInfo.name = GetDeviceName(hDeviceInfoSet, deviceInfoData);
                        deviceInfo.description = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                        if (deviceInfo.name == "COM1")
                        {
                            deviceInfo.bus_description = GetDeviceBusDescription(hDeviceInfoSet, deviceInfoData);
                        }
                        devices.Add(deviceInfo);

                        iMemberIndex++;
                    }
                }
                finally
                {
                    SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
                }
            }
            return devices;
        }

        public static List<DeviceInfo> GetAllUSBDEvices()
        {
            Guid[] guids = GetClassGUIDs("Ports");
            //Guid[] guids = GetClassGUIDs("COM");
            List<DeviceInfo> devices = new List<DeviceInfo>();
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(IntPtr.Zero, "USB", 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT | SetupAPI.DiGetClassFlags.DIGCF_ALLCLASSES);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }

            try
            {
                UInt32 iMemberIndex = 0;
                while (true)
                {
                    SetupAPI.SP_DEVINFO_DATA deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
                    deviceInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));
                    bool success = SetupAPI.SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                    if (!success)
                    {
                        // No more devices in the device information set
                        break;
                    }

                    DeviceInfo deviceInfo = new DeviceInfo();

                    deviceInfo.name = GetDeviceName(hDeviceInfoSet, deviceInfoData);
                    deviceInfo.description = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                    //if (deviceInfo.name == "COM1")
                    //{
                    deviceInfo.bus_description = GetDeviceBusDescription(hDeviceInfoSet, deviceInfoData);
                    //}
                    devices.Add(deviceInfo);
                    Console.WriteLine(" Device name: {0} ", deviceInfo.name);
                    Console.WriteLine(" Device description: {0} ", deviceInfo.description);
                    Console.WriteLine(" Device bus_desccription: {0} ", deviceInfo.bus_description);
                    iMemberIndex++;
                }
            }
            finally
            {
                SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
            }

            return devices;
        }

        //DeviceInfoAdvanced
        public static List<DeviceInfoAdvanced> GetAllUSBDEvicesADvacned()
        {
            StringBuilder devIDStrBuilder = new StringBuilder(260);
            /* populate the BusReportedDevieDesc with the corresponding GUID devpkey.h */
            initDEVPKEY_Device_BusReportedDeviceDesc();
            initDEVPKEY_Device_Manufacturer();
            initDEVPKEY_Device_FriendlyName();
            initDEVPKEY_Device_LocationInfo();
            initDEVPKEY_Device_ContainerID();

            List<DeviceInfoAdvanced> devices = new List<DeviceInfoAdvanced>();
            /* get an Hardware device info handle for devices enumarated as "USB" */
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(IntPtr.Zero, "USB", 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT | SetupAPI.DiGetClassFlags.DIGCF_ALLCLASSES);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }

            try
            {
                UInt32 iMemberIndex = 0;
                while (true)
                {
                    SetupAPI.SP_DEVINFO_DATA deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
                    deviceInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));
                    /* get the device information data structure i.e SP_DEVINFO_DATA from the device information set for the correspoinding device index */
                    bool success = SetupAPI.SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                    if (!success)
                    {
                        // No more devices in the device information set
                        break;
                    }

                    DeviceInfoAdvanced deviceInfo = new DeviceInfoAdvanced();


                    //deviceInfo.CM_Get_Device_ID;
                    if (GetDeviceID(deviceInfoData, out deviceInfo.DeviceInstanceID) != true)
                    {
                        continue;
                    }
                    Console.WriteLine("************************");
                    Console.WriteLine("CM_Get_Device_ID, aka DeviceInstanceID:{0}", deviceInfo.DeviceInstanceID);

                    //deviceInfo.DeviceDescription;
                    deviceInfo.DeviceDescription = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Description:{0}", deviceInfo.DeviceDescription);

                    //deviceType
                    string deviceType = GetDeviceType(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Type:{0}", deviceType);

                    //deviceInfo.HardwareIDs
                    deviceInfo.HardwareIDs = GetHardwareID(hDeviceInfoSet, deviceInfoData);

                    string[] hardWareIDArray = deviceInfo.HardwareIDs.Split('\0');
                    Console.WriteLine("HardwareIDs:");
                    foreach (string hardwareID in hardWareIDArray)
                    {
                        if (hardwareID.Length > 1)
                            Console.WriteLine("       " + hardwareID);
                    }
                    //deviceInfo.BusRprtedDevDesc
                    deviceInfo.BusRprtedDevDesc = GetDeviceBusDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Bus Reported Dev Descriptor:{0}", deviceInfo.BusRprtedDevDesc);

                    //deviceInfo.DeviceManufacturer
                    deviceInfo.DeviceManufacturer = GetDeviceManufacturer(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Manufacturer:{0}", deviceInfo.DeviceManufacturer);

                    //deviceInfo.DeviceFriendlyName
                    deviceInfo.DeviceFriendlyName = GetDeviceFriendlyName(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Friendly Name:{0}", deviceInfo.DeviceFriendlyName);

                    //deviceInfo.DeviceLocationInfo
                    deviceInfo.DeviceLocationInfo = GetDeviceLocationInfo(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device location info:{0}", deviceInfo.DeviceLocationInfo);

                    //deviceInfo.ContainerID
                    deviceInfo.ContainerID = GetDeviceContainerID(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Container ID:{0}", deviceInfo.ContainerID);

                    string[] splitStringArray = deviceInfo.DeviceInstanceID.Split('&');
                    //deviceInfo.VID
                    /* we look for index of "VID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 1)
                    {
                        int VID_index = splitStringArray[0].IndexOf("VID_");
                        if (VID_index > -1)
                        {
                            deviceInfo.VID = splitStringArray[0].Substring(VID_index + "VID_".Length, 4);
                            Console.WriteLine("VID:{0}", deviceInfo.VID);
                        }
                    }
                    //deviceInfo.PID
                    /* we look for index of "PID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 2)
                    {
                        int PID_index = splitStringArray[1].IndexOf("PID_");
                        if (PID_index > -1)
                        {
                            deviceInfo.PID = splitStringArray[1].Substring(PID_index + "PID_".Length, 4);
                            Console.WriteLine("PID:{0}", deviceInfo.PID);
                        }
                    }

                    //deviceInfo.MI
                    /* we look for index of "MI_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 3)
                    {
                        int MI_index = splitStringArray[2].IndexOf("MI_");
                        if (MI_index > -1)
                        {
                            deviceInfo.MI = splitStringArray[2].Substring(MI_index + "MI_".Length, 2);
                            Console.WriteLine("MI:{0}", deviceInfo.MI);
                        }
                    }
                    Console.WriteLine("************************");
                    devices.Add(deviceInfo);
                    iMemberIndex++;
                }
            }
            finally
            {
                if (SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet) == true)
                {
                    Console.WriteLine("Destroy Device Info List successful");
                }
                else
                {
                    Console.WriteLine("Destroy Device Info List Failed");
                }
            }

            return devices;
        }

        public static List<DeviceInfoAdvanced> GetAllUsbHidDEvices()
        {
            StringBuilder devIDStrBuilder = new StringBuilder(260);
            /* populate the BusReportedDevieDesc with the corresponding GUID devpkey.h */
            initDEVPKEY_Device_BusReportedDeviceDesc();
            initDEVPKEY_Device_Manufacturer();
            initDEVPKEY_Device_FriendlyName();
            initDEVPKEY_Device_LocationInfo();
            initDEVPKEY_Device_ContainerID();
            /* HID class GUID this value can be found in the following location  
             * https://learn.microsoft.com/en-us/windows-hardware/drivers/install/system-defined-device-setup-classes-available-to-vendors 
             * for many device stup classes. 
              
              The actual location where the  GUID_DEVCLASS_HIDCLASS   is defined is in https://github.com/tpn/winsdk-10/blob/master/Include/10.0.14393.0/shared/devguid.h 
                and the same GUID number is re-declared as GUID_HIDClass in https://github.com/tpn/winsdk-10/blob/master/Include/10.0.10240.0/shared/dinputd.h.

               WARNING!!: Do not mistake this with the device interface class 	
                  GUID_DEVINTERFACE_HID {4D1E55B2-F16F-11CF-88CB-001111000030}

              You can read more about Setup classes vs. interface classes here: https://learn.microsoft.com/en-us/windows-hardware/drivers/install/setup-classes-versus-interface-classes
             */


            //Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k);
            Guid guid_hid_class = new Guid(0x745a17a0, 0x74d3, 0x11d0, 0xb6, 0xfe, 0x00, 0xa0, 0xc9, 0x0f, 0x57, 0xda);

            //Guid ghid;
            //SetupAPI.HidD_GetHidGuid(out ghid);
            List<DeviceInfoAdvanced> devices = new List<DeviceInfoAdvanced>();
            /* get an Hardware device info handle for devices enumarated as "USB" */
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(ref guid_hid_class, IntPtr.Zero, IntPtr.Zero, SetupAPI.DiGetClassFlags.DIGCF_PRESENT);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }

            try
            {
                UInt32 iMemberIndex = 0;
                while (true)
                {
                    SetupAPI.SP_DEVINFO_DATA deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
                    deviceInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));
                    /* get the device information data structure i.e SP_DEVINFO_DATA from the device information set for the correspoinding device index */
                    bool success = SetupAPI.SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                    if (!success)
                    {
                        // No more devices in the device information set
                        break;
                    }

                    DeviceInfoAdvanced deviceInfo = new DeviceInfoAdvanced();


                    //deviceInfo.CM_Get_Device_ID;
                    if (GetDeviceID(deviceInfoData, out deviceInfo.DeviceInstanceID) != true)
                    {
                        continue;
                    }
                    Console.WriteLine("************************");
                    Console.WriteLine("CM_Get_Device_ID, aka DeviceInstanceID:{0}", deviceInfo.DeviceInstanceID);

                    //deviceInfo.DeviceDescription;
                    deviceInfo.DeviceDescription = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Description:{0}", deviceInfo.DeviceDescription);

                    //deviceType
                    string deviceType = GetDeviceType(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Type:{0}", deviceType);

                    //deviceInfo.HardwareIDs
                    deviceInfo.HardwareIDs = GetHardwareID(hDeviceInfoSet, deviceInfoData);

                    string[] hardWareIDArray = deviceInfo.HardwareIDs.Split('\0');
                    Console.WriteLine("HardwareIDs:");
                    foreach (string hardwareID in hardWareIDArray)
                    {
                        if (hardwareID.Length > 1)
                            Console.WriteLine("       " + hardwareID);
                    }
                    //deviceInfo.BusRprtedDevDesc
                    deviceInfo.BusRprtedDevDesc = GetDeviceBusDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Bus Reported Dev Descriptor:{0}", deviceInfo.BusRprtedDevDesc);

                    //deviceInfo.DeviceManufacturer
                    deviceInfo.DeviceManufacturer = GetDeviceManufacturer(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Manufacturer:{0}", deviceInfo.DeviceManufacturer);

                    //deviceInfo.DeviceFriendlyName
                    deviceInfo.DeviceFriendlyName = GetDeviceFriendlyName(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Friendly Name:{0}", deviceInfo.DeviceFriendlyName);

                    //deviceInfo.DeviceLocationInfo
                    deviceInfo.DeviceLocationInfo = GetDeviceLocationInfo(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device location info:{0}", deviceInfo.DeviceLocationInfo);

                    //deviceInfo.ContainerID
                    deviceInfo.ContainerID = GetDeviceContainerID(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Container ID:{0}", deviceInfo.ContainerID);

                    string[] splitStringArray = deviceInfo.DeviceInstanceID.Split('&');
                    //deviceInfo.VID
                    /* we look for index of "VID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 1)
                    {
                        int VID_index = splitStringArray[0].IndexOf("VID_");
                        if (VID_index > -1)
                        {
                            deviceInfo.VID = splitStringArray[0].Substring(VID_index + "VID_".Length, 4);
                            Console.WriteLine("VID:{0}", deviceInfo.VID);
                        }
                    }
                    //deviceInfo.PID
                    /* we look for index of "PID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 2)
                    {
                        int PID_index = splitStringArray[1].IndexOf("PID_");
                        if (PID_index > -1)
                        {
                            deviceInfo.PID = splitStringArray[1].Substring(PID_index + "PID_".Length, 4);
                            Console.WriteLine("PID:{0}", deviceInfo.PID);
                        }
                    }

                    //deviceInfo.MI
                    /* we look for index of "MI_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 3)
                    {
                        int MI_index = splitStringArray[2].IndexOf("MI_");
                        if (MI_index > -1)
                        {
                            deviceInfo.MI = splitStringArray[2].Substring(MI_index + "MI_".Length, 2);
                            Console.WriteLine("MI:{0}", deviceInfo.MI);
                        }
                    }
                    Console.WriteLine("************************");
                    devices.Add(deviceInfo);
                    iMemberIndex++;
                }
            }
            finally
            {
                if (SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet) == true)
                {
                    Console.WriteLine("Destroy Device Info List successful");
                }
                else
                {
                    Console.WriteLine("Destroy Device Info List Failed");
                }
            }

            return devices;
        }

        /* this functions resturns a list of SetuiApiUSBDevInfo for the comports that matches the passed pid and vids */
        public static List<DeviceInfoAdvanced> ComportInfoList()
        {
            StringBuilder devIDStrBuilder = new StringBuilder(260);
            /* populate the BusReportedDevieDesc with the corresponding GUID devpkey.h */
            initDEVPKEY_Device_BusReportedDeviceDesc();
            initDEVPKEY_Device_Manufacturer();
            initDEVPKEY_Device_FriendlyName();
            initDEVPKEY_Device_LocationInfo();
            initDEVPKEY_Device_ContainerID();
            /* HID class GUID this value can be found in the following location  
             * https://learn.microsoft.com/en-us/windows-hardware/drivers/install/system-defined-device-setup-classes-available-to-vendors 
             * for many device stup classes. 
              
              The actual location where the  GUID_DEVCLASS_HIDCLASS   is defined is in https://github.com/tpn/winsdk-10/blob/master/Include/10.0.14393.0/shared/devguid.h 
                and the same GUID number is re-declared as GUID_HIDClass in https://github.com/tpn/winsdk-10/blob/master/Include/10.0.10240.0/shared/dinputd.h.

               WARNING!!: Do not mistake this with the device interface class 	
                  GUID_DEVINTERFACE_HID {4D1E55B2-F16F-11CF-88CB-001111000030}

              You can read more about Setup classes vs. interface classes here: https://learn.microsoft.com/en-us/windows-hardware/drivers/install/setup-classes-versus-interface-classes
             */


            //Guid(uint a, ushort b, ushort c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k);
            Guid comport_class = new Guid(0x4d36e978, 0xe325, 0x11ce, 0xbf, 0xc1, 0x08, 0x00, 0x2b, 0xe1, 0x03, 0x18);

            //Guid ghid;
            //SetupAPI.HidD_GetHidGuid(out ghid);
            List<DeviceInfoAdvanced> devices = new List<DeviceInfoAdvanced>();
            /* get an Hardware device info handle for devices enumarated as "USB" */
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(ref comport_class, IntPtr.Zero, IntPtr.Zero, SetupAPI.DiGetClassFlags.DIGCF_PRESENT);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }

            try
            {
                UInt32 iMemberIndex = 0;
                while (true)
                {
                    SetupAPI.SP_DEVINFO_DATA deviceInfoData = new SetupAPI.SP_DEVINFO_DATA();
                    deviceInfoData.cbSize = (uint)Marshal.SizeOf(typeof(SetupAPI.SP_DEVINFO_DATA));
                    /* get the device information data structure i.e SP_DEVINFO_DATA from the device information set for the correspoinding device index */
                    bool success = SetupAPI.SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
                    if (!success)
                    {
                        // No more devices in the device information set
                        break;
                    }

                    DeviceInfoAdvanced deviceInfo = new DeviceInfoAdvanced();


                    //deviceInfo.CM_Get_Device_ID;
                    if (GetDeviceID(deviceInfoData, out deviceInfo.DeviceInstanceID) != true)
                    {
                        continue;
                    }
                    Console.WriteLine("************************");
                    Console.WriteLine("CM_Get_Device_ID, aka DeviceInstanceID:{0}", deviceInfo.DeviceInstanceID);


                    //deviceInfo.DeviceDescription;
                    deviceInfo.DeviceDescription = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Description:{0}", deviceInfo.DeviceDescription);

                    //deviceType
                    string deviceType = GetDeviceType(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Type:{0}", deviceType);

                    //deviceInfo.HardwareIDs
                    deviceInfo.HardwareIDs = GetHardwareID(hDeviceInfoSet, deviceInfoData);

                    string[] hardWareIDArray = deviceInfo.HardwareIDs.Split('\0');
                    Console.WriteLine("HardwareIDs:");
                    foreach (string hardwareID in hardWareIDArray)
                    {
                        if (hardwareID.Length > 1)
                            Console.WriteLine("       " + hardwareID);
                    }
                    //deviceInfo.BusRprtedDevDesc
                    deviceInfo.BusRprtedDevDesc = GetDeviceBusDescription(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Bus Reported Dev Descriptor:{0}", deviceInfo.BusRprtedDevDesc);

                    //deviceInfo.DeviceManufacturer
                    deviceInfo.DeviceManufacturer = GetDeviceManufacturer(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Manufacturer:{0}", deviceInfo.DeviceManufacturer);

                    //deviceInfo.DeviceFriendlyName
                    deviceInfo.DeviceFriendlyName = GetDeviceFriendlyName(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Friendly Name:{0}", deviceInfo.DeviceFriendlyName);

                    //deviceInfo.DeviceLocationInfo
                    deviceInfo.DeviceLocationInfo = GetDeviceLocationInfo(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device location info:{0}", deviceInfo.DeviceLocationInfo);

                    //deviceInfo.ContainerID
                    deviceInfo.ContainerID = GetDeviceContainerID(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("Device Container ID:{0}", deviceInfo.ContainerID);

                    string[] splitStringArray = deviceInfo.DeviceInstanceID.Split('&');
                    //deviceInfo.VID
                    /* we look for index of "VID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 1)
                    {
                        int VID_index = splitStringArray[0].IndexOf("VID_");
                        if (VID_index > -1)
                        {
                            deviceInfo.VID = splitStringArray[0].Substring(VID_index + "VID_".Length, 4);
                            Console.WriteLine("VID:{0}", deviceInfo.VID);
                        }
                    }
                    //deviceInfo.PID
                    /* we look for index of "PID_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 2)
                    {
                        int PID_index = splitStringArray[1].IndexOf("PID_");
                        if (PID_index > -1)
                        {
                            deviceInfo.PID = splitStringArray[1].Substring(PID_index + "PID_".Length, 4);
                            Console.WriteLine("PID:{0}", deviceInfo.PID);
                        }
                    }

                    //deviceInfo.MI
                    /* we look for index of "MI_" then extract index+4 to end*/
                    if (splitStringArray.Length >= 3)
                    {
                        int MI_index = splitStringArray[2].IndexOf("MI_");
                        if (MI_index > -1)
                        {
                            deviceInfo.MI = splitStringArray[2].Substring(MI_index + "MI_".Length, 2);
                            Console.WriteLine("MI:{0}", deviceInfo.MI);
                        }
                    }

                    //deviceInfo.name
                    deviceInfo.port_name = GetDeviceName(hDeviceInfoSet, deviceInfoData);
                    Console.WriteLine("port_name:{0}", deviceInfo.port_name);

                    Console.WriteLine("************************");
                    devices.Add(deviceInfo);
                    iMemberIndex++;
                }
            }
            finally
            {
                if (SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet) == true)
                {
                    Console.WriteLine("Destroy Device Info List successful");
                }
                else
                {
                    Console.WriteLine("Destroy Device Info List Failed");
                }
            }

            return devices;
        }

        private static bool GetDeviceID(SetupAPI.SP_DEVINFO_DATA devInfoData, out string device_id)
        {
            StringBuilder devIDStrBuilder = new StringBuilder((int)StringLength.MAX_PATH);
            if (SetupAPI.CM_Get_Device_ID(devInfoData.DevInst, devIDStrBuilder, (uint)StringLength.MAX_PATH, 0) != 0)
            {
                device_id = "";
                return false;
            }
            device_id = devIDStrBuilder.ToString();
            return true;
        }

        private static string GetDeviceDescription(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, SetupAPI.SPDRP.SPDRP_DEVICEDESC,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize);
            if (!success)
            {
                // throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDeviceRegistryProperty for SPDRP_DEVICEDESC " + deviceInfoData.ClassGuid);
                return "";
            }
            return Encoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetDeviceType(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, SetupAPI.SPDRP.SPDRP_DEVTYPE,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize);
            if (!success)
            {
                // throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDeviceRegistryProperty for SPDRP_DEVTYPE " + deviceInfoData.ClassGuid);
                return "";
            }
            return Encoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetHardwareID(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, SetupAPI.SPDRP.SPDRP_HARDWAREID,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize);
            if (!success)
            {
                // throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDeviceRegistryProperty for SPDRP_HARDWAREID " + deviceInfoData.ClassGuid);
                return "";
            }
            return Encoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetDeviceBusDescription(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DEVPKEY_Device_BusReportedDeviceDesc,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize, 0);
            if (!success)
            {
                //throw new Exception("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDevicePropertyW for DEVPKEY_Device_BusReportedDeviceDesc " + deviceInfoData.ClassGuid);
                return "";
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetDeviceManufacturer(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DEVPKEY_Device_Manufacturer,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize, 0);
            if (!success)
            {
                //throw new Exception("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDevicePropertyW for DEVPKEY_Device_Manufacturer " + deviceInfoData.ClassGuid);
                return "";
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }


        private static string GetDeviceFriendlyName(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DEVPKEY_Device_FriendlyName,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize, 0);
            if (!success)
            {
                //throw new Exception("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDevicePropertyW for DEVPKEY_Device_FriendlyName " + deviceInfoData.ClassGuid);
                return "";
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetDeviceLocationInfo(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DEVPKEY_Device_LocationInfo,
            out propRegDataType, ptrBuf, SetupAPI.BUFFER_SIZE, out RequiredSize, 0);
            if (!success)
            {
                //throw new Exception("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDevicePropertyW for DEVPKEY_Device_LocationInfo " + deviceInfoData.ClassGuid);
                return "";
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

        private static string GetDeviceContainerID(IntPtr hDeviceInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            byte[] ptrBuf = new byte[16];
            uint propRegDataType;
            uint RequiredSize;
            bool success = SetupAPI.SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DEVPKEY_Device_ContainerId,
            out propRegDataType, ptrBuf, (uint)ptrBuf.Length, out RequiredSize, 0);
            if (!success)
            {
                //throw new Exception("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                Console.WriteLine("Can not read SetupDiGetDevicePropertyW for DEVPKEY_Device_ContainerId " + deviceInfoData.ClassGuid);
                return "";
            }

            Guid guid = new Guid(ptrBuf);
            return guid.ToString();
        }

        private static string GetDeviceName(IntPtr pDevInfoSet, SetupAPI.SP_DEVINFO_DATA deviceInfoData)
        {
            IntPtr hDeviceRegistryKey = SetupAPI.SetupDiOpenDevRegKey(pDevInfoSet, ref deviceInfoData,
            SetupAPI.DICS_FLAG_GLOBAL, 0, SetupAPI.DIREG_DEV, SetupAPI.KEY_QUERY_VALUE);
            if (hDeviceRegistryKey == IntPtr.Zero)
            {
                //throw new Exception("Failed to open a registry key for device-specific configuration information");
                Console.WriteLine("Failed to open a registry key for device-specific configuration information");
                return "";
            }

            byte[] ptrBuf = new byte[SetupAPI.BUFFER_SIZE];
            uint length = (uint)ptrBuf.Length;
            try
            {
                uint lpRegKeyType;
                int result = SetupAPI.RegQueryValueEx(hDeviceRegistryKey, "PortName", 0, out lpRegKeyType, ptrBuf, ref length);
                if (result != 0)
                {
                    //throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
                    Console.WriteLine("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
                    return "";
                }
            }
            finally
            {
                SetupAPI.RegCloseKey(hDeviceRegistryKey);
            }

            return Encoding.Unicode.GetString(ptrBuf, 0, (int)length - utf16terminatorSize_bytes);
        }




        public static void test_GetClassGUIDs()
        {
            Guid[] guid = GetClassGUIDs("PORTS");
            if (guid?.Length != 0)
            {
                Console.WriteLine("GUID for PORT: {0}", guid[0].ToString());
            }

            guid = GetClassGUIDs("HIDClass");
            if (guid?.Length != 0)
            {
                Console.WriteLine("GUID for HIDClass: {0}", guid[0].ToString());
            }

            guid = GetClassGUIDs("USBDevice");
            if (guid?.Length != 0)
            {
                Console.WriteLine("GUID for USBDevice: {0}", guid[0].ToString());
            }
            //guid = GetClassGUIDs("USB");
            //if (guid?.Length != 0)
            //{
            //    Console.WriteLine("GUID for USB: {0}", guid[0].ToString());
            //}
        }

        /* This function gets the GUIDS for the class name passed in.
        * This only works for GUIDs defined in System-defined device setup classes available to vendors
        * As an example if you pass "PORTS" it will return the GUID 
        * {4d36e978-e325-11ce-bfc1-08002be10318}, whcihs is associated with class name  Ports. 
        * You can find a list of class names and associated GUID in the follwing link 
        * https://learn.microsoft.com/en-us/windows-hardware/drivers/install/system-defined-device-setup-classes-available-to-vendors?source=recommendations
            * */
        private static Guid[] GetClassGUIDs(string className)
        {
            UInt32 requiredSize = 0;
            Guid[] guidArray = new Guid[1];

            bool status = SetupAPI.SetupDiClassGuidsFromName(className, ref guidArray[0], 1, out requiredSize);
            if (true == status)
            {
                if (1 < requiredSize)
                {
                    guidArray = new Guid[requiredSize];
                    SetupAPI.SetupDiClassGuidsFromName(className, ref guidArray[0], requiredSize, out requiredSize);
                }
            }
            else
                throw new System.ComponentModel.Win32Exception();

            return guidArray;

        }
        /* This funtion test the SetupDiGetClassDevs() function with  "USB" passed in as the enumerator */
        public static void test_SetupDiGetClassDevs_usb()
        {
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(IntPtr.Zero, "USB", 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT | SetupAPI.DiGetClassFlags.DIGCF_ALLCLASSES);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }
        }

        /* This funtion test the SetupDiGetClassDevs() function with  "PORT" passed in as the enumerator */
        public static void test_SetupDiGetClassDevs_port()
        {
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs(IntPtr.Zero, "PORT", 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT | SetupAPI.DiGetClassFlags.DIGCF_ALLCLASSES);
            if (hDeviceInfoSet == IntPtr.Zero)
            {
                Console.WriteLine(" SetupDiGetClassDevs failed ");
            }
        }

    }

}
