using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HttpServer.Core.Logging;
using HttpServer.Core.Routes;
using HttpServer.Core.Validations;

namespace HttpServer.Core
{
    public class WebServer : IWebServer
    {
        private readonly Socket socket;
        private IList<IRequestValidator> requestValidators = new List<IRequestValidator>();
        private IList<IRequestRouteExecutor> requestExecutors = new List<IRequestRouteExecutor>();
        private IList<ILogger> loggers = new List<ILogger>();

        public WebServer()
            : this(IPAddress.Loopback.ToString(), 80)
        {
        }

        public WebServer(string ipAddress, int port)
        {
            this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            this.socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        }

        public async Task StartAsync()
        {
            this.socket.Listen(1);
            while (true)
            {
                using (var client = await this.socket.AcceptAsync())
                {
                    ArraySegment<byte> data = new ArraySegment<byte>(new byte[client.ReceiveBufferSize]);
                    await client.ReceiveAsync(data, SocketFlags.None);

                    var request = WebRequest.Parse(Encoding.UTF8.GetString(data.Array));

                    // Log Requests
                    await this.LogRequestAsync(request);

                    WebResponse response;

                    // Validate Request
                    response = this.ValidateRequest(request);

                    if (response != null)
                    {
                        await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())), SocketFlags.None);
                        break;
                    }

                    // Match Request/Dispatch route
                    response = await this.ExecuteRequestAsync(request);

                    // Log response
                    await this.LogResponseAsync(response);

                    // Send response
                    await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response?.ToString())), SocketFlags.None);
                }
            }
        }

        public WebServer AddRequestValidators(IRequestValidator validator)
        {
            this.requestValidators.Add(validator);
            return this;
        }

        public WebServer AddRequestExecutors(IRequestRouteExecutor executor)
        {
            this.requestExecutors.Add(executor);
            return this;
        }

        public WebServer AddLoggers(ILogger logger)
        {
            this.loggers.Add(logger);
            return this;
        }

        public void Terminate()
        {
            this.socket.Disconnect(false);
            this.socket.Close();
            this.socket.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this.Terminate();
        }

        private WebResponse ValidateRequest(WebRequest request)
        {
            if (this.requestValidators == null)
            {
                return null;
            }

            foreach (var validator in this.requestValidators)
            {
                var response = validator.Validate(request);
                if (response != null)
                {
                    return response;
                }
            }

            return null;
        }

        private async Task<WebResponse> ExecuteRequestAsync(WebRequest request)
        {
            if (this.requestExecutors == null)
            {
                return WebResponse.Create(HttpStatusCode.NotFound, string.Empty, null);
            }

            WebResponse response = null;

            // Handle errors from request handlers
            try
            {
                foreach (var executor in this.requestExecutors)
                {
                    response = await executor.ExecuteAsync(request);
                    if (response != null)
                    {
                        break;
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                await this.LogRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.Forbidden);
            }
            catch (IndexOutOfRangeException ex)
            {
                await this.LogRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.ExpectationFailed);
            }
            catch (AccessViolationException ex)
            {
                await this.LogRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                await this.LogRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.InternalServerError);
            }

            if (response != null)
            {
                return response;
            }

            return WebResponse.Create(HttpStatusCode.NotFound);
        }

        private async Task LogRequestAsync(WebRequest request)
        {
            if (this.loggers != null)
            {
                foreach (var logger in this.loggers)
                {
                    await logger.LogAsync(request);
                }
            }
        }

        private async Task LogResponseAsync(WebResponse response)
        {
            if (this.loggers != null)
            {
                foreach (var logger in this.loggers)
                {
                    await logger.LogAsync(response);
                }
            }
        }

        private async Task LogRequestAsync(string message)
        {
            if (this.loggers != null)
            {
                foreach (var logger in this.loggers)
                {
                    await logger.LogAsync(message);
                }
            }
        }
    }
}