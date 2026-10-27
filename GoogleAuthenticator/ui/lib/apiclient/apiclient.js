import registry from './registry.json' with { type: 'json' }
import IAuthenticator from "./iauthenticator.js"
import AuthenticationError from './authentication-error.js' 
import ApiError from './api-error.js' 

export default class ApiClient extends EventTarget {

    static defaultOptions = {
        'auth-type': 'google',
        'auth-dir': 'authenticators'
    }

    #apiUrl = null
    get apiUrl() { return this.#apiUrl }

    #authenticator = null
    #options = null
    get options() { return { ...this.#options } }

    static #loadDependency(dependency) {
        if (dependency.type === 'module') {
            return import(dependency.src)
        }

        return new Promise((resolve, reject) => {
            const script = document.createElement('script')
            script.src = dependency.src
            script.onload = resolve
            script.onerror = () =>
                reject(new Error(`Nem sikerült betölteni: ${dependency.src}`))
            document.head.append(script)
        })
    }

    #prepareBody(body, headers) {
        if (body !== null) {
            if (ArrayBuffer.isView(body)) {
            return new Blob([
                body.buffer.slice(
                    body.byteOffset,
                    body.byteOffset + body.byteLength
                )])
            }
        }

        if (body instanceof Blob
            || body instanceof FormData
            || body instanceof URLSearchParams
            || body instanceof ArrayBuffer) {
            return body
        }
        
        if (typeof body === 'object') {
            if (!headers.has('Content-Type')) {
                headers.set('Content-Type', 'application/json')
            }
            return JSON.stringify(body)
        }

        return body
    }

    async #getAuthenticator() {
        const authConfig = registry[this.#options['auth-type']]
        if (!authConfig) {
            throw new Error("Unsupported authenticator")
        }

        // függőségi szkriptek vagy modulok betöltése
        const loading = (authConfig.dependencies ?? [])
            .map(src => ApiClient.#loadDependency(src))
        await Promise.all(loading)

        // Authenticator betöltése
        const path = `${this.#options['auth-dir']}/${authConfig.authenticator}`        
        const { default: Authenticator } = await import(path)
        this.#authenticator = new Authenticator(this)
        if (!(this.#authenticator instanceof IAuthenticator)) {
            throw new Error("Invalid authenticator")
        }
    }

    constructor(url, options = {}) {
        super()
        this.#apiUrl = url
        this.#options = {
            ...ApiClient.defaultOptions,
            ...options
        }
    }

    async request(path, options = {}) {
        let authOptions = {}
        const shouldAuthorize = options.shouldAuthorize !== false
        if (shouldAuthorize) {
            try {
                if (!this.#authenticator) {
                    await this.#getAuthenticator()
                }
                // hitelesítés kérése
                authOptions = await this.#authenticator.authorizeRequest()
            } catch (error) {
                throw new AuthenticationError(error.message, { cause: error })
            }
        }

        const fetchOptions = {
            method: options.method ?? 'get',
            headers: new Headers(options.headers),
            mode: authOptions.mode ?? options.mode,
            credentials: authOptions.credentials ?? options.credentials,
            cache: authOptions.cache ?? options.cache,
            redirect: authOptions.redirect ?? options.redirect,
            referrer: authOptions.referrer ?? options.referrer,
            integrity: authOptions.integrity ?? options.integrity,
            keepalive: authOptions.keepalive ?? options.keepalive,
            signal: options.signal
        }
        new Headers(authOptions.headers).forEach((value, key) => {
            fetchOptions.headers.set(key, value)
        })
        fetchOptions.body = this.#prepareBody(options.body, fetchOptions.headers)

        let response
        try {
            response = await fetch(`${this.#apiUrl}${path}`, fetchOptions)
        } catch (error) {
            throw new ApiError(`Hálózati hiba: ${error.message}`, { cause: error })
        }

        if (response.status === 401 && shouldAuthorize) {
            try {
                await this.#authenticator.clearAuthentication()
            } catch(err) {
                // A helyi takarítás hibája nem írhatja felül az API válaszát.
                console.error(err)
            }
        }

        return response
    }

    async getContent(response, responseType = 'auto') {
        if (response.status === 401) {
            throw new ApiError('A kérés hitelesítése sikertelen')
        }

        if (!response.ok) {
            throw new ApiError(`HTTP hiba: ${response.status} ${response.statusText}`)
        }

        if (response.status === 204) {
            return null
        }

        if (responseType === 'auto') {
            const contentType = response.headers.get('Content-Type')?.split(';')[0].trim().toLowerCase()

            if (contentType === 'application/json' || contentType?.endsWith('+json')) {
                responseType = 'json'
            } else if (contentType?.startsWith('text/')) {
                responseType = 'text'
            } else {
                responseType = 'blob'
            }
        }

        try {
            switch (responseType) {
                case 'json':
                    return await response.json()
                case 'text':
                    return await response.text()
                case 'blob':
                    return await response.blob()
                case 'arrayBuffer':
                    return await response.arrayBuffer()
                default:
                    throw new ApiError(
                        `Ismeretlen responseType: ${responseType}`
                    )
            }
        } catch (error) {
            throw new ApiError(
                'A válasz tartalmának feldolgozása sikertelen',
                { cause: error }
            )
        }
    }

    async get(path, options = {}) {
        const response = await this.request(path, {
            ...options,
            method: 'get'
        })
        return this.getContent(response, options.responseType)
    }

    async post(path, data, options = {}) {
        const response = await this.request(path, {
            ...options,
            method: 'post',
            body: data
        })
        return this.getContent(response, options.responseType)
    }

    async put(path, data, options = {}) {
        const response = await this.request(path, {
            ...options,
            method: 'put',
            body: data
        })
        return this.getContent(response, options.responseType)
    }

    async delete(path, options = {}) {
        const response = await this.request(path, {
            ...options,
            method: 'delete'
        })
        return this.getContent(response, options.responseType)
    }
}