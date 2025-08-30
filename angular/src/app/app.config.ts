import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { appRoutes } from './app.routes';
import { APP_ROUTE_PROVIDER } from './route.provider';
import { provideAbpCore, withOptions } from '@abp/ng.core';
import { environment } from '../environments/environment';
import { registerLocale } from '@abp/ng.core/locale';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideTenantManagementConfig } from '@abp/ng.tenant-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { provideLogo, withEnvironmentOptions } from '@volo/ngx-lepton-x.core';
import { ThemeLeptonXModule } from '@abp/ng.theme.lepton-x';
import { SideMenuLayoutModule } from '@abp/ng.theme.lepton-x/layouts';
import { AccountLayoutModule } from '@abp/ng.theme.lepton-x/account';
import { ThemeSharedModule, withValidationBluePrint, provideAbpThemeShared } from '@abp/ng.theme.shared';

export const appConfig: ApplicationConfig = {
    providers: [
        provideRouter(appRoutes),
        APP_ROUTE_PROVIDER,
        provideAbpCore(withOptions({
            environment,
            registerLocaleFn: registerLocale(),
        })),
        provideAbpOAuth(),
        provideSettingManagementConfig(),
        provideAccountConfig(),
        provideIdentityConfig(),
        provideTenantManagementConfig(),
        provideFeatureManagementConfig(),
        provideAnimations(),
        provideLogo(withEnvironmentOptions(environment)),
        importProvidersFrom(ThemeLeptonXModule.forRoot(), SideMenuLayoutModule.forRoot(), AccountLayoutModule.forRoot(), ThemeSharedModule),
        provideAbpThemeShared(withValidationBluePrint({
            wrongPassword: 'Please choose 1q2w3E*'
        })),
        provideHttpClient(withInterceptorsFromDi()),
    ]
};
