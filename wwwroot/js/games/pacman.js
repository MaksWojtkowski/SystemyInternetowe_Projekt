(function () {
    'use strict';

    const rowCount    = 21;
    const columnCount = 19;
    const tileSize    = 32;
    const boardWidth  = columnCount * tileSize;
    const boardHeight = rowCount    * tileSize;
    const IMG_PATH    = '/img/games/pacman/';

    const canvas  = document.getElementById('gameCanvas');
    const context = canvas.getContext('2d');
    canvas.width  = boardWidth;
    canvas.height = boardHeight;
    
    const tileMap = [
        "XXXXXXXXXXXXXXXXXXX",
        "X        X        X",
        "X XX XXX X XXX XX X",
        "X                 X",
        "X XX X XXXXX X XX X",
        "X    X       X    X",
        "XXXX XXXX XXXX XXXX",
        "X    X       X    X",
        "X XX X XXrXX X XX X",
        "X       bpo       X",
        "X XX X XXXXX X XX X",
        "X    X       X    X",
        "XXXX X XXXXX X XXXX",
        "X        X        X",
        "X XX XXX X XXX XX X",
        "X  X     P     X  X",
        "XX X X XXXXX X X XX",
        "X    X   X   X    X",
        "X XXXXXX X XXXXXX X",
        "X                 X",
        "XXXXXXXXXXXXXXXXXXX"
    ];
    
    const powerPelletPositions = [
        { r: 3, c: 1 }, { r: 3, c: 17 },
        { r: 17, c: 1 }, { r: 17, c: 17 },
    ];

    const walls       = new Set();
    const foods       = new Set();
    const powerPellets = new Set();
    const ghosts      = new Set();
    let pacman;

    const directions  = ['U', 'D', 'L', 'R'];
    let score         = 0;
    let lives         = 3;
    let running       = false;
    let loopId        = null;
    let startedAt     = null;
    let timerInterval = null;
    let images        = {};
    
    let frightenedTimer = null;
    let frightenedMode  = false;
    
    class Block {
        constructor(image, x, y, width, height) {
            this.image     = image;
            this.x         = x; this.startX = x;
            this.y         = y; this.startY = y;
            this.width     = width;
            this.height    = height;
            this.direction = 'R';
            this.velocityX = 0;
            this.velocityY = 0;
        }

        updateDirection(direction) {
            const prev = this.direction;
            this.direction = direction;
            this.updateVelocity();
            this.x += this.velocityX;
            this.y += this.velocityY;
            for (let wall of walls.values()) {
                if (collision(this, wall)) {
                    this.x -= this.velocityX;
                    this.y -= this.velocityY;
                    this.direction = prev;
                    this.updateVelocity();
                    return;
                }
            }
        }

        updateVelocity() {
            const speed = tileSize / 4;
            const map = { U:[0,-speed], D:[0,speed], L:[-speed,0], R:[speed,0] };
            [this.velocityX, this.velocityY] = map[this.direction] ?? [0,0];
        }

        reset() { this.x = this.startX; this.y = this.startY; }
    }

    function collision(a, b) {
        return a.x < b.x + b.width  && a.x + a.width  > b.x &&
            a.y < b.y + b.height && a.y + a.height > b.y;
    }
    
    function loadMap(imgs) {
        walls.clear(); foods.clear(); ghosts.clear(); powerPellets.clear();

        for (let r = 0; r < rowCount; r++) {
            for (let c = 0; c < columnCount; c++) {
                const ch = tileMap[r][c];
                const x  = c * tileSize;
                const y  = r * tileSize;

                if      (ch === 'X') walls.add(new Block(imgs.wall,        x, y, tileSize, tileSize));
                else if (ch === 'b') ghosts.add(new Block(imgs.blueGhost,  x, y, tileSize, tileSize));
                else if (ch === 'o') ghosts.add(new Block(imgs.orangeGhost,x, y, tileSize, tileSize));
                else if (ch === 'p') ghosts.add(new Block(imgs.pinkGhost,  x, y, tileSize, tileSize));
                else if (ch === 'r') ghosts.add(new Block(imgs.redGhost,   x, y, tileSize, tileSize));
                else if (ch === 'P') pacman = new Block(imgs.pacmanRight,  x, y, tileSize, tileSize);
                else if (ch === ' ') foods.add(new Block(null, x + 14, y + 14, 4, 4));
            }
        }
        
        for (const pos of powerPelletPositions) {
            const x = pos.c * tileSize + 10;
            const y = pos.r * tileSize + 10;
            powerPellets.add(new Block(null, x, y, 12, 12));
        }
    }

    function resetPositions() {
        pacman.reset();
        pacman.velocityX = 0;
        pacman.velocityY = 0;
        for (let ghost of ghosts.values()) {
            ghost.reset();
            ghost.updateDirection(directions[Math.floor(Math.random() * 4)]);
        }
    }
    
    function draw() {
        context.clearRect(0, 0, canvas.width, canvas.height);
        
        for (let wall of walls.values())
            context.drawImage(wall.image, wall.x, wall.y, wall.width, wall.height);
        
        context.fillStyle = 'white';
        for (let food of foods.values())
            context.fillRect(food.x, food.y, food.width, food.height);
        
        context.fillStyle = '#ffff00';
        for (let pp of powerPellets.values()) {
            context.beginPath();
            context.arc(pp.x + pp.width/2, pp.y + pp.height/2, pp.width/2, 0, Math.PI*2);
            context.fill();
        }
        
        for (let ghost of ghosts.values()) {
            if (frightenedMode) {
                context.fillStyle = '#0000ff';
                context.fillRect(ghost.x, ghost.y, ghost.width, ghost.height);
            } else {
                context.drawImage(ghost.image, ghost.x, ghost.y, ghost.width, ghost.height);
            }
        }
        
        context.drawImage(pacman.image, pacman.x, pacman.y, pacman.width, pacman.height);
        
        context.fillStyle = 'rgba(0,0,0,.6)';
        context.fillRect(0, 0, boardWidth, 22);
        context.fillStyle = 'white';
        context.font      = '14px sans-serif';
        context.fillText(`♥ ${lives}   ${score} pkt`, 8, 15);
    }
    
    function move() {
        pacman.x += pacman.velocityX;
        pacman.y += pacman.velocityY;
        
        for (let wall of walls.values()) {
            if (collision(pacman, wall)) {
                pacman.x -= pacman.velocityX;
                pacman.y -= pacman.velocityY;
                break;
            }
        }
        
        for (let ghost of ghosts.values()) {
            if (collision(ghost, pacman)) {
                if (frightenedMode) {
                    score += 200;
                    window.arcade.setScore(score);
                    ghost.reset();
                } else {
                    lives--;
                    if (lives <= 0) { endGame(); return; }
                    resetPositions();
                }
            }

            if (ghost.y === tileSize * 9 &&
                ghost.direction !== 'U' && ghost.direction !== 'D') {
                ghost.updateDirection('U');
            }

            ghost.x += ghost.velocityX;
            ghost.y += ghost.velocityY;

            for (let wall of walls.values()) {
                if (collision(ghost, wall) ||
                    ghost.x <= 0 || ghost.x + ghost.width >= boardWidth) {
                    ghost.x -= ghost.velocityX;
                    ghost.y -= ghost.velocityY;
                    ghost.updateDirection(directions[Math.floor(Math.random() * 4)]);
                }
            }
        }
        
        let eaten = null;
        for (let food of foods.values()) {
            if (collision(pacman, food)) { eaten = food; score += 10; break; }
        }
        if (eaten) {
            foods.delete(eaten);
            window.arcade.setScore(score);
        }
        
        let eatenPP = null;
        for (let pp of powerPellets.values()) {
            if (collision(pacman, pp)) { eatenPP = pp; break; }
        }
        if (eatenPP) {
            powerPellets.delete(eatenPP);
            score += 50;
            window.arcade.setScore(score);
            activateFrightened();
        }
        
        if (foods.size === 0 && powerPellets.size === 0) {
            loadMap(images);
            resetPositions();
        }
    }

    function activateFrightened() {
        frightenedMode = true;
        if (frightenedTimer) clearTimeout(frightenedTimer);
        frightenedTimer = setTimeout(() => {
            frightenedMode = false;
        }, 8000); // 8 sekund
    }
    
    function gameLoop() {
        if (!running) return;
        move();
        draw();
        loopId = setTimeout(gameLoop, 50);
    }
    
    function endGame() {
        running = false;
        if (loopId) clearTimeout(loopId);
        clearInterval(timerInterval);
        if (frightenedTimer) clearTimeout(frightenedTimer);

        context.fillStyle = 'rgba(255,45,120,.25)';
        context.fillRect(0, 0, canvas.width, canvas.height);

        const duration = Math.floor((Date.now() - startedAt) / 1000);
        setTimeout(() => window.arcade.gameOver(score, duration), 400);
    }
    
    function handleKey(e) {
        if (!running) return;
        const map = {
            ArrowUp:'U', KeyW:'U', ArrowDown:'D', KeyS:'D',
            ArrowLeft:'L', KeyA:'L', ArrowRight:'R', KeyD:'R',
        };
        const dir = map[e.code];
        if (!dir) return;
        if (e.code.startsWith('Arrow')) e.preventDefault();

        pacman.updateDirection(dir);
        const imgMap = { U:images.pacmanUp, D:images.pacmanDown, L:images.pacmanLeft, R:images.pacmanRight };
        pacman.image = imgMap[pacman.direction];
    }

    document.addEventListener('keyup', handleKey);
    
    window.gameStart = function () {
        if (running) return;
        if (Object.keys(images).length > 0) startGame();
        else loadImages(imgs => { images = imgs; startGame(); });
    };

    function startGame() {
        score          = 0;
        lives          = 3;
        running        = true;
        frightenedMode = false;
        startedAt      = Date.now();

        loadMap(images);
        resetPositions();
        for (let ghost of ghosts.values())
            ghost.updateDirection(directions[Math.floor(Math.random() * 4)]);

        window.arcade.setScore(0);
        window.arcade.setTime(0);

        timerInterval = setInterval(() => {
            window.arcade.setTime(Math.floor((Date.now() - startedAt) / 1000));
        }, 1000);

        gameLoop();
    }
    
    function loadImages(callback) {
        const sources = {
            wall:'wall.png', blueGhost:'blueGhost.png', orangeGhost:'orangeGhost.png',
            pinkGhost:'pinkGhost.png', redGhost:'redGhost.png',
            pacmanUp:'pacmanUp.png', pacmanDown:'pacmanDown.png',
            pacmanLeft:'pacmanLeft.png', pacmanRight:'pacmanRight.png',
        };
        let loaded = 0;
        const total = Object.keys(sources).length;
        const imgs  = {};
        for (const [key, file] of Object.entries(sources)) {
            const img    = new Image();
            img.onload   = () => { if (++loaded === total) callback(imgs); };
            img.onerror  = () => { if (++loaded === total) callback(imgs); };
            img.src      = IMG_PATH + file;
            imgs[key]    = img;
        }
    }
    
    loadImages(imgs => { images = imgs; loadMap(images); draw(); });

}());
