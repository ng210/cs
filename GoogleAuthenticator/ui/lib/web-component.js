export default class WebComponent extends HTMLElement {
    #options = null
    #css = null
    #template = null

    #initialized = false

    constructor(options) {
        super()
        this.#options = { ...options }
    }

    async connectedCallback() {
        if (!this.#initialized) {
            if (!this.#css && this.#options.css) {
                this.#css = await fetch(this.#options.css).then(resp => resp.text())
            }
            if (!this.#template && this.#options.template) {
                this.#template = await fetch(this.#options.template).then(resp => resp.text())
            }
            this.initializeDom()
        }
    }

    initializeDom() {
        this.attachShadow({mode:'open'})
        this.shadowRoot.adoptedStyleSheets = [this.#css]
    }
}