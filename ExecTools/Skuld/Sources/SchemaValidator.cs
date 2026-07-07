using Json.Schema;
using System.Reflection;

namespace Skuld.Sources
{
    public static class SchemaValidator
    {
        private static readonly JsonSchema Schema = LoadSchema();

        public static void Validate(string rawJson, string sourceLabel)
        {
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(rawJson);
            var result = Schema.Evaluate(jsonDoc.RootElement, new EvaluationOptions
            {
                OutputFormat = OutputFormat.List
            });

            if (result.IsValid) return;

            var details = result.Details ?? [];

            var messages = details
                .Where(d => !d.IsValid)
                .Select(d => d.Errors)
                .Where(errors => errors is { Count: > 0 })
                .SelectMany(errors => errors!.Values)
                .Distinct()
                .Take(5)
                .ToList();

            var messageText = messages.Count > 0
                ? string.Join("; ", messages)
                : "Schema validation failed (no detailed error messages available).";

            throw new InvalidDataException($"[{sourceLabel}] Schema validation failed: {messageText}");
        }

        private static JsonSchema LoadSchema()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .First(n => n.EndsWith("materialTable.schema.json"));

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            return JsonSchema.FromText(reader.ReadToEnd());
        }
    }
}