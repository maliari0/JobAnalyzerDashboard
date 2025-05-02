import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { ForgotPasswordRequest } from '../models/auth.model';

@Component({
  selector: 'app-forgot-password-component',
  standalone: false,
  templateUrl: './forgot-password-component.component.html',
  styleUrl: './forgot-password-component.component.css'
})
export class ForgotPasswordComponentComponent implements OnInit {
  forgotPasswordForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  success = '';

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // Form oluştur
    this.forgotPasswordForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  // Form kontrollerine kolay erişim için getter
  get f() { return this.forgotPasswordForm.controls; }

  onSubmit(): void {
    this.submitted = true;

    // Form geçersizse işlemi durdur
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = '';

    // Şifre sıfırlama isteği oluştur
    const forgotPasswordRequest: ForgotPasswordRequest = {
      email: this.f['email'].value
    };

    // Şifre sıfırlama isteği gönder
    this.authService.forgotPassword(forgotPasswordRequest)
      .subscribe({
        next: (response) => {
          // Başarılı istek
          this.success = 'Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.';
          this.loading = false;
        },
        error: (error) => {
          // Hata durumu
          this.error = error.error?.message || 'Şifre sıfırlama isteği sırasında bir hata oluştu.';
          this.loading = false;
        }
      });
  }
}
