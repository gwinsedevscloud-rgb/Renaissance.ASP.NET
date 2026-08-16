window.renaissanceReferrals = (function () {
    var audioContext = null;

    function getContext() {
        if (!audioContext) {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }
        return audioContext;
    }

    function unlockAudio() {
        try {
            var ctx = getContext();
            if (ctx.state === 'suspended') {
                ctx.resume();
            }
        } catch (_) { /* ignore */ }
    }

    ['click', 'keydown', 'touchstart'].forEach(function (event) {
        document.addEventListener(event, unlockAudio, { once: true, passive: true });
    });

    function playTone(ctx, frequency, startTime, duration, volume, type) {
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();
        osc.type = type || 'sine';
        osc.frequency.setValueAtTime(frequency, startTime);

        gain.gain.setValueAtTime(0.0001, startTime);
        gain.gain.exponentialRampToValueAtTime(volume, startTime + 0.018);
        gain.gain.exponentialRampToValueAtTime(0.0001, startTime + duration);

        osc.connect(gain);
        gain.connect(ctx.destination);
        osc.start(startTime);
        osc.stop(startTime + duration + 0.04);
    }

    function playAlert() {
        try {
            var ctx = getContext();
            if (ctx.state === 'suspended') {
                ctx.resume();
            }

            var now = ctx.currentTime;
            // Soft two-tone chime: E5 → G5 with a light overtone
            playTone(ctx, 659.25, now, 0.13, 0.065);
            playTone(ctx, 783.99, now + 0.11, 0.24, 0.055);
            playTone(ctx, 1567.98, now + 0.11, 0.16, 0.012, 'triangle');
        } catch (_) { /* ignore */ }
    }

    function showBrowserNotification(title, body) {
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
    }

    return {
        playAlert: playAlert,
        showBrowserNotification: showBrowserNotification,
        notify: function (title, body) {
            playAlert();
            if (document.hidden) {
                showBrowserNotification(title, body);
            }
        }
    };
})();
