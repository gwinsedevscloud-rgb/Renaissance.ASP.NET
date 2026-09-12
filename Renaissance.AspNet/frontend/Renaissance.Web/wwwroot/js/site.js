window.renaissanceCloseMainNav = () => {
    const shell = document.querySelector(".ren-shell-with-sidebar");
    if (shell) {
        shell.classList.remove("sidebar-open");
    }

    const el = document.getElementById("mainNav");
    if (!el) {
        return;
    }

    el.classList.remove("is-open", "show");

    if (typeof bootstrap !== "undefined") {
        const instance = bootstrap.Collapse.getInstance(el);
        if (instance) {
            instance.hide();
        }
    }
};

window.renaissanceSyncHeaderHeight = () => {
    window.renaissanceSyncShellMetrics();
};

window.renaissanceSyncShellMetrics = () => {
    const root = document.documentElement;
    const mobileBar = document.querySelector(".ren-mobile-bar");

    if (mobileBar && getComputedStyle(mobileBar).display !== "none") {
        const height = Math.ceil(mobileBar.getBoundingClientRect().height);
        root.style.setProperty("--ren-header-height", `${height}px`);
    } else if (window.matchMedia("(min-width: 992px)").matches) {
        root.style.setProperty("--ren-header-height", "0px");
    }

    // Desktop sidebar offset is CSS-driven (expanded vs collapsed).
    // Clear any stale inline pad so collapse/expand cannot desync content.
    if (window.matchMedia("(min-width: 992px)").matches) {
        root.style.removeProperty("--ren-shell-pad");
    } else {
        root.style.removeProperty("--ren-shell-pad");
    }
};

window.renaissanceGetSidebarCollapsed = () => {
    try {
        return localStorage.getItem("ren-sidebar-collapsed") === "1";
    } catch {
        return false;
    }
};

window.renaissanceSetSidebarCollapsed = (collapsed) => {
    try {
        localStorage.setItem("ren-sidebar-collapsed", collapsed ? "1" : "0");
    } catch {
        /* ignore */
    }
};

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
