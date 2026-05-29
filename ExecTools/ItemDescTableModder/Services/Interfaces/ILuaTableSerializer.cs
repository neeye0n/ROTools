using MoonSharp.Interpreter;

namespace ItemDescTableModder.Services.Interfaces
{
    public interface ILuaTableSerializer
    {
        string GenerateTableContent(Table table);
        string SerializeTable(Table table);
        string SerializeValue(DynValue value);
        string EscapeString(string input);
    }
}
