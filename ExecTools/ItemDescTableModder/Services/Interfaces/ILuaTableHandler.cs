using ItemDescTableModder.Models;

namespace ItemDescTableModder.Services.Interfaces
{
    public interface ILuaTableHandler
    {
        /// <summary>
        /// Loads a Lua table from a file
        /// </summary>
        /// <param name="filePath">Path to the Lua file</param>
        /// <param name="tableIdentifier">The name of the table in the Lua file</param>
        /// <returns>The loaded Lua table model</returns>
        LuaTableModel? LoadFile(string filePath, string tableIdentifier = "tbl");

        LuaTableModel? LoadFile(string filePath);

        /// <summary>
        /// Saves a Lua table model to a file
        /// </summary>
        /// <param name="model">The model to save</param>
        /// <param name="filePath">The path to save to</param>
        void SaveToFile(LuaTableModel model, string filePath);
    }
}
