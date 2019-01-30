using SourceCode.Clay.Json.Serializers;
using System;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SourceCode.Clay.Json.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var ser = new StringJsonSerializer();
            var pipe = new Pipe();

            var deserialize = ser.Deserialize(pipe.Reader, null);
            await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes("\"hello "));
            await pipe.Writer.FlushAsync();
            await Task.Yield();
            await pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes("world\""));
            await pipe.Writer.FlushAsync();
            pipe.Writer.Complete();

            var actual = await deserialize;
            Assert.Equal("hello world", actual);
        }
    }
}
