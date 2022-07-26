﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HttpServer.Core
{
    public class WebResponse
    {
        private WebResponse(HttpStatusCode statusCode, string content, Dictionary<string, string> headers)
        {
            this.Headers = headers ?? new Dictionary<string, string> { { "Content-Type", "text/html;charset=UTF-8" } };

            this.StatusCode = statusCode;
            this.Body = content ?? string.Empty;
        }

        public Dictionary<string, string> Headers { get; private set; }

        public HttpStatusCode StatusCode { get; private set; }

        public string Body { get; private set; }

        public static WebResponse Create(HttpStatusCode statusCode, string content, Dictionary<string, string> headers)
        {
            return new WebResponse(statusCode, content, headers);
        }

        public static WebResponse Create(HttpStatusCode statusCode)
        {
            return new WebResponse(statusCode, string.Empty, null);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"HTTP/1.1 {(int)this.StatusCode} {this.StatusCode.ToString()}");
            builder.AppendLine($"Date: {DateTime.Now}");

            foreach (var header in this.Headers)
            {
                builder.AppendLine($"{header.Key}: {header.Value}");
            }

            builder.AppendLine(string.Empty);
            builder.AppendLine(this.Body);

            return builder.ToString();
        }
    }
}
