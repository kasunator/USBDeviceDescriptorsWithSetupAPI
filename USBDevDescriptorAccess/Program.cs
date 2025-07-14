using System;
using USBDevDescriptorAccess;

namespace USBDevDescriptorAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World! C#");
            //Win32DeviceMgmt.GetAllCOMPorts();
            //Win32DeviceMgmt.GetAllUSBDEvices();
            //Win32DeviceMgmt.GetAllUSBDEvicesADvacned();
            //Win32DeviceMgmt.GetAllUsbHidDEvices();
            //Win32DeviceMgmt.test_GetClassGUIDs();
            //Win32DeviceMgmt.test_SetupDiGetClassDevs_usb();
            Win32DeviceMgmt.test_SetupDiGetClassDevs_port();
        }
    }
}
