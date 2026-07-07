using Skuld.Config;
using Skuld.Models;

namespace Skuld.Annotations
{
    public static class AnnotationBuilder
    {
        public static Dictionary<int, List<Usage>> Build(
            IEnumerable<(CategorySource Source, MaterialTableFile Table)> loaded)
        {
            var annotations = new Dictionary<int, List<Usage>>();

            foreach (var (_, table) in loaded)
            {
                foreach (var entry in table.Entries)
                {
                    foreach (var mat in entry.Materials)
                    {
                        if (!annotations.TryGetValue(mat.MatId, out var list))
                            annotations[mat.MatId] = list = [];

                        string label;

                        if (table.Display.IsSuffix && table.Display.EnableDetailedDescriptions)
                        {
                            label = entry.AltLabel ?? string.Empty;
                        }
                        else if (!string.IsNullOrWhiteSpace(entry.AltLabel) && entry.AltLabel.Length < entry.Label.Length)
                        {
                            label = entry.AltLabel;
                        }
                        else
                        {
                            label = entry.Label;
                        }

                        list.Add(new Usage(table.Display, label, mat.Qty));
                    }
                }
            }

            return annotations;
        }
    }
}