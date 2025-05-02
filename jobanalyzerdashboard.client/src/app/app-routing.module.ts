import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
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
import { ChangePasswordComponentComponent } from './change-password-component/change-password-component.component';

// Guards
import { AuthGuard } from './guards/auth.guard';

const routes: Routes = [
  // Ana Sayfalar
  { path: '', component: JobListComponentComponent },
  { path: 'job/:id', component: JobDetailComponentComponent },

  // Kimlik Doğrulama Gerektiren Sayfalar
  {
    path: 'profile',
    component: ProfileComponentComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'history',
    component: ApplicationHistoryComponentComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'change-password',
    component: ChangePasswordComponentComponent,
    canActivate: [AuthGuard]
  },

  // Kimlik Doğrulama Sayfaları
  { path: 'login', component: LoginComponentComponent },
  { path: 'register', component: RegisterComponentComponent },
  { path: 'forgot-password', component: ForgotPasswordComponentComponent },
  { path: 'reset-password', component: ResetPasswordComponentComponent },
  { path: 'confirm-email', component: ConfirmEmailComponentComponent },

  // Diğer Sayfalar
  { path: 'unauthorized', component: UnauthorizedComponentComponent },
  { path: 'n8n-test', component: N8nTestComponent },

  // Bulunamayan Sayfalar
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
