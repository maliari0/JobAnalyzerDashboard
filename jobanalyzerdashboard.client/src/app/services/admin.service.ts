import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { User } from '../models/auth.model';
import {
  UserListResponse,
  UserDeleteResponse,
  UserUpdateRequest,
  UserUpdateResponse,
  AdminDashboardStats
} from '../models/admin.model';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = '/api/admin';

  constructor(private http: HttpClient) { }

  // Kullanıcıları listele
  getUsers(page: number = 1, pageSize: number = 10): Observable<UserListResponse> {
    return this.http.get<UserListResponse>(`${this.apiUrl}/users?page=${page}&pageSize=${pageSize}`)
      .pipe(
        catchError(error => {
          console.error('Error fetching users:', error);
          return throwError(() => error);
        })
      );
  }

  // Kullanıcı sil
  deleteUser(email: string): Observable<UserDeleteResponse> {
    return this.http.delete<UserDeleteResponse>(`${this.apiUrl}/users/${email}`)
      .pipe(
        catchError(error => {
          console.error('Error deleting user:', error);
          return throwError(() => error);
        })
      );
  }

  // Kullanıcı güncelle
  updateUser(updateData: UserUpdateRequest): Observable<UserUpdateResponse> {
    return this.http.put<UserUpdateResponse>(`${this.apiUrl}/users/${updateData.id}`, updateData)
      .pipe(
        catchError(error => {
          console.error('Error updating user:', error);
          return throwError(() => error);
        })
      );
  }

  // Admin paneli istatistikleri
  getDashboardStats(): Observable<AdminDashboardStats> {
    return this.http.get<AdminDashboardStats>(`${this.apiUrl}/dashboard/stats`)
      .pipe(
        catchError(error => {
          console.error('Error fetching dashboard stats:', error);
          return throwError(() => error);
        })
      );
  }
}
