import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Application } from '../models/application.model';
import { ApplicationService, StatusUpdateModel } from '../services/application.service';

@Component({
  selector: 'app-application-history-component',
  standalone: false,
  templateUrl: './application-history-component.component.html',
  styleUrl: './application-history-component.component.css'
})
export class ApplicationHistoryComponentComponent implements OnInit {
  applications: Application[] = [];
  loading = false;
  error = '';
  statusOptions = ['Pending', 'Accepted', 'Rejected', 'Interview'];
  selectedApplication: Application | null = null;
  selectedEmailApplication: Application | null = null;
  updatingStatus = false;
  sendingEmail = false;

  constructor(
    private applicationService: ApplicationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications(): void {
    this.loading = true;
    this.error = '';

    this.applicationService.getApplications().subscribe({
      next: (data) => {
        this.applications = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Başvuru geçmişi yüklenirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  viewJobDetails(jobId: number): void {
    // Eğer iş ilanı silinmişse, detay sayfasına yönlendirme
    const application = this.applications.find(a => a.jobId === jobId);
    if (application && application.isJobDeleted) {
      alert('Bu iş ilanı silinmiş. Detaylar görüntülenemiyor.');
      return;
    }

    this.router.navigate(['/job', jobId]);
  }

  selectApplication(application: Application): void {
    this.selectedApplication = application;
  }

  updateStatus(status: string): void {
    if (!this.selectedApplication || this.updatingStatus) {
      return;
    }

    this.updatingStatus = true;

    const statusModel: StatusUpdateModel = {
      status: status,
      details: `Durum ${status} olarak güncellendi.`
    };

    this.applicationService.updateApplicationStatus(this.selectedApplication.id, statusModel).subscribe({
      next: (updatedApplication) => {
        // Güncellenen başvuruyu listede bul ve güncelle
        const index = this.applications.findIndex(a => a.id === updatedApplication.id);
        if (index !== -1) {
          this.applications[index] = updatedApplication;
        }

        this.selectedApplication = null;
        this.updatingStatus = false;
      },
      error: (err) => {
        this.error = 'Başvuru durumu güncellenirken bir hata oluştu.';
        this.updatingStatus = false;
        console.error(err);
      }
    });
  }

  viewEmailContent(application: Application): void {
    this.selectedEmailApplication = application;
  }

  closeAllModals(): void {
    this.selectedApplication = null;
    this.selectedEmailApplication = null;
  }

  sendEmail(): void {
    if (!this.selectedEmailApplication || this.sendingEmail) {
      return;
    }

    this.sendingEmail = true;

    this.applicationService.sendEmail(
      this.selectedEmailApplication.id,
      this.selectedEmailApplication.emailContent
    ).subscribe({
      next: (response) => {
        this.sendingEmail = false;
        alert('E-posta başarıyla gönderildi!');

        // Başvuru durumunu güncelle
        const index = this.applications.findIndex(a => a.id === this.selectedEmailApplication?.id);
        if (index !== -1 && response && response.success) {
          this.applications[index].status = 'Sent';
          this.selectedEmailApplication = null;
        }
      },
      error: (err) => {
        this.sendingEmail = false;
        alert('E-posta gönderilirken bir hata oluştu: ' + (err.error?.message || err.message));
        console.error('E-posta gönderme hatası:', err);
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'status-pending';
      case 'applying': return 'status-pending';
      case 'sent': return 'status-sent';
      case 'accepted': return 'status-accepted';
      case 'rejected': return 'status-rejected';
      case 'interview': return 'status-interview';
      default: return '';
    }
  }
}
