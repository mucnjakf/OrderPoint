// Spaces after every 4 digits of the card number and the slash after the month, put in by the browser as the guest
// types, so the caret never waits for the server
const formats = {
    "card-number": { maxDigits: 19, format: digits => digits.replace(/(\d{4})(?=\d)/g, "$1 ") },
    "expiry": {
        maxDigits: 4,
        format: digits => digits.length > 2 ? `${digits.slice(0, 2)}/${digits.slice(2)}` : digits
    },
    "cvc": { maxDigits: 4, format: digits => digits }
};

export function formatCardFields(container) {
    for (const field of container.querySelectorAll("[data-format]")) {
        // MudBlazor may put the attribute on the input or on the field around it
        const input = field.matches("input") ? field : field.querySelector("input");
        const { maxDigits, format } = formats[field.dataset.format];

        input.addEventListener("input", () => {
            const caret = input.selectionStart ?? input.value.length;
            const digitsBeforeCaret = input.value.slice(0, caret).replace(/\D/g, "").length;
            const formatted = format(input.value.replace(/\D/g, "").slice(0, maxDigits));

            input.value = formatted;

            // The caret stays after the same digit it was after
            let position = 0;
            let digitsSeen = 0;

            while (position < formatted.length && digitsSeen < digitsBeforeCaret) {
                if (/\d/.test(formatted[position])) {
                    digitsSeen++;
                }

                position++;
            }

            input.setSelectionRange(position, position);
        });
    }
}