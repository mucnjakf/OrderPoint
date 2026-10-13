// The current height (already at least the held one) becomes the minimum
export function holdHeight(element) {
    element.style.minHeight = `${element.offsetHeight}px`;
}

export function releaseHeight(element) {
    element.style.minHeight = "";
}