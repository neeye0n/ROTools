using Spectre.Console;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Skuld
{
    internal class Program
    {
        const string ColorReset = "^000000";
        const string ZipInternalPath = "System/LuaFiles514/";
        const string EmbResouceFile = "itemInfo_f.lua";
        const string AnnotationsFile = "itemAnnotations.lua";

        static async Task Main()
        {
            // ── Variable Setting ─────────────────────────────────────
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            var assembly = typeof(Program).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith(EmbResouceFile, StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                AnsiConsole.MarkupLine("❌ [red]Warning: Embedded resource file not found. Terminating...[/]");
                Thread.Sleep(5000);
                return;
            }

            AnsiConsole.MarkupLine("[bold blue]SKULD[/] is writing... ✏️");

            // ── Load generatorConfig.json ─────────────────────────────────────
            string configPath = Path.Combine(AppContext.BaseDirectory, "generatorConfig.json");
            if (!File.Exists(configPath))
            {
                var defaultConfig = new GeneratorConfig();
                await File.WriteAllTextAsync(configPath, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
                AnsiConsole.MarkupLine($"⚙️ [yellow]Created default generatorConfig.json at:[/]\n  {configPath}");
                AnsiConsole.MarkupLine("⚙️ [yellow]Press any key to close the app, then re-run it.[/]");
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                return;
            }

            GeneratorConfig config;
            try
            {
                var configJson = await File.ReadAllTextAsync(configPath);
                config = JsonSerializer.Deserialize<GeneratorConfig>(configJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"❌ [red]Error reading generatorConfig.json:[/]\n  {ex.Message}");
                return;
            }

#if DEBUG
            AnsiConsole.MarkupLine($"📜 [cyan]Resource URL:[/]\n  [grey]{config.ResourceUrl}[/]");
            AnsiConsole.WriteLine();
#endif

            // ── Build CategoryConfig list from JSON config ─────────────────────
            var Categories = new List<CategoryConfig>
            {
                CategoryConfig.From("brewingMatsTable.json",  config.BrewingConfig,  isSuffix: false),
                CategoryConfig.From("cookingMatsTable.json",  config.CookingConfig,  isSuffix: false),
                CategoryConfig.From("questMatsTable.json",    config.QuestConfig,    isSuffix: false),
                CategoryConfig.From("instanceMatsTable.json", config.InstanceConfig, isSuffix: true),
                CategoryConfig.From("petEvoMatsTable.json",   config.PetEvoConfig,   isSuffix: false),
            };

            using var http = new HttpClient();

            // ── Fetch and arrange resource data ───────────────────────────────
            var annotations = new Dictionary<int, List<Usage>>();
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("🔃 Fetching and Arranging Data...", async ctx =>
            {
                foreach (var cat in Categories)
                {
                    ctx.Status($"🔍 Reading {cat.File}...");
                    await Task.Delay(500);
                    try
                    {
                        var json = await http.GetStringAsync(config.ResourceUrl + cat.File);
                        var table = JsonSerializer.Deserialize<Dictionary<string, JsonElement[]>>(json)!;

                        int count = 0;
                        foreach (var (productName, materials) in table)
                        {
                            foreach (var mat in materials)
                            {
                                if (!mat.TryGetProperty("matId", out var matIdEl)) continue;
                                int matId = matIdEl.GetInt32();
                                int qty = mat.TryGetProperty("qty", out var qtyEl) ? qtyEl.GetInt32() : 1;

                                if (!annotations.ContainsKey(matId))
                                    annotations[matId] = [];

                                annotations[matId].Add(new Usage(cat, productName, qty));
                                count++;
                            }
                        }

#if DEBUG
                        ctx.Status($"✔️ Loaded {table.Count}, {count} material entries.");
#endif
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"❌ [red]Error loading {cat.File}:[/]\n  {ex.Message}");
                    }
                    await Task.Delay(500);
                }
            });



            // ── Build itemAnnotations.lua content ─────────────────────────────
            AnsiConsole.MarkupLine("\n🔨 [bold blue]Generating itemAnnotations.lua[/]...");
            var sb = new StringBuilder();

            sb.AppendLine("-- Processed By ItemDescTableModder.");
            sb.AppendLine();
            sb.AppendLine("itemAnnotations = {");

            foreach (var (matId, usages) in annotations)
            {
                // ── Name tags ─────────────────────────────────────────────────
                var prefixTags = new List<string>();
                var suffixParts = new List<string>();

                foreach (var u in usages)
                {
                    if (!u.Category.EnableTags) continue;

                    if (u.Category.IsSuffix)
                        suffixParts.Add($"{u.ProductName}-{u.Qty}");
                    else if (!prefixTags.Contains(u.Category.Tag))
                        prefixTags.Add(u.Category.Tag);
                }

                string nameTagPrefix = string.Join("", prefixTags);
                string nameTagSuffix = suffixParts.Count > 0
                    ? "(" + string.Join(", ", suffixParts) + ")"
                    : "";

                sb.AppendLine($"    [{matId}] = {{");
                sb.AppendLine($"        nameTagPrefix = \"{nameTagPrefix}\",");
                sb.AppendLine($"        nameTagSuffix = \"{nameTagSuffix}\",");
                sb.AppendLine($"        descLines = {{");

                // ── Desc lines ────────────────────────────────────────────────
                var byCategory = new Dictionary<string, (CategoryConfig Cat, List<Usage> Usages)>();
                foreach (var u in usages)
                {
                    if (!byCategory.ContainsKey(u.Category.DescriptionHeader))
                        byCategory[u.Category.DescriptionHeader] = (u.Category, new List<Usage>());
                    byCategory[u.Category.DescriptionHeader].Usages.Add(u);
                }

                foreach (var (label, entry) in byCategory)
                {
                    string hColor = $"^{entry.Cat.DescriptionHeaderColor}";
                    string rColor = $"^{entry.Cat.DetailedDescriptionColor}";

                    if (entry.Cat.EnableDescription)
                        sb.AppendLine($"            \"{hColor}[{label}]{ColorReset}\",");

                    if (entry.Cat.EnableDetailedDescriptions)
                        foreach (var u in entry.Usages)
                            sb.AppendLine($"            \"{rColor}{u.ProductName} - {u.Qty} ea.{ColorReset}\",");
                }

                sb.AppendLine($"        }},");
                sb.AppendLine($"    }},");
            }

            sb.AppendLine("}");

            // ── Register Windows-1252 encoding ────────────────────────────────
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var win1252 = Encoding.GetEncoding(1252);

            // ── Build System.zip in memory ────────────────────────────────────
            AnsiConsole.MarkupLine("📦 [blue bold]Building System.zip...[/]");
            using var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                // itemAnnotations.lua — generated content
                var annotationsEntry = zip.CreateEntry(ZipInternalPath + AnnotationsFile);
                using (var annotationEntryStream = annotationsEntry.Open())
                {
                    var generatedContentBytes = win1252.GetBytes(sb.ToString());
                    annotationEntryStream.Write(generatedContentBytes, 0, generatedContentBytes.Length);
                }

                // Embedded resource
                using var resourceStream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(resourceStream, win1252);
                string resourceContent = await reader.ReadToEndAsync();

                var itemInfoEntry = zip.CreateEntry(ZipInternalPath + "itemInfo_f.lua");
                using var itemInfoEntryStream = itemInfoEntry.Open();
                var embeddedResourceBtyee = win1252.GetBytes(resourceContent);
                itemInfoEntryStream.Write(embeddedResourceBtyee, 0, embeddedResourceBtyee.Length);

            }

            // ── Write System.zip ──────────────────────────────────────────────
            string zipPath = Path.Combine(AppContext.BaseDirectory, "System.zip");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            await File.WriteAllBytesAsync(zipPath, zipStream.ToArray());
            AnsiConsole.MarkupLine($"✅ [cyan]Done! Written to:[/]\n  [grey]{zipPath}[/]");
            AnsiConsole.MarkupLine($"✅ [cyan]Total items annotated:[/] [yellow bold]{annotations.Count}[/]");
            AnsiConsole.MarkupLine("[green]Press any key to exit...[/] ✎");
            if (!Console.IsInputRedirected)
            {
                Console.ReadKey();
            }
        }
    }

    record CategoryConfig(
        string File,
        bool EnableTags,
        string Tag,
        bool EnableDescription,
        string DescriptionHeader,
        string DescriptionHeaderColor,
        bool EnableDetailedDescriptions,
        string DetailedDescriptionColor,
        bool IsSuffix = false)
    {
        public static CategoryConfig From(string file, CategoryJsonConfig c, bool isSuffix) =>
            new(file, c.EnableTags == 1, $"[{c.TagText}]", c.EnableDescriptions == 1,
                c.HeaderText, c.DescriptionHeaderColor,
                c.EnableDetailedDescriptions == 1, c.DetailedDescriptionColor,
                isSuffix);
    }

    record Usage(CategoryConfig Category, string ProductName, int Qty);
}
