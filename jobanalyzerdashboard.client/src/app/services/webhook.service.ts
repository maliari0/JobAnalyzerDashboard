import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface WebhookTestResult {
  success: boolean;
  message: string;
  timestamp: string;
}

export interface JobIntakeModel {
  title: string;
  description: string;
  company?: string;
  location?: string;
  employmentType: string;
  salary: string;
  contactEmail: string;
  companyWebsite: string;
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebhookService {
  private apiUrl = '/api/webhook';

  constructor(private http: HttpClient) { }

  testWebhook(): Observable<WebhookTestResult> {
    return this.http.get<WebhookTestResult>(`${this.apiUrl}/test`);
  }

  sendJobIntake(job: JobIntakeModel): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/job-intake`, job);
  }

  sendWebhookData(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }
}
