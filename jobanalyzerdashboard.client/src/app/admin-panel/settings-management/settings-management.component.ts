import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

interface Integration {
  active: boolean;
  webhookUrl?: string;
  apiKey?: string;
  botToken?: string;
  chatId?: string;
}

interface Integrations {
  n8n: Integration;
  telegram: Integration;
}

@Component({
  selector: 'app-settings-management',
  standalone: false,
  templateUrl: './settings-management.component.html',
  styleUrl: './settings-management.component.css'
})
export class SettingsManagementComponent implements OnInit {
  activeTab: string = 'general';
  loading: boolean = false;
  error: string | null = null;
  successMessage: string | null = null;

  // Genel ayarlar formu
  generalSettingsForm: FormGroup;

  // E-posta şablonu formu
  emailTemplateForm: FormGroup;
  selectedTemplateType: string = 'application';

  // Entegrasyonlar
  integrations: Integrations = {
    n8n: {
      active: false,
      webhookUrl: 'https://n8n-service-a2yz.onrender.com/webhook/apply-auto',
      apiKey: ''
    },
    telegram: {
      active: false,
      botToken: '',
      chatId: ''
    }
  };

  constructor(private fb: FormBuilder) {
    // Genel ayarlar formu oluştur
    this.generalSettingsForm = this.fb.group({
      appName: ['Job Analyzer Dashboard', Validators.required],
      baseUrl: ['https://dory-assuring-rattler.ngrok-free.app', Validators.required],
      defaultPageSize: [10, [Validators.required, Validators.min(5), Validators.max(100)]],
      autoApplyEnabled: [true],
      minQualityScore: [70, [Validators.required, Validators.min(0), Validators.max(100)]]
    });

    // E-posta şablonu formu oluştur
    this.emailTemplateForm = this.fb.group({
      subject: ['', Validators.required],
      body: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadSettings();
  }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }

  loadSettings(): void {
    this.loading = true;

    // Normalde burada API'den ayarları yükleriz
    // Şimdilik varsayılan değerlerle dolduruyoruz

    setTimeout(() => {
      this.loading = false;
    }, 500);
  }

  saveGeneralSettings(): void {
    if (this.generalSettingsForm.invalid) return;

    this.loading = true;

    // Normalde burada API'ye ayarları kaydederiz
    // Şimdilik sadece başarılı mesajı gösteriyoruz

    setTimeout(() => {
      this.successMessage = 'Genel ayarlar başarıyla kaydedildi.';
      this.loading = false;

      // 3 saniye sonra başarı mesajını temizle
      setTimeout(() => {
        this.successMessage = null;
      }, 3000);
    }, 500);
  }

  resetGeneralSettings(): void {
    this.generalSettingsForm.reset({
      appName: 'Job Analyzer Dashboard',
      baseUrl: 'https://dory-assuring-rattler.ngrok-free.app',
      defaultPageSize: 10,
      autoApplyEnabled: true,
      minQualityScore: 70
    });
  }

  loadEmailTemplate(): void {
    this.loading = true;

    // Normalde burada API'den şablonu yükleriz
    // Şimdilik varsayılan değerlerle dolduruyoruz

    let subject = '';
    let body = '';

    switch (this.selectedTemplateType) {
      case 'application':
        subject = 'İş Başvurusu: {position} Pozisyonu';
        body = 'Sayın Yetkili,\n\nİsim: {name}\n\n{company} firmasında açılan {position} pozisyonu için başvuruda bulunmak istiyorum.\n\nÖzgeçmişim ekte yer almaktadır.\n\nSaygılarımla,\n{name}';
        break;
      case 'confirmation':
        subject = 'E-posta Doğrulama';
        body = 'Merhaba {name},\n\nJob Analyzer Dashboard hesabınızı doğrulamak için aşağıdaki bağlantıya tıklayın.\n\n{confirmationLink}\n\nSaygılarımla,\nJob Analyzer Dashboard Ekibi';
        break;
      case 'password-reset':
        subject = 'Şifre Sıfırlama';
        body = 'Merhaba {name},\n\nJob Analyzer Dashboard şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın.\n\n{resetLink}\n\nSaygılarımla,\nJob Analyzer Dashboard Ekibi';
        break;
    }

    this.emailTemplateForm.setValue({
      subject,
      body
    });

    setTimeout(() => {
      this.loading = false;
    }, 500);
  }

  saveEmailTemplate(): void {
    if (this.emailTemplateForm.invalid) return;

    this.loading = true;

    // Normalde burada API'ye şablonu kaydederiz
    // Şimdilik sadece başarılı mesajı gösteriyoruz

    setTimeout(() => {
      this.successMessage = 'E-posta şablonu başarıyla kaydedildi.';
      this.loading = false;

      // 3 saniye sonra başarı mesajını temizle
      setTimeout(() => {
        this.successMessage = null;
      }, 3000);
    }, 500);
  }

  resetEmailTemplate(): void {
    this.loadEmailTemplate();
  }

  saveIntegration(type: string): void {
    this.loading = true;

    // Normalde burada API'ye entegrasyon ayarlarını kaydederiz
    // Şimdilik sadece başarılı mesajı gösteriyoruz

    setTimeout(() => {
      this.successMessage = `${type === 'n8n' ? 'n8n Otomasyonu' : 'Telegram Bildirimleri'} ayarları başarıyla kaydedildi.`;
      this.loading = false;

      // 3 saniye sonra başarı mesajını temizle
      setTimeout(() => {
        this.successMessage = null;
      }, 3000);
    }, 500);
  }

  testIntegration(type: string): void {
    this.loading = true;

    // Normalde burada API'ye test isteği göndeririz
    // Şimdilik sadece başarılı mesajı gösteriyoruz

    setTimeout(() => {
      this.successMessage = `${type === 'n8n' ? 'n8n Otomasyonu' : 'Telegram Bildirimleri'} bağlantısı başarıyla test edildi.`;
      this.loading = false;

      // 3 saniye sonra başarı mesajını temizle
      setTimeout(() => {
        this.successMessage = null;
      }, 3000);
    }, 500);
  }

  toggleIntegration(type: string): void {
    if (type === 'n8n') {
      this.integrations.n8n.active = !this.integrations.n8n.active;
    } else if (type === 'telegram') {
      this.integrations.telegram.active = !this.integrations.telegram.active;
    }

    this.saveIntegration(type);
  }
}
