(function () {
    'use strict';

    const menu         = document.getElementById('gameMenu');
    const btnStart     = document.getElementById('btnStart');
    const elScore      = document.getElementById('statScore');
    const elBest       = document.getElementById('statBest');
    const elTime       = document.getElementById('statTime');
    const elSaveStatus = document.getElementById('saveStatus');
    
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
    
    if (btnStart) {
        btnStart.addEventListener('click', () => {
            hideMenu();
            if (typeof window.gameStart === 'function') window.gameStart();
        });
    }
    
    document.addEventListener('keydown', e => {
        if (e.code === 'Space' && menu && !menu.classList.contains('hidden')) {
            e.preventDefault();
            btnStart?.click();
        }
    });

    function showAchievementToast(icon, name, description) {
        const toast = document.createElement('div');
        toast.className = 'achievement-toast';
        toast.innerHTML = `
            <span style="font-size:1.5rem">${icon}</span>
            <div>
                <div style="font-family:'Press Start 2P',monospace;font-size:.5rem;
                            color:#facc15;letter-spacing:.06em;margin-bottom:.3rem">
                    ODZNAKA!
                </div>
                <div style="font-size:.8rem;font-weight:600;color:#e2e2ec">${name}</div>
                <div style="font-size:.72rem;color:#6b6b85;margin-top:.15rem">${description}</div>
            </div>
        `;
        
        const style = document.createElement('style');
        style.className = 'slideIn';
        document.head.appendChild(style);
        document.body.appendChild(toast);

        // Znikaj po 4 sekundach
        setTimeout(() => {
            toast.style.transition = 'opacity .3s';
            toast.style.opacity    = '0';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }
    
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
                const data = await res.json();
                elSaveStatus.textContent = '✓ Wynik zapisany';
                elSaveStatus.className   = 'save-status ok';
                if (!window.ARCADE.personalBest || score > window.ARCADE.personalBest) {
                    elBest.textContent         = score.toLocaleString('pl-PL');
                    window.ARCADE.personalBest = score;
                }

                if (data.newAchievements?.length > 0) {
                    data.newAchievements.forEach((a, i) => {
                        setTimeout(() => {
                            showAchievementToast(a.icon, a.name, a.description);
                        }, i * 2500);
                    });
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
