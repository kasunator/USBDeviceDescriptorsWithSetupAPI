using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SetupAPIDLLImports;

namespace USBDevDescriptorAccess
{
    class Win32DeviceMgmt
    {
        const int utf16terminatorSize_bytes = 2;

        public struct DeviceInfo
        {
            public string name;
            public string description;
            public string bus_description;
        }

        public struct DeviceInfoAdvanced
        {
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

        }


        static SetupAPI.DEVPROPKEY DEVPKEY_Device_BusReportedDeviceDesc;

        static Win32DeviceMgmt()
        {
            DEVPKEY_Device_BusReportedDeviceDesc = new SetupAPI.DEVPROPKEY();
            DEVPKEY_Device_BusReportedDeviceDesc.fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2);
            DEVPKEY_Device_BusReportedDeviceDesc.pid = 4;
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
            IntPtr hDeviceInfoSet = SetupAPI.SetupDiGetClassDevs( IntPtr.Zero,"USB", 0, SetupAPI.DiGetClassFlags.DIGCF_PRESENT| SetupAPI.DiGetClassFlags.DIGCF_ALLCLASSES );
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
            List<DeviceInfoAdvanced> devices = new List<DeviceInfoAdvanced>();
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

                    DeviceInfoAdvanced deviceInfo = new DeviceInfoAdvanced();
                    if (SetupAPI.CM_Get_Device_ID(deviceInfoData.DevInst, devIDStrBuilder, 260, 0) != 0)
                        continue;

                    Console.WriteLine("CM_Get_Device_ID:{0}", devIDStrBuilder.ToString());


                    //deviceInfo.DeviceDescription
                    //SetupDiGetDeviceRegistryProperty(,,SPDRP_DEVICEDESC)

                    //deviceInfo.HardwareIDs


                    //deviceInfo.BusRprtedDevDesc


                    //deviceInfo.DeviceManufacturer


                    //deviceInfo.DeviceFriendlyName


                    //deviceInfo.DeviceLocationInfo


                    //deviceInfo.ContainerID


                    //deviceInfo.VID


                    //deviceInfo.PID


                    //deviceInfo.MI


                    devices.Add(deviceInfo);
                    iMemberIndex++;
                }
            }
            finally
            {
                SetupAPI.SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
            }

            return devices;
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
                Console.WriteLine("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
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
                Console.WriteLine("Can not read Bus provided device description device " + deviceInfoData.ClassGuid);
                return "";
            }
            return System.Text.UnicodeEncoding.Unicode.GetString(ptrBuf, 0, (int)RequiredSize - utf16terminatorSize_bytes);
        }

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
    }


}
