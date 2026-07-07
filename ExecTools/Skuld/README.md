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
   - Add, remove, or point to the material sources you want processed
   - See Configuration section below
3. **Run** `Skuld.exe`
   - Generates `System.zip` with your custom settings
4. **Install**:
   - Extract `System.zip` into your Ragnarok Online main directory
   - ⚠️ Back up your `System` folder first!

## ⚡ Features

✨ **Automatic Data Processing**
- Fetches material table data from remote or local JSON sources
- Processes brewing, cooking, crafting, instances, pet evolution, and repeatable quests out of the box

✨ **Rich Item Annotations**
- Color-coded category tags (`[Brew]`, `[Cook]`, `[Craft]`, `[Inst]`, `[Pet]`, `[Exp]`)
- Custom item descriptions with flexible formatting
- Customizable colors and text — now defined per source file, not just per built-in category

✨ **Bring Your Own (BYO) Sources**
- Add your own material tables, whether hosted online or sitting on your own PC
- Full control over tags, colors, and descriptions for anything you add

## ⚙️ Configuration

`skuldConf.json` tells SKULD **where to look** for material data. All the display settings (tags, colors, headers) live *inside each source JSON file itself*, so every source is fully self-contained.

### `skuldConf.json`

```json
{
  "sources": [
    {
      "sourceType": "ResourceUrl",
      "path": "https://neeye0n.github.io/flux/skuld/brewing.json"
    },
    {
      "sourceType": "ResourceUrl",
      "path": "https://neeye0n.github.io/flux/skuld/cooking.json"
    },
    {
      "sourceType": "LocalFile",
      "path": "C:/RO/MyCustomPotions.json"
    }
  ]
}
```
<sup><sup>Get the default json files [here](https://github.com/neeye0n/flux/tree/main/skuld).</sup></sup>

- **`sourceType`** — either:
  - `"ResourceUrl"` — fetches the JSON from a URL
  - `"LocalFile"` — reads the JSON from a file path on your computer
- **`path`** — the full URL (for `ResourceUrl`) or full file path (for `LocalFile`)

Add as many entries as you like — SKULD loads every valid source in the list and combines the results into one `System.zip`.

### Default Sources 

Out of the box, `skuldConf.json` points at SKULD's these hosted tables covering:

| Tag | Category | Purpose |
|-----|----------|---------|
| `[Brew]` | Brewing | Alchemy (i.e. Brewing) Related |
| `[Cook]` | Cooking | Cooking Materials |
| `[Craft]` | Quest Items | Crafting items |
| `[Inst]` | Instance | Instance Access |
| `[Pet]` | Pet Evo | Pet evolution |
| `[Exp]` | Exp Quest | Repeatable exp quests |

You don't need to touch anything to keep using these — they're only removed from `skuldConf.json` if you delete them yourself.

## 🧩 Bring Your Own (BYO) Sources

Want to add your own material tables — your server's custom items, a private crafting list, anything? SKULD supports it.

1. **Grab the template**: download `resourceTemplate.json` from the [latest release](https://github.com/neeye0n/ROTools/releases) <sub><sup>or [here](https://github.com/neeye0n/flux/tree/main/skuld).</sup></sub>
2. **Fill it in** with your own data (see format below)
3. **Add it to `skuldConf.json`**:
   - Hosting it online? Use `"sourceType": "ResourceUrl"` and point `path` at the URL
   - Keeping it local? Use `"sourceType": "LocalFile"` and point `path` at the file on your PC
4. **Run** `Skuld.exe` — your source gets processed right alongside the built-in ones

### `resourceTemplate.json`

```json
{
  "schemaVersion": 1,
  "sourceName": "Category Name",
  "description": "Human readable description",
  "display": {
    "enableTags": true,
    "tagText": "Tag Text",
    "isSuffix": false,
    "enableDescriptions": true,
    "headerText": "Header Text",
    "descriptionHeaderColor": "000000",
    "enableDetailedDescriptions": false,
    "detailedDescriptionColor": "000000"
  },
  "entries": [
    {
      "entryId": 1,
      "label": "Product",
      "alt-label": "Short Name",
      "materials": [
        { "matId": 101, "matName": "Material 1", "qty": 1 },
        { "matId": 102, "matName": "Material 2", "qty": 100 }
      ]
    }
  ]
}
```

**Field guide:**

| Field | Meaning |
|-------|---------|
| `schemaVersion` | Must be `1` — don't change this |
| `sourceName` | Short name for this source, shown in log messages (not shown in-game) |
| `description` | Free-text note for your own reference (not shown in-game) |
| `display.enableTags` | Show a tag like `[Brew]` next to the item name |
| `display.tagText` | The text inside the tag brackets |
| `display.isSuffix` | If `true`, shows a short name after the item name instead of a tag (e.g. instance drops). **Requires `enableDetailedDescriptions` to also be `true`** — see note below |
| `display.enableDescriptions` | Show a header line in the item description |
| `display.headerText` | The header text shown |
| `display.descriptionHeaderColor` | Hex color (6 characters, no `#`) for the header |
| `display.enableDetailedDescriptions` | Show a full list of every product/quantity |
| `display.detailedDescriptionColor` | Hex color for the detailed list |
| `entries[].entryId` | A unique number for this entry — use the real item ID if it's a craftable item |
| `entries[].label` | The full product or category label shown |
| `entries[].alt-label` | A shorter version of `label`, usually used when `label` is longer compared to common knowledge **OR** when `display.isSuffix` is `true` (**required** in that case — see note below) |
| `entries[].materials[]` | The list of materials needed, each with `matId`, `matName`, and `qty` |

⚠️ **`isSuffix` requirements:** If `display.isSuffix` is set to `true`:
- `display.enableDetailedDescriptions` **must also be `true`**. Suffix mode works by appending short names after the item's tag, and those short names are pulled from the detailed description list — so it can't function with detailed descriptions turned off.
- Every entry in the file **must have a non-empty `alt-label`**. This is the shorter name that actually gets shown as the suffix — `label` alone is not used in suffix mode.

If either of these is missed, the source will fail validation and be skipped entirely (see Troubleshooting below).

⚠️ **The schema matters.** SKULD checks every source file against this format before using it. If a required field is missing, misspelled, or the wrong type (e.g. text where a number is expected), that source is skipped and an error is logged — the rest of your sources still process normally. Double-check your file matches `resourceTemplate.json` closely, especially:
- Every required field is present and spelled exactly right (case-sensitive) — note it's `alt-label`, not `altLabel` or `alt_label`
- Colors are exactly 6 hex characters, no `#`
- `entryId` values are unique within the file
- `qty` values are whole numbers greater than 0
- If `isSuffix` is `true`, `enableDetailedDescriptions` is also `true` and every entry has an `alt-label`

## 📦 Output

Generates `System.zip` containing:
- `LuaFiles514/itemAnnotations.lua` (generated with your config)
- `LuaFiles514/itemInfo_f.lua` (modified itemInfo_f.lua script to make annotations work)

## 🔧 Troubleshooting

### "requires a non-empty 'alt-label'" error
- This means `display.isSuffix` is set to `true`, but one or more entries are missing `alt-label`
- Add a short `alt-label` to every entry in the file, or set `isSuffix` back to `false` if you don't need suffix-style display

### "Dropping source ..." warning
- One of your sources didn't pass validation — check the error message for details (missing field, bad color, etc.)
- Compare your file against `resourceTemplate.json` to spot the difference
- Other sources are unaffected — only the broken one is skipped

### "Config is empty or malformed"
- Check `skuldConf.json` syntax (must be valid JSON)
- If it's corrupted beyond repair, SKULD backs up the broken file and creates a fresh default — just re-add your custom sources afterward

### Items not showing annotations in-game
- Verify `System.zip` was extracted to your RO main folder
- Restart your RO client after installation
- **Ensure your client has MultiItem support enabled** (see Requirements)

### Can't fetch data
- Verify internet connection
- If using a `LocalFile` source, double-check the file path is correct and accessible

## ❤️ Credits

**SKULD** is part of the [ROTools](https://github.com/neeye0n/ROTools) collection

Built with ❤️ by [neeye0n](https://github.com/neeye0n)