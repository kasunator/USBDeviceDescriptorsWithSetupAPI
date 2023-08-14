using System;
using USBDevDescriptorAccess;

namespace USBDevDescriptorAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //Win32DeviceMgmt.GetAllCOMPorts();
            //Win32DeviceMgmt.GetAllUSBDEvices();
            Win32DeviceMgmt.GetAllUSBDEvicesADvacned();
            //Win32DeviceMgmt.GetAllUsbHidDEvices();
        }
    }
}
