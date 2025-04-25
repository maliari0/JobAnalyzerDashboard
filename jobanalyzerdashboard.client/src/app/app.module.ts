import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { JobListComponentComponent } from './job-list-component/job-list-component.component';
import { JobDetailComponentComponent } from './job-detail-component/job-detail-component.component';
import { ProfileComponentComponent } from './profile-component/profile-component.component';
import { ApplicationHistoryComponentComponent } from './application-history-component/application-history-component.component';
import { N8nTestComponent } from './n8n-test/n8n-test.component';

// Servisler
import { JobService } from './services/job.service';
import { ProfileService } from './services/profile.service';
import { ApplicationService } from './services/application.service';
import { WebhookService } from './services/webhook.service';

@NgModule({
  declarations: [
    AppComponent,
    JobListComponentComponent,
    JobDetailComponentComponent,
    ProfileComponentComponent,
    ApplicationHistoryComponentComponent,
    N8nTestComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule
  ],
  providers: [
    JobService,
    ProfileService,
    ApplicationService,
    WebhookService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
