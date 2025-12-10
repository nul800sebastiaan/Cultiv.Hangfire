const template = document.createElement('template');
template.innerHTML = `
            <style>           
                .hangfire-wrapper {
                     margin: -20px -20px;
                }
                .hangfire-content { 
                    display: block;
                    border: none;
                    height: 100vh;
                    width: 100%;
                }
            </style>

            <uui-box class="hangfire-wrapper">                
                <iframe name="hangfireIframe" class="hangfire-content" id="Hangfire" src="/cultiv/hangfire/"></iframe>
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

