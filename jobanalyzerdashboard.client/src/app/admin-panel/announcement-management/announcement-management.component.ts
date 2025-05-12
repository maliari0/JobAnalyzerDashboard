import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Announcement } from '../../models/announcement.model';
import { AnnouncementService } from '../../services/announcement.service';

@Component({
  selector: 'app-announcement-management',
  standalone: false,
  templateUrl: './announcement-management.component.html',
  styleUrl: './announcement-management.component.css'
})
export class AnnouncementManagementComponent implements OnInit {
  announcements: Announcement[] = [];
  loading = false;
  error = '';
  successMessage = '';

  showAddForm = false;
  showEditForm = false;
  selectedAnnouncement: Announcement | null = null;

  announcementForm: FormGroup;

  constructor(
    private announcementService: AnnouncementService,
    private fb: FormBuilder
  ) {
    this.announcementForm = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(100)]],
      content: ['', [Validators.required, Validators.maxLength(500)]],
      type: ['info', Validators.required],
      isActive: [true],
      expiresAt: [null]
    });
  }

  ngOnInit(): void {
    this.loadAnnouncements();
  }

  loadAnnouncements(): void {
    this.loading = true;
    this.error = '';

    this.announcementService.getAllAnnouncements().subscribe({
      next: (data) => {
        this.announcements = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Duyurular yüklenirken bir hata oluştu. Lütfen admin olarak giriş yaptığınızdan emin olun.';
        this.loading = false;
        console.error('Duyurular yüklenirken hata:', err);
      }
    });
  }

  toggleAddForm(): void {
    this.showAddForm = !this.showAddForm;
    if (this.showAddForm) {
      this.showEditForm = false;
      this.selectedAnnouncement = null;
      this.resetForm();
    }
  }

  editAnnouncement(announcement: Announcement): void {
    this.selectedAnnouncement = announcement;
    this.showEditForm = true;
    this.showAddForm = false;

    this.announcementForm.patchValue({
      title: announcement.title,
      content: announcement.content,
      type: announcement.type,
      isActive: announcement.isActive,
      expiresAt: announcement.expiresAt ? new Date(announcement.expiresAt) : null
    });
  }

  saveAnnouncement(): void {
    if (this.announcementForm.invalid) {
      return;
    }

    this.loading = true;
    const formData = this.announcementForm.value;

    if (this.showEditForm && this.selectedAnnouncement) {
      // Update existing announcement
      this.announcementService.updateAnnouncement(this.selectedAnnouncement.id, formData).subscribe({
        next: () => {
          this.successMessage = 'Duyuru başarıyla güncellendi.';
          this.loading = false;
          this.loadAnnouncements();
          this.showEditForm = false;
          this.selectedAnnouncement = null;
          this.resetForm();

          // Clear success message after 3 seconds
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (err) => {
          this.error = 'Duyuru güncellenirken bir hata oluştu.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      // Create new announcement
      this.announcementService.createAnnouncement(formData).subscribe({
        next: () => {
          this.successMessage = 'Duyuru başarıyla oluşturuldu.';
          this.loading = false;
          this.loadAnnouncements();
          this.showAddForm = false;
          this.resetForm();

          // Clear success message after 3 seconds
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        },
        error: (err) => {
          this.error = 'Duyuru oluşturulurken bir hata oluştu.';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  deleteAnnouncement(id: number): void {
    if (!confirm('Bu duyuruyu silmek istediğinize emin misiniz?')) {
      return;
    }

    this.loading = true;

    this.announcementService.deleteAnnouncement(id).subscribe({
      next: () => {
        this.successMessage = 'Duyuru başarıyla silindi.';
        this.loading = false;
        this.loadAnnouncements();

        // Clear success message after 3 seconds
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (err) => {
        this.error = 'Duyuru silinirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  resetForm(): void {
    this.announcementForm.reset({
      title: '',
      content: '',
      type: 'info',
      isActive: true,
      expiresAt: null
    });
  }

  cancelEdit(): void {
    this.showEditForm = false;
    this.selectedAnnouncement = null;
    this.resetForm();
  }

  cancelAdd(): void {
    this.showAddForm = false;
    this.resetForm();
  }

  getTypeLabel(type: string): string {
    switch (type) {
      case 'info': return 'Bilgi';
      case 'warning': return 'Uyarı';
      case 'success': return 'Başarı';
      case 'error': return 'Hata';
      default: return type;
    }
  }
}
