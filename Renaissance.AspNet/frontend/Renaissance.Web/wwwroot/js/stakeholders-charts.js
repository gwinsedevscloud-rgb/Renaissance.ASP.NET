window.renaissanceCharts = {
    _instances: {},

    destroyAll() {
        Object.values(this._instances).forEach(c => c?.destroy?.());
        this._instances = {};
    },

    renderAll(payload) {
        if (typeof Chart === 'undefined') return;
        this.destroyAll();

        const teal = '#0d6e6e';
        const tealLight = 'rgba(13, 110, 110, 0.15)';
        const forecast = 'rgba(20, 128, 128, 0.45)';
        const palette = ['#0d6e6e', '#2563eb', '#7c3aed', '#d97706', '#dc2626', '#0891b2', '#65a30d', '#9333ea'];

        const baseOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } }
        };

        if (payload.activityCombined) {
            const ctx = document.getElementById('chart-activity');
            if (ctx) {
                this._instances.activity = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: payload.activityCombined.labels,
                        datasets: [
                            {
                                label: 'Actual encounters',
                                data: payload.activityCombined.actual,
                                borderColor: teal,
                                backgroundColor: tealLight,
                                fill: true,
                                tension: 0.35,
                                pointRadius: 2
                            },
                            {
                                label: 'Forecast',
                                data: payload.activityCombined.forecast,
                                borderColor: forecast,
                                borderDash: [6, 4],
                                fill: false,
                                tension: 0.35,
                                pointRadius: 2
                            }
                        ]
                    },
                    options: {
                        ...baseOptions,
                        plugins: {
                            legend: { display: true, position: 'top' },
                            tooltip: { mode: 'index', intersect: false }
                        },
                        scales: {
                            y: { beginAtZero: true, ticks: { precision: 0 } }
                        }
                    }
                });
            }
        }

        if (payload.modules) {
            const ctx = document.getElementById('chart-modules');
            if (ctx) {
                this._instances.modules = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: payload.modules.labels,
                        datasets: [{
                            label: 'Total records',
                            data: payload.modules.totals,
                            backgroundColor: palette,
                            borderRadius: 6
                        }]
                    },
                    options: {
                        ...baseOptions,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            }
        }

        if (payload.moduleShare) {
            const ctx = document.getElementById('chart-module-share');
            if (ctx) {
                this._instances.moduleShare = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: payload.moduleShare.labels,
                        datasets: [{
                            data: payload.moduleShare.values,
                            backgroundColor: palette,
                            borderWidth: 2,
                            borderColor: '#fff'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { position: 'bottom' } }
                    }
                });
            }
        }

        if (payload.clients) {
            const ctx = document.getElementById('chart-clients');
            if (ctx) {
                this._instances.clients = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: payload.clients.labels,
                        datasets: [
                            {
                                label: 'Registrations',
                                data: payload.clients.actual,
                                borderColor: '#2563eb',
                                backgroundColor: 'rgba(37, 99, 235, 0.12)',
                                fill: true,
                                tension: 0.35,
                                pointRadius: 2
                            },
                            {
                                label: 'Forecast',
                                data: payload.clients.forecast,
                                borderColor: 'rgba(37, 99, 235, 0.45)',
                                borderDash: [6, 4],
                                fill: false,
                                tension: 0.35,
                                pointRadius: 2
                            }
                        ]
                    },
                    options: {
                        ...baseOptions,
                        plugins: { legend: { display: true, position: 'top' } },
                        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                    }
                });
            }
        }

        if (payload.demographics) {
            const ctx = document.getElementById('chart-demographics');
            if (ctx) {
                this._instances.demographics = new Chart(ctx, {
                    type: 'polarArea',
                    data: {
                        labels: payload.demographics.labels,
                        datasets: [{
                            data: payload.demographics.values,
                            backgroundColor: ['rgba(13,110,110,0.65)', 'rgba(37,99,235,0.65)', 'rgba(124,58,237,0.65)', 'rgba(217,119,6,0.65)']
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { position: 'bottom' } }
                    }
                });
            }
        }

        if (payload.pharmacy) {
            const ctx = document.getElementById('chart-pharmacy');
            if (ctx) {
                this._instances.pharmacy = new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: payload.pharmacy.labels,
                        datasets: [{
                            data: payload.pharmacy.values,
                            backgroundColor: ['#f59e0b', '#0d6e6e', '#dc2626']
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { position: 'bottom' } }
                    }
                });
            }
        }
    }
};
