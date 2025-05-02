import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models/auth.model';

@Component({
  selector: 'app-user-menu-component',
  standalone: false,
  templateUrl: './user-menu-component.component.html',
  styleUrl: './user-menu-component.component.css'
})
export class UserMenuComponentComponent implements OnInit {
  currentUser: User | null = null;
  isMenuOpen = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Kullanıcı bilgilerini al
    this.authService.currentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  logout(): void {
    this.authService.logout();
    this.closeMenu();
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
    this.closeMenu();
  }
}
