import WebApp from '../../components/web-app/web-app.js'
import appSettings from './appsettings.json' with { type: 'json' }
// import UserCard from '../../components/user-card/user-card.js'
import MessageBox from '../../components/message-box/message-box.js'

class AdventApp extends WebApp {
    constructor() {
        super(appSettings)
    }

    async connectedCallback() {
        await super.connectedCallback()

        try {
            // load components
            // - user-card
            // - question-card
            // - advent-calendar


            // const user = await this.apiClient.get('/users/me')
            // this.views.login.style.display = 'none'
            // this.views.content.style.display = ''
            // console.log('Bejelentkezett felhasználó:', user)
            // this.views['user-card'].user = user
        
        } catch (error) {
            this.views.login.style.display = 'none'
            this.views.content.style.display = 'none'
            await MessageBox.showModal(error.message, 'Hiba', 'Ok')
        }
    }

    initializeDom() {
        super.initializeDom()

        // this.views['user-card'] = this.shadowRoot.querySelector('user-card')
        // this.shadowRoot.querySelector('section.content-view>button')
        //     .addEventListener('click', () => this.logout())
    }
}

customElements.define('advent-app', AdventApp)