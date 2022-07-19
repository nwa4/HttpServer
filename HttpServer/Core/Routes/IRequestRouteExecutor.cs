using System.Threading.Tasks;

namespace HttpServer.Core.Routes
{
    public interface IRequestRouteExecutor
    {
        Task<WebResponse> ExecuteAsync(WebRequest request);
    }
}
