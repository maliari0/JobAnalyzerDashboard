import { Component, OnInit } from '@angular/core';
import { AdminService } from '../services/admin.service';
import { AdminDashboardStats } from '../models/admin.model';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-admin-panel',
  standalone: false,
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent implements OnInit {
  activeTab: string = 'dashboard';
  stats: AdminDashboardStats = {
    totalUsers: 0,
    activeUsers: 0,
    totalJobs: 0,
    totalApplications: 0
  };
  loading: boolean = false;
  error: string | null = null;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Kullanıcının admin olup olmadığını kontrol et
    const currentUser = this.authService.currentUserValue;
    if (!currentUser || currentUser.role !== 'Admin') {
      this.router.navigate(['/']);
      return;
    }

    this.loadDashboardStats();
  }

  loadDashboardStats(): void {
    this.loading = true;
    this.error = null;

    this.adminService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard stats:', err);
        this.error = 'İstatistikler yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}
