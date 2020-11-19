﻿using System;
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
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10); // response seems to come in under 20ms

        static void Main(string[] args)
        {
            var device = HidDevices.Enumerate(UsbVendorId, UsbProductId).FirstOrDefault();
            if (device is null)
            {
                Console.WriteLine("Device not connected or another program is using it");
                return;
            }

            var usbReader = new UsbReader(device);

            var clockResponse = usbReader
                .MakeRequest("clock")
                .FindResponse("clck", Timeout);
            
            Console.WriteLine($"Clock response: {clockResponse}");
            
            var uptimeResponse = usbReader
                .MakeRequest("uptime")
                .FindResponse("UPT", Timeout);
            
            Console.WriteLine($"Uptime response: {uptimeResponse}");
            
            var zonesResponse = usbReader
                .MakeRequest("zstatus")
                .FindResponse("zstatus", Timeout);
            
            Console.WriteLine($"Zones response: {zonesResponse}");

            if (zonesResponse != null)
            {
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
}