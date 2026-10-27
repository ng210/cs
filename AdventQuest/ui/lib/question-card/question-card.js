import _css from './question-card.css' with { type: "css" };
const _template = await fetch(import.meta.resolve('./question-card.template.html')).then(resp => resp.text())

export default class CardElement extends HTMLElement {
    static observedAttributes = ["day", "date", "state", "image", "type", "question", "options", "onclose"];
    static #states = ['passed', 'missed', 'active', 'opened', 'closed']
    static #template = (() => {
        let template = document.createElement('template')
        template.innerHTML = _template
        return template
    })()

    #isOpen = false
    get isOpen() { return this.#isOpen }

    #initialized = false


    // #elements = {

    // }
    // #day = 1
    // #state = 0
    // #src = ''
    // #type = 'select'
    // #question = ''
    // #onclose = () => {}

    // #text = null
    // #select = null
    // #labels = null
    // #options = null

    constructor(onClose) {
        super()
        console.log('ctor')
        // if (CardElement.#template == null) {
        //     CardElement.#template = document.createElement('template')
        //     CardElement.#template.innerHTML = _template
        // }
        // const html = _template
        //     .replace(
        //         /[<>"'&]]/g,
        //         (match, sign) => ({'<':'&lt;', '>':'&gt;', '"':'&quot;', '\'':'&#39;', '&':'&amp;'})[sign])
        //     .replace(
        //         /\{\{(\w+)\}\}/g,
        //         (match, name) => this.getAttribute(name) ?? '')

        // for (let name of CardElement.observedAttributes) {
        //     let el = this.#shadowRoot.querySelector(`[name="${name}"`)
        //     if (el) {
        //         this.#elements[name] = el
        //         let value = this.getAttribute(name)
        //         console.log(name, value)
        //         el.innerHTML = el.innerHTML.replace(`\{\{${name}\}\}`, value)
        //     }
        // }

        // this.#text = this.shadowRoot.querySelector('div.text')
        // this.#select = this.shadowRoot.querySelector('div.select')

        // this.#labels = this.#select.querySelectorAll('label')
        // this.#options = this.#select.querySelector('[name="answer"]')
    }

    connectedCallback() {
        console.log('connected')

        if (!this.#initialized) {
            this.attachShadow({ mode: "open" })
            this.shadowRoot.adoptedStyleSheets = [_css]
            const clone = CardElement.#template.cloneNode()
            clone.innerHTML
                .replace(
                    /[<>"'&]]/g,
                    (match, sign) => ({'<':'&lt;', '>':'&gt;', '"':'&quot;', '\'':'&#39;', '&':'&amp;'})[sign])
                .replace(
                    /{{(\w+)}}/g,
                    (match, name) => this.getAttribute(name) ?? '')
            this.shadowRoot.appendChild(clone)

            // const html = template
            //     .replace(
            //         /[<>"'&]]/g,
            //         (match, sign) => ({'<':'&lt;', '>':'&gt;', '"':'&quot;', '\'':'&#39;', '&':'&amp;'})[sign])
            //     .replace(
            //         /\{\{(\w+)\}\}/g,
            //         (match, name) => this.getAttribute(name) ?? '')
            // CardElement.#template = document.createElement('template')
            // CardElement.#template.innerHTML = html

            // for (let name of CardElement.observedAttributes) {
            //     let el = this.#shadowRoot.querySelector(`[name="${name}"`)
            //     if (el) {
            //         this.#elements[name] = el
            //         let value = this.getAttribute(name)
            //         console.log(name, value)
            //         el.innerHTML = el.innerHTML.replace(`\{\{${name}\}\}`, value)
            //     }
            // }
            // console.log(this.#elements)

            // let btn = shadowRoot.querySelector('button')
            // btn.addEventListener('click', event => this.close(event))

            // const text = shadowRoot.querySelector('div.text')
            // this.#text = text.querySelector('input')
            // const select = shadowRoot.querySelector('div.select')
            // this.#labels = select.querySelectorAll('label')
            // this.#options = select.querySelector('[name="answer"]')
        }
    }

    disconnectedCallback() {

    }

    connectedMoveCallback() {
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue == newValue) return

        switch (name) {
            // case 'day':
            //     this.#day = newValue
            //     break
            // case 'state':
            //     this.#state = newValue
            //     break
            // case 'src':
            //     this.#src = newValue
            //     break
            // case 'type':
            //     this.#type = newValue
            //     break
            // case 'question':
            //     this.#question = newValue
            //     break
            // case 'onclose':
            //     this.#onclose = newValue
            //     break
            // case 'options':
            //     let span = this.shadowRoot.querySelector('span')    
            //     for (let value of newValue.split(';')) {
            //         console.log(value )
            //     }
                
            //     break
            // case 'type':
                // this.#type = newValue
                // switch (this.#type) {
                //     case 'text':
                //         this.#text.style.display = 'block'
                //         this.#select.style.display = 'none'
                //         break
                //     case 'select':
                //         this.#select.style.display = 'block'
                //         this.#text.style.display = 'none'
                //         const answers = this.getAttribute('answers').split(';')
                //         const inputs = select.querySelectorAll('input')
                //         for (let i in answers) {
                //             this.#labels[i].innerHTML = answers[i]
                //             this.#options[i].value = answers[i]
                //         }
                //         break
                // }
                // break
        }
    }

    update() {
        
    }

    flip() {
        if (!this.#isOpen) {
            const rect = this.getBoundingClientRect()
            const cx = (window.innerWidth - (rect.left + rect.right)) / 2
            const cy = (window.innerHeight - (rect.top + rect.bottom)) / 2
            this.style.setProperty('--move-x', `${cx}px`)
            this.style.setProperty('--move-y', `${cy}px`)

            this.#isOpen = true
            this.classList.add('flipped')
            this.classList.remove('unflipped')
        }
    }

    unflip() {
        if (this.#isOpen) {
            this.#isOpen = false
            this.classList.add('unflipped')
            this.classList.remove('flipped')
        }
    }

    close(event) {
        let answer = ''
        // switch (this.#type) {
        //     case 'text':
        //         answer = this.#text.value
        //         break
        //     case 'select':
        //         answer = this.shadowRoot.querySelector('input[name="answer"]:checked')?.value
        //         break
        // }
        // this.#onClose({'id':this.getAttribute('day'), 'answer':answer})
        event.stopPropagation()
    }

    onClick(event) {
        if (!this.#isOpen) {
            this.flip()
            this.setPointerCapture(event.pointerId)
        } else {
            debugger
        }
    }
}

customElements.define('question-card', CardElement)