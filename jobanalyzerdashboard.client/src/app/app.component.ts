import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'Job Analyzer Dashboard';
  currentRoute = '';
  isLoggedIn = false;

  private authSubscription: Subscription | null = null;
  private routerSubscription: Subscription | null = null;

  constructor(
    private readonly router: Router,
    private readonly authService: AuthService
  ) {
    this.routerSubscription = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.currentRoute = event.url;
    });
  }

  ngOnInit(): void {
    // Kullanıcı giriş durumunu takip et
    this.authSubscription = this.authService.currentUser.subscribe(user => {
      this.isLoggedIn = !!user;
    });
  }

  ngOnDestroy(): void {
    // Abonelikleri temizle
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }

    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  isActive(route: string): boolean {
    if (route === '/' && this.currentRoute === '') {
      return true;
    }
    return this.currentRoute === route;
  }
}
