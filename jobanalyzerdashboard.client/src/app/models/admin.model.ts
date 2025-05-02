import { User } from './auth.model';

export interface UserListResponse {
  users: User[];
  totalCount: number;
}

export interface UserDeleteResponse {
  success: boolean;
  message: string;
}

export interface UserUpdateRequest {
  id: number;
  isActive: boolean;
  role: string;
}

export interface UserUpdateResponse {
  success: boolean;
  message: string;
  user: User;
}

export interface AdminDashboardStats {
  totalUsers: number;
  activeUsers: number;
  totalJobs: number;
  totalApplications: number;
}
