(function () {
    'use strict';

    let _dotNet = null;
    let _el = null;
    let _active = false;
    let _visited = [];
    let _visitedSet = new Set();

    // Risale il DOM dalla coordinata per trovare la cella con data-cell-idx
    function cellAt(x, y) {
        let node = document.elementFromPoint(x, y);
        while (node && node !== document.body) {
            if (node.dataset && node.dataset.cellIdx !== undefined) return node;
            node = node.parentElement;
        }
        return null;
    }

    // Aggiunge la cella alle selezionate se vuota e non ancora visitata
    function tryVisit(x, y) {
        const cell = cellAt(x, y);
        if (!cell || !cell.classList.contains('cell-vuota')) return;
        const idx = parseInt(cell.dataset.cellIdx);
        if (_visitedSet.has(idx)) return;
        _visited.push(idx);
        _visitedSet.add(idx);
        cell.classList.remove('cell-vuota');
        cell.classList.add('cell-drag');
    }

    function onPointerDown(e) {
        // Solo tasto sinistro del mouse; touch e penna passano sempre
        if (e.pointerType === 'mouse' && e.button !== 0) return;
        const cell = cellAt(e.clientX, e.clientY);
        if (!cell || !cell.classList.contains('cell-vuota')) return;
        _active = true;
        _visited = [];
        _visitedSet = new Set();
        tryVisit(e.clientX, e.clientY);
    }

    function onPointerMove(e) {
        if (!_active) return;
        tryVisit(e.clientX, e.clientY);
    }

    function onPointerUp() {
        if (!_active) return;
        _active = false;
        // Informa Blazor solo se è stato un vero drag (più di 1 cella)
        if (_visited.length > 1 && _dotNet) {
            _dotNet.invokeMethodAsync('CompletaDragJS', _visited);
        }
        _visited = [];
        _visitedSet = new Set();
    }

    window.belix = window.belix || {};
    window.belix.editorDrag = {
        init: function (dotNet, el) {
            _dotNet = dotNet;
            _el = el;
            el.addEventListener('pointerdown', onPointerDown);
            el.addEventListener('pointermove', onPointerMove);
            el.addEventListener('pointerup', onPointerUp);
            el.addEventListener('pointercancel', onPointerUp);
        },
        dispose: function () {
            if (_el) {
                _el.removeEventListener('pointerdown', onPointerDown);
                _el.removeEventListener('pointermove', onPointerMove);
                _el.removeEventListener('pointerup', onPointerUp);
                _el.removeEventListener('pointercancel', onPointerUp);
                _el = null;
            }
            _dotNet = null;
            _active = false;
        }
    };
})();
