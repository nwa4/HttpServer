using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HttpServer.Core
{
    public class WebRequest
    {
        public WebRequest()
        {
        }

        private WebRequest(Dictionary<string, string> headers, HttpMethod httpMethod, string route, string httpVersion, string content)
        {
            this.Headers = headers;
            this.Method = httpMethod;
            this.Route = route;
            this.HttpVersion = httpVersion;
            this.Body = content;
        }

        public Dictionary<string, string> Headers { get; private set; }

        public HttpMethod Method { get; private set; }

        public string Route { get; private set; }

        public string HttpVersion { get; private set; }

        public string Body { get; private set; }

        public static WebRequest Parse(string rawRequest)
        {
            if (string.IsNullOrWhiteSpace(rawRequest))
            {
                return new WebRequest();
            }

            var headerAndBody = rawRequest.Split(new string[] { Environment.NewLine + Environment.NewLine }, StringSplitOptions.None);

            if (headerAndBody.Length == 1)
            {
                return new WebRequest();
            }

            var headerLines = headerAndBody[0].Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var firstLine = headerLines[0].Split(' ');
            var verb = firstLine[0];
            var route = firstLine[1];
            var httpVersion = firstLine[2];

            HttpMethod method = new HttpMethod(verb);

            var headers = new Dictionary<string, string>();
            for (int i = 1; i < headerLines.Length; i++)
            {
                var line = headerLines[i].Split(' ');
                if (line.Length == 2)
                {
                    headers.Add(line[0], line[1]);
                }
            }

            return new WebRequest(headers, method, route, httpVersion, headerAndBody.Length == 2 ? headerAndBody[1] : string.Empty);
        }
    }
}
