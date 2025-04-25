import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Profile, Resume } from '../models/profile.model';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-profile-component',
  standalone: false,
  templateUrl: './profile-component.component.html',
  styleUrl: './profile-component.component.css'
})
export class ProfileComponentComponent implements OnInit {
  profileForm!: FormGroup;
  profile: Profile | null = null;
  loading = false;
  error = '';
  successMessage = '';
  isEditing = false;

  // Özgeçmiş yükleme için
  selectedResumeFile: File | null = null;
  uploadingResume = false;
  resumeUploadError = '';
  resumeUploadSuccess = '';

  constructor(
    private fb: FormBuilder,
    private profileService: ProfileService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProfile();
  }

  initForm(): void {
    this.profileForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      linkedInUrl: [''],
      githubUrl: [''],
      portfolioUrl: [''],
      skills: [''],
      education: [''],
      experience: [''],
      preferredJobTypes: [''],
      preferredLocations: [''],
      minimumSalary: [''],
    });
  }

  loadProfile(): void {
    this.loading = true;
    this.error = '';

    this.profileService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.profileForm.patchValue(data);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Profil bilgileri yüklenirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  toggleEditMode(): void {
    this.isEditing = !this.isEditing;
    this.successMessage = '';

    if (!this.isEditing && this.profile) {
      // Edit modundan çıkıldığında formu sıfırla
      this.profileForm.patchValue(this.profile);
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    this.successMessage = '';

    const updatedProfile = {
      ...this.profile,
      ...this.profileForm.value
    };

    this.profileService.updateProfile(updatedProfile).subscribe({
      next: (data) => {
        this.profile = data;
        this.loading = false;
        this.isEditing = false;
        this.successMessage = 'Profil başarıyla güncellendi.';
      },
      error: (err) => {
        this.error = 'Profil güncellenirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  hasError(controlName: string, errorName: string): boolean {
    const control = this.profileForm.get(controlName);
    return !!(control && control.touched && control.hasError(errorName));
  }


  onResumeFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.selectedResumeFile = files[0];


      if (this.selectedResumeFile) {

        if (this.selectedResumeFile.type !== 'application/pdf') {
          this.resumeUploadError = 'Sadece PDF dosyaları yüklenebilir.';
          this.selectedResumeFile = null;
          return;
        }


        if (this.selectedResumeFile.size > 5 * 1024 * 1024) {
          this.resumeUploadError = 'Dosya boyutu 5MB\'dan küçük olmalıdır.';
          this.selectedResumeFile = null;
          return;
        }

        this.resumeUploadError = '';
      }
    }
  }

  uploadResume(): void {
    if (!this.selectedResumeFile) {
      return;
    }

    this.uploadingResume = true;
    this.resumeUploadError = '';
    this.resumeUploadSuccess = '';

    this.profileService.uploadResume(this.selectedResumeFile).subscribe({
      next: (resume) => {
        this.uploadingResume = false;
        this.resumeUploadSuccess = 'Özgeçmiş başarıyla yüklendi.';
        this.selectedResumeFile = null;


        this.loadProfile();


        const fileInput = document.getElementById('resumeFile') as HTMLInputElement;
        if (fileInput) {
          fileInput.value = '';
        }
      },
      error: (err) => {
        this.uploadingResume = false;
        this.resumeUploadError = 'Özgeçmiş yüklenirken bir hata oluştu.';
        console.error(err);
      }
    });
  }

  setDefaultResume(index: number): void {
    if (!this.profile || !this.profile.resumes || index >= this.profile.resumes.length) {
      return;
    }

    const resumeId = this.profile.resumes[index].id;

    this.profileService.setDefaultResume(resumeId).subscribe({
      next: () => {

        this.loadProfile();
      },
      error: (err) => {
        this.error = 'Varsayılan özgeçmiş ayarlanırken bir hata oluştu.';
        console.error(err);
      }
    });
  }

  deleteResume(index: number): void {
    if (!this.profile || !this.profile.resumes || index >= this.profile.resumes.length) {
      return;
    }

    if (!confirm('Bu özgeçmişi silmek istediğinize emin misiniz?')) {
      return;
    }

    const resumeId = this.profile.resumes[index].id;

    this.profileService.deleteResume(resumeId).subscribe({
      next: () => {

        this.loadProfile();
      },
      error: (err) => {
        this.error = 'Özgeçmiş silinirken bir hata oluştu.';
        console.error(err);
      }
    });
  }

  openResumeUploadDialog(): void {

    this.isEditing = true;


    setTimeout(() => {
      const fileInput = document.getElementById('resumeFile');
      if (fileInput) {
        fileInput.scrollIntoView({ behavior: 'smooth' });
      }
    }, 100);
  }
}
