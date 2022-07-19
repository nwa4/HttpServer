using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core
{
    public interface IWebServer : IDisposable
    {
        Task StartAsync();
        void Stop();

    }
}
