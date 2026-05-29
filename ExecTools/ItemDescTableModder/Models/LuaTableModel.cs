using MoonSharp.Interpreter;

namespace ItemDescTableModder.Models
{
    public class LuaTableModel
    {
        public string OriginalContent { get; set; }
        public Table TableData { get; set; }
        public string TableIdentifier { get; set; }

        public LuaTableModel(string originalContent, Table tableData, string tableIdentifier)
        {
            OriginalContent = originalContent;
            TableData = tableData;
            TableIdentifier = tableIdentifier;
        }

    }
}
