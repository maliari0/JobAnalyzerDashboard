import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    // Kullanıcı giriş yapmış mı kontrol et
    if (this.authService.isLoggedIn) {
      // Eğer rol kontrolü gerekiyorsa
      if (route.data['roles'] && route.data['roles'].length) {
        // Kullanıcının rolü uygun mu kontrol et
        const userRole = this.authService.currentUserValue?.role;
        if (userRole && route.data['roles'].includes(userRole)) {
          return true;
        } else {
          // Yetkisiz erişim
          this.router.navigate(['/unauthorized']);
          return false;
        }
      }
      
      // Rol kontrolü gerekmiyorsa veya rol uygunsa
      return true;
    }
    
    // Kullanıcı giriş yapmamışsa, giriş sayfasına yönlendir
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
