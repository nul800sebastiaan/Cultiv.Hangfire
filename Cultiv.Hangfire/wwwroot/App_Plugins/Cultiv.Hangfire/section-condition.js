import { UmbConditionBase } from '@umbraco-cms/backoffice/extension-registry';

export class HangfireSectionCondition extends UmbConditionBase {
    async permitted() {
        try {
            const response = await fetch('/umbraco/management/api/v1/cultiv-hangfire/config');
            const config = await response.json();
            return config.useStandaloneSection === true;
        } catch (error) {
            console.error('Failed to load Hangfire configuration:', error);
            return false;
        }
    }
}

export default HangfireSectionCondition;
