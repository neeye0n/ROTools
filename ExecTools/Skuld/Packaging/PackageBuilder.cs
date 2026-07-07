using System.IO.Compression;
using System.Text;

namespace Skuld.Packaging
{
    public static class PackageBuilder
    {
        private const string ZipInternalPath = "System/LuaFiles514/";
        private const string AnnotationsFile = "itemAnnotations.lua";
        private const string ItemInfoScriptFile = "itemInfo_f.lua";

        public static async Task<byte[]> BuildAsync(string annotationsLua, Stream embeddedResourceStream, Encoding win1252)
        {
            using var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                await WriteEntryAsync(zip, ZipInternalPath + AnnotationsFile, win1252.GetBytes(annotationsLua));

                using var reader = new StreamReader(embeddedResourceStream, win1252);
                var resourceContent = await reader.ReadToEndAsync();
                await WriteEntryAsync(zip, ZipInternalPath + ItemInfoScriptFile, win1252.GetBytes(resourceContent));
            }
            return zipStream.ToArray();
        }

        private static async Task WriteEntryAsync(ZipArchive zip, string path, byte[] bytes)
        {
            var entry = zip.CreateEntry(path);
            await using var stream = entry.Open();
            await stream.WriteAsync(bytes);
        }
    }
}