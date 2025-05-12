export interface Announcement {
  id: number;
  title: string;
  content: string;
  isActive: boolean;
  createdAt: string;
  expiresAt?: string;
  type: 'info' | 'warning' | 'success' | 'error';
  createdById?: number;
}

export interface AnnouncementCreateModel {
  title: string;
  content: string;
  isActive: boolean;
  expiresAt?: string;
  type: 'info' | 'warning' | 'success' | 'error';
}

export interface AnnouncementUpdateModel {
  title: string;
  content: string;
  isActive: boolean;
  expiresAt?: string;
  type: 'info' | 'warning' | 'success' | 'error';
}
