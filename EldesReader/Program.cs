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
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1); // full response seems to come in under 50ms

        static void Main(string[] args)
        {
            var device = HidDevices.Enumerate(UsbVendorId, UsbProductId).FirstOrDefault();
            if (device is null)
            {
                Console.WriteLine("Device not connected or another program is using it");
                return;
            }

            var usbReader = new UsbReader(device);
            
            var versionResponse = usbReader
                .MakeRequest("getvertion")
                .FindResponse(" V", Timeout); // eg. ESIM364 V02.16.02
            
            Console.WriteLine($"Version response: {versionResponse}");
            
            var serviceModeResponse = usbReader
                .MakeRequest("getservicemodestatus")
                .FindResponse("servicemode", Timeout); // eg. servicemode:off
            
            Console.WriteLine($"Service mode response: {serviceModeResponse}");
            
            var clockResponse = usbReader
                .MakeRequest("clock")
                .FindResponse("clck", Timeout); // eg. clck 2020-11-21 01:32:33
            
            Console.WriteLine($"Clock response: {clockResponse}");
            
            var uptimeResponse = usbReader
                .MakeRequest("uptime")
                .FindResponse("UPT", Timeout); // eg. UPT 16404
            
            Console.WriteLine($"Uptime response: {uptimeResponse}");
            
            var zonesResponse = usbReader
                .MakeRequest("zstatus")
                .FindResponse("zstatus", Timeout);

            Console.WriteLine($"Zones response: {zonesResponse}");

            if (zonesResponse != null)
            {
                var zoneStatusHexStr = zonesResponse[14..16];
                var zoneStatusInt = int.Parse(zoneStatusHexStr, NumberStyles.HexNumber);
                var zones = new BitArray(new[] {zoneStatusInt});
                
                var tamperStatusHexStr = zonesResponse[46..48];
                var tamperStatusInt = int.Parse(tamperStatusHexStr, NumberStyles.HexNumber);
                var tampers = new BitArray(new[] {tamperStatusInt});
                
                for (int i = 0; i < 32; i++)
                {
                    Console.WriteLine($"Zone {i+1:00}: {(zones[i] ? "Open" : "Closed")}, {(tampers[i] ? "Tamper" : "No tamper")}");
                }
            }
        }
    }
}