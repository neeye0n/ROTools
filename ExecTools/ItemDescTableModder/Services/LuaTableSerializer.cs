using ItemDescTableModder.Services.Interfaces;
using MoonSharp.Interpreter;
using System.Text;

namespace ItemDescTableModder.Services
{
    public class LuaTableSerializer : LuaServiceBase, ILuaTableSerializer
    {
        public LuaTableSerializer() : base()
        {
        }

        public string GenerateTableContent(Table table)
        {
            var sb = new StringBuilder();
            int count = 0;
            int limit = table.Pairs.Count() - 1;
            foreach (TablePair pair in table.Pairs)
            {
                // Format new items
                string delimiter = count < limit ? "," : "";
                sb.AppendLine($"\t[{pair.Key.ToString()}] = {SerializeTable(pair.Value.Table)}{delimiter}");
                count++;
            }

            return sb.ToString();
        }

        public string SerializeTable(Table table)
        {
            var sb = new StringBuilder();
            int count = 0;
            int limit = table.Pairs.Count() - 1;
            sb.AppendLine("{");

            foreach (TablePair pair in table.Pairs)
            {
                string delimiter = count < limit ? "," : "";
                if (pair.Key.Type != DataType.Number)
                {
                    sb.AppendLine($"\t\t{pair.Key.ToPrintString()} = {SerializeValue(pair.Value)}{delimiter}");
                }
                else
                {
                    sb.AppendLine($"\t\t{SerializeValue(pair.Value)}{delimiter}");
                }
                count++;
            }

            sb.Append("\t}");
            return sb.ToString();
        }

        public string SerializeValue(DynValue value)
        {
            return value.Type switch
            {
                DataType.String => $"\"{EscapeString(value.String)}\"",
                DataType.Number => value.Number.ToString(),
                DataType.Boolean => value.Boolean ? "true" : "false",
                DataType.Table => SerializeTable(value.Table),
                _ => "nil",
            };
        }

        public string EscapeString(string input)
        {
            // Convert to encoding bytes first
            byte[] bytes = Encoding.GetBytes(input);
            string encoded = Encoding.GetString(bytes);

            // Escape special characters
            return encoded
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r");
        }
    }
}
