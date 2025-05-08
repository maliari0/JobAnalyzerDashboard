import { Job } from './job.model';

export interface Application {
  id: number;
  jobId: number;
  appliedDate: string;
  status: string;
  responseDetails?: string;
  responseDate?: string;
  appliedMethod: string;
  job?: Job;

  jobTitle?: string;
  company?: string;
  notes?: string;

  sentMessage?: string;
  isAutoApplied?: boolean;
  notionPageId?: string;
  cvAttached?: boolean;
  telegramNotificationSent?: string;

  isJobDeleted?: boolean;
  emailContent?: string;
}
