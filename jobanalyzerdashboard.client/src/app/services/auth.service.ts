import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import {
  User,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  EmailConfirmationRequest
} from '../models/auth.model';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/user';
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;
  private tokenExpirationTimer: any;

  constructor(private http: HttpClient, private router: Router) {
    // Tarayıcı belleğinden kullanıcı bilgilerini al
    const storedUser = localStorage.getItem('currentUser');
    this.currentUserSubject = new BehaviorSubject<User | null>(storedUser ? JSON.parse(storedUser) : null);
    this.currentUser = this.currentUserSubject.asObservable();

    // Token süresi kontrolü
    if (storedUser) {
      this.checkTokenExpiration();
    }
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get isLoggedIn(): boolean {
    return !!this.currentUserSubject.value && !!localStorage.getItem('token');
  }

  public get token(): string | null {
    return localStorage.getItem('token');
  }

  register(registerData: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, registerData)
      .pipe(
        tap(response => {
          if (response.success && response.token && response.user) {
            this.storeUserData(response);
          }
        }),
        catchError(error => {
          console.error('Registration error:', error);
          return throwError(() => error);
        })
      );
  }

  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, loginData)
      .pipe(
        tap(response => {
          if (response.success && response.token && response.user) {
            this.storeUserData(response);
          }
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    // Tarayıcı belleğinden kullanıcı bilgilerini temizle
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    localStorage.removeItem('tokenExpiration');

    // BehaviorSubject'i sıfırla
    this.currentUserSubject.next(null);

    // Token süre kontrolünü temizle
    if (this.tokenExpirationTimer) {
      clearTimeout(this.tokenExpirationTimer);
      this.tokenExpirationTimer = null;
    }

    // Ana sayfaya yönlendir
    this.router.navigate(['/']);
  }

  forgotPassword(forgotPasswordData: ForgotPasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/forgot-password`, forgotPasswordData)
      .pipe(
        catchError(error => {
          console.error('Forgot password error:', error);
          return throwError(() => error);
        })
      );
  }

  resetPassword(resetPasswordData: ResetPasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/reset-password`, resetPasswordData)
      .pipe(
        catchError(error => {
          console.error('Reset password error:', error);
          return throwError(() => error);
        })
      );
  }

  changePassword(changePasswordData: ChangePasswordRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/change-password`, changePasswordData)
      .pipe(
        catchError(error => {
          console.error('Change password error:', error);
          return throwError(() => error);
        })
      );
  }

  confirmEmail(confirmEmailData: EmailConfirmationRequest): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/confirm-email?token=${encodeURIComponent(confirmEmailData.token)}&email=${encodeURIComponent(confirmEmailData.email)}`
    ).pipe(
      catchError(error => {
        console.error('Email confirmation error:', error);
        return throwError(() => error);
      })
    );
  }

  getCurrentUser(): Observable<User> {
    // Eğer zaten kullanıcı bilgisi varsa, onu döndür
    if (this.currentUserValue) {
      return of(this.currentUserValue);
    }

    // Yoksa API'den al
    return this.http.get<User>(`${this.apiUrl}/me`)
      .pipe(
        tap(user => {
          // Kullanıcı bilgilerini güncelle
          this.currentUserSubject.next(user);
          localStorage.setItem('currentUser', JSON.stringify(user));
        }),
        catchError(error => {
          console.error('Get current user error:', error);
          return throwError(() => error);
        })
      );
  }

  private storeUserData(response: AuthResponse): void {
    if (!response.user || !response.token) {
      return;
    }

    // Kullanıcı bilgilerini ve token'ı sakla
    localStorage.setItem('currentUser', JSON.stringify(response.user));
    localStorage.setItem('token', response.token);

    // Token süresini hesapla (varsayılan olarak 1 saat)
    const expirationDate = new Date(new Date().getTime() + 60 * 60 * 1000);
    localStorage.setItem('tokenExpiration', expirationDate.toISOString());

    // Token süre kontrolünü başlat
    this.setTokenExpirationTimer(60 * 60 * 1000);

    // BehaviorSubject'i güncelle
    this.currentUserSubject.next(response.user);
  }

  private checkTokenExpiration(): void {
    const expirationDateStr = localStorage.getItem('tokenExpiration');
    if (!expirationDateStr) {
      return;
    }

    const expirationDate = new Date(expirationDateStr);
    const now = new Date();

    if (expirationDate <= now) {
      // Token süresi dolmuş, kullanıcıyı çıkış yaptır
      this.logout();
      return;
    }

    // Token süresi dolmamış, zamanlayıcıyı ayarla
    const expirationTime = expirationDate.getTime() - now.getTime();
    this.setTokenExpirationTimer(expirationTime);
  }

  private setTokenExpirationTimer(duration: number): void {
    this.tokenExpirationTimer = setTimeout(() => {
      this.logout();
    }, duration);
  }
}
