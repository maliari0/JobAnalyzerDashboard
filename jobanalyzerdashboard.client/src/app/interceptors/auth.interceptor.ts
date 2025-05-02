import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService, private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Token'ı al
    const token = this.authService.token;
    
    // Eğer token varsa, isteğe ekle
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    // İsteği gönder ve hata durumunda işle
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // 401 Unauthorized hatası durumunda kullanıcıyı çıkış yaptır
        if (error.status === 401) {
          this.authService.logout();
          this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
        }
        
        // 403 Forbidden hatası durumunda yetki hatası sayfasına yönlendir
        if (error.status === 403) {
          this.router.navigate(['/unauthorized']);
        }
        
        // Hatayı tekrar fırlat
        return throwError(() => error);
      })
    );
  }
}
