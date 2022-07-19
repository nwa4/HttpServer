using HttpServer.Core.Logging;
using HttpServer.Core.Routes;
using HttpServer.Core.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer.Core
{
    public class WebServer : IWebServer
    {
        #region Fields
        readonly Socket _socket;
        IList<IRequestValidator> _requestValidators = new List<IRequestValidator>();
        IList<IRequestRouteExecutor> _requestExecutors = new List<IRequestRouteExecutor>();
        IList<ILogger> _loggers = new List<ILogger>();
        #endregion

        #region Constructors
        public WebServer() : this(IPAddress.Loopback.ToString(), 80) { }

        public WebServer(string ipAddress, int port)
        {
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), port));
        }
        #endregion

        #region Public Methods
        public async Task StartAsync()
        {
            _socket.Listen(1);
            while (true)
            {
                using (var client = await _socket.AcceptAsync())
                {
                    ArraySegment<byte> data = new ArraySegment<byte>(new byte[client.ReceiveBufferSize]);
                    await client.ReceiveAsync(data, SocketFlags.None);

                    var request = WebRequest.Parse(Encoding.UTF8.GetString(data.Array));

                    //Log Requests
                    await logRequestAsync(request);

                    WebResponse response;

                    //Validate Request
                    response = validateRequest(request);

                    if (response != null)
                    {
                        await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response.ToString())), SocketFlags.None);
                        break;
                    }

                    //Match Request/Dispatch route
                    response = await executeRequestAsync(request);

                    //Log response
                    await logResponseAsync(response);
                    
                    //Send response
                    await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(response?.ToString())), SocketFlags.None);

                }

            }
        }

        public WebServer AddRequestValidators(IRequestValidator validator)
        {
            this._requestValidators.Add(validator);
            return this;
        }

        public WebServer AddRequestExecutors(IRequestRouteExecutor executor)
        {
            this._requestExecutors.Add(executor);
            return this;
        }

        public WebServer AddLoggers(ILogger logger)
        {
            this._loggers.Add(logger);
            return this;
        }

        public void Dispose()
        {
            this.Stop();
        }

        public void Stop()
        {
            _socket.Disconnect(false);
            _socket.Close();
        }

        #endregion

        #region Private Methods

        WebResponse validateRequest(WebRequest request)
        {
            if (_requestValidators == null)
            {
                return null;
            }

            foreach (var validator in _requestValidators)
            {
                var response = validator.Validate(request);
                if (response != null)
                {
                    return response;
                }
            }

            return null;
        }

        async Task<WebResponse> executeRequestAsync(WebRequest request)
        {
            if (_requestExecutors == null)
            {
                return WebResponse.Create(HttpStatusCode.NotFound, String.Empty, null);
            }

            WebResponse response = null;

            //Handle errors from request handlers
            try
            {
                foreach (var executor in _requestExecutors)
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
                await logRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.Forbidden);
            }
            catch(IndexOutOfRangeException ex)
            {
                await logRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.ExpectationFailed);
            }
            catch (AccessViolationException ex)
            {
                await logRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.Unauthorized);
            }
            catch (Exception ex)
            {
                await logRequestAsync(ex.Message);
                response = WebResponse.Create(HttpStatusCode.InternalServerError);
            }

            if (response != null)
            {
                return response;
            }
            
            return WebResponse.Create(HttpStatusCode.NotFound); ;
        }

        async Task logRequestAsync(WebRequest request)
        {
            if (_loggers != null)
            {
                foreach (var logger in _loggers)
                {
                    await logger.LogAsync(request);
                }
            }
        }

        async Task logResponseAsync(WebResponse response)
        {
            if (_loggers != null)
            {
                foreach (var logger in _loggers)
                {
                    await logger.LogAsync(response);
                }
            }
        }

        async Task logRequestAsync(string message)
        {
            if (_loggers != null)
            {
                foreach (var logger in _loggers)
                {
                    await logger.LogAsync(message);
                }
            }
        }
        #endregion
    }
}
