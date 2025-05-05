import { Component, OnInit } from '@angular/core';
import { ApplicationService } from '../../services/application.service';
import { Application } from '../../models/application.model';

@Component({
  selector: 'app-application-management',
  standalone: false,
  templateUrl: './application-management.component.html',
  styleUrl: './application-management.component.css'
})
export class ApplicationManagementComponent implements OnInit {
  // Veri
  applications: Application[] = [];
  selectedApplication: Application | null = null;
  editingApplication: Application | null = null;

  // Filtreleme ve sıralama
  searchTerm: string = '';
  statusFilter: string = '';
  methodFilter: string = '';
  sortBy: string = 'appliedDate';
  sortDirection: string = 'desc';

  // Sayfalama
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  totalPages: number = 0;

  // UI durumu
  loading: boolean = false;
  error: string | null = null;

  constructor(private applicationService: ApplicationService) { }

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.loading = true;
    this.error = null;

    this.applicationService.getApplications().subscribe({
      next: (data: Application[]) => {
        // Filtreleme ve sıralama işlemlerini client tarafında yapalım
        let filteredApplications = [...data];

        // Durum filtresi
        if (this.statusFilter) {
          filteredApplications = filteredApplications.filter(app => app.status === this.statusFilter);
        }

        // Başvuru yöntemi filtresi
        if (this.methodFilter) {
          const isAutoApplied = this.methodFilter === 'Auto';
          filteredApplications = filteredApplications.filter(app =>
            app.isAutoApplied === isAutoApplied
          );
        }

        // Arama filtresi
        if (this.searchTerm) {
          const searchLower = this.searchTerm.toLowerCase();
          filteredApplications = filteredApplications.filter(app =>
            (app.jobTitle?.toLowerCase().includes(searchLower) || false) ||
            (app.company?.toLowerCase().includes(searchLower) || false) ||
            (app.notes?.toLowerCase().includes(searchLower) || false)
          );
        }

        // Sıralama
        filteredApplications.sort((a, b) => {
          if (this.sortBy === 'appliedDate') {
            const aDate = new Date(a.appliedDate).getTime();
            const bDate = new Date(b.appliedDate).getTime();
            return this.sortDirection === 'asc' ? aDate - bDate : bDate - aDate;
          } else if (this.sortBy === 'status') {
            return this.sortDirection === 'asc'
              ? a.status.localeCompare(b.status)
              : b.status.localeCompare(a.status);
          } else if (this.sortBy === 'jobTitle') {
            const aTitle = a.jobTitle || '';
            const bTitle = b.jobTitle || '';
            return this.sortDirection === 'asc'
              ? aTitle.localeCompare(bTitle)
              : bTitle.localeCompare(aTitle);
          }
          return 0;
        });

        this.applications = filteredApplications;
        this.totalItems = filteredApplications.length;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading applications:', err);
        this.error = 'Başvurular yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  viewApplicationDetails(applicationId: number): void {
    this.loading = true;

    this.applicationService.getApplicationById(applicationId).subscribe({
      next: (application) => {
        this.selectedApplication = application;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading application details:', err);
        this.error = 'Başvuru detayları yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  editApplication(application: Application): void {
    // Düzenleme için kopya oluştur
    this.editingApplication = { ...application };

    // Detay modalını kapat
    this.selectedApplication = null;
  }

  cancelEdit(): void {
    this.editingApplication = null;
  }

  saveApplication(): void {
    if (!this.editingApplication) return;

    this.loading = true;

    // Gerçek bir API çağrısı yerine, client tarafında güncelleme yapalım
    setTimeout(() => {
      // Listedeki başvuruyu güncelle
      const index = this.applications.findIndex(a => a.id === this.editingApplication!.id);
      if (index !== -1) {
        this.applications[index] = { ...this.editingApplication! };
      }

      // Modalı kapat
      this.editingApplication = null;
      this.loading = false;

      // Başarı mesajı göster
      this.error = null;
      alert('Başvuru başarıyla güncellendi.');
    }, 500);
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.loadApplications();
  }
}
