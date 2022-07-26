using System.Threading.Tasks;

namespace HttpServer.Core.Logging
{
    public interface ILogger
    {
        Task LogAsync(WebRequest request);

        Task LogAsync(WebResponse response);

        Task LogAsync(string message);
    }
}
