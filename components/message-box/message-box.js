import _css from './message-box.css' with { type: 'css' }
const _template = await (async () => {
    let dirPath = import.meta.url.slice(0, import.meta.url.lastIndexOf('/'))
    let path = `${dirPath}/message.template.html`
    return fetch(path).then(resp => resp.text())
})()

export default class MessageBox extends HTMLElement {

    static observedAttributes = []

    static #counter = 1

    static #template = (() => {
        const template = document.createElement('template')
        template.innerHTML = _template
        return template
    })()

    static #overlay = (() => {
        const overlay = document.createElement('div')
        Object.assign(overlay.style, {
            position: 'fixed',
            inset: '0',
            background: '#0008',
            zIndex: '9998',
            display: 'none'
        })
        document.body.appendChild(overlay)
        return overlay
    })()

    static #instance = null
    static #resolve = null

    // Gyakran használt DOM hivatkozások
    #header = null
    #body = null
    #buttons = null

    // Belső állapot elemei és egyéb objektumok (resources)
    #initialized = false
    
    constructor() {
        super()
        // Az állapot és az objektumok kezdeti értékének beállítása.
        // Attribútumokat és DOM elemeket ne használj, itt még nem érhetőek el!
    }

    connectedCallback() {
        if (!this.#initialized) {
            // A connectedCallback többször is meghívásra kerülhet,
            // de a DOM kezdeti beállításait csak egyszer végezzük el!
            this.#initializeDOM()
            this.#initialized = true
        }
        // A DOM aktualizálása a komponens megjelenését aktualizálja.
        this.#update()
    }

    disconnectedCallback() {
        // A komponensen kívüli objektumok törlése, felszabadítása:
        // számláló, időzítő, 'observer', eseménykezelők, stb.
    }

    attributeChangedCallback(name, oldValue, newValue) {
        // Csak a tényleges változást kezeljük!
        if (oldValue === newValue) return

        // Az attribútum értéke módosítja a belső állapot részét jelentő változókat.
        // Ezek a változók függetlenek a DOM-tól, ezért ezeket az attribútumokat
        // még a DOM kialakítása előtt is feldolgozhatjuk.

        // Vannak attribútumok, amelyek közvetlenül a DOM elemeire hatnak. Ezeket az #update
        // metódusban kezeljük, ahol a DOM elemek biztosan elérhetőek.
        this.#update()
    }

    #initializeDOM() {
        const root = this.attachShadow({ mode: 'open' })
        root.append(MessageBox.#template.content.cloneNode(true))
        root.adoptedStyleSheets = [_css]

        this.#header = root.querySelector('h1')
        this.#body = root.querySelector('div')
        this.#buttons = [...root.querySelectorAll('button')]
        for (let btn of this.#buttons) {
            btn.innerText = btn.className
            btn.addEventListener('click', event => this.#onClick(event))
        }
    }

    #update() {
        if (this.#initialized) {
            // DOM elemet értintő attribútum
            const title = this.getAttribute('title') ?? 'Message'
            const text = this.getAttribute('text') ?? 'Text'
            const types = (this.getAttribute('type') ?? 'Ok').split('|')

            // Az értékét a DOM elembe kell írni.
            for (let btn of this.#buttons) {
                btn.style.display = types.includes(btn.className) ? 'block' : 'none'
            }
            this.#header.textContent = title
            this.#body.textContent = text
            
            // További 'render' műveletek helye.
        }
    }

    #onClick(event) {
        const cls = event.target.className
        const ix = Object.keys(this.#buttons).indexOf(cls)
        const response = {
            id: this.id,
            title: this.getAttribute('title'),
            message: this.getAttribute('text'),
            response: ix,
            responseText: event.target.className
        }
        if (MessageBox.#instance == this) {
            MessageBox.#resolve(response)
        } else {
            this.dispatchEvent(new CustomEvent('message-box', {
                bubbles: true,
                composed: true,
                detail: response
            }));
        }
        this.show(false)
    }

    show(isVisible, x = -1, y = -1) {
        const rect = this.getBoundingClientRect()
        if (x == -1) x = (window.innerWidth - rect.width) / 2
        if (y == -1) y = (window.innerHeight - rect.height) / 2
        this.style.left = x + 'px'
        this.style.top = y + 'px'
        if (!isVisible) {
            this.remove()
            if (this == MessageBox.#instance) {
                MessageBox.#instance = null
                MessageBox.#overlay.style.display = 'none'
            }
        }
    }

    static show(text, title = 'Message', type = 'Ok', x = -1, y = -1) {
        const msgBox = document.createElement('message-box')
        msgBox.setAttribute('title', title)
        msgBox.setAttribute('text', text)
        msgBox.setAttribute('type', type)
        document.body.appendChild(msgBox)
        msgBox.id = 'mb' + (MessageBox.#counter.toString().padStart(4, '0'))
        MessageBox.#counter++
        msgBox.show(true, x, y)
        return msgBox
    }

    static showModal(text, title = 'Message', type = 'Ok', x = -1, y = -1) {
        if (MessageBox.#instance != null) {
            MessageBox.#instance.show(false)
        }
        MessageBox.#overlay.style.display = 'block'
        MessageBox.#instance = MessageBox.show(text, title, type, x, y)
        return new Promise(
            resolve => {
                MessageBox.#resolve = resolve
            })
    }
}

customElements.define('message-box', MessageBox)