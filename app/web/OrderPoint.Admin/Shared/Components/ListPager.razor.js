// Inside a dialog its content scrolls, elsewhere the page does
export function scrollToTop(element) {
    const dialogContent = element.closest(".mud-dialog-content");

    if (dialogContent) {
        dialogContent.scrollTo({ top: 0, behavior: "smooth" });
        return;
    }

    window.scrollTo({ top: 0, behavior: "smooth" });
}