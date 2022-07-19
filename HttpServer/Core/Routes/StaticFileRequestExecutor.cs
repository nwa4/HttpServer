using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServer.Core.Routes
{
    public class StaticFileRequestExecutor : IRequestRouteExecutor
    {
        private readonly string webRoot;
        private readonly string defaultPage = "index.html";

        public StaticFileRequestExecutor(String webRoot)
        {
            this.webRoot = webRoot;
        }
        public async Task<WebResponse> ExecuteAsync(WebRequest request)
        {
            if (String.IsNullOrWhiteSpace(request.Route) || String.IsNullOrWhiteSpace(request.HttpVersion))
            {
                return WebResponse.Create(HttpStatusCode.BadRequest);
            }

            var fileName = request.Route == "/" ? this.defaultPage : request.Route.Substring(1);

            var files = new DirectoryInfo(webRoot).GetFiles();
            var file = files.FirstOrDefault(f=>f.Name.ToLower() == fileName.ToLower());

            if (file == null)
            {
                return null;
            }
            else
            {
                String content = string.Empty;
                using (var reader = new StreamReader(file.FullName, Encoding.UTF8))
                {
                    content = await reader.ReadToEndAsync();
                }
                
                return WebResponse.Create(HttpStatusCode.OK, content, null);
            }
        }
    }
}