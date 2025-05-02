import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { EmailConfirmationRequest } from '../models/auth.model';

@Component({
  selector: 'app-confirm-email-component',
  standalone: false,
  templateUrl: './confirm-email-component.component.html',
  styleUrl: './confirm-email-component.component.css'
})
export class ConfirmEmailComponentComponent implements OnInit {
  loading = true;
  error = '';
  success = '';
  token: string = '';
  email: string = '';

  constructor(
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
      this.error = 'Geçersiz e-posta doğrulama bağlantısı.';
      this.loading = false;
      return;
    }

    // E-posta doğrulama isteği oluştur
    const confirmEmailRequest: EmailConfirmationRequest = {
      token: this.token,
      email: this.email
    };

    // E-posta doğrulama isteği gönder
    this.authService.confirmEmail(confirmEmailRequest)
      .subscribe({
        next: (response) => {
          // Başarılı istek
          this.success = 'E-posta adresiniz başarıyla doğrulandı.';
          this.loading = false;
          
          // 3 saniye sonra giriş sayfasına yönlendir
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        },
        error: (error) => {
          // Hata durumu
          this.error = error.error?.message || 'E-posta doğrulama sırasında bir hata oluştu.';
          this.loading = false;
        }
      });
  }
}
