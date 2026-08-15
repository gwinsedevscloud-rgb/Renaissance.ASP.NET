window.renaissanceReferrals = {
    playAlert: function () {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = 'sine';
            osc.frequency.value = 880;
            gain.gain.value = 0.08;
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start();
            osc.stop(ctx.currentTime + 0.15);
        } catch (_) { /* ignore */ }
    },
    showBrowserNotification: function (title, body) {
        if (!('Notification' in window)) return;
        if (Notification.permission === 'granted') {
            new Notification(title, { body: body, tag: 'renaissance-referral' });
        } else if (Notification.permission !== 'denied') {
            Notification.requestPermission().then(function (p) {
                if (p === 'granted') {
                    new Notification(title, { body: body, tag: 'renaissance-referral' });
                }
            });
        }
    },
    notify: function (title, body) {
        this.playAlert();
        if (document.hidden) {
            this.showBrowserNotification(title, body);
        }
    }
};
