const template = document.createElement('template');
template.innerHTML = `
            <style>
                .hangfireWrapper {
                    margin: -20px -20px;
                }

                .hangfireContent {
                    position: absolute;
                    width: 100% !important;
                    height: 100%;
                }
                uui-box {
                    height: 100%;
                }
            </style>

            <uui-box>
                <div class="hangfireWrapper">
                    <iframe name="hangfireIframe" class="hangfireContent" id="Hangfire"
                            src="/umbraco/hangfire/"></iframe>
                </div>
            </uui-box>
`;


export default class CultivHangfireDashboardElement extends HTMLElement {

    constructor() {
        super();
        this.attachShadow({ mode: 'open' });
        this.shadowRoot.appendChild(template.content.cloneNode(true));
    }
}

customElements.define('cultiv-hangfire-dashboard', CultivHangfireDashboardElement);

