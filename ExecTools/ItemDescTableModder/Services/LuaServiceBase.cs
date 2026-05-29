using System.Text;

namespace ItemDescTableModder.Services
{
    public abstract class LuaServiceBase
    {
        protected readonly Encoding Encoding;

        protected LuaServiceBase()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding = Encoding.GetEncoding(1252) ?? throw new Exception("Windows-1252 encoding not available");
        }
    }
}
