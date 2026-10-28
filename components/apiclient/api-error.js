export default class ApiError extends Error {
    constructor(message, options) {
        super(message, options)
        this.name = 'ApiError'
    }
}