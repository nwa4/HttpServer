using System.Linq;
using System.Net;
using System.Net.Http;

namespace HttpServer.Core.Validations
{
    public class HttpMethodRequestValidator : IRequestValidator
    {
        private readonly HttpMethod[] _supportedMethods;

        public HttpMethodRequestValidator(HttpMethod[] supportedMethods)
        {
            this._supportedMethods = supportedMethods;
        }
        public WebResponse Validate(WebRequest request)
        {
            if (!this._supportedMethods.Contains(request.Method))
            {
                return WebResponse.Create(HttpStatusCode.MethodNotAllowed);
            }

            return null;
        }
    }
}
