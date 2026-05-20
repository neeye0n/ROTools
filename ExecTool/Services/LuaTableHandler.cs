using ItemDescTableModder.Models;
using ItemDescTableModder.Services.Interfaces;
using MoonSharp.Interpreter;
using System.Text;
using System.Text.RegularExpressions;

namespace ItemDescTableModder.Services
{
    public class LuaTableHandler(ILuaTableSerializer serializer) : LuaServiceBase(), ILuaTableHandler
    {
        private readonly string HandlerSignature = "-- Processed By ItemDescTableModder";
        private readonly ILuaTableSerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

        public LuaTableModel? LoadFile(string filePath, string tableIdentifier = "tbl")
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            bool isProcessed = File.ReadAllLines(filePath)[0].Equals(HandlerSignature);
            if (isProcessed)
                return null;

            // Read with proper encoding
            string content = File.ReadAllText(filePath, Encoding);

            // Extract just the table portion
            var script = new Script();
            script.DoString(content);
            var tableData = script.Globals.Get(tableIdentifier).Table;

            return new LuaTableModel(content, tableData, tableIdentifier);
        }

        public LuaTableModel? LoadFile(string filePath)
        {
            var tableIdentifier = string.Empty;
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            bool isProcessed = File.ReadAllLines(filePath)[0].Equals(HandlerSignature);
            if (isProcessed)
                return null;

            // Read with proper encoding
            string content = File.ReadAllText(filePath, Encoding);
            var script = new Script();
            script.DoString(content);

            // Try to detect table identifier
            foreach (var pair in script.Globals.Pairs.Where(k => k.Key.String.Contains("tbl") && k.Value.Table.Keys.Any()))
            {
                if (pair.Value.Type == DataType.Table)
                {
                    tableIdentifier = pair.Key.String;
                    break;
                }
            }

            if (string.IsNullOrEmpty(tableIdentifier))
                throw new InvalidOperationException("No item table found in Lua file.");

            // Extract just the table portion
            var tableData = script.Globals.Get(tableIdentifier).Table;
            return new LuaTableModel(content, tableData, tableIdentifier);
        }

        public void SaveToFile(LuaTableModel model, string filePath)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (File.Exists(filePath))
                File.Delete(filePath);

            // Reconstruct the file content
            string newContent = RebuildLuaFile(model);

            // Save with original encoding
            File.WriteAllText(filePath, newContent, Encoding);
        }

        private string RebuildLuaFile(LuaTableModel model)
        {
            // 1. Preserve everything before the table
            StringBuilder preTableContent = new();
            // Simple Signing
            preTableContent.AppendLine(HandlerSignature);
            var preTableMatch = Regex.Match(model.OriginalContent, @"^(.*?)\b" + model.TableIdentifier + @"\s*=\s*\{", RegexOptions.Singleline);
            preTableContent.Append(preTableMatch.Success ? preTableMatch.Groups[1].Value : string.Empty);

            // 2. Generate the new table content
            string tableContent = _serializer.GenerateTableContent(model.TableData);

            // 3. Preserve everything after the table
            var postTableMatch = Regex.Match(model.OriginalContent, @"\}([^\}]*)$", RegexOptions.Singleline);
            string postTableContent = postTableMatch.Success ? postTableMatch.Groups[1].Value : string.Empty;

            return $"{preTableContent}{model.TableIdentifier} = {{\n{tableContent}\n}}{postTableContent}";
        }
    }
}
