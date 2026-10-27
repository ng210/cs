import CardElement from './lib/question-card/question-card.js'

let _cards = null
let _selectedCard = null

window.resetCards = function() {
    for (let card of _cards) {
        card.unflip()
    }
}

async function createCard(data) {
    const mainElement = document.getElementsByTagName('main')[0]
    let card = new CardElement(onClose)
    for (let name in data) {
        card.setAttribute(name, data[name])
    }
    card.setAttribute('onclose', onClose)
    mainElement.appendChild(card)
    return card
}

async function main() {
    const cardData = await fetch('cards.json').then(resp => resp.json())
    let day = 1
    for (let data of cardData) {
        data.day = day++
        await createCard(data)
        if (day == 5) break
    }

    _cards = document.getElementsByTagName('my-card')
    addEventListener('click', onClick)
}

function onClick(event) {
    if (event.target instanceof CardElement) {
        if (_selectedCard != event.target) {
            if (_selectedCard != null) _selectedCard.unflip()
            _selectedCard = event.target
            _selectedCard.flip()
        }
    } else {
        if (_selectedCard != null) {
            _selectedCard.unflip()
            _selectedCard = null
        }
    }
}

async function onClose(data) {
    _selectedCard.unflip()
    _selectedCard = null
}

main()
