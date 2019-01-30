using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Clay.Json.Serializers
{
    public sealed class StringJsonSerializer : JsonSerializerBase<string>
    {
        public override async ValueTask<string> Deserialize(PipeReader pipe, string instance, JsonReaderState state = default, CancellationToken cancellationToken = default)
        {
            await ReadNextTokenAsync(pipe, state, cancellationToken);
            return ReadString(pipe, state);
        }

        public override ValueTask Serialize(PipeWriter pipe, string instance, JsonWriterState state = default, CancellationToken cancellationToken = default)
        {
            var writer = new Utf8JsonWriter(pipe, state);
            writer.WriteStringValue(instance, true);
            return default;
        }
    }
}
