(function () {
    'use strict';
    
    const SHAPES = [
        [[0,0,0,0],[1,1,1,1],[0,0,0,0],[0,0,0,0]], // I
        [[1,0,0],[1,1,1],[0,0,0]],                   // J
        [[0,0,1],[1,1,1],[0,0,0]],                   // L
        [[1,1],[1,1]],                               // O
        [[0,1,1],[1,1,0],[0,0,0]],                   // S
        [[1,1,1],[0,1,0],[0,0,0]],                   // T
        [[1,1,0],[0,1,1],[0,0,0]],                   // Z
    ];

    const SHAPE_COLORS = [
        '#00BCD4','#485FE5','#FF9800','#FFEB3B',
        '#4CAF50','#A629BC','#F44336',
    ];

    const BLOCK_SIZE          = 32;
    const BLOCK_BACKGROUND    = '#292929';
    const COLOR_EMPTY_BLOCK   = '#343434';
    const COLOR_SIDEBAR_BORDER = '#DDD';
    const COLOR_FONT          = '#FFF';
    const COLOR_GAME_OVER_OVERLAY = '#000000bb';

    const GRAVITY_SPEED        = 1;
    const GRAVITY_ACCELERATION = 0.00001;
    const GRAVITY_THRESHOLD    = 1000;

    const GRID_COLS = 10;
    const GRID_ROWS = 20;

    const SIDEBAR_BORDER       = 12;
    const SIDEBAR_WIDTH_BLOCKS = 6;

    const INPUT_REPEAT_THRESHOLD = 400;
    const INPUT_REPEAT_INTERVAL  = 5;
    const MAX_DT                 = 100;

    const BLOCK_EMPTY         = -1;
    const INPUT_STATE_INITIAL  = 0;
    const INPUT_STATE_CHARGING = 1;
    const INPUT_STATE_REPEATING = 2;

    const GRID_WIDTH    = GRID_COLS * BLOCK_SIZE;
    const GRID_HEIGHT   = GRID_ROWS * BLOCK_SIZE;
    const SIDEBAR_WIDTH = SIDEBAR_WIDTH_BLOCKS * BLOCK_SIZE;
    const SIDEBAR_CONTENT_X = GRID_WIDTH + SIDEBAR_BORDER + BLOCK_SIZE / 2;
    const SIDEBAR_CONTENT_Y = BLOCK_SIZE;
    const CANVAS_WIDTH  = GRID_WIDTH + SIDEBAR_BORDER + SIDEBAR_WIDTH;
    const CANVAS_HEIGHT = GRID_HEIGHT;

    const KEY_TO_INPUT = {
        ArrowLeft: 'moveLeft', ArrowRight: 'moveRight',
        ArrowDown: 'moveDown', ArrowUp: 'rotate',
        ' ': 'hardDrop',
    };
    
    const canvas  = document.getElementById('gameCanvas');
    const ctx     = canvas.getContext('2d');
    canvas.width  = CANVAS_WIDTH;
    canvas.height = CANVAS_HEIGHT;
    
    let state, inputs, rafId;
    let running       = false;
    let startedAt     = null;
    let timerInterval = null;
    let lastScore     = 0;
    
    function getRandomIndex(n) { return Math.floor(Math.random() * n); }
    function getRandomShapeId() { return getRandomIndex(SHAPES.length); }

    function makeEmptyGrid() {
        return Array.from({ length: GRID_ROWS }, () =>
            Array(GRID_COLS).fill(BLOCK_EMPTY));
    }

    function createCurrentPiece(shapeId) {
        const shape = SHAPES[shapeId];
        return { shapeId, shape,
            position: { x: getRandomIndex(GRID_COLS - shape[0].length + 1), y: 0 } };
    }

    function getInitialState() {
        return {
            isGameOver: false, score: 0,
            gravity: { progress: 0, speed: GRAVITY_SPEED },
            currentPiece: createCurrentPiece(getRandomShapeId()),
            nextShapeId:  getRandomShapeId(),
            grid: makeEmptyGrid(),
        };
    }

    function canGridFitShape(grid, shape, shapeX, shapeY) {
        return shape.every((row, i) => row.every((isSolid, j) => {
            if (!isSolid) return true;
            const gy = shapeY + i, gx = shapeX + j;
            if (gy >= grid.length) return false;
            if (gx < 0 || gx >= grid[0].length) return false;
            return grid[gy][gx] === BLOCK_EMPTY;
        }));
    }

    function moveCurrentPiece(grid, piece, dx, dy) {
        const { shape, position } = piece;
        const ok = canGridFitShape(grid, shape, position.x + dx, position.y + dy);
        if (ok) { position.x += dx; position.y += dy; }
        return ok;
    }

    function attachToGrid(grid, piece) {
        const { shapeId, shape, position } = piece;
        for (let i = 0; i < shape.length; i++)
            for (let j = 0; j < shape[0].length; j++)
                if (shape[i][j]) grid[position.y + i][position.x + j] = shapeId;
    }

    function clearCompleteLines(grid) {
        let cleared = 0;
        for (let i = grid.length - 1; i >= 0; i--) {
            if (grid[i].every(c => c !== BLOCK_EMPTY)) cleared++;
            else if (cleared > 0) grid[i + cleared] = [...grid[i]];
        }
        for (let i = 0; i < cleared; i++) grid[i].fill(BLOCK_EMPTY);
        return cleared;
    }

    function handleLanding(st) {
        attachToGrid(st.grid, st.currentPiece);
        const lines = clearCompleteLines(st.grid);
        // Punkty: 1=100, 2=300, 3=500, 4=800
        const pts = [0, 100, 300, 500, 800];
        st.score += pts[lines] ?? lines * 100;

        const newPiece = createCurrentPiece(st.nextShapeId);
        if (canGridFitShape(st.grid, newPiece.shape, newPiece.position.x, newPiece.position.y)) {
            st.currentPiece = newPiece;
            st.nextShapeId  = getRandomShapeId();
        } else {
            st.isGameOver = true;
        }
    }

    function moveDown(st) {
        st.gravity.progress = 0;
        const ok = moveCurrentPiece(st.grid, st.currentPiece, 0, 1);
        if (!ok) handleLanding(st);
        return ok;
    }

    function rotate(shape) {
        return Array.from({ length: shape[0].length }, (_, i) =>
            Array.from({ length: shape.length }, (_, j) => shape[shape.length - 1 - j][i]));
    }

    function rotateCurrentPiece(grid, piece) {
        const newShape = rotate(piece.shape);
        if (canGridFitShape(grid, newShape, piece.position.x, piece.position.y))
            piece.shape = newShape;
    }

    function updateGravity(st, dt) {
        st.gravity.speed    += GRAVITY_ACCELERATION * dt;
        st.gravity.progress += st.gravity.speed * dt;
        if (st.gravity.progress >= GRAVITY_THRESHOLD) moveDown(st);
    }

    function handleInputState(input, dt) {
        if (!input) return false;
        input.timer += dt;
        switch (input.state) {
            case INPUT_STATE_INITIAL:
                input.state = INPUT_STATE_CHARGING; return true;
            case INPUT_STATE_CHARGING:
                if (input.timer >= INPUT_REPEAT_THRESHOLD) {
                    input.state = INPUT_STATE_REPEATING; input.timer = 0;
                }
                return false;
            case INPUT_STATE_REPEATING:
                const ok = input.timer >= INPUT_REPEAT_INTERVAL;
                if (ok) input.timer = 0;
                return ok;
        }
        return false;
    }

    function updateCurrentPiece(st, inp, dt) {
        const active = t => handleInputState(inp[t], dt);
        if (active('moveLeft'))  moveCurrentPiece(st.grid, st.currentPiece, -1, 0);
        if (active('moveRight')) moveCurrentPiece(st.grid, st.currentPiece,  1, 0);
        if (active('rotate'))    rotateCurrentPiece(st.grid, st.currentPiece);
        if (active('moveDown'))  moveDown(st);
        if (active('hardDrop'))  { while (moveDown(st)) {} }
    }

    function updateState(st, inp, dt) {
        if (st.isGameOver) return;
        updateCurrentPiece(st, inp, dt);
        updateGravity(st, dt);
    }
    
    function drawBlock(color, x, y) {
        ctx.fillStyle = color;
        ctx.fillRect(x + 1, y + 1, BLOCK_SIZE - 1, BLOCK_SIZE - 1);
    }

    function drawShape(shape, colorId, x, y) {
        const color = SHAPE_COLORS[colorId];
        for (let i = 0; i < shape.length; i++)
            for (let j = 0; j < shape[0].length; j++)
                if (shape[i][j])
                    drawBlock(color, x + j * BLOCK_SIZE, y + i * BLOCK_SIZE);
    }

    function render(st) {
        const { grid, currentPiece, nextShapeId } = st;
        
        ctx.fillStyle = BLOCK_BACKGROUND;
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        
        for (let i = 0; i < grid.length; i++)
            for (let j = 0; j < grid[0].length; j++) {
                const id    = grid[i][j];
                const color = id === BLOCK_EMPTY ? COLOR_EMPTY_BLOCK : SHAPE_COLORS[id];
                drawBlock(color, j * BLOCK_SIZE, i * BLOCK_SIZE);
            }
        
        drawShape(currentPiece.shape, currentPiece.shapeId,
            currentPiece.position.x * BLOCK_SIZE,
            currentPiece.position.y * BLOCK_SIZE);
        
        ctx.fillStyle = COLOR_SIDEBAR_BORDER;
        ctx.fillRect(GRID_WIDTH, 0, SIDEBAR_BORDER, CANVAS_HEIGHT);
        
        ctx.fillStyle = COLOR_FONT;
        ctx.font = 'bold 14px monospace';
        ctx.textAlign = 'left';
        ctx.textBaseline = 'top';
        ctx.fillText('NEXT:', SIDEBAR_CONTENT_X, SIDEBAR_CONTENT_Y);
        drawShape(SHAPES[nextShapeId], nextShapeId,
            SIDEBAR_CONTENT_X, SIDEBAR_CONTENT_Y + BLOCK_SIZE * 1.5);
        
        ctx.font = 'bold 14px monospace';
        ctx.fillStyle = COLOR_FONT;
        ctx.fillText('SCORE:', SIDEBAR_CONTENT_X, SIDEBAR_CONTENT_Y + BLOCK_SIZE * 5);
        ctx.fillText(`${st.score}`.padStart(7, '0'),
            SIDEBAR_CONTENT_X, SIDEBAR_CONTENT_Y + BLOCK_SIZE * 6);
        
        if (st.isGameOver) {
            ctx.fillStyle = COLOR_GAME_OVER_OVERLAY;
            ctx.fillRect(0, 0, GRID_WIDTH, CANVAS_HEIGHT);
            ctx.fillStyle = COLOR_FONT;
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.font = 'bold 24px monospace';
            ctx.fillText('GAME OVER', GRID_WIDTH / 2, CANVAS_HEIGHT / 2);
        }
    }
    
    let previousTime = 0;

    function loop(currentTime) {
        if (!running) return;

        const dt = Math.min(currentTime - previousTime, MAX_DT);
        previousTime = currentTime;

        updateState(state, inputs, dt);
        
        if (state.score !== lastScore) {
            lastScore = state.score;
            window.arcade.setScore(state.score);
        }

        render(state);
        
        if (state.isGameOver) {
            endGame(); return;
        }

        rafId = requestAnimationFrame(loop);
    }
    
    function endGame() {
        running = false;
        cancelAnimationFrame(rafId);
        clearInterval(timerInterval);
        const duration = Math.floor((Date.now() - startedAt) / 1000);
        setTimeout(() => window.arcade.gameOver(state.score, duration), 600);
    }
    
    function handleKeyDown(e) {
        if (e.repeat) return;
        const inputType = KEY_TO_INPUT[e.key];
        if (inputType) {
            e.preventDefault();
            inputs[inputType] = { state: INPUT_STATE_INITIAL, timer: 0 };
        }
    }

    function handleKeyUp(e) {
        const inputType = KEY_TO_INPUT[e.key];
        if (inputType) inputs[inputType] = undefined;
    }

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup',   handleKeyUp);
    
    window.gameStart = function () {
        if (running) return;

        state     = getInitialState();
        inputs    = {};
        lastScore = 0;
        running   = true;
        startedAt = Date.now();

        window.arcade.setScore(0);
        window.arcade.setTime(0);

        timerInterval = setInterval(() => {
            window.arcade.setTime(Math.floor((Date.now() - startedAt) / 1000));
        }, 1000);

        previousTime = performance.now();
        rafId = requestAnimationFrame(loop);
    };
    
    const initState = getInitialState();
    render(initState);

}());
