(function () {
    'use strict';

    const IMG_PATH   = '/img/games/flappy/';
    const boardWidth  = 360;
    const boardHeight = 640;

    const canvas  = document.getElementById('gameCanvas');
    const context = canvas.getContext('2d');
    canvas.width  = boardWidth;
    canvas.height = boardHeight;
    
    const birdW = 34;
    const birdH = 24;
    const birdStartX = boardWidth / 8;
    const birdStartY = boardHeight / 2;

    let bird = { x: birdStartX, y: birdStartY, width: birdW, height: birdH };
    
    const pipeWidth  = 64;
    const pipeHeight = 512;
    let pipeArray    = [];
    
    const velocityX = -2;
    const gravity   = 0.1;
    let velocityY   = -4.5;
    
    let score     = 0;
    let running   = false;
    let gameOver  = false;
    let startedAt = null;
    let rafId     = null;
    let pipeTimer = null;
    let timerInterval = null;
    let images    = {};
    
    function loadImages(callback) {
        const sources = {
            bird:       IMG_PATH + 'flappybird.png',
            topPipe:    IMG_PATH + 'toppipe.png',
            bottomPipe: IMG_PATH + 'bottompipe.png',
            bg:         IMG_PATH + 'flappybirdbg.png',
        };
        let loaded = 0;
        const total = Object.keys(sources).length;
        const imgs  = {};
        for (const [key, src] of Object.entries(sources)) {
            const img   = new Image();
            img.onload  = () => { if (++loaded === total) callback(imgs); };
            img.onerror = () => { if (++loaded === total) callback(imgs); };
            img.src     = src;
            imgs[key]   = img;
        }
    }
    
    function placePipes() {
        if (!running || gameOver) return;

        const randomY     = -pipeHeight / 4 - Math.random() * (pipeHeight / 2);
        const openingSpace = boardHeight / 4;

        pipeArray.push({
            img: images.topPipe,
            x: boardWidth, y: randomY,
            width: pipeWidth, height: pipeHeight,
            passed: false
        });
        pipeArray.push({
            img: images.bottomPipe,
            x: boardWidth, y: randomY + pipeHeight + openingSpace,
            width: pipeWidth, height: pipeHeight,
            passed: false
        });
    }
    
    function detectCollision(a, b) {
        return a.x < b.x + b.width  && a.x + a.width  > b.x &&
            a.y < b.y + b.height && a.y + a.height > b.y;
    }
    
    function update() {
        if (!running) return;
        rafId = requestAnimationFrame(update);
        
        if (images.bg) {
            context.drawImage(images.bg, 0, 0, boardWidth, boardHeight);
        } else {
            context.fillStyle = '#70c5ce';
            context.fillRect(0, 0, boardWidth, boardHeight);
        }
        
        velocityY += gravity;
        bird.y     = Math.max(bird.y + velocityY, 0);
        context.drawImage(images.bird, bird.x, bird.y, bird.width, bird.height);
        
        if (bird.y > boardHeight) {
            endGame(); return;
        }
        
        for (let i = 0; i < pipeArray.length; i++) {
            const pipe = pipeArray[i];
            pipe.x += velocityX;
            context.drawImage(pipe.img, pipe.x, pipe.y, pipe.width, pipe.height);

            if (!pipe.passed && bird.x > pipe.x + pipe.width) {
                score += 0.5; // dwie rury = 1 punkt
                pipe.passed = true;
                window.arcade.setScore(Math.floor(score));
            }

            if (detectCollision(bird, pipe)) {
                endGame(); return;
            }
        }
        
        while (pipeArray.length > 0 && pipeArray[0].x < -pipeWidth) {
            pipeArray.shift();
        }
        
        context.fillStyle = 'white';
        context.font      = '45px sans-serif';
        context.fillText(Math.floor(score), 5, 45);
    }
    
    function endGame() {
        running  = false;
        gameOver = true;
        cancelAnimationFrame(rafId);
        clearInterval(pipeTimer);
        clearInterval(timerInterval);

        const duration = Math.floor((Date.now() - startedAt) / 1000);
        
        context.fillStyle = 'white';
        context.font      = '45px sans-serif';
        context.fillText(Math.floor(score), 5, 45);
        context.fillStyle = 'rgba(0,0,0,.45)';
        context.fillRect(0, 0, boardWidth, boardHeight);
        context.fillStyle = 'white';
        context.font      = '30px sans-serif';
        context.fillText('GAME OVER', boardWidth / 2 - 80, boardHeight / 2);

        setTimeout(() => window.arcade.gameOver(Math.floor(score), duration), 600);
    }
    
    function jump() {
        if (!running) return;
        velocityY = -4.5;
    }

    document.addEventListener('keydown', e => {
        if (e.code === 'Space' || e.code === 'ArrowUp' || e.code === 'KeyX') {
            e.preventDefault();
            jump();
        }
    });

    canvas.addEventListener('click',     jump);
    canvas.addEventListener('touchstart', e => { e.preventDefault(); jump(); }, { passive: false });
    
    window.gameStart = function () {
        if (running) return;
        
        bird.x    = birdStartX;
        bird.y    = birdStartY;
        pipeArray = [];
        score     = 0;
        velocityY = 0;
        gameOver  = false;
        running   = true;
        startedAt = Date.now();

        window.arcade.setScore(0);
        window.arcade.setTime(0);

        timerInterval = setInterval(() => {
            window.arcade.setTime(Math.floor((Date.now() - startedAt) / 1000));
        }, 1000);

        pipeTimer = setInterval(placePipes, 1500);

        if (Object.keys(images).length > 0) {
            rafId = requestAnimationFrame(update);
        } else {
            loadImages(imgs => {
                images = imgs;
                rafId  = requestAnimationFrame(update);
            });
        }
    };
    
    loadImages(imgs => {
        images = imgs;
        if (images.bg) {
            context.drawImage(images.bg, 0, 0, boardWidth, boardHeight);
        } else {
            context.fillStyle = '#70c5ce';
            context.fillRect(0, 0, boardWidth, boardHeight);
        }
        context.drawImage(images.bird, bird.x, bird.y, bird.width, bird.height);
    });

}());
