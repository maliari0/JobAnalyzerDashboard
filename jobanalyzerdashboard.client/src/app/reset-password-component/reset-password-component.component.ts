import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ResetPasswordRequest } from '../models/auth.model';

@Component({
  selector: 'app-reset-password-component',
  standalone: false,
  templateUrl: './reset-password-component.component.html',
  styleUrl: './reset-password-component.component.css'
})
export class ResetPasswordComponentComponent implements OnInit {
  resetPasswordForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  success = '';
  token: string = '';
  email: string = '';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // URL'den token ve email parametrelerini al
    this.token = this.route.snapshot.queryParams['token'] || '';
    this.email = this.route.snapshot.queryParams['email'] || '';

    // Parametreler eksikse hata göster
    if (!this.token || !this.email) {
      this.error = 'Geçersiz şifre sıfırlama bağlantısı.';
    }

    // Form oluştur
    this.resetPasswordForm = this.formBuilder.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, {
      validator: this.passwordMatchValidator
    });
  }

  // Şifre eşleşme kontrolü
  passwordMatchValidator(formGroup: FormGroup) {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      formGroup.get('confirmPassword')?.setErrors({ passwordMismatch: true });
    } else {
      formGroup.get('confirmPassword')?.setErrors(null);
    }
  }

  // Form kontrollerine kolay erişim için getter
  get f() { return this.resetPasswordForm.controls; }

  onSubmit(): void {
    this.submitted = true;

    // Form geçersizse işlemi durdur
    if (this.resetPasswordForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    // Şifre sıfırlama isteği oluştur
    const resetPasswordRequest: ResetPasswordRequest = {
      token: this.token,
      email: this.email,
      password: this.f['password'].value,
      confirmPassword: this.f['confirmPassword'].value
    };

    // Şifre sıfırlama isteği gönder
    this.authService.resetPassword(resetPasswordRequest)
      .subscribe({
        next: (response) => {
          // Başarılı istek
          this.success = 'Şifreniz başarıyla sıfırlandı.';
          
          // 3 saniye sonra giriş sayfasına yönlendir
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        },
        error: (error) => {
          // Hata durumu
          this.error = error.error?.message || 'Şifre sıfırlama sırasında bir hata oluştu.';
          this.loading = false;
        }
      });
  }
}
