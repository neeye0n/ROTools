using System.Text;

namespace Skuld.Annotations
{
    public static class LuaWriter
    {
        private const string ColorReset = "^000000";

        public static string Build(Dictionary<int, List<Usage>> annotations)
        {
            var sb = new StringBuilder();
            sb.AppendLine("-- Created by SKULD.").AppendLine();
            sb.AppendLine("itemAnnotations = {");

            foreach (var (matId, usages) in annotations)
                AppendEntry(sb, matId, usages);

            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendEntry(StringBuilder sb, int matId, List<Usage> usages)
        {
            var prefixTags = new List<string>();
            var suffixParts = new List<string>();

            foreach (var u in usages)
            {
                if (!u.Display.EnableTags) continue;

                if (u.Display.IsSuffix && u.Display.EnableDetailedDescriptions)
                    suffixParts.Add($"{u.Label}-{u.Qty}");
                else if (!prefixTags.Contains($"[{u.Display.TagText}]"))
                    prefixTags.Add($"[{u.Display.TagText}]");
            }

            string nameTagPrefix = string.Join("", prefixTags);
            string nameTagSuffix = suffixParts.Count > 0 ? $"({string.Join(", ", suffixParts)})" : "";

            sb.AppendLine($"    [{matId}] = {{");
            sb.AppendLine($"        nameTagPrefix = \"{nameTagPrefix}\",");
            sb.AppendLine($"        nameTagSuffix = \"{nameTagSuffix}\",");
            sb.AppendLine("        descLines = {");

            foreach (var group in usages.GroupBy(u => u.Display.HeaderText))
            {
                var display = group.First().Display;

                if (display.EnableDescriptions)
                    sb.AppendLine($"            \"^{display.DescriptionHeaderColor}[{group.Key}]{ColorReset}\",");

                if (display.EnableDetailedDescriptions)
                    foreach (var u in group)
                        sb.AppendLine($"            \"^{display.DetailedDescriptionColor}{u.Label} - {u.Qty} ea.{ColorReset}\",");
            }

            sb.AppendLine("        },").AppendLine("    },");
        }
    }
}