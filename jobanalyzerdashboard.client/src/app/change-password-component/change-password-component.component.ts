import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ChangePasswordRequest } from '../models/auth.model';

@Component({
  selector: 'app-change-password-component',
  standalone: false,
  templateUrl: './change-password-component.component.html',
  styleUrl: './change-password-component.component.css'
})
export class ChangePasswordComponentComponent implements OnInit {
  changePasswordForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  success = '';

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
    if (!this.authService.isLoggedIn) {
      this.router.navigate(['/login']);
    }
  }

  ngOnInit(): void {
    // Form oluştur
    this.changePasswordForm = this.formBuilder.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: this.passwordMatchValidator
    });
  }

  // Şifre eşleşme kontrolü
  passwordMatchValidator(formGroup: FormGroup) {
    const newPassword = formGroup.get('newPassword')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;

    if (newPassword !== confirmPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordMismatch: true });
    } else {
      formGroup.get('confirmPassword')?.setErrors(null);
    }
  }

  // Form kontrollerine kolay erişim için getter
  get f() { return this.changePasswordForm.controls; }

  onSubmit(): void {
    this.submitted = true;

    // Form geçersizse işlemi durdur
    if (this.changePasswordForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    // Şifre değiştirme isteği oluştur
    const changePasswordRequest: ChangePasswordRequest = {
      currentPassword: this.f['currentPassword'].value,
      newPassword: this.f['newPassword'].value,
      confirmPassword: this.f['confirmPassword'].value
    };

    // Şifre değiştirme isteği gönder
    this.authService.changePassword(changePasswordRequest)
      .subscribe({
        next: (response) => {
          // Başarılı istek
          this.success = 'Şifreniz başarıyla değiştirildi.';
          this.changePasswordForm.reset();
          this.submitted = false;
          this.loading = false;
        },
        error: (error) => {
          // Hata durumu
          this.error = error.error?.message || 'Şifre değiştirme sırasında bir hata oluştu.';
          this.loading = false;
        }
      });
  }
}
