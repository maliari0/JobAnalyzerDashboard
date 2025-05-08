import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, timer } from 'rxjs';
import { Job } from '../models/job.model';
import { JobService } from '../services/job.service';
import { ApplicationService } from '../services/application.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-job-detail-component',
  templateUrl: './job-detail-component.component.html',
  styleUrls: ['./job-detail-component.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class JobDetailComponentComponent implements OnInit {
  job: Job | null = null;
  loading = false;
  error = '';
  showApplicationForm = false;
  applyingInProgress = false;
  notifyingInProgress = false;
  applicationMessage = '';
  cvAttached = true;
  isAdmin = false;

  showEmailModal = false;
  emailContent = '';
  loadingEmailContent = false;
  emailContentError = '';
  applicationId?: number;
  sendingEmail = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private applicationService: ApplicationService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private profileService: ProfileService
  ) {}

  ngOnInit(): void {
    this.loading = true;

    // Kullanıcının admin olup olmadığını kontrol et
    const currentUser = this.authService.currentUserValue;
    this.isAdmin = currentUser?.role === 'Admin';

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      const jobId = parseInt(id, 10);

      // İş ilanını getir
      this.jobService.getJobById(jobId).subscribe({
        next: (job) => {
          this.job = job;
          this.loading = false;

          // Kullanıcının bu ilana başvurup başvurmadığını kontrol et
          if (currentUser) {
            this.checkIfUserHasApplied(jobId);
          }
        },
        error: (err) => {
          this.error = 'İş ilanı yüklenirken bir hata oluştu.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.error = 'Geçersiz iş ilanı ID\'si.';
      this.loading = false;
    }
  }

  // Kullanıcının ilana başvurup başvurmadığını kontrol et
  checkIfUserHasApplied(jobId: number): void {
    this.jobService.hasUserAppliedToJob(jobId).subscribe({
      next: (response) => {
        if (response.success && response.hasApplied) {
          // Kullanıcı bu ilana daha önce başvurmuş, ilanı başvuruldu olarak işaretle
          if (this.job) {
            this.job.isApplied = true;
            console.log('Kullanıcı bu ilana daha önce başvurmuş.');
          }
        }
      },
      error: (err) => {
        console.error('Kullanıcının ilana başvurup başvurmadığı kontrol edilirken hata oluştu:', err);
      }
    });
  }

  toggleApplicationForm(): void {
    this.showApplicationForm = !this.showApplicationForm;
  }

  applyToJob(): void {
    if (!this.job || this.applyingInProgress) {
      return;
    }

    // CV yüklenip yüklenmediğini kontrol et
    this.profileService.hasUploadedResume().subscribe(
      (response) => {
        if (!response.hasResume) {
          // CV yüklenmemişse, kullanıcıyı profil sayfasına yönlendir
          alert('Başvuru yapmak için önce CV yüklemeniz gerekmektedir. Profil sayfasına yönlendiriliyorsunuz.');
          this.router.navigate(['/profile'], { fragment: 'resumeUploadSection' });
          return;
        }

        // CV yüklenmişse, başvuru işlemine devam et
        this.applyingInProgress = true;

        const application = {
          jobId: this.job!.id,
          message: this.applicationMessage,
          appliedMethod: 'Manual',
          isAutoApplied: false,
          cvAttached: this.cvAttached
        };

        this.applicationService.createApplication(application).subscribe({
          next: (response) => {
            this.applyingInProgress = false;
            this.job!.isApplied = true;
            this.job!.appliedDate = new Date().toISOString();
            this.showApplicationForm = false;
            this.applicationMessage = '';
            alert('Başvurunuz başarıyla gönderildi!');
          },
          error: (err) => {
            this.applyingInProgress = false;
            this.error = 'Başvuru yapılırken bir hata oluştu.';
            console.error(err);
          }
        });
      },
      (err) => {
        console.error('CV kontrolü yapılırken bir hata oluştu:', err);
        this.error = 'CV kontrolü yapılırken bir hata oluştu.';
      }
    );
  }

  autoApply(): void {
    if (!this.job || this.applyingInProgress) {
      return;
    }

    // CV yüklenip yüklenmediğini kontrol et
    this.profileService.hasUploadedResume().subscribe(
      (response) => {
        if (!response.hasResume) {
          // CV yüklenmemişse, kullanıcıyı profil sayfasına yönlendir
          alert('Başvuru yapmak için önce CV yüklemeniz gerekmektedir. Profil sayfasına yönlendiriliyorsunuz.');
          this.router.navigate(['/profile'], { fragment: 'resumeUploadSection' });
          return;
        }

        // CV yüklenmişse, başvuru işlemine devam et
        this.applyingInProgress = true;

        // Mevcut kullanıcının ID'sini al
        const currentUser = this.authService.currentUserValue;
        const userId = currentUser?.id;

        console.log('Otomatik başvuru yapılıyor, kullanıcı ID:', userId);

        this.jobService.autoApply(this.job!.id, userId).subscribe({
          next: (response) => {
            if (response.success) {
              // Başvuru durumunu henüz değiştirme, sadece işaretleme
              this.showApplicationForm = false;

              // Başvuru işlemi başlatıldı, jobId'yi kullanacağız
              console.log('Otomatik başvuru başlatıldı:', response);

              // n8n'den gelen e-posta içeriğini doğrudan göster
              this.loadingEmailContent = true;

              // response içindeki emailContent'i al
              if (response.emailContent) {
                this.loadingEmailContent = false;
                this.emailContent = response.emailContent;
                this.showEmailModal = true;
                this.applyingInProgress = false;

                // İlanı henüz başvuruldu olarak işaretleme
                // Kullanıcı "Gönder" tuşuna bastıktan sonra işaretlenecek
              } else {
                // E-posta içeriği yoksa, varsayılan bir içerik oluştur
                this.loadingEmailContent = false;
                this.emailContent = `Merhaba,\n\nİlanınızda belirtilen ${this.job!.title} pozisyonu için başvurmak istiyorum. Özgeçmişimi ekte bulabilirsiniz.\n\nTeşekkürler,\n${this.authService.currentUserValue?.firstName} ${this.authService.currentUserValue?.lastName}`;
                this.showEmailModal = true;
                this.applyingInProgress = false;

                // İlanı henüz başvuruldu olarak işaretleme
                // Kullanıcı "Gönder" tuşuna bastıktan sonra işaretlenecek
              }
            } else {
              // Başarısız yanıt durumunda
              this.job!.isApplied = false;
              this.job!.appliedDate = undefined;
              this.applyingInProgress = false;

              this.error = response.message || 'Otomatik başvuru yapılırken bir hata oluştu.';
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
            this.applyingInProgress = false;
            console.error(err);
          }
        });
      },
      (err) => {
        console.error('CV kontrolü yapılırken bir hata oluştu:', err);
        this.error = 'CV kontrolü yapılırken bir hata oluştu.';
      }
    );
  }

  // E-posta içeriği modalını kapat
  closeEmailModal(): void {
    this.showEmailModal = false;
    this.emailContent = '';
    this.emailContentError = '';
  }

  // E-posta gönder
  sendEmail(): void {
    if (!this.job || !this.emailContent || this.sendingEmail) {
      return;
    }

    this.sendingEmail = true;

    // Önce OAuth durumunu kontrol et
    this.profileService.getOAuthStatus().subscribe({
      next: (tokens) => {
        // Google OAuth token'ı var mı kontrol et
        const googleToken = tokens.find(t => t.provider === 'Google');

        if (!googleToken) {
          // OAuth token'ı yoksa, kullanıcıya bildir ve yetkilendirme sayfasına yönlendir
          this.sendingEmail = false;

          if (confirm('E-posta göndermek için Google hesabınızı yetkilendirmeniz gerekiyor. Yetkilendirme sayfasına yönlendirilmek istiyor musunuz?')) {
            const currentUser = this.authService.currentUserValue;
            this.profileService.authorizeGoogle(currentUser?.id);
          }

          return;
        }

        // OAuth token'ı varsa, başvuru kaydı oluştur
        const application = {
          jobId: this.job!.id,
          message: 'Otomatik oluşturulan başvuru', // Message alanı null olamaz
          appliedMethod: 'Manual',
          isAutoApplied: true, // n8n tarafından oluşturulduğu için true
          cvAttached: true,
          telegramNotification: 'true', // n8n webhook'u tetiklendiği için true
          emailContent: this.emailContent // E-posta içeriği
        };

        this.applicationService.createApplication(application).subscribe({
          next: (createdApplication) => {
            // Oluşturulan başvuru kaydı ile e-posta gönder
            // Kullanıcının profil ID'sini al
            const currentUser = this.authService.currentUserValue;
            const profileId = currentUser?.profileId;

            this.applicationService.sendEmail(
              createdApplication.id,
              this.emailContent,
              profileId
            ).subscribe({
              next: (response) => {
                this.sendingEmail = false;
                if (response && response.success) {
                  // Başvuru başarılı olduğunda, ilanı başvuruldu olarak işaretle
                  this.job!.isApplied = true;
                  this.job!.appliedDate = new Date().toISOString();

                  alert('E-posta başarıyla gönderildi!');
                  this.closeEmailModal();
                } else {
                  alert('E-posta gönderilirken bir hata oluştu: ' + (response?.message || 'Bilinmeyen hata'));
                }
              },
              error: (err) => {
                this.sendingEmail = false;

                // Gmail API hatası kontrolü
                if (err.error?.message && err.error.message.includes('Gmail API')) {
                  if (confirm('E-posta göndermek için Gmail API\'yi etkinleştirmeniz gerekiyor. Google Cloud Console\'a yönlendirilmek istiyor musunuz?')) {
                    window.open('https://console.developers.google.com/apis/api/gmail.googleapis.com/overview?project=602631296713', '_blank');
                  }
                } else {
                  alert('E-posta gönderilirken bir hata oluştu: ' + (err.error?.message || err.message || 'Bilinmeyen hata'));
                }

                console.error('E-posta gönderme hatası:', err);
              }
            });
          },
          error: (error) => {
            this.sendingEmail = false;
            alert('Başvuru kaydı oluşturulurken bir hata oluştu: ' + (error.error?.message || error.message || 'Bilinmeyen hata'));
            console.error('Başvuru kaydı oluşturma hatası:', error);
          }
        });
      },
      error: (error) => {
        this.sendingEmail = false;
        alert('OAuth durumu kontrol edilirken bir hata oluştu: ' + (error.error?.message || error.message || 'Bilinmeyen hata'));
        console.error('OAuth durumu kontrol hatası:', error);
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
      default: return 'action-default';
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
