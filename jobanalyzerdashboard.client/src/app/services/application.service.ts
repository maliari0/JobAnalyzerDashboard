import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Application } from '../models/application.model';

export interface ApplicationFilter {
  jobId?: number;
  status?: string;
  appliedMethod?: string;
  isAutoApplied?: boolean;
  fromDate?: Date;
  toDate?: Date;
  sortBy?: string;
  sortDirection?: string;
}

export interface ApplicationStats {
  totalApplications: number;
  pendingApplications: number;
  acceptedApplications: number;
  rejectedApplications: number;
  interviewApplications: number;
  autoAppliedCount: number;
  manualAppliedCount: number;
  applicationMethodBreakdown: { method: string, count: number }[];
  statusBreakdown: { status: string, count: number }[];
  recentApplications: { id: number, jobId: number, jobTitle: string, appliedDate: string, status: string }[];
}

export interface ApplicationCreateModel {
  jobId: number;
  appliedMethod?: string;
  message?: string;
  isAutoApplied?: boolean;
  cvAttached?: boolean;
}

export interface StatusUpdateModel {
  status: string;
  details?: string;
}

export interface AutoApplyModel {
  jobId: number;
  message?: string;
  telegramNotification?: string;
  notionPageId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {
  private apiUrl = '/api/application';

  constructor(private http: HttpClient) { }

  getApplications(filter?: ApplicationFilter): Observable<Application[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.jobId) params = params.set('jobId', filter.jobId.toString());
      if (filter.status) params = params.set('status', filter.status);
      if (filter.appliedMethod) params = params.set('appliedMethod', filter.appliedMethod);
      if (filter.isAutoApplied !== undefined) params = params.set('isAutoApplied', filter.isAutoApplied.toString());
      if (filter.fromDate) params = params.set('fromDate', filter.fromDate.toISOString());
      if (filter.toDate) params = params.set('toDate', filter.toDate.toISOString());
      if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
      if (filter.sortDirection) params = params.set('sortDirection', filter.sortDirection);
    }

    return this.http.get<Application[]>(this.apiUrl, { params });
  }

  getStats(): Observable<ApplicationStats> {
    return this.http.get<ApplicationStats>(`${this.apiUrl}/stats`);
  }

  getApplicationById(id: number): Observable<Application> {
    return this.http.get<Application>(`${this.apiUrl}/${id}`);
  }

  createApplication(application: ApplicationCreateModel): Observable<Application> {
    return this.http.post<Application>(this.apiUrl, application);
  }

  updateApplicationStatus(id: number, model: StatusUpdateModel): Observable<Application> {
    return this.http.put<Application>(`${this.apiUrl}/${id}/status`, model);
  }

  autoApply(model: AutoApplyModel): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auto-apply`, model);
  }

  deleteApplication(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
