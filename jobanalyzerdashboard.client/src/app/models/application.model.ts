import { Job } from './job.model';

export interface Application {
  id: number;
  jobId: number;
  appliedDate: string;
  status: string; // Pending, Accepted, Rejected, Interview
  responseDetails?: string;
  responseDate?: string;
  appliedMethod: string; // Email, Form, API, n8n
  job?: Job;

  // N8n entegrasyonu için eklenen alanlar
  sentMessage?: string; // Gönderilen başvuru mesajı
  isAutoApplied?: boolean; // Otomatik başvuru yapıldı mı?
  notionPageId?: string; // Notion'daki sayfa ID'si
  cvAttached?: boolean; // CV eklendi mi?
  telegramNotificationSent?: string; // Telegram bildirimi gönderildi mi?

  // İş ilanı silinmiş mi?
  isJobDeleted?: boolean;

  // LLM tarafından oluşturulan e-posta içeriği
  emailContent?: string;
}
