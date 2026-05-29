function extractItemsFromTable(table) {
    const rows = Array.from(table.querySelectorAll("tbody tr"))
        .filter(tr => !tr.matches(".filled_header_db"))
        .map(tr => Array.from(tr.querySelectorAll("td")));

    const result = {};

    for (const cells of rows) {
        const productionName = cells[0]?.innerText.trim();
        if (!productionName) continue;

        if (!result[productionName]) result[productionName] = [];

        const td = cells[1]; // "Source Item(s)" column
        if (!td) continue;

        const children = Array.from(td.childNodes).filter(n => n.nodeType !== 3); // skip text nodes

        for (let i = 0; i < children.length; i++) {
            const node = children[i];
            if (node.tagName === "A") {
                const href = node.getAttribute("href") || "";
                const idMatch = href.match(/item_id=([^&]+)/);
                const matId = Number(idMatch ? idMatch[1] : null);
                const matName = node.innerText.trim();

                let qty = 1;
                const next = children[i + 1];
                if (next?.tagName === "B") {
                    const parsedQty = parseInt(next.innerText.trim());
                    if (!isNaN(parsedQty)) qty = parsedQty;
                }

                result[productionName].push({ matId, matName, qty });
            }
        }
    }

    return result;
}

const table = document.querySelector("table.content_box_db");

if (table) {
    const data = extractItemsFromTable(table);
    console.log(JSON.stringify(data, null, 2));
}