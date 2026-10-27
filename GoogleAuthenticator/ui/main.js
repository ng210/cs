import ApiClient from './lib/apiclient/apiclient.js'
import MessageBox from './components/messagebox/message-box.js'
import AppData from './app.data.json' with { type: 'json'}

const _content = document.querySelector('main > section.content')
const _login = document.querySelector('main > section.log')
const _userCard = _content.querySelector('user-card')

const _apiClient = ( ()=> {
    let dirPath = import.meta.url.slice(0, import.meta.url.lastIndexOf('/'))
    let path = `${dirPath}/authenticators`
    let options = {
        'auth-type': 'google',
        'auth-dir': path,
        ...AppData.web
    }
    return new ApiClient(AppData.web.api_url, options)
})()

// A hitelesítő komponens által kért bejelentkezési nézet.
document.body.addEventListener('login-required', event => {
	_content.style.display = 'none'
	_login.innerHTML = event.detail.view
	_login.style.display = 'flex'
    let container = _login.querySelector('#gis-container')
    event.detail.sender.initializeAuthenticationView(container)
})

// Az aktuális felhasználó lekérése.
try {
    const user = await _apiClient.get('/users/me')

    _login.style.display = 'none'
    _content.style.display = 'block'

    console.log('Bejelentkezett felhasználó:', user)

    _userCard.user = user

} catch (error) {
    _login.style.display = 'none'
    _content.style.display = 'none'
    await MessageBox.showModal(error.message, 'Hiba', 'Ok')
}
