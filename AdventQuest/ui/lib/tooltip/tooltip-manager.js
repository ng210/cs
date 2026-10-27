import _css from './tooltip.css' with { type: 'css' }

export default class TooltipManager extends HTMLElement {

    static observedAttributes = ["delay"]

    #tooltip
    #target = null
    #timer = null
    #title = null
    #x = 0
    #y = 0

    constructor() {
        super()
    }

    connectedCallback() {
        if (!this.#tooltip) {
            const root = this.attachShadow({ mode: "open" })
            root.adoptedStyleSheets = [_css]
            root.innerHTML = '<div class="tooltip" role="tooltip"></div>'
            this.#tooltip = root.querySelector(".tooltip")
        }

        document.addEventListener("pointerover", this.#onPointerOver)
        document.addEventListener("pointerout", this.#onPointerOut)
    }

    disconnectedCallback() {
        document.removeEventListener("pointerover", this.#onPointerOver)
        document.removeEventListener("pointerout", this.#onPointerOut)

        this.#clear()
    }

    attributeChangedCallback(name, oldValue, newValue) {
        if (oldValue === newValue)
            return

        if (name === "delay" && this.#target) {
            this.#schedule(this.#x, this.#y)
        }
    }

    #onPointerOver = event => {
        const element = event.target

        if (!(element instanceof Element))
            return

        if (this.#target && (element === this.#target || this.#target.contains(element))) {
            return
        }

        this.#clear()
        const target = element.closest("[tooltip]")
        if (!target || !target.getAttribute("tooltip")?.trim()) {
            return
        }

        this.#target = target
        this.#title = target.getAttribute("tooltip")

        this.#schedule(event.clientX, event.clientY)
    }

    #onPointerOut = event => {
        if (!this.#target || !this.#target.contains(event.target))
            return

        if (event.relatedTarget instanceof Node &&
            this.#target.contains(event.relatedTarget)) {
            return
        }

        this.#clear()
    }

    #schedule(x, y) {
        this.#x = x
        this.#y = y
        clearTimeout(this.#timer)
        this.#tooltip.classList.remove("visible")
        const delay = Number(this.getAttribute("delay") ?? 5000)
        this.#timer = setTimeout(() => {
            if (!this.#target)
                return

            this.#tooltip.textContent = this.#title
            this.#setPosition(x, y)
            this.#tooltip.classList.add("visible")
        }, delay)
    }

    #setPosition(x, y) {
        const offset = 12;

        this.#tooltip.style.left = "0px"
        this.#tooltip.style.top = "0px"

        const rect = this.#tooltip.getBoundingClientRect()

        x += offset
        y += offset

        if (x + rect.width > window.innerWidth - 4)
            x -= rect.width + offset;

        if (y + rect.height > window.innerHeight - 4)
            y -= rect.height + offset;

        this.#tooltip.style.left = `${Math.max(4, x)}px`;
        this.#tooltip.style.top = `${Math.max(4, y)}px`;
    }

    #clear() {
        clearTimeout(this.#timer)
        this.#timer = null

        this.#tooltip?.classList.remove("visible")

        this.#target = null
        this.#title = null
    }
}

customElements.define("tooltip-manager", TooltipManager)