using Skuld.Models;

namespace Skuld.Sources
{
    public static class MaterialTableValidator
    {
        public static void Validate(MaterialTableFile table, string sourceLabel)
        {
            ValidateDisplay(table.Display, sourceLabel);

            var seenIds = new HashSet<int>();
            foreach (var entry in table.Entries)
            {
                if (!seenIds.Add(entry.EntryId))
                    throw new InvalidDataException($"[{sourceLabel}] Duplicate entryId {entry.EntryId} ('{entry.Label}').");

                foreach (var mat in entry.Materials)
                {
                    if (mat.Qty <= 0)
                        throw new InvalidDataException($"[{sourceLabel}] '{entry.Label}' has non-positive qty for matId {mat.MatId}.");

                    if (mat.MatId == entry.EntryId)
                        throw new InvalidDataException($"[{sourceLabel}] '{entry.Label}' self-references as its own material.");
                }
            }
        }

        private static void ValidateDisplay(CategoryDisplayConfig display, string sourceLabel)
        {
            if (!IsValidHexColor(display.DescriptionHeaderColor))
                throw new InvalidDataException($"[{sourceLabel}] display.descriptionHeaderColor is not a valid 6-digit hex color.");

            if (!IsValidHexColor(display.DetailedDescriptionColor))
                throw new InvalidDataException($"[{sourceLabel}] display.detailedDescriptionColor is not a valid 6-digit hex color.");
        }

        private static bool IsValidHexColor(string? color) =>
            !string.IsNullOrWhiteSpace(color) &&
            color.Length == 6 &&
            color.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}