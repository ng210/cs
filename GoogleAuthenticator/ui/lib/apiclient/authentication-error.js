export default class AuthenticationError extends Error {
    constructor(message, options) {
        super(message, options)
        this.name = 'AuthenticationError'
    }
}
