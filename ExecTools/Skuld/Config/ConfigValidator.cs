namespace Skuld.Config
{
    public static class ConfigValidator
    {
        public static (List<CategorySource> Valid, List<(CategorySource Source, List<string> Errors)> Dropped)
            Validate(GeneratorConfig config)
        {
            var valid = new List<CategorySource>();
            var dropped = new List<(CategorySource, List<string>)>();

            foreach (var src in config.Sources ?? [])
            {
                var errors = ValidateOne(src);
                if (errors.Count == 0) valid.Add(src);
                else dropped.Add((src, errors));
            }

            return (valid, dropped);
        }

        private static List<string> ValidateOne(CategorySource src)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(src.Path))
                errors.Add("missing 'path'");
            else if (src.SourceType == SourceType.ResourceUrl && !Uri.TryCreate(src.Path, UriKind.Absolute, out _))
                errors.Add("RemoteUrl 'path' is not a valid absolute URL");

            return errors;
        }
    }
}