using System;
using System.Diagnostics;
using System.Text;
using HidLibrary;

namespace EldesReader
{
    public class UsbReader
    {
        private readonly HidDevice _device;
        private readonly int _packetLength;

        public UsbReader(HidDevice device)
        {
            _device = device;
            _packetLength = device.Capabilities.OutputReportByteLength - 1;
        }
        
        public UsbReader MakeRequest(string command, string? responseText = null)
        {
            var requestData = new byte[_packetLength];

            var request = Encoding.ASCII.GetBytes($"{(char) (command.Length+1)}{command}\r");
            Array.Copy(request, 0, requestData, 1, request.Length);

            _device.Write(requestData);

            return this;
        }
        
        // The alarm panel sends packets constantly and we need to find the one which matches our request
        public string? FindResponse(string responseTextPart, TimeSpan? timeout = null)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed <= (timeout ?? TimeSpan.MaxValue))
            {
                var responseData = _device.Read();
                
                if (responseData.Data.Length < 2)
                {
                    continue;
                }
                
                var responseLength = responseData.Data[1];

                if (responseLength == 0)
                {
                    continue;
                }
                
                var response = Encoding.ASCII.GetString(responseData.Data[2..][..responseLength]);

                if (response.Contains(responseTextPart))
                {
                    return response;
                }
            }

            return null;
        }
    }
}