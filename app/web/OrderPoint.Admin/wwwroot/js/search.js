window.registerSearchShortcut = dotNetReference => {
    document.addEventListener("keydown", event => {
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
            event.preventDefault();
            dotNetReference.invokeMethodAsync("OpenSearchFromShortcutAsync");
        }
    });
};

window.scrollSearchResultIntoView = element => element.scrollIntoView({ block: "nearest" });