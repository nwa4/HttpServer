using System;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public class ConsoleLogger : ILogger
    {
        public async Task LogAsync(WebRequest request)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now} {request?.Method} {request.Route} {request.HttpVersion} recieved.");
        }

        public async Task LogAsync(string message)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now} Message: {message}.");
        }

        public async Task LogAsync(WebResponse response)
        {
            await Console.Out.WriteLineAsync($"{DateTime.Now} Response: {response?.StatusCode}.");
        }
    }
}
