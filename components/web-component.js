export default class WebComponent extends HTMLElement {
    static observedAttributes = []

    #options = null
    #css = null
    #template = null
    get template() { return this.#template }
    #templateNode = null
    get templateNode() { return this.#templateNode }

    #initialized = false

    constructor(options) {
        super()
        this.#options = { ...options }
    }

    async connectedCallback() {
        if (!this.#initialized) {
            if (!this.#css && this.#options.css) {
                this.#css = (await import(this.#options.css, { with: { type: 'css' } })).default
            }
            if (!this.#template && this.#options.template) {
                this.#template = await fetch(this.#options.template).then(resp => resp.text())
                this.#templateNode = document.createElement('template')
                this.#templateNode.innerHTML = this.#template
            }
            // initialize DOM
            this.attachShadow({mode:'open'})
            this.shadowRoot.adoptedStyleSheets = [this.#css]
            this.initializeDom()
        }

        this.update()
    }

    disconnectedCallback() {

    }

    initializeDom() {
        let node = this.templateNode.content.cloneNode(true)
        // replace placeholders
        this.shadowRoot.append(node)
    }

    update() {

    }

    attributeChangedCallback(name, oldValue, newValue) {

    }

}