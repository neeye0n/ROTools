# ⚙️ Configuration Guide – `ItemDescTableModder.conf`

This file controls how `ItemDescTableModder` customizes item names and descriptions in `itemInfo_EN.lua`.

You can edit this file using any text editor (e.g., Notepad, VSCode) to enable or disable tags and apply hex-based color formatting to different sections.

---

## 📌 Top-Level Settings

### `itemIdDescTextColor`
- Color for the label text `Item ID:`
- Format: hex color code without `#`  

### `itemIdDescValueColor`
- Color for the item ID number itself eg. `1234`

---

## 🧪 Tag Categories

Each tag category (Brew, Cook, Quest, Instance) has its own config block. Here's what each setting does:

### Common Options per Category

| Key                     | Type    | Description |
|------------------------|---------|-------------|
| `enableTags`           | number (`1` or `0`) | Shows a tag like `[Brew]` in the item name |
| `enableDescriptions`   | number (`1` or `0`) | Adds a special section in the description box |
| `tagText`              | string  | The text inside the tag (e.g., `"Cook"` → `[Cook]`) |
| `descriptionHeaderColor` | string | Hex code for the header color (e.g., `Cooking Material`) |
| `descriptionRowsColor` | string  | Hex code used for each row inside that section |

---