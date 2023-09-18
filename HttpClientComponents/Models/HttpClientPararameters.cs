using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace Rise.Common.Api.HttpClientComponents.Models
{
    class HttpClientPararameters
    {
        public string Uri { get; set; }

        public SortedDictionary<string, string> Headers { get; set; }

        public string BearerToken { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public object Body { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public HttpMethod Method { get; set; }

        public HttpStatusCode AcceptedStatusCode { get; set; }
    }
}
