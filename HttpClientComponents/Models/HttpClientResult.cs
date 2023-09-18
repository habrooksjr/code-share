using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace Rise.Common.Api.HttpClientComponents.Models
{
    public class HttpClientResult
    {
        public int? StatusCode { get; private set; }

        public bool Success { get; private set; }

        public bool Retryable { get; private set; }

        public string ErrorMessage { get; private set; }

        public Exception Error { get; private set; } 

        protected HttpClientResult(HttpStatusCode? statusCode, bool success, string errorMsg, Exception ex, bool retryable)
        {
            StatusCode = statusCode != null ? (int)statusCode.Value : (int?)null;

            Success = success;

            ErrorMessage = errorMsg;

            Error = ex;

            Retryable = retryable;
        }

        public static HttpClientResult Ok()
        {
            return new HttpClientResult(HttpStatusCode.OK, true, null, null, false);
        }

        public static HttpClientResult<T> Ok<T>(T value)
        {
            return new HttpClientResult<T>(value, HttpStatusCode.OK, true, null, null, false);
        }

        public static HttpClientResult Fail(HttpStatusCode statusCode, string errorMsg)
        {
            return new HttpClientResult(statusCode, false, errorMsg, null, false);
        }

        public static HttpClientResult Fail(Exception ex)
        {
            return new HttpClientResult(null, false, ex.Message, ex, false);
        }

        public static HttpClientResult<T> Fail<T>(HttpStatusCode statusCode, string errorMsg)
        {
            return new HttpClientResult<T>(default(T), statusCode, false, errorMsg, null, false);
        }

        public static HttpClientResult<T> Fail<T>(string errorMsg)
        {
            return new HttpClientResult<T>(default(T), null, false, errorMsg, null, false);
        }

        public static HttpClientResult<T> Fail<T>(Exception ex)
        {
            return new HttpClientResult<T>(default(T), null, false, ex.Message, ex, false);
        }

        public static HttpClientResult Retry(HttpStatusCode statusCode, string errorMsg)
        {
            return new HttpClientResult(statusCode, false, errorMsg, null, true);
        }

        public static HttpClientResult Retry(Exception ex)
        {
            return new HttpClientResult(null, false, ex.Message, ex, true);
        }

        public static HttpClientResult<T> Retry<T>(HttpStatusCode statusCode, string errorMsg)
        {
            return new HttpClientResult<T>(default(T), statusCode, false, errorMsg, null, true);
        }

        public static HttpClientResult<T> Retry<T>(Exception ex)
        {
            return new HttpClientResult<T>(default(T), null, false, ex.Message, ex, true);
        }
    }

    public class HttpClientResult<T> : HttpClientResult
    {
        public T Value { get; private set; }

        protected internal HttpClientResult(T value, HttpStatusCode? statusCode, bool success, string errorMsg, Exception ex, bool retryable) : base(statusCode, success, errorMsg, ex, retryable)
        {
            Value = value;
        }
    }
}
