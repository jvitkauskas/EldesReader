using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using HidLibrary;

namespace EldesReader
{
    class Program
    {
        private static readonly int UsbVendorId = 0xc201;
        private static readonly int UsbProductId = 0x1318;

        static void Main(string[] args)
        {
            var device = HidDevices.Enumerate(UsbVendorId, UsbProductId).FirstOrDefault();
            if (device is null)
            {
                Console.WriteLine("Device not connected");
                return;
            }

            var usbReader = new UsbReader(device);

            var clockResponse = usbReader.MakeRequest("clock", "clck");
            Console.WriteLine($"Clock response: {clockResponse}");
            
            var uptimeResponse = usbReader.MakeRequest("uptime", "UPT");
            Console.WriteLine($"Uptime response: {uptimeResponse}");
            
            var zonesResponse = usbReader.MakeRequest("zstatus");
            Console.WriteLine($"Zones response: {zonesResponse}");

            var zoneStatusHexStr = zonesResponse[14..16]; // eg. zstatus:0000002E000000000000000000000000000000 => 2E
            var zoneStatusInt = int.Parse(zoneStatusHexStr, NumberStyles.HexNumber);
            var zones = new BitArray(new[] {zoneStatusInt});

            int i = 0;
            foreach (bool zone in zones)
            {
                Console.WriteLine($"Zone {++i}: {(zone ? "Open" : "Closed")}");
            }
        }
    }
}