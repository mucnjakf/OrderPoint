// Brings a table to the middle of the scrollable map, e.g. after "Show on map" or when the phone's sheet opens
export function scrollToTable(scrollArea, tableId) {
    const table = scrollArea.querySelector(`[data-piece-id="${tableId}"]`);

    table?.scrollIntoView({ behavior: "smooth", block: "center", inline: "center" });
}