using System.IO;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Clay.Json
{
    public interface IJsonSerializer<T>
    {
        ValueTask Serialize(PipeWriter pipe, T instance, JsonWriterState state = default, CancellationToken cancellationToken = default);

        ValueTask<T> Deserialize(PipeReader pipe, T instance, JsonReaderState state = default, CancellationToken cancellationToken = default);
    }
}
