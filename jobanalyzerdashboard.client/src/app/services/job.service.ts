import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { Job } from '../models/job.model';

export interface JobFilter {
  category?: string;
  minQualityScore?: number;
  maxQualityScore?: number;
  tag?: string;
  actionSuggestion?: string;
  minSalary?: number;
  employmentType?: string;
  isApplied?: boolean;
  sortBy?: string;
  sortDirection?: string;
}

export interface JobStats {
  totalJobs: number;
  appliedJobs: number;
  highQualityJobs: number;
  averageSalary: number;
  categoryBreakdown: { category: string, count: number }[];
  qualityScoreBreakdown: { score: number, count: number }[];
}

export interface ApplicationRequest {
  method?: string;
  message?: string;
  attachCV?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private apiUrl = '/api/job';

  constructor(private http: HttpClient) { }

  getJobs(filter?: JobFilter): Observable<Job[]> {
    let params = new HttpParams();

    if (filter) {
      if (filter.category) params = params.set('category', filter.category);
      if (filter.minQualityScore) params = params.set('minQualityScore', filter.minQualityScore.toString());
      if (filter.maxQualityScore) params = params.set('maxQualityScore', filter.maxQualityScore.toString());
      if (filter.tag) params = params.set('tag', filter.tag);
      if (filter.actionSuggestion) params = params.set('actionSuggestion', filter.actionSuggestion);
      if (filter.minSalary) params = params.set('minSalary', filter.minSalary.toString());
      if (filter.employmentType) params = params.set('employmentType', filter.employmentType);
      if (filter.isApplied !== undefined) params = params.set('isApplied', filter.isApplied.toString());
      if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
      if (filter.sortDirection) params = params.set('sortDirection', filter.sortDirection);
    }

    return this.http.get<Job[]>(this.apiUrl, { params })
      .pipe(
        map(jobs => this.processJobsData(jobs))
      );
  }

  getJobById(id: number): Observable<Job> {
    return this.http.get<Job>(`${this.apiUrl}/${id}`)
      .pipe(
        map(job => this.processJobData(job))
      );
  }

  // Job verilerini işleyen yardımcı metot
  private processJobData(job: Job): Job {
    try {
      // Tags alanını işle
      if (job && job.tags) {
        // Tags string ise ve JSON formatında ise parse et
        if (typeof job.tags === 'string') {
          try {
            job.parsedTags = JSON.parse(job.tags);
          } catch (e) {
            // Parse hatası durumunda boş dizi ata
            job.parsedTags = [];
            console.warn('Tags parse edilemedi:', job.tags);
          }
        } else if (Array.isArray(job.tags)) {
          // Eğer zaten dizi ise doğrudan ata
          job.parsedTags = job.tags as any;
        }
      } else {
        job.parsedTags = [];
      }

      // Türkçe karakter düzeltmeleri
      if (job.title) {
        job.title = this.fixTurkishCharacters(job.title);
      }

      if (job.description) {
        job.description = this.fixTurkishCharacters(job.description);
      }

      if (job.company) {
        job.company = this.fixTurkishCharacters(job.company);
      }

      if (job.location) {
        job.location = this.fixTurkishCharacters(job.location);
      }

      // Şirket adını kontrol et
      if (job && job.company === "Bilinmeyen Şirket" && job.description) {
        // Açıklamadan şirket adını çıkarmaya çalış
        const companyMatch = job.description.match(/([A-Za-z0-9\s]+) (şirketimiz|firması|şirketi)/i);

        if (companyMatch && companyMatch[1]) {
          job.company = companyMatch[1].trim();
        }
      }

      // Açıklamadan şirket adını çıkarmaya çalış (alternatif yöntem)
      if (job && job.company === "Bilinmeyen Şirket" && job.description) {
        const words = job.description.split(' ');
        for (let i = 0; i < words.length - 1; i++) {
          if (words[i].toLowerCase() === 'merhaba,' && i + 1 < words.length) {
            // "Merhaba, [Şirket Adı]" formatını kontrol et
            const potentialCompany = words[i + 1];
            if (potentialCompany && potentialCompany.length > 2 &&
                potentialCompany !== 'ben' &&
                potentialCompany !== 'biz' &&
                potentialCompany !== 'size') {
              job.company = potentialCompany;
              break;
            }
          }
        }
      }
    } catch (e) {
      console.error('Job verisi işlenirken hata oluştu:', e);
    }
    return job;
  }

  // Türkçe karakter düzeltme metodu
  private fixTurkishCharacters(text: string): string {
    if (!text) return text;

    return text
      .replace(/\uFFFD/g, 'ç') // Bozuk ç karakteri
      .replace(/\u00E7/g, 'ç') // Latin-1 ç karakteri
      .replace(/\u00C7/g, 'Ç') // Latin-1 Ç karakteri
      .replace(/\u011F/g, 'ğ') // Latin Extended-A ğ karakteri
      .replace(/\u011E/g, 'Ğ') // Latin Extended-A Ğ karakteri
      .replace(/\u0131/g, 'ı') // Latin Extended-A ı karakteri
      .replace(/\u0130/g, 'İ') // Latin Extended-A İ karakteri
      .replace(/\u00F6/g, 'ö') // Latin-1 ö karakteri
      .replace(/\u00D6/g, 'Ö') // Latin-1 Ö karakteri
      .replace(/\u015F/g, 'ş') // Latin Extended-A ş karakteri
      .replace(/\u015E/g, 'Ş') // Latin Extended-A Ş karakteri
      .replace(/\u00FC/g, 'ü') // Latin-1 ü karakteri
      .replace(/\u00DC/g, 'Ü') // Latin-1 Ü karakteri
      .replace(/T\uFFFDrk/g, 'Türk') // Bozuk "Türk" kelimesi
      .replace(/t\uFFFDrk/g, 'türk') // Bozuk "türk" kelimesi
      .replace(/\uFFFDal/g, 'çal') // Bozuk "çal" hecesi
      .replace(/\uFFFDi/g, 'çi') // Bozuk "çi" hecesi
      .replace(/i\uFFFDin/g, 'için') // Bozuk "için" kelimesi
      .replace(/\uFFFDe/g, 'çe') // Bozuk "çe" hecesi
      .replace(/\uFFFDa/g, 'ça') // Bozuk "ça" hecesi
      .replace(/\uFFFDo/g, 'ço') // Bozuk "ço" hecesi
      .replace(/\uFFFDu/g, 'çu') // Bozuk "çu" hecesi
      .replace(/\uFFFD/g, '') // Diğer bozuk karakterleri temizle
      .replace(/sirket/g, 'şirket') // "sirket" -> "şirket"
      .replace(/Sirket/g, 'Şirket') // "Sirket" -> "Şirket"
      .replace(/calisacak/g, 'çalışacak') // "calisacak" -> "çalışacak"
      .replace(/Calisacak/g, 'Çalışacak') // "Calisacak" -> "Çalışacak"
      .replace(/gelistirici/g, 'geliştirici') // "gelistirici" -> "geliştirici"
      .replace(/Gelistirici/g, 'Geliştirici') // "Gelistirici" -> "Geliştirici"
      .replace(/maas/g, 'maaş') // "maas" -> "maaş"
      .replace(/Maas/g, 'Maaş') // "Maas" -> "Maaş"
      .replace(/araligi/g, 'aralığı') // "araligi" -> "aralığı"
      .replace(/Araligi/g, 'Aralığı') // "Araligi" -> "Aralığı"
      .replace(/basvuru/g, 'başvuru') // "basvuru" -> "başvuru"
      .replace(/Basvuru/g, 'Başvuru') // "Basvuru" -> "Başvuru"
      .replace(/icin/g, 'için') // "icin" -> "için"
      .replace(/Icin/g, 'İçin'); // "Icin" -> "İçin"
  }

  // Birden fazla job için process işlemi
  private processJobsData(jobs: Job[]): Job[] {
    return jobs.map(job => this.processJobData(job));
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  getTags(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/tags`);
  }

  getStats(): Observable<JobStats> {
    return this.http.get<JobStats>(`${this.apiUrl}/stats`);
  }

  applyToJob(id: number, request?: ApplicationRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/apply/${id}`, request || {});
  }

  webhookNotify(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/webhook-notify/${id}`, {});
  }

  autoApply(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auto-apply/${id}`, {});
  }

  deleteJob(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
