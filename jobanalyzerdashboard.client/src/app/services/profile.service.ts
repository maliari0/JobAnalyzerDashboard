import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Profile, Resume } from '../models/profile.model';

export interface N8nSettings {
  notionIntegration?: boolean;
  telegramIntegration?: boolean;
  autoApplyEnabled?: boolean;
  minQualityScore?: number;
  preferredCategories?: string[];
  telegramChatId?: string;
  notionPageId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl = '/api/profile';

  constructor(private http: HttpClient) { }

  getProfile(): Observable<Profile> {
    return this.http.get<Profile>(this.apiUrl);
  }

  updateProfile(profile: Profile): Observable<Profile> {
    return this.http.put<Profile>(this.apiUrl, profile);
  }

  updateN8nSettings(settings: N8nSettings): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/n8n-settings`, settings);
  }

  uploadResume(file: File, resumeName?: string): Observable<Resume> {
    const formData = new FormData();
    formData.append('file', file);

    if (resumeName) {
      formData.append('resumeName', resumeName);
    }

    return this.http.post<Resume>(`${this.apiUrl}/upload-resume`, formData);
  }

  getResumes(): Observable<Resume[]> {
    return this.http.get<Resume[]>(`${this.apiUrl}/resumes`);
  }

  setDefaultResume(resumeId: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/resumes/${resumeId}/set-default`, {});
  }

  deleteResume(resumeId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/resumes/${resumeId}`);
  }

  getDefaultResume(): Observable<Resume> {
    return this.http.get<Resume>(`${this.apiUrl}/resumes/default`);
  }

  testTelegramConnection(chatId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/test-telegram`, { chatId });
  }

  testNotionConnection(pageId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/test-notion`, { pageId });
  }
}
