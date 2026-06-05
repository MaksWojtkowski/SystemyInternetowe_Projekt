(function () {
    'use strict';

    const GRID  = 20;
    const COLS  = 24;
    const ROWS  = 24;
    const W     = GRID * COLS;
    const H     = GRID * ROWS;
    const SPEED = 120; // ms między krokami

    const COLORS = {
        bg:       '#050508',
        grid:     '#0d0d14',
        head:     '#00e5ff',
        body:     '#00b8cc',
        food:     '#ff2d78',
        foodGlow: 'rgba(255,45,120,.4)',
    };

    const canvas = document.getElementById('gameCanvas');
    const ctx    = canvas.getContext('2d');
    canvas.width  = W;
    canvas.height = H;

    let snake, direction, nextDir, food, score;
    let loop, timerInterval, startedAt;
    let running = false;

    function initState() {
        const cx = Math.floor(COLS / 2);
        const cy = Math.floor(ROWS / 2);
        snake     = [{ x: cx, y: cy }, { x: cx - 1, y: cy }, { x: cx - 2, y: cy }];
        direction = { x: 1, y: 0 };
        nextDir   = { x: 1, y: 0 };
        score     = 0;
        food      = spawnFood();
    }

    function spawnFood() {
        let pos;
        do {
            pos = {
                x: Math.floor(Math.random() * COLS),
                y: Math.floor(Math.random() * ROWS)
            };
        } while (snake.some(s => s.x === pos.x && s.y === pos.y));
        return pos;
    }

    function draw() {
        // Tlo
        ctx.fillStyle = COLORS.bg;
        ctx.fillRect(0, 0, W, H);

        // Siatka
        ctx.strokeStyle = COLORS.grid;
        ctx.lineWidth   = 0.5;
        for (let x = 0; x <= COLS; x++) {
            ctx.beginPath(); ctx.moveTo(x * GRID, 0); ctx.lineTo(x * GRID, H); ctx.stroke();
        }
        for (let y = 0; y <= ROWS; y++) {
            ctx.beginPath(); ctx.moveTo(0, y * GRID); ctx.lineTo(W, y * GRID); ctx.stroke();
        }

        // Jedzenie
        ctx.shadowColor = COLORS.foodGlow;
        ctx.shadowBlur  = 14;
        ctx.fillStyle   = COLORS.food;
        ctx.beginPath();
        ctx.arc(food.x * GRID + GRID / 2, food.y * GRID + GRID / 2, GRID / 2 - 2, 0, Math.PI * 2);
        ctx.fill();
        ctx.shadowBlur = 0;

        // Waz
        snake.forEach((seg, i) => {
            const isHead = i === 0;
            ctx.shadowColor = isHead ? COLORS.head : 'transparent';
            ctx.shadowBlur  = isHead ? 10 : 0;
            ctx.fillStyle   = isHead ? COLORS.head : COLORS.body;
            roundRect(seg.x * GRID + 1, seg.y * GRID + 1, GRID - 2, GRID - 2, isHead ? 5 : 3);
        });
        ctx.shadowBlur = 0;

        // Wynik na canvasie
        ctx.fillStyle = 'rgba(255,255,255,.18)';
        ctx.font      = '11px "Press Start 2P", monospace';
        ctx.fillText(String(score), 8, 18);
    }

    function roundRect(x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.lineTo(x + w - r, y);
        ctx.quadraticCurveTo(x + w, y, x + w, y + r);
        ctx.lineTo(x + w, y + h - r);
        ctx.quadraticCurveTo(x + w, y + h, x + w - r, y + h);
        ctx.lineTo(x + r, y + h);
        ctx.quadraticCurveTo(x, y + h, x, y + h - r);
        ctx.lineTo(x, y + r);
        ctx.quadraticCurveTo(x, y, x + r, y);
        ctx.closePath();
        ctx.fill();
    }

    function step() {
        direction = { ...nextDir };
        const head = { x: snake[0].x + direction.x, y: snake[0].y + direction.y };

        if (head.x < 0 || head.x >= COLS || head.y < 0 || head.y >= ROWS) {
            endGame(); return;
        }
        if (snake.some(s => s.x === head.x && s.y === head.y)) {
            endGame(); return;
        }

        snake.unshift(head);

        if (head.x === food.x && head.y === food.y) {
            score += 10;
            window.arcade.setScore(score);
            food = spawnFood();
        } else {
            snake.pop();
        }

        draw();
    }

    function endGame() {
        running = false;
        clearInterval(loop);
        clearInterval(timerInterval);

        const duration = Math.floor((Date.now() - startedAt) / 1000);

        ctx.fillStyle = 'rgba(255,45,120,.2)';
        ctx.fillRect(0, 0, W, H);

        setTimeout(() => window.arcade.gameOver(score, duration), 400);
    }

    // Wywoływane przez gamePlay.js po kliknięciu START
    window.gameStart = function () {
        if (running) return;

        initState();
        running   = true;
        startedAt = Date.now();

        window.arcade.setScore(0);
        window.arcade.setTime(0);

        timerInterval = setInterval(() => {
            window.arcade.setTime(Math.floor((Date.now() - startedAt) / 1000));
        }, 1000);

        loop = setInterval(step, SPEED);
        draw();
    };

    // Sterowanie klawiatura
    const KEYS = {
        ArrowUp:    { x:  0, y: -1 }, KeyW: { x:  0, y: -1 },
        ArrowDown:  { x:  0, y:  1 }, KeyS: { x:  0, y:  1 },
        ArrowLeft:  { x: -1, y:  0 }, KeyA: { x: -1, y:  0 },
        ArrowRight: { x:  1, y:  0 }, KeyD: { x:  1, y:  0 },
    };

    document.addEventListener('keydown', e => {
        const d = KEYS[e.code];
        if (!d) return;
        if (e.code.startsWith('Arrow')) e.preventDefault();
        if (!running) return;
        if (d.x !== -direction.x || d.y !== -direction.y) nextDir = d;
    });

    // Sterowanie dotykowe
    let tx = 0, ty = 0;
    canvas.addEventListener('touchstart', e => {
        tx = e.touches[0].clientX;
        ty = e.touches[0].clientY;
        e.preventDefault();
    }, { passive: false });

    canvas.addEventListener('touchend', e => {
        if (!running) return;
        const dx = e.changedTouches[0].clientX - tx;
        const dy = e.changedTouches[0].clientY - ty;
        let d;
        if (Math.abs(dx) > Math.abs(dy)) {
            d = dx > 0 ? { x: 1, y: 0 } : { x: -1, y: 0 };
        } else {
            d = dy > 0 ? { x: 0, y: 1 } : { x: 0, y: -1 };
        }
        if (d.x !== -direction.x || d.y !== -direction.y) nextDir = d;
        e.preventDefault();
    }, { passive: false });

    // Inicjalny render przed startem
    initState();
    draw();
}());
