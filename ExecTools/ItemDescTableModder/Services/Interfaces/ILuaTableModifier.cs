using ItemDescTableModder.Models;
using MoonSharp.Interpreter;

namespace ItemDescTableModder.Services.Interfaces
{
    public interface ILuaTableModifier
    {
        /// <summary>
        /// Modifies a specific item in the Lua table
        /// </summary>
        /// <param name="model">The Lua table model</param>
        /// <param name="itemId">The ID of the item to modify</param>
        /// <param name="modifyAction">The action to perform on the item</param>
        void ModifyItem(LuaTableModel model, int itemId, Action<Table> modifyAction);

        /// <summary>
        /// Gets all item IDs from the Lua table
        /// </summary>
        /// <param name="model">The Lua table model</param>
        /// <returns>An enumerable of item IDs</returns>
        IEnumerable<int> GetItemIds(LuaTableModel model);
    }
}
