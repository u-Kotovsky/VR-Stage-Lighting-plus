#if UNITY_EDITOR && !COMPILER_UDONSHARP
using System.IO;
using System.Text;

namespace VRSL.EditorScripts
{
    public class StringWriterUtf8 : StringWriter
    {
        public StringWriterUtf8(StringBuilder sb) : base(sb)
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
#endif