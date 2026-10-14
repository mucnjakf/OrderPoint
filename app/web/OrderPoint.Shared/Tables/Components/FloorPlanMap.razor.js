// Drags the pieces of an editable floor plan in whole cells; the component saves a drop and says whether it fits
export function enableDragging(map, reference) {
    let drag = null;

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    function reset(piece) {
        piece.style.transform = "";
        piece.style.zIndex = "";
    }

    function onPointerDown(event) {
        const piece = event.target.closest("[data-piece-id]");

        if (!piece || event.button !== 0) {
            return;
        }

        const rect = map.getBoundingClientRect();
        const columns = Number(map.dataset.columns);
        const rows = Number(map.dataset.rows);

        drag = {
            piece,
            pointerId: event.pointerId,
            startX: event.clientX,
            startY: event.clientY,
            cellWidth: rect.width / columns,
            cellHeight: rect.height / rows,
            columns,
            rows,
            x: Number(piece.dataset.x),
            y: Number(piece.dataset.y),
            width: Number(piece.dataset.width),
            height: Number(piece.dataset.height),
            dx: 0,
            dy: 0
        };

        piece.setPointerCapture(event.pointerId);
    }

    function onPointerMove(event) {
        if (!drag || event.pointerId !== drag.pointerId) {
            return;
        }

        const dx = clamp(
            Math.round((event.clientX - drag.startX) / drag.cellWidth),
            -drag.x,
            drag.columns - drag.width - drag.x);

        const dy = clamp(
            Math.round((event.clientY - drag.startY) / drag.cellHeight),
            -drag.y,
            drag.rows - drag.height - drag.y);

        if (dx === drag.dx && dy === drag.dy) {
            return;
        }

        drag.dx = dx;
        drag.dy = dy;
        drag.piece.style.transform = `translate(${dx * drag.cellWidth}px, ${dy * drag.cellHeight}px)`;
        drag.piece.style.zIndex = "1";
    }

    // A kept drop is re-rendered at its new cell, which also clears the transform; a refused one jumps back
    async function onPointerUp(event) {
        if (!drag || event.pointerId !== drag.pointerId) {
            return;
        }

        const { piece, x, y, dx, dy } = drag;
        drag = null;

        if (dx === 0 && dy === 0) {
            reset(piece);
            return;
        }

        const isKept = await reference.invokeMethodAsync("OnPieceDroppedAsync", piece.dataset.pieceId, x + dx, y + dy);

        if (!isKept) {
            reset(piece);
        }
    }

    function onPointerCancel(event) {
        if (!drag || event.pointerId !== drag.pointerId) {
            return;
        }

        reset(drag.piece);
        drag = null;
    }

    map.addEventListener("pointerdown", onPointerDown);
    map.addEventListener("pointermove", onPointerMove);
    map.addEventListener("pointerup", onPointerUp);
    map.addEventListener("pointercancel", onPointerCancel);

    return {
        dispose() {
            map.removeEventListener("pointerdown", onPointerDown);
            map.removeEventListener("pointermove", onPointerMove);
            map.removeEventListener("pointerup", onPointerUp);
            map.removeEventListener("pointercancel", onPointerCancel);
        }
    };
}