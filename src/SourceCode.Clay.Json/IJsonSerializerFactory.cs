using System.IO;
using System.Threading.Tasks;

namespace SourceCode.Clay.Json
{
    public interface IJsonSerializerFactory
    {
        IJsonSerializer<T> CreateSerializer<T>();
    }
}
