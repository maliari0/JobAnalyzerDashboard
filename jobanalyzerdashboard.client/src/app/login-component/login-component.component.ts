import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { LoginRequest } from '../models/auth.model';

@Component({
  selector: 'app-login-component',
  standalone: false,
  templateUrl: './login-component.component.html',
  styleUrl: './login-component.component.css'
})
export class LoginComponentComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  submitted = false;
  error = '';
  returnUrl: string = '/';

  constructor(
    private formBuilder: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService
  ) {
    // Kullanıcı zaten giriş yapmışsa ana sayfaya yönlendir
    if (this.authService.isLoggedIn) {
      this.router.navigate(['/']);
    }
  }

  ngOnInit(): void {
    // Form oluştur
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    // Giriş sonrası yönlendirilecek URL'i al
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  // Form kontrollerine kolay erişim için getter
  get f() { return this.loginForm.controls; }

  onSubmit(): void {
    this.submitted = true;

    // Form geçersizse işlemi durdur
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    // Giriş isteği oluştur
    const loginRequest: LoginRequest = {
      email: this.f['email'].value,
      password: this.f['password'].value
    };

    // Giriş yap
    this.authService.login(loginRequest)
      .subscribe({
        next: (response) => {
          if (response.success) {
            // Başarılı giriş, yönlendir
            this.router.navigate([this.returnUrl]);
          } else {
            // Başarısız giriş
            this.error = response.message;
            this.loading = false;
          }
        },
        error: (error) => {
          // Hata durumu
          this.error = error.error?.message || 'Giriş sırasında bir hata oluştu.';
          this.loading = false;
        }
      });
  }
}
