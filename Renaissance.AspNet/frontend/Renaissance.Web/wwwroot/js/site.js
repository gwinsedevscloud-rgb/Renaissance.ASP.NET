window.renaissanceDownloadBytes = (fileName, contentType, byteArray) => {
    const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
};

window.renaissanceCopyText = async (text) => {
    await navigator.clipboard.writeText(text);
};

window.renaissanceCloseMainNav = () => {
    const el = document.getElementById('mainNav');
    if (!el || typeof bootstrap === 'undefined') {
        return;
    }

    const instance = bootstrap.Collapse.getInstance(el);
    if (instance) {
        instance.hide();
        return;
    }

    if (el.classList.contains('show')) {
        el.classList.remove('show');
        document.querySelectorAll('[data-bs-target="#mainNav"]').forEach((btn) => {
            btn.setAttribute('aria-expanded', 'false');
            btn.classList.add('collapsed');
        });
    }
};

window.renaissanceSyncHeaderHeight = () => {
    const header = document.querySelector('.ren-header');
    if (!header) {
        return;
    }

    const height = Math.ceil(header.getBoundingClientRect().height);
    document.documentElement.style.setProperty('--ren-header-height', `${height}px`);
};

(function () {
    const hideDelayMs = 900;
    const timers = new WeakMap();

    function markScrolling(element) {
        if (!(element instanceof Element)) {
            return;
        }

        element.classList.add('is-scrolling');

        const existing = timers.get(element);
        if (existing) {
            clearTimeout(existing);
        }

        timers.set(
            element,
            setTimeout(() => {
                element.classList.remove('is-scrolling');
                timers.delete(element);
            }, hideDelayMs)
        );
    }

    function onScroll(event) {
        const target = event.target;
        if (target === document) {
            markScrolling(document.documentElement);
            return;
        }

        if (target instanceof Element) {
            markScrolling(target);
        }
    }

    function syncHeader() {
        window.renaissanceSyncHeaderHeight();
    }

    window.addEventListener(
        'scroll',
        () => markScrolling(document.documentElement),
        { passive: true }
    );
    document.addEventListener('scroll', onScroll, { passive: true, capture: true });
    window.addEventListener('resize', syncHeader, { passive: true });
    document.addEventListener('DOMContentLoaded', syncHeader);
    syncHeader();
})();
