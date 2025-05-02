import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../services/admin.service';
import { User } from '../../models/auth.model';
import { UserUpdateRequest } from '../../models/admin.model';

@Component({
  selector: 'app-user-management',
  standalone: false,
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  totalUsers: number = 0;
  currentPage: number = 1;
  pageSize: number = 10;
  loading: boolean = false;
  error: string | null = null;

  // Düzenleme modu
  editingUser: User | null = null;

  // Rol seçenekleri
  roles: string[] = ['User', 'Admin'];

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;

    this.adminService.getUsers(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.users = data.users;
        this.totalUsers = data.totalCount;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading users:', err);
        this.error = 'Kullanıcılar yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  // Math nesnesi için getter
  get Math() {
    return Math;
  }

  startEdit(user: User): void {
    this.editingUser = { ...user };
  }

  cancelEdit(): void {
    this.editingUser = null;
  }

  saveUser(): void {
    if (!this.editingUser) return;

    const updateData: UserUpdateRequest = {
      id: this.editingUser.id,
      isActive: this.editingUser.isActive,
      role: this.editingUser.role
    };

    this.loading = true;

    this.adminService.updateUser(updateData).subscribe({
      next: (response) => {
        // Kullanıcı listesini güncelle
        const index = this.users.findIndex(u => u.id === response.user.id);
        if (index !== -1) {
          this.users[index] = response.user;
        }

        this.editingUser = null;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error updating user:', err);
        this.error = 'Kullanıcı güncellenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  deleteUser(email: string): void {
    if (!confirm(`${email} e-posta adresine sahip kullanıcıyı silmek istediğinizden emin misiniz?`)) {
      return;
    }

    this.loading = true;

    this.adminService.deleteUser(email).subscribe({
      next: (response) => {
        // Kullanıcı listesini güncelle
        this.users = this.users.filter(u => u.email !== email);
        this.loading = false;
      },
      error: (err) => {
        console.error('Error deleting user:', err);
        this.error = 'Kullanıcı silinirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }
}
