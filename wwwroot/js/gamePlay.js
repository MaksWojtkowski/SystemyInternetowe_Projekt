(function () {
    'use strict';

    const menu         = document.getElementById('gameMenu');
    const btnStart     = document.getElementById('btnStart');
    const elScore      = document.getElementById('statScore');
    const elBest       = document.getElementById('statBest');
    const elTime       = document.getElementById('statTime');
    const elSaveStatus = document.getElementById('saveStatus');

    // ── Menu show/hide ────────────────────────────────────────────────────────
    function hideMenu() {
        menu?.classList.add('hidden');
    }

    function showMenu(title, sub, btnLabel) {
        const elTitle = menu?.querySelector('.menu-title');
        const elSub   = menu?.querySelector('.menu-sub');
        if (elTitle) elTitle.textContent = title;
        if (elSub)   elSub.textContent   = sub;
        if (btnStart) btnStart.textContent = btnLabel ?? 'START';
        menu?.classList.remove('hidden');
    }

    // ── Przycisk START ────────────────────────────────────────────────────────
    if (btnStart) {
        btnStart.addEventListener('click', () => {
            hideMenu();
            if (typeof window.gameStart === 'function') window.gameStart();
        });
    }

    // Spacja gdy menu widoczne
    document.addEventListener('keydown', e => {
        if (e.code === 'Space' && menu && !menu.classList.contains('hidden')) {
            e.preventDefault();
            btnStart?.click();
        }
    });

    // ── Zapis wyniku ──────────────────────────────────────────────────────────
    async function saveScore(score, durationSeconds) {
        elSaveStatus.textContent = 'Zapisywanie…';
        elSaveStatus.className   = 'save-status';

        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            const res   = await fetch('/api/scores', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token?.value ?? ''
                },
                body: JSON.stringify({
                    gameSlug: window.ARCADE.slug,
                    score,
                    durationSeconds
                })
            });

            if (res.ok) {
                elSaveStatus.textContent = '✓ Wynik zapisany';
                elSaveStatus.className   = 'save-status ok';
                if (!window.ARCADE.personalBest || score > window.ARCADE.personalBest) {
                    elBest.textContent         = score.toLocaleString('pl-PL');
                    window.ARCADE.personalBest = score;
                }
            } else {
                const err = await res.json().catch(() => ({}));
                elSaveStatus.textContent = '✗ ' + (err.error ?? 'Błąd zapisu');
                elSaveStatus.className   = 'save-status err';
            }
        } catch {
            elSaveStatus.textContent = '✗ Brak połączenia';
            elSaveStatus.className   = 'save-status err';
        }
    }

    // ── window.arcade API ─────────────────────────────────────────────────────
    window.arcade = {

        setScore(val) {
            elScore.textContent = val.toLocaleString('pl-PL');
        },

        setTime(seconds) {
            elTime.textContent = seconds + 's';
        },

        async gameOver(score, durationSeconds) {
            showMenu(
                'KONIEC GRY',
                `Twój wynik: ${score.toLocaleString('pl-PL')} pkt`,
                'ZAGRAJ PONOWNIE'
            );

            if (window.ARCADE.isLoggedIn) {
                await saveScore(score, durationSeconds);
            }
        }
    };
})();
