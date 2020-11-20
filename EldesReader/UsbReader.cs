using System;
using System.Diagnostics;
using System.Text;
using HidLibrary;

namespace EldesReader
{
    public class UsbReader
    {
        private readonly HidDevice _device;

        public UsbReader(HidDevice device)
        {
            _device = device;
        }
        
        public UsbReader MakeRequest(string command, string? responseText = null)
        {
            var request = Encoding.ASCII.GetBytes($"\0{(char) (command.Length+1)}{command}\r");

            _device.Write(request);

            return this;
        }
        
        // The alarm panel sends packets constantly and we need to find the one which matches our request
        public string? FindResponse(string responseTextPart, TimeSpan? timeout = null)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var response = new StringBuilder();

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
                
                    var chunk = Encoding.ASCII.GetString(responseData.Data[2..][..responseLength]);

                    if (response.Length == 0 && !chunk.Contains(responseTextPart))
                    {
                        continue;
                    }
                
                    response.Append(chunk);

                    if (chunk.Contains('\n'))
                    {
                        return response.ToString();
                    }
                }

                return null;
            }
            finally
            {
                _device.CloseDevice();
            }
        }
    }
}