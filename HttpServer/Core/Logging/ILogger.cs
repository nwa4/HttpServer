using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public interface ILogger
    {
        Task LogAsync(WebRequest request);
        Task LogAsync(WebResponse response);
        Task LogAsync(String message);
    }
}
