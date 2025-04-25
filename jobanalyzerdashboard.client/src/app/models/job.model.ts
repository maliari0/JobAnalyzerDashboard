export interface Job {
  id: number;
  title: string;
  description: string;
  company: string;
  location: string;
  employmentType: string;
  salary: string;
  postedDate: string;
  applicationDeadline?: string;
  qualityScore: number; // 1-5 arası puan
  source: string; // Email, Webhook, n8n
  isApplied: boolean;
  appliedDate?: string;

  // N8n entegrasyonu için eklenen alanlar
  actionSuggestion: string; // sakla, bildir, ilgisiz
  category: string; // frontend, backend, mobile, devops, data science, diğer
  tags: string[]; // remote, junior, b2b, vb.
  companyWebsite: string;
  contactEmail: string;
  url: string;
  isJobPosting: boolean; // İş ilanı mı?
  parsedMinSalary: number; // Maaş aralığından çıkarılan minimum maaş değeri
}
