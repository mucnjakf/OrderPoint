// Prints just the receipt: its markup in a hidden frame, so the page around it stays off the paper
export function printReceipt(receipt) {
    const frame = document.createElement("iframe");
    frame.style.position = "fixed";
    frame.style.width = "0";
    frame.style.height = "0";
    frame.style.border = "0";
    document.body.appendChild(frame);

    const frameDocument = frame.contentDocument;
    frameDocument.open();
    frameDocument.write("<!DOCTYPE html><html><head><title>Receipt</title></head>");
    frameDocument.write(`<body style="margin: 16px; color: #000; background: #fff;">${receipt.outerHTML}</body></html>`);
    frameDocument.close();

    frame.contentWindow.focus();
    frame.contentWindow.print();

    // Printing has copied the page by now
    setTimeout(() => frame.remove(), 1000);
}