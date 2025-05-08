import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Profile, Resume } from '../models/profile.model';
import { OAuthToken } from '../models/oauth.model';

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

  uploadResume(file: File, isDefault: boolean = false): Observable<Resume> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('isDefault', isDefault.toString());

    return this.http.post<Resume>(`${this.apiUrl}/resumes`, formData);
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

  hasUploadedResume(): Observable<{hasResume: boolean}> {
    return this.http.get<{hasResume: boolean}>(`${this.apiUrl}/resumes/has-resume`);
  }

  testTelegramConnection(chatId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/test-telegram`, { chatId });
  }

  testNotionConnection(pageId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/test-notion`, { pageId });
  }

  // OAuth işlemleri için eklenen metodlar
  getOAuthStatus(): Observable<OAuthToken[]> {
    console.log('Calling OAuth status endpoint');
    return this.http.get<OAuthToken[]>(`${this.apiUrl}/oauth-status`, {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });
  }

  authorizeGoogle(userId?: number): void {
    console.log('Redirecting to Google authorization');

    // Mevcut kullanıcı ID'sini al
    const currentUser = JSON.parse(localStorage.getItem('currentUser') || '{}');
    const currentUserId = currentUser?.id || 0;

    // Eğer userId belirtilmemişse, mevcut kullanıcı ID'sini kullan
    const finalUserId = userId || currentUserId;

    // URL'yi oluştur
    let url = `${this.apiUrl}/authorize-google?userId=${finalUserId}`;

    console.log('Authorization URL:', url);
    window.location.href = url;
  }

  revokeOAuth(provider: string): Observable<any> {
    console.log(`Revoking OAuth for provider: ${provider}`);
    return this.http.delete<any>(`${this.apiUrl}/revoke-oauth/${provider}`, {
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });
  }
}
