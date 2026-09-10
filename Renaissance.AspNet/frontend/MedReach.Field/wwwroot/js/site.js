window.renaissanceCloseMainNav = () => {
    const el = document.getElementById('fieldNav');
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
        document.querySelectorAll('[data-bs-target="#fieldNav"]').forEach((btn) => {
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
    function syncHeader() {
        window.renaissanceSyncHeaderHeight();
    }

    window.addEventListener('resize', syncHeader, { passive: true });
    document.addEventListener('DOMContentLoaded', syncHeader);
    syncHeader();
})();
