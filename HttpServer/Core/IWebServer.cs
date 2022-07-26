using System;
using System.Threading.Tasks;

namespace HttpServer.Core
{
    public interface IWebServer : IDisposable
    {
        Task StartAsync();

        void Terminate();
    }
}
