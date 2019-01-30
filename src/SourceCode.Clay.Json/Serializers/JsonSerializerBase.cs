using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SourceCode.Clay.Json.Serializers
{
    public abstract class JsonSerializerBase<T> : IJsonSerializer<T>
    {
        protected const JsonTokenType EndOfDocument = (JsonTokenType)100;

        private delegate TValue Reader<TValue>(Utf8JsonReader reader);

        protected async ValueTask<JsonTokenType> ReadNextTokenAsync(PipeReader pipe, JsonReaderState state, CancellationToken cancellationToken)
        {
            bool ForwardException(PipeReader pipe, Exception e)
            {
                pipe.Complete(e);
                return false;
            }

            if (pipe == null) throw new ArgumentNullException(nameof(pipe));

            try
            {
                while (true)
                {
                    if (!pipe.TryRead(out ReadResult readResult))
                        readResult = await pipe.ReadAsync(cancellationToken);
                    if (readResult.IsCanceled)
                        throw new OperationCanceledException();

                    JsonTokenType IsReaderReady(ReadResult readResult, JsonReaderState state)
                    {
                        var reader = new Utf8JsonReader(readResult.Buffer, readResult.IsCompleted, state);
                        var readerReady = reader.Read();
                        return readerReady ? reader.TokenType : JsonTokenType.None;
                    }

                    var token = IsReaderReady(readResult, state);
                    if (token == JsonTokenType.None && readResult.IsCompleted)
                    {
                        pipe.Complete();
                        return EndOfDocument;
                    }

                    pipe.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
                    if (token != JsonTokenType.None) return token;
                }
            }
            catch (Exception e) when (ForwardException(pipe, e)) { throw; }
        }

        private TValue Read<TValue>(PipeReader pipe, JsonReaderState state, Reader<TValue> reader)
        {
            if (pipe == null) throw new ArgumentNullException(nameof(pipe));
            if (!pipe.TryRead(out ReadResult readResult))
                throw new InvalidOperationException("ReadNextTokenAsync must be called before Read.");

            var jsonReader = new Utf8JsonReader(readResult.Buffer, readResult.IsCompleted, state);
            if (!jsonReader.Read())
                throw new InvalidOperationException("ReadNextTokenAsync must be called before Read.");

            var result = reader(jsonReader);
            pipe.AdvanceTo(jsonReader.Position);
            return result;
        }

        private static readonly Reader<bool> BooleanReader = r => r.GetBoolean();
        public bool ReadBoolean(PipeReader pipe, JsonReaderState state) => Read(pipe, state, BooleanReader);

        private static readonly Reader<decimal> DecimalReader = r => r.GetDecimal();
        public decimal ReadDecimal(PipeReader pipe, JsonReaderState state) => Read(pipe, state, DecimalReader);

        private static readonly Reader<double> DoubleReader = r => r.GetDouble();
        public double ReadDouble(PipeReader pipe, JsonReaderState state) => Read(pipe, state, DoubleReader);

        private static readonly Reader<int> Int32Reader = r => r.GetInt32();
        public int ReadInt32(PipeReader pipe, JsonReaderState state) => Read(pipe, state, Int32Reader);

        private static readonly Reader<long> Int64Reader = r => r.GetInt64();
        public long ReadInt64(PipeReader pipe, JsonReaderState state) => Read(pipe, state, Int64Reader);

        private static readonly Reader<float> SingleReader = r => r.GetSingle();
        public float ReadSingle(PipeReader pipe, JsonReaderState state) => Read(pipe, state, SingleReader);

        private static readonly Reader<string> StringReader = r => r.GetString();
        public string ReadString(PipeReader pipe, JsonReaderState state) => Read(pipe, state, StringReader);

        private static readonly Reader<uint> UInt32Reader = r => r.GetUInt32();
        public uint ReadUInt32(PipeReader pipe, JsonReaderState state) => Read(pipe, state, UInt32Reader);

        private static readonly Reader<ulong> UInt64Reader = r => r.GetUInt64();
        public ulong ReadUInt64(PipeReader pipe, JsonReaderState state) => Read(pipe, state, UInt64Reader);

        private static readonly Reader<(bool, decimal)> DecimalOptional = r => (r.TryGetDecimal(out var v), v);
        public bool TryGetDecimal(out decimal value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, DecimalOptional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, double)> DoubleOptional = r => (r.TryGetDouble(out var v), v);
        public bool TryReadDouble(out double value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, DoubleOptional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, int)> Int32Optional = r => (r.TryGetInt32(out var v), v);
        public bool TryReadInt32(out int value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, Int32Optional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, long)> Int64Optional = r => (r.TryGetInt64(out var v), v);
        public bool TryReadInt64(out long value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, Int64Optional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, float)> SingleOptional = r => (r.TryGetSingle(out var v), v);
        public bool TryReadSingle(out float value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, SingleOptional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, uint)> UInt32Optional = r => (r.TryGetUInt32(out var v), v);
        public bool TryReadUInt32(out uint value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, UInt32Optional);
            value = result.Item2;
            return result.Item1;
        }

        private static readonly Reader<(bool, ulong)> UInt64Optional = r => (r.TryGetUInt64(out var v), v);
        public bool TryReadUInt64(out ulong value, PipeReader pipe, JsonReaderState state)
        {
            var result = Read(pipe, state, UInt64Optional);
            value = result.Item2;
            return result.Item1;
        }

        public abstract ValueTask<T> Deserialize(PipeReader pipe, T instance, JsonReaderState state = default, CancellationToken cancellationToken = default);

        public abstract ValueTask Serialize(PipeWriter pipe, T instance, JsonWriterState state = default, CancellationToken cancellationToken = default);
    }
}
