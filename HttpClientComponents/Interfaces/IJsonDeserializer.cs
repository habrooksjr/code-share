using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Common.Api.HttpClientComponents.Interfaces
{
    public interface IJsonDeserializer
    {
        Task<HttpClientResult<T>> TryDeserializeAsync<T>(Stream streamContent, CancellationToken cancellationToken);

        Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);
    }
}
