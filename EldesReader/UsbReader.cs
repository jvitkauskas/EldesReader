using System;
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
        
        public string MakeRequest(string command, string? responseText = null)
        {
            var requestData = new byte[_packetLength];

            var request = Encoding.ASCII.GetBytes($"{(char) (command.Length+1)}{command}\r");
            Array.Copy(request, 0, requestData, 1, request.Length);

            _device.Write(requestData);

            while (true) // TODO: timeout?
            {
                var responseData = _device.Read();
                
                var responseLength = responseData.Data[1];

                if (responseLength == 0)
                {
                    continue;
                }
                
                var response = Encoding.ASCII.GetString(responseData.Data[2..][..responseLength]);

                if (response.Contains(responseText ?? command))
                {
                    return response;
                }
            }
        }
    }
}