using System;
using System.Net.Http;
using System.Threading.Tasks;
using HttpServer.Core;
using HttpServer.Core.Logging;
using HttpServer.Core.Routes;
using HttpServer.Core.Validations;

namespace HttpServer
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure
            var webRoot = $"{Environment.CurrentDirectory}\\webroot";
            var accpetedMethods = new HttpMethod[] { HttpMethod.Get };
            var address = "127.0.0.1";
            var port = 80;

            // Build
            IWebServer httpServer = new WebServer(address, port)
                                    .AddRequestValidators(new HttpMethodRequestValidator(accpetedMethods))
                                    .AddRequestExecutors(new StaticFileRequestExecutor(webRoot))
                                    .AddLoggers(new ConsoleLogger());

            Console.WriteLine($"HttpServer runing on {address}:{port}");

            // Run Server
            using (httpServer)
            {
                await httpServer.StartAsync();
            }

            Console.ReadLine();
        }
    }
}