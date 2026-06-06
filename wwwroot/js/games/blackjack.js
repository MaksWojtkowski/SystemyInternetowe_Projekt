(function () {
    'use strict';

    const CARD_PATH = '/img/games/blackjack/cards/';
    
    let dealerSum      = 0;
    let yourSum        = 0;
    let dealerAceCount = 0;
    let yourAceCount   = 0;
    let hidden;
    let deck;
    let canHit    = true;
    let startedAt = null;
    let gamesWon  = 0;
    let running   = false;
    
    let elDealerCards, elYourCards, elDealerSum, elYourSum;
    let elResults, elHit, elStay, elNewRound, elHiddenCard;

    function buildUI() {
        const container = document.getElementById('bjContainer');
        if (!container) return;

        container.innerHTML = `
            <div class="bj-table">
                <div class="bj-section">
                    <div class="bj-label">KRUPIER <span class="bj-sum" id="bjDealerSum">?</span></div>
                    <div class="bj-cards" id="bjDealerCards"></div>
                </div>

                <div class="bj-divider"></div>

                <div class="bj-section">
                    <div class="bj-label">TY <span class="bj-sum" id="bjYourSum">0</span></div>
                    <div class="bj-cards" id="bjYourCards"></div>
                </div>

                <div class="bj-result" id="bjResult"></div>

                <div class="bj-buttons">
                    <button class="bj-btn bj-btn--hit"  id="bjHit">HIT</button>
                    <button class="bj-btn bj-btn--stay" id="bjStay">STAY</button>
                    <button class="bj-btn bj-btn--new"  id="bjNewRound" style="display:none">
                        NOWA RUNDA
                    </button>
                </div>
            </div>
        `;

        elDealerCards = document.getElementById('bjDealerCards');
        elYourCards   = document.getElementById('bjYourCards');
        elDealerSum   = document.getElementById('bjDealerSum');
        elYourSum     = document.getElementById('bjYourSum');
        elResults     = document.getElementById('bjResult');
        elHit         = document.getElementById('bjHit');
        elStay        = document.getElementById('bjStay');
        elNewRound    = document.getElementById('bjNewRound');

        elHit.addEventListener('click', hit);
        elStay.addEventListener('click', stay);
        elNewRound.addEventListener('click', newRound);
    }
    
    function buildDeck() {
        const values = ['A','2','3','4','5','6','7','8','9','10','J','Q','K'];
        const types  = ['C','D','H','S'];
        deck = [];
        for (const t of types)
            for (const v of values)
                deck.push(`${v}-${t}`);
    }

    function shuffleDeck() {
        for (let i = 0; i < deck.length; i++) {
            const j   = Math.floor(Math.random() * deck.length);
            const tmp = deck[i];
            deck[i]   = deck[j];
            deck[j]   = tmp;
        }
    }

    function getValue(card) {
        const val = card.split('-')[0];
        if (isNaN(val)) return val === 'A' ? 11 : 10;
        return parseInt(val);
    }

    function checkAce(card)  { return card[0] === 'A' ? 1 : 0; }

    function reduceAce(sum, aces) {
        while (sum > 21 && aces > 0) { sum -= 10; aces--; }
        return sum;
    }
    
    function makeCardImg(cardName, hidden = false) {
        const img = document.createElement('img');
        img.src   = hidden
            ? CARD_PATH + 'BACK.png'
            : CARD_PATH + cardName + '.png';
        img.className  = 'bj-card';
        img.dataset.card = cardName;
        return img;
    }
    
    function startRound() {
        dealerSum = yourSum = dealerAceCount = yourAceCount = 0;
        canHit    = true;
        elDealerCards.innerHTML = '';
        elYourCards.innerHTML   = '';
        elResults.textContent   = '';
        elHit.style.display     = '';
        elStay.style.display    = '';
        elNewRound.style.display = 'none';
        elDealerSum.textContent = '?';
        elYourSum.textContent   = '0';

        buildDeck();
        shuffleDeck();
        
        hidden = deck.pop();
        dealerSum      += getValue(hidden);
        dealerAceCount += checkAce(hidden);
        
        const hiddenImg = makeCardImg(hidden, true);
        hiddenImg.id    = 'bjHiddenCard';
        elDealerCards.append(hiddenImg);
        
        while (dealerSum < 17) {
            const card = deck.pop();
            dealerSum      += getValue(card);
            dealerAceCount += checkAce(card);
            elDealerCards.append(makeCardImg(card));
        }
        
        for (let i = 0; i < 2; i++) {
            const card = deck.pop();
            yourSum      += getValue(card);
            yourAceCount += checkAce(card);
            elYourCards.append(makeCardImg(card));
        }
        elYourSum.textContent = reduceAce(yourSum, yourAceCount);
    }
    
    function hit() {
        if (!canHit || !running) return;

        const card = deck.pop();
        yourSum      += getValue(card);
        yourAceCount += checkAce(card);
        elYourCards.append(makeCardImg(card));
        elYourSum.textContent = reduceAce(yourSum, yourAceCount);

        if (reduceAce(yourSum, yourAceCount) > 21) {
            canHit = false;
            endRound();
        }
    }
    
    function stay() {
        if (!running) return;
        canHit = false;
        endRound();
    }
    
    function endRound() {
        const dSum = reduceAce(dealerSum, dealerAceCount);
        const ySum = reduceAce(yourSum,   yourAceCount);
        
        const hiddenEl = document.getElementById('bjHiddenCard');
        if (hiddenEl) hiddenEl.src = CARD_PATH + hidden + '.png';

        elDealerSum.textContent = dSum;
        elYourSum.textContent   = ySum;

        let message = '';
        let won     = false;

        if      (ySum > 21)             { message = '💀 PRZEGRANA — przekroczyłeś 21!'; }
        else if (dSum > 21)             { message = '🏆 WYGRANA — krupier przekroczył 21!'; won = true; }
        else if (ySum === dSum)         { message = '🤝 REMIS'; }
        else if (ySum > dSum)           { message = '🏆 WYGRANA!'; won = true; }
        else                            { message = '💀 PRZEGRANA'; }

        if (won) {
            gamesWon++;
            window.arcade.setScore(gamesWon);
        }
        else{
            gamesWon = 0;
            window.arcade.setScore(gamesWon);
        }

        elResults.textContent    = message;
        elResults.className      = `bj-result ${won ? 'bj-win' : 'bj-lose'}`;
        elHit.style.display      = 'none';
        elStay.style.display     = 'none';
        elNewRound.style.display = '';
    }
    
    function newRound() {
        startRound();
    }
    
    function endGame() {
        running = false;
        const duration = Math.floor((Date.now() - startedAt) / 1000);
        window.arcade.gameOver(gamesWon, duration);
    }
    
    window.gameStart = function () {
        if (running) return;
        running   = true;
        gamesWon  = 0;
        startedAt = Date.now();

        window.arcade.setScore(0);
        window.arcade.setTime(0);
        
        const timerInterval = setInterval(() => {
            if (!running) { clearInterval(timerInterval); return; }
            window.arcade.setTime(Math.floor((Date.now() - startedAt) / 1000));
        }, 1000);

        buildUI();
        startRound();
        
        const endBtn = document.createElement('button');
        endBtn.className   = 'bj-btn bj-btn--end';
        endBtn.textContent = 'ZAKOŃCZ SESJĘ';
        endBtn.style.marginTop = '1rem';
        endBtn.addEventListener('click', () => {
            clearInterval(timerInterval);
            endGame();
        });
        document.querySelector('.bj-buttons')?.after(endBtn);
    };
    
    const container = document.getElementById('bjContainer');
    if (container) {
        container.innerHTML = `
            <div style="text-align:center;padding:2rem;
                        font-family:'Press Start 2P',monospace;
                        font-size:.6rem;color:#00e5ff;opacity:.4">
                [ BLACKJACK ]<br>
                <span style="font-size:.45rem;color:#6b6b85">Naciśnij START żeby zacząć</span>
            </div>
        `;
    }

}());
