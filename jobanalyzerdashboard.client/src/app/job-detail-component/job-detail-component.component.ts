import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Job } from '../models/job.model';
import { ApplicationRequest, JobService } from '../services/job.service';
import { ApplicationService } from '../services/application.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-job-detail-component',
  standalone: false,
  templateUrl: './job-detail-component.component.html',
  styleUrl: './job-detail-component.component.css'
})
export class JobDetailComponentComponent implements OnInit {
  job: Job | null = null;
  loading = false;
  error = '';
  applyingInProgress = false;
  notifyingInProgress = false;
  applicationMessage = '';
  attachCV = true;
  showApplicationForm = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private applicationService: ApplicationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const jobId = Number(params.get('id'));
      if (jobId) {
        this.loadJobDetails(jobId);
      } else {
        this.error = 'Geçersiz iş ilanı ID\'si.';
      }
    });
  }

  loadJobDetails(id: number): void {
    this.loading = true;
    this.error = '';

    this.jobService.getJobById(id)
      .pipe(
        finalize(() => {
          this.loading = false;
          // Değişiklikleri manuel olarak tetikle
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (data) => {
          this.job = data;

          // Varsayılan başvuru mesajını oluştur
          this.generateDefaultMessage();

          // Değişiklikleri manuel olarak tetikle
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = 'İş ilanı detayları yüklenirken bir hata oluştu.';
          console.error(err);
        }
      });
  }

  generateDefaultMessage(): void {
    if (!this.job) return;

    this.applicationMessage = `Merhaba,

${this.job.title} pozisyonu için başvurumu yapmak istiyorum. Özgeçmişimi ekte bulabilirsiniz.

Saygılarımla,
Ali Yılmaz`;
  }

  toggleApplicationForm(): void {
    this.showApplicationForm = !this.showApplicationForm;
    if (this.showApplicationForm && !this.applicationMessage) {
      this.generateDefaultMessage();
    }
  }

  applyToJob(): void {
    if (!this.job || this.job.isApplied || this.applyingInProgress) {
      return;
    }

    this.applyingInProgress = true;

    const request: ApplicationRequest = {
      method: 'Manual',
      message: this.applicationMessage,
      attachCV: this.attachCV
    };

    this.jobService.applyToJob(this.job.id, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.job!.isApplied = true;
          this.job!.appliedDate = new Date().toISOString();
          this.showApplicationForm = false;
        }
        this.applyingInProgress = false;
      },
      error: (err) => {
        this.error = 'Başvuru yapılırken bir hata oluştu.';
        this.applyingInProgress = false;
        console.error(err);
      }
    });
  }

  autoApply(): void {
    if (!this.job || this.job.isApplied || this.applyingInProgress) {
      return;
    }

    this.applyingInProgress = true;
    this.error = ''; // Önceki hata mesajlarını temizle

    this.jobService.autoApply(this.job.id)
      .pipe(
        finalize(() => {
          this.applyingInProgress = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          if (response.success) {
            this.job!.isApplied = true;
            this.job!.appliedDate = new Date().toISOString();
            this.showApplicationForm = false;
            alert('Otomatik başvuru işlemi başlatıldı! n8n webhook\'u tetiklendi.');
          } else {
            // Başarısız yanıt durumunda
            this.error = response.message || 'Otomatik başvuru yapılırken bir hata oluştu.';
            // İlan başvuruldu olarak işaretlenmediyse, UI'da da gösterme
            this.job!.isApplied = false;
            this.job!.appliedDate = undefined;
          }
          this.cdr.detectChanges();
        },
        error: (err) => {
          // HTTP hatası durumunda
          this.error = 'Otomatik başvuru yapılırken bir hata oluştu: ' +
                      (err.error?.message || err.message || 'Bilinmeyen hata');
          // İlan başvuruldu olarak işaretlenmediyse, UI'da da gösterme
          this.job!.isApplied = false;
          this.job!.appliedDate = undefined;
          console.error(err);
        }
      });
  }

  notifyWebhook(): void {
    if (!this.job || this.notifyingInProgress) {
      return;
    }

    this.notifyingInProgress = true;

    this.jobService.webhookNotify(this.job.id)
      .pipe(
        finalize(() => {
          this.notifyingInProgress = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (response) => {
          if (response.success) {
            alert('Webhook bildirimi başarıyla gönderildi!');
          }
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.error = 'Webhook bildirimi gönderilirken bir hata oluştu.';
          console.error(err);
        }
      });
  }

  deleteJob(): void {
    if (!this.job) {
      return;
    }


    if (!confirm(`"${this.job.title}" ilanını silmek istediğinize emin misiniz?`)) {
      return;
    }

    this.loading = true;

    this.jobService.deleteJob(this.job.id).subscribe({
      next: (response) => {
        if (response.success) {
          alert('İş ilanı başarıyla silindi!');

          this.router.navigate(['/']);
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'İş ilanı silinirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  getQualityScoreClass(score: number): string {
    if (score >= 4) return 'high-quality';
    if (score >= 3) return 'medium-quality';
    return 'low-quality';
  }

  getActionSuggestionClass(action: string): string {
    switch (action.toLowerCase()) {
      case 'sakla': return 'action-store';
      case 'bildir': return 'action-notify';
      case 'ilgisiz': return 'action-ignore';
      default: return '';
    }
  }

  getActionSuggestionText(action: string): string {
    switch (action.toLowerCase()) {
      case 'sakla': return 'Sakla';
      case 'bildir': return 'Bildir';
      case 'ilgisiz': return 'İlgisiz';
      default: return action;
    }
  }

  clearError(): void {
    this.error = '';
  }
}
