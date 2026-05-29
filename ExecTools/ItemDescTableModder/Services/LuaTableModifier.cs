using ItemDescTableModder.Models;
using ItemDescTableModder.Services.Interfaces;
using MoonSharp.Interpreter;

namespace ItemDescTableModder.Services
{
    public class LuaTableModifier : ILuaTableModifier
    {
        public void ModifyItem(LuaTableModel model, int itemId, Action<Table> modifyAction)
        {
            ArgumentNullException.ThrowIfNull(model);

            DynValue item = model.TableData.Get(itemId);
            if (item.IsNotNil() && item.Type == DataType.Table)
            {
                modifyAction(item.Table);
            }
        }

        public IEnumerable<int> GetItemIds(LuaTableModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            foreach (var item in model.TableData.Pairs)
            {
                if (item.Key.Type == DataType.Number)
                {
                    yield return (int)item.Key.Number;
                }
            }
        }
    }
}
