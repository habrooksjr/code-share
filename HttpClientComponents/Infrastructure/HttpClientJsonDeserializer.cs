using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Rise.Common.Api.HttpClientComponents.Interfaces;
using Rise.Common.Api.HttpClientComponents.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Common.Api.HttpClientComponents.Infrastructure
{
    public class HttpClientJsonDeserializer : IJsonDeserializer
    {
        public HttpClientJsonDeserializer()
        {
            _jsonSerializer = new JsonSerializer { ContractResolver = JsonSerializerSettings.ContractResolver };
        }

        private readonly JsonSerializer _jsonSerializer;

        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken)
        {
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var loaded = await JToken.LoadAsync(jsonReader, cancellationToken);
                    return loaded.ToObject<T>(_jsonSerializer);
                }
            }
        }

        public async Task<HttpClientResult<T>> TryDeserializeAsync<T>(Stream streamContent, CancellationToken cancellationToken)
        {
            try
            {
                var result = await DeserializeAsync<T>(streamContent, cancellationToken);
                return HttpClientResult<T>.Ok(result);
            }
            catch (Exception e)
            {
                return HttpClientResult.Fail<T>(e.Message);
            }
        }
    }
}
