import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Profile, Resume } from '../models/profile.model';
import { ProfileService } from '../services/profile.service';
import { TURKEY_CITIES, COUNTRY_CODES } from '../shared/data/turkey-cities';

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

  // Ülke kodları ve şehirler
  countryCodes = COUNTRY_CODES;
  turkeyCities = TURKEY_CITIES;
  selectedCountryCode: string = '+90';
  selectedCities: string[] = [];

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
      countryCode: ['+90'],
      phoneNumber: ['', [Validators.pattern(/^\d{10}$/)]],
      linkedInUrl: [''],
      githubUrl: [''],
      portfolioUrl: [''],
      skills: [''],
      education: [''],
      experience: [''],
      preferredJobTypes: [''],
      preferredLocations: [[]],
      minimumSalary: [''],
    });
  }

  loadProfile(): void {
    this.loading = true;
    this.error = '';

    this.profileService.getProfile().subscribe({
      next: (data) => {
        console.log('Profil verisi:', data);
        console.log('Özgeçmişler:', data.resumes);

        // Özgeçmişleri kontrol et ve düzelt
        if (!data.resumes) {
          data.resumes = [];
        } else if (!Array.isArray(data.resumes)) {
          // Eğer resumes bir dizi değilse, boş dizi olarak ayarla
          console.error('Özgeçmişler bir dizi değil:', data.resumes);
          data.resumes = [];
        }

        this.profile = data;

        // Telefon numarasını parçalara ayır
        if (data.phone) {
          const phoneMatch = data.phone.match(/^(\+\d+)?\s*(\d+)$/);
          if (phoneMatch) {
            this.selectedCountryCode = phoneMatch[1] || '+90';
            const phoneNumber = phoneMatch[2];

            this.profileForm.patchValue({
              ...data,
              countryCode: this.selectedCountryCode,
              phoneNumber: phoneNumber
            });
          } else {
            this.profileForm.patchValue({
              ...data,
              countryCode: '+90',
              phoneNumber: data.phone
            });
          }
        } else {
          this.profileForm.patchValue(data);
        }

        // Tercih edilen lokasyonları ayarla
        this.selectedCities = [];
        if (data.preferredLocations && data.preferredLocations.trim() !== '') {
          try {
            const locations = JSON.parse(data.preferredLocations);
            if (Array.isArray(locations)) {
              this.selectedCities = locations;
              this.profileForm.get('preferredLocations')?.setValue(this.selectedCities);
            }
          } catch (error) {
            console.error('Lokasyon verisi ayrıştırılamadı:', error);
            // Hata durumunda boş dizi kullan
            this.selectedCities = [];
            this.profileForm.get('preferredLocations')?.setValue([]);
          }
        }

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

    const formValues = this.profileForm.value;

    // Telefon numarasını birleştir
    const phone = formValues.countryCode + ' ' + formValues.phoneNumber;

    // Tercih edilen lokasyonları JSON'a dönüştür
    const preferredLocations = JSON.stringify(formValues.preferredLocations || []);

    const updatedProfile = {
      ...this.profile,
      ...formValues,
      phone: phone,
      preferredLocations: preferredLocations
    };

    // Form değerlerinden eklediğimiz özel alanları kaldır
    delete updatedProfile.countryCode;
    delete updatedProfile.phoneNumber;

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

    // Varsayılan olarak işaretle (ilk yüklenen özgeçmiş varsayılan olsun)
    const isDefault = !this.profile?.resumes || this.profile.resumes.length === 0;

    this.profileService.uploadResume(this.selectedResumeFile, isDefault).subscribe({
      next: (resume) => {
        this.uploadingResume = false;
        this.resumeUploadSuccess = 'Özgeçmiş başarıyla yüklendi.';
        this.selectedResumeFile = null;

        // Profil bilgilerini yenile
        this.loadProfile();

        // Dosya input alanını temizle
        const fileInput = document.getElementById('resumeFile') as HTMLInputElement;
        if (fileInput) {
          fileInput.value = '';
        }

        // Kısa bir süre sonra düzenleme modundan çık
        setTimeout(() => {
          this.isEditing = false;
          this.successMessage = 'Özgeçmiş başarıyla yüklendi.';
        }, 1000);
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
    const resumeName = this.profile.resumes[index].fileName;

    this.profileService.setDefaultResume(resumeId).subscribe({
      next: () => {
        // Profil bilgilerini yenile
        this.loadProfile();

        // Başarı mesajı göster
        this.successMessage = `"${resumeName}" özgeçmişi varsayılan olarak ayarlandı.`;

        // Düzenleme modundaysa, görüntüleme moduna geç
        if (this.isEditing) {
          this.isEditing = false;
        }
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
    const resumeName = this.profile.resumes[index].fileName;

    this.profileService.deleteResume(resumeId).subscribe({
      next: () => {
        // Profil bilgilerini yenile
        this.loadProfile();

        // Başarı mesajı göster
        this.successMessage = `"${resumeName}" özgeçmişi başarıyla silindi.`;

        // Düzenleme modundaysa, görüntüleme moduna geç
        if (this.isEditing) {
          this.isEditing = false;
        }
      },
      error: (err) => {
        this.error = 'Özgeçmiş silinirken bir hata oluştu.';
        console.error(err);
      }
    });
  }

  openResumeUploadDialog(): void {
    // Düzenleme moduna geç
    this.isEditing = true;

    // Düzenleme moduna geçtikten sonra özgeçmiş yükleme bölümüne kaydır
    setTimeout(() => {
      const resumeUploadSection = document.getElementById('resumeUploadSection');
      if (resumeUploadSection) {
        resumeUploadSection.scrollIntoView({ behavior: 'smooth', block: 'center' });

        // Görsel olarak vurgulamak için
        resumeUploadSection.classList.add('highlight-section');
        setTimeout(() => {
          resumeUploadSection.classList.remove('highlight-section');
        }, 2000);
      } else {
        console.error('Özgeçmiş yükleme bölümü bulunamadı!');
      }
    }, 500); // Düzenleme moduna geçiş için daha uzun bir süre bekle
  }

  // CV dosyasının URL'sini düzeltmek için yardımcı metot
  getResumeUrl(filePath: string): string {
    if (!filePath) return '';

    // Eğer filePath zaten tam bir URL ise, olduğu gibi döndür
    if (filePath.startsWith('http://') || filePath.startsWith('https://')) {
      return filePath;
    }

    // Özel endpoint'i kullanarak URL oluştur
    // Dosya adını çıkar
    const fileName = filePath.split('/').pop();
    if (!fileName) return '';

    // API URL'sini kullanarak tam URL oluştur
    const currentUrl = window.location.href;
    const apiBaseUrl = currentUrl.substring(0, currentUrl.indexOf('/', 8)); // 8: "https://".length

    // Özel endpoint URL'sini oluştur
    const viewUrl = `${apiBaseUrl}/api/profile/resumes/view/${fileName}`;
    console.log('View URL:', viewUrl);

    return viewUrl;
  }
}
