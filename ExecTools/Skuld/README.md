# 🎨 SKULD - Item Annotation Creation Tool

**SKULD** is a tool that generates annotated item information for **Ragnarok Online (Classic)** clients. It automatically creates color-coded item descriptions, tags and detailed usage information.

## ⚠️ Requirements

Your RO client must be compiled/configured with [MultiItem support](https://llchrisll.github.io/ROTPDocs/guides/multiitem/) to read and display these annotations in-game.

Without MultiItem support, the generated annotations will have no effect.

[Learn more about MultiItem Setup](https://llchrisll.github.io/ROTPDocs/guides/multiitem/)

## 🚀 Quick Start

### Option 1: Simple (Pre-Generated)

1. **Download** `System.zip` from the [latest release](https://github.com/neeye0n/ROTools/releases)
   - Already generated with default settings
2. **Install**:
   - Extract `System.zip` into your Ragnarok Online main directory
   - ⚠️ Back up your `System` folder first!

### Option 2: Custom Configuration

1. **Download** from the [latest release](https://github.com/neeye0n/ROTools/releases):
   - `Skuld.exe`
   - `skuldConf.json`
2. **Customize** `skuldConf.json`:
   - Enable/disable tags and descriptions per category
   - Adjust colors and text labels
   - See Configuration section below
3. **Run** `Skuld.exe`
   - Generates `System.zip` with your custom settings
4. **Install**:
   - Extract `System.zip` into your Ragnarok Online main directory
   - ⚠️ Back up your `System` folder first!

## ⚡ Features

✨ **Automatic Data Processing**
- Fetches material table data from remote JSON sources
- Processes brewing, cooking, crafting, instances, pet evolution, and repeatable quests

✨ **Rich Item Annotations**
- Color-coded category tags (`[Brew]`, `[Cook]`, `[Craft]`, `[Inst]`, `[Pet]`, `[Exp]`)
- Custom item descriptions with flexible formatting
- Customizable colors and text per category

## ⚙️ Configuration

Edit `skuldConf.json` to customize categories and appearance.

### Category Configuration

Each category has these settings:

```json
{
  "tagText": "Brew",                          // Tag shown in brackets [Brew]
  "headerText": "Brewing Material",           // Description header text, shown in brackets [Brewing Material]
  "descriptionHeaderColor": "00897B",         // Hex color for header
  "detailedDescriptionColor": "43A047",       // Hex color for details
  "enableTags": 1,                            // Show category tag (1=yes, 0=no)
  "enableDescriptions": 1,                    // Show header (1=yes, 0=no)
  "enableDetailedDescriptions": 0             // Show detailed list (1=yes, 0=no)
}
```

### Categories and Default Settings

| Tag | Name | Purpose |
|-----|------|---------|
| `[Brew]` | brewingConfig | Alchemy (ie. Brewing) Related |
| `[Cook]` | cookingConfig | Cooking Materials |
| `[Craft]` | questConfig | Crafting items |
| `[Inst]` | instanceConfig | Instance Access |
| `[Pet]` | petEvoConfig | Pet evolution |
| `[Exp]` | expQuestConfig | Repeatable exp quests |

### Example: Hide All Tags

```json
{
  "brewingConfig": {
    "enableTags": 0,
    // ... rest of config
  },
  "cookingConfig": {
    "enableTags": 0,
    // ... rest of config
  }
  // ... disable for other categories too
}
```

### Example: Show Detailed Descriptions

```json
{
  "questConfig": {
    "enableDetailedDescriptions": 1,  // Show list of products using this item
    // ... rest of config
  }
}
```

### Example: Custom Colors

```json
{
  "brewingConfig": {
    "descriptionHeaderColor": "FF0000",  // Change to red
    "detailedDescriptionColor": "00FF00" // Change to green
    // ... rest of config
  }
}
```

## 📦 Output

Generates `System.zip` containing:
- `LuaFiles514/itemAnnotations.lua` (generated with your config)
- `LuaFiles514/itemInfo_f.lua` (modified itemInfo_f.lua script to make annotations work)

## 🔧 Troubleshooting

### "Config is malformed or missing required fields"
- Check `skuldConf.json` syntax (must be valid JSON)
- Ensure all hex colors are exactly 6 characters
- Copy from the default config if unsure

### Items not showing annotations in-game
- Verify `System.zip` was extracted to your RO main folder
- Restart your RO client after installation
- **Ensure your client has MultiItem support enabled** (see Requirements)

### Can't fetch data
- Verify internet connection

## ❤️ Credits

**SKULD** is part of the [ROTools](https://github.com/neeye0n/ROTools) collection

Built with ❤️ by [neeye0n](https://github.com/neeye0n)