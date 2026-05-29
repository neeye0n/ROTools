const result = {};

document.querySelectorAll('[role="rowgroup"] > [role="row"]').forEach(row => {
    const cells = row.querySelectorAll('[role="cell"]');

    if (cells.length >= 2) {
        const col1Text = cells[0].innerText.trim();
        const col2Text = cells[1].innerText.trim();

        const items = [];
        const regex = /(\d+)\s+([^\(]+?)\s+\((\d+)\)/g;
        let match;

        while ((match = regex.exec(col2Text)) !== null) {
            const qty = parseInt(match[1]);
            const matName = match[2].trim();
            const matId = parseInt(match[3]);

            items.push({ matId, matName, qty });
        }

        if (items.length > 0) {
            result[col1Text] = items;
        }
    }
});

console.log(JSON.stringify(result));