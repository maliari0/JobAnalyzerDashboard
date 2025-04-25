import { Component, OnInit } from '@angular/core';
import { WebhookService } from '../services/webhook.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-n8n-test',
  templateUrl: './n8n-test.component.html',
  styleUrls: ['./n8n-test.component.css'],
  standalone: false
})
export class N8nTestComponent implements OnInit {
  testResult: any = null;
  loading = false;
  error = '';

  // N8n webhook URL'i
  webhookUrl = 'https://n8n-service-a2yz.onrender.com/webhook/job-intake';

  // Test verisi
  testData = {
    title: 'Junior Frontend Developer',
    description: 'Merhaba, şirketimiz remote çalışacak React.js bilen bir junior frontend geliştirici arıyor. Maaş aralığı 35.000 - 45.000 TL\'dir. Başvurular için wotimi2216@f5url.com adresine mail atabilirsiniz.',
    employment_type: 'remote',
    company_website: 'https://webstartup.com',
    contact_email: 'wotimi2216@f5url.com',
    url: 'https://webstartup.com/careers/frontend-jr-2024'
  };

  constructor(private webhookService: WebhookService, private http: HttpClient) { }

  ngOnInit(): void {
  }

  testWebhook(): void {
    this.loading = true;
    this.error = '';
    this.testResult = null;

    this.webhookService.testWebhook().subscribe({
      next: (result) => {
        this.testResult = result;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Webhook test edilirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  sendTestData(): void {
    this.loading = true;
    this.error = '';
    this.testResult = null;

    // Doğrudan n8n webhook URL'ine istek gönder
    this.http.post(this.webhookUrl, this.testData).subscribe({
      next: (result) => {
        this.testResult = result;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Test verisi gönderilirken bir hata oluştu: ' + (err.message || err.statusText);
        this.loading = false;
        console.error(err);

        // Hata durumunda bile yanıtı göster
        if (err.error) {
          this.testResult = {
            error: err.error,
            status: err.status,
            message: err.message
          };
        }
      }
    });
  }

  // Webhook URL'ini güncelle
  updateWebhookUrl(url: string): void {
    if (url && url.trim() !== '') {
      this.webhookUrl = url.trim();
    }
  }
}
