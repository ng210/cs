import _css from './user-card.css' with {type:'css'}
export default class UserCard extends HTMLElement {
    #user = {}
    set user(data) {
        this.#user = data
        this.#update()
    }

    #initialized = false
    #shadow = null

    static #templatePromise = null
    async getTemplate() {
        if (!UserCard.#templatePromise) {
            let dirPath = import.meta.url.slice(0, import.meta.url.lastIndexOf('/'))
            let path = `${dirPath}/user-card.template.html`
            UserCard.#templatePromise = fetch(path).then(resp => resp.text())
        }
        return UserCard.#templatePromise
    }

    constructor(user) {
        super()
        this.#user = user??{}
    }

    connectedCallback() {
        if (!this.#initialized) {
            this.#initializeDom()
        }
        this.#update()
    }

    #initializeDom() {
        this.#shadow = this.attachShadow({ mode: 'open' })
        this.#shadow.adoptedStyleSheets = [_css]
        this.#initialized = true
    }

    async #update() {
        if (!this.#initialized) return
        const content = (await this.getTemplate()).replaceAll(
            /{{(\w+)}}/g,
            (m, key) => {
                let value = this.#user[key]??'n.a.'
                if (key == 'profileImageUrl' && value == 'n.a.') {
                    value = 'assets/profil.png'
                }
                return value
            })
        this.#shadow.innerHTML = content
    }
}

customElements.define('user-card', UserCard)