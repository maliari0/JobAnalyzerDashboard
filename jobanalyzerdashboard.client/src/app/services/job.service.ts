import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Job } from '../models/job.model';

export interface JobFilter {
  category?: string;
  minQualityScore?: number;
  maxQualityScore?: number;
  tag?: string;
  actionSuggestion?: string;
  minSalary?: number;
  employmentType?: string;
  isApplied?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface JobStats {
  totalJobs: number;
  appliedJobs: number;
  highQualityJobs: number;
  averageSalary: number;
  categoryBreakdown: { category: string, count: number }[];
  qualityScoreBreakdown: { score: number, count: number }[];
}

export interface ApplicationRequest {
  method?: string;
  message?: string;
  attachCV?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private apiUrl = '/api/job';

  constructor(private http: HttpClient) { }

  getJobs(filter?: JobFilter): Observable<Job[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.category) params = params.set('category', filter.category);
      if (filter.minQualityScore) params = params.set('minQualityScore', filter.minQualityScore.toString());
      if (filter.maxQualityScore) params = params.set('maxQualityScore', filter.maxQualityScore.toString());
      if (filter.tag) params = params.set('tag', filter.tag);
      if (filter.actionSuggestion) params = params.set('actionSuggestion', filter.actionSuggestion);
      if (filter.minSalary) params = params.set('minSalary', filter.minSalary.toString());
      if (filter.employmentType) params = params.set('employmentType', filter.employmentType);
      if (filter.isApplied !== undefined) params = params.set('isApplied', filter.isApplied.toString());
      if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
      if (filter.sortDirection) params = params.set('sortDirection', filter.sortDirection);
    }

    return this.http.get<Job[]>(this.apiUrl, { params });
  }

  getJobById(id: number): Observable<Job> {
    return this.http.get<Job>(`${this.apiUrl}/${id}`);
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  getTags(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/tags`);
  }

  getStats(): Observable<JobStats> {
    return this.http.get<JobStats>(`${this.apiUrl}/stats`);
  }

  applyToJob(id: number, request?: ApplicationRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/apply/${id}`, request || {});
  }

  webhookNotify(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/webhook-notify/${id}`, {});
  }

  autoApply(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auto-apply/${id}`, {});
  }

  deleteJob(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
