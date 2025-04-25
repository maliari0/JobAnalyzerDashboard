import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { JobListComponentComponent } from './job-list-component/job-list-component.component';
import { JobDetailComponentComponent } from './job-detail-component/job-detail-component.component';
import { ProfileComponentComponent } from './profile-component/profile-component.component';
import { ApplicationHistoryComponentComponent } from './application-history-component/application-history-component.component';
import { N8nTestComponent } from './n8n-test/n8n-test.component';

const routes: Routes = [
  { path: '', component: JobListComponentComponent },
  { path: 'job/:id', component: JobDetailComponentComponent },
  { path: 'profile', component: ProfileComponentComponent },
  { path: 'history', component: ApplicationHistoryComponentComponent },
  { path: 'n8n-test', component: N8nTestComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
