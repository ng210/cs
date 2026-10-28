import WebComponent from '../web-component.js'
import ApiClient from '../apiclient/apiclient.js'

export default class WebApp extends WebComponent {
    static #defaultSettings = {
        'api-url': '',
        'css': './style.css',
        'template': './template.html',
        'auth-type': 'google',
        'auth-dir': '../authenticators',
        'views': {
            'login': '.login-view',
            'content': '.content-view'
        }
    }
    
    #apiClient = null
    get apiClient() { return this.#apiClient }
    #componentApi = null
    #views = { }
    get views() { return this.#views }
    #settings = null
    get settings() { return this.#settings }

    #loginRequiredHandler = event => {
        this.#views.content.style.display = 'none'
        this.#views.login.style.display = 'block'
        event.detail.sender.initializeAuthenticationView(this.#views.login)
    }

    constructor(settings) {
        settings = {
            ...WebApp.#defaultSettings,
            ...settings
        }

        settings['api-url'] = new URL(settings['api-url'], document.location.href).href,
        settings['auth-dir'] = new URL(settings['auth-dir'], import.meta.url).href,
        settings['css'] = new URL(settings['css'], document.location.href).href,
        settings['template'] = new URL(settings['template'], document.location.href).href

        super(settings)
        this.#settings = settings
        console.log(this.#settings['api-url'])
        console.log(this.#settings['auth-dir'])
        console.log(this.#settings['css'])
        console.log(this.#settings['template'])
        this.#apiClient = new ApiClient(settings)
        this.#componentApi = settings.componentApi ?? settings.apiUrl + '/components'
        this.#settings = {
            ...this.#settings,
            ...settings
        }
    }

    disconnectedCallback() {
        document.removeEventListener(this.#loginRequiredHandler)
    }

    initializeDom() {
        super.initializeDom()
        for (let key in this.settings.views) {
            this.#views[key] = this.shadowRoot.querySelector(this.settings.views[key])
        }

        if (!this.#views.login) throw new Error('Login view missing!')
        if (!this.#views.content) throw new Error('Content view missing!')

        // handle authentication
        document.addEventListener('login-required', this.#loginRequiredHandler)
    }

    update() {

    }

    attributeChangedCallback(name, oldValue, newValue) {

    }
}
