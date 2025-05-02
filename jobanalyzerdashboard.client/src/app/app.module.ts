import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgSelectModule } from '@ng-select/ng-select';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { JobListComponentComponent } from './job-list-component/job-list-component.component';
import { JobDetailComponentComponent } from './job-detail-component/job-detail-component.component';
import { ProfileComponentComponent } from './profile-component/profile-component.component';
import { ApplicationHistoryComponentComponent } from './application-history-component/application-history-component.component';
import { N8nTestComponent } from './n8n-test/n8n-test.component';

// Auth Bileşenleri
import { LoginComponentComponent } from './login-component/login-component.component';
import { RegisterComponentComponent } from './register-component/register-component.component';
import { ForgotPasswordComponentComponent } from './forgot-password-component/forgot-password-component.component';
import { ResetPasswordComponentComponent } from './reset-password-component/reset-password-component.component';
import { ConfirmEmailComponentComponent } from './confirm-email-component/confirm-email-component.component';
import { UnauthorizedComponentComponent } from './unauthorized-component/unauthorized-component.component';
import { UserMenuComponentComponent } from './user-menu-component/user-menu-component.component';
import { ChangePasswordComponentComponent } from './change-password-component/change-password-component.component';

// Servisler
import { JobService } from './services/job.service';
import { ProfileService } from './services/profile.service';
import { ApplicationService } from './services/application.service';
import { WebhookService } from './services/webhook.service';
import { AuthService } from './services/auth.service';

// Interceptors
import { AuthInterceptor } from './interceptors/auth.interceptor';

@NgModule({
  declarations: [
    AppComponent,
    JobListComponentComponent,
    JobDetailComponentComponent,
    ProfileComponentComponent,
    ApplicationHistoryComponentComponent,
    N8nTestComponent,
    LoginComponentComponent,
    RegisterComponentComponent,
    ForgotPasswordComponentComponent,
    ResetPasswordComponentComponent,
    ConfirmEmailComponentComponent,
    UnauthorizedComponentComponent,
    UserMenuComponentComponent,
    ChangePasswordComponentComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgSelectModule,
    AppRoutingModule
  ],
  providers: [
    JobService,
    ProfileService,
    ApplicationService,
    WebhookService,
    AuthService,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
