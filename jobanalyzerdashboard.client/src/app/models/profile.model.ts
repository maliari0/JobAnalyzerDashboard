export interface Resume {
  id: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileType: string;
  uploadDate: string;
  isDefault: boolean;
}

export interface Profile {
  id: number;
  fullName: string;
  email: string;
  phone: string;
  linkedInUrl: string;
  githubUrl: string;
  portfolioUrl: string;
  skills: string;
  education: string;
  experience: string;
  preferredJobTypes: string;
  preferredLocations: string;
  minimumSalary: string;
  resumeFilePath: string; // Eski alan, geriye uyumluluk için

  // Özgeçmiş yönetimi için eklenen alanlar
  resumes?: Resume[]; // Birden fazla özgeçmiş desteği

  // N8n entegrasyonu için eklenen alanlar
  notionPageId?: string; // Notion'daki kullanıcı profil sayfası ID'si
  telegramChatId?: string; // Telegram chat ID'si
  preferredModel?: string; // Tercih edilen başvuru mesajı modeli
  technologyStack?: string; // Teknoloji stack'i
  position?: string; // Güncel pozisyon
  preferredCategories?: string[]; // Tercih edilen kategoriler (frontend, backend, vb.)
  minQualityScore?: number; // Minimum kalite puanı (1-5 arası)
  autoApplyEnabled?: boolean; // Otomatik başvuru etkin mi?
}
