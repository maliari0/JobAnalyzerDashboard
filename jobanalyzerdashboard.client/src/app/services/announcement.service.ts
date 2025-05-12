import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { Announcement, AnnouncementCreateModel, AnnouncementUpdateModel } from '../models/announcement.model';

@Injectable({
  providedIn: 'root'
})
export class AnnouncementService {
  private apiUrl = '/api/announcement';

  constructor(private http: HttpClient) { }

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    if (!token) {
      console.warn('No token found in localStorage');
      return new HttpHeaders({
        'Content-Type': 'application/json'
      });
    }
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getAnnouncements(): Observable<Announcement[]> {
    // API'nin doğru porta yönlendirildiğinden emin olmak için tam URL kullanıyoruz
    return this.http.get<Announcement[]>(this.apiUrl, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      })
    });
  }

  getAllAnnouncements(): Observable<Announcement[]> {
    const headers = this.getAuthHeaders();
    // API'nin doğru porta yönlendirildiğinden emin olmak için tam URL kullanıyoruz
    return this.http.get<Announcement[]>(`${this.apiUrl}/all`, { headers });
  }

  getAnnouncementById(id: number): Observable<Announcement> {
    const headers = this.getAuthHeaders();
    return this.http.get<Announcement>(`${this.apiUrl}/${id}`, { headers });
  }

  createAnnouncement(announcement: AnnouncementCreateModel): Observable<Announcement> {
    const headers = this.getAuthHeaders();
    return this.http.post<Announcement>(this.apiUrl, announcement, { headers });
  }

  updateAnnouncement(id: number, announcement: AnnouncementUpdateModel): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.put<any>(`${this.apiUrl}/${id}`, announcement, { headers });
  }

  deleteAnnouncement(id: number): Observable<any> {
    const headers = this.getAuthHeaders();
    return this.http.delete<any>(`${this.apiUrl}/${id}`, { headers });
  }
}
