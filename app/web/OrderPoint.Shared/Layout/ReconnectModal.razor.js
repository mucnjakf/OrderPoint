// Blazor sets the state classes on the dialog; this opens and closes it and wires the buttons
const reconnectModal = document.getElementById("components-reconnect-modal");

reconnectModal.addEventListener("components-reconnect-state-changed", onReconnectStateChanged);
reconnectModal.addEventListener("cancel", event => event.preventDefault());
document.getElementById("components-reconnect-button").addEventListener("click", retryAsync);
document.getElementById("components-resume-button").addEventListener("click", resumeAsync);

function onReconnectStateChanged(event) {
    const state = event.detail.state;

    if (state === "hide") {
        reconnectModal.close();
        return;
    }

    if (state === "rejected") {
        location.reload();
        return;
    }

    if (!reconnectModal.open) {
        reconnectModal.showModal();
    }

    if (state === "failed") {
        document.addEventListener("visibilitychange", retryWhenVisibleAsync);
    }
}

// true: reconnected; false: the server no longer has the circuit; throws: the server is unreachable
async function retryAsync() {
    document.removeEventListener("visibilitychange", retryWhenVisibleAsync);

    try {
        const isReconnected = await Blazor.reconnect();

        if (isReconnected) {
            return;
        }

        const isResumed = await Blazor.resumeCircuit();

        if (isResumed) {
            reconnectModal.close();
        } else {
            location.reload();
        }
    } catch {
        document.addEventListener("visibilitychange", retryWhenVisibleAsync);
    }
}

async function resumeAsync() {
    try {
        const isResumed = await Blazor.resumeCircuit();

        if (!isResumed) {
            location.reload();
        }
    } catch {
        reconnectModal.classList.replace("components-reconnect-paused", "components-reconnect-resume-failed");
    }
}

async function retryWhenVisibleAsync() {
    if (document.visibilityState === "visible") {
        await retryAsync();
    }
}