using Newtonsoft.Json;
using Rise.Common.Api.HttpClientComponents.Infrastructure;
using Rise.Common.Api.HttpClientComponents.Models;
using Rise.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Common.Api.HttpClientComponents.Abstractions
{
    public abstract class BaseHttpClient
    {
        public BaseHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _jsonDeserializer = new HttpClientJsonDeserializer();
        }

        private readonly HttpClient _httpClient;

        private readonly HttpClientJsonDeserializer _jsonDeserializer;

        private List<HttpStatusCode> _retryableStatusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.InternalServerError,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.BadGateway,
            HttpStatusCode.TooManyRequests
        };

        public async Task<HttpClientResult<T>> Get<T>(string uri) where T : class
        {
            return await Get<T>(uri, CancellationToken.None);
        }

        public async Task<HttpClientResult<T>> Get<T>(string uri, CancellationToken cancellationToken) where T : class
        {
            var parameters = new HttpClientPararameters
            {
                Method = HttpMethod.Get,
                AcceptedStatusCode = HttpStatusCode.OK,
                Uri = uri,
                CancellationToken = cancellationToken
            };

            return await Send<T>(parameters);
        }

        public async Task<HttpClientResult> Post(string uri, object body)
        {
            return await Post(uri, CancellationToken.None);
        }

        public async Task<HttpClientResult> Post(string uri, object body, CancellationToken cancellationToken)
        {
            var parameters = new HttpClientPararameters
            {
                Method = HttpMethod.Post,
                AcceptedStatusCode = HttpStatusCode.Created,
                Uri = uri,
                Body = body,
                CancellationToken = cancellationToken
            };

            return await Send(parameters);
        }

        public async Task<HttpClientResult> Put(string uri, object body)
        {
            return await Post(uri, CancellationToken.None);
        }

        public async Task<HttpClientResult> Put(string uri, object body, CancellationToken cancellationToken)
        {
            var parameters = new HttpClientPararameters
            {
                Method = HttpMethod.Put,
                AcceptedStatusCode = HttpStatusCode.Created,
                Uri = uri,
                Body = body,
                CancellationToken = cancellationToken
            };

            return await Send(parameters);
        }

        private async Task<HttpClientResult> Send(HttpClientPararameters parameters)
        {
            var httpRequestMessage = new HttpRequestMessage(parameters.Method, parameters.Uri);

            if(parameters.Body != null)
            {
                httpRequestMessage.Content = parameters.Body.AsHttpContent();
            }

            try
            {
                try
                {
                    using (var httpClient = _httpClient ?? new HttpClient())
                    {
                        var res = await httpClient.SendAsync(httpRequestMessage, parameters.CancellationToken);

                        if (res.Content == null) return HttpClientResult.Fail<T>(HttpStatusCode.NoContent, "Response content was null");

                        if (_retryableStatusCodes.Contains(res.StatusCode)) return HttpClientResult.Retry(res.StatusCode, $"Retryable response code returned");

                        if (res.StatusCode != parameters.AcceptedStatusCode) return HttpClientResult.Fail(res.StatusCode, $"Response code is was not expected");
                    }

                    return HttpClientResult.Ok();
                }
                catch (AggregateException ex)
                {
                    var innerEx = ex.GetInnerMostException();
                    throw innerEx;
                }
            }
            catch (TimeoutException ex)
            {
                return HttpClientResult.Retry(ex);
            }
            catch (OperationCanceledException ex)
            {
                return HttpClientResult.Retry(ex);
            }
            catch (Exception ex)
            {
                return HttpClientResult.Fail(ex);
            }
        }

        private async Task<HttpClientResult<T>> Send<T>(HttpClientPararameters parameters) where T : class
        {
            var httpRequestMessage = new HttpRequestMessage(parameters.Method, parameters.Uri);

            try
            {
                try
                {
                    T result = null;

                    using (var httpClient = _httpClient ?? new HttpClient())
                    {
                        var res = await httpClient.SendAsync(httpRequestMessage, parameters.CancellationToken);

                        if (res.Content == null) return HttpClientResult.Fail<T>(HttpStatusCode.NoContent, "Response content was null");

                        if (_retryableStatusCodes.Contains(res.StatusCode)) return HttpClientResult.Retry<T>(res.StatusCode, $"Retryable response code returned");

                        if (res.StatusCode != parameters.AcceptedStatusCode) return HttpClientResult.Fail<T>(res.StatusCode, $"Response code is was not expected");

                        var resStream = await res.Content.ReadAsStreamAsync();

                        var deserializationResult = await _jsonDeserializer.TryDeserializeAsync<T>(resStream, parameters.CancellationToken);

                        if (deserializationResult.Success == false) return HttpClientResult.Fail<T>("Failed to deserialize response");

                        result = deserializationResult.Value;
                    }

                    return HttpClientResult.Ok(result);
                }
                catch (AggregateException ex)
                {
                    var innerEx = ex.GetInnerMostException();
                    throw innerEx;
                }
            }
            catch (TimeoutException ex)
            {
                return HttpClientResult.Retry<T>(ex);
            }
            catch (OperationCanceledException ex)
            {
                return HttpClientResult.Retry<T>(ex);
            }
            catch (Exception ex)
            {
                return HttpClientResult.Fail<T>(ex);
            }
        }
    }
}
