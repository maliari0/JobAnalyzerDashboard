import { Component, OnInit } from '@angular/core';
import { JobService } from '../../services/job.service';
import { Job } from '../../models/job.model';

@Component({
  selector: 'app-job-management',
  standalone: false,
  templateUrl: './job-management.component.html',
  styleUrl: './job-management.component.css'
})
export class JobManagementComponent implements OnInit {
  // Veri
  jobs: Job[] = [];
  selectedJob: Job | null = null;
  jobToDelete: Job | null = null;

  // Filtreleme ve sıralama
  searchTerm: string = '';
  categoryFilter: string = '';
  sortBy: string = 'createdAt';
  sortDirection: string = 'desc';
  categories: string[] = [];

  // Sayfalama
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  totalPages: number = 0;

  // UI durumu
  loading: boolean = false;
  error: string | null = null;

  constructor(private jobService: JobService) { }

  ngOnInit(): void {
    this.loadJobs();
    this.loadCategories();
  }

  loadJobs(): void {
    this.loading = true;
    this.error = null;

    this.jobService.getJobs().subscribe({
      next: (data: Job[]) => {
        // Filtreleme ve sıralama işlemlerini client tarafında yapalım
        let filteredJobs = [...data];

        // Kategori filtresi
        if (this.categoryFilter) {
          filteredJobs = filteredJobs.filter(job => job.category === this.categoryFilter);
        }

        // Arama filtresi
        if (this.searchTerm) {
          const searchLower = this.searchTerm.toLowerCase();
          filteredJobs = filteredJobs.filter(job =>
            job.title.toLowerCase().includes(searchLower) ||
            job.company.toLowerCase().includes(searchLower) ||
            job.description.toLowerCase().includes(searchLower)
          );
        }

        // Sıralama
        filteredJobs.sort((a, b) => {
          const aValue = a[this.sortBy as keyof Job];
          const bValue = b[this.sortBy as keyof Job];

          if (typeof aValue === 'string' && typeof bValue === 'string') {
            return this.sortDirection === 'asc'
              ? aValue.localeCompare(bValue)
              : bValue.localeCompare(aValue);
          } else {
            // Sayısal değerler için
            const aNum = aValue as number;
            const bNum = bValue as number;
            return this.sortDirection === 'asc' ? aNum - bNum : bNum - aNum;
          }
        });

        this.jobs = filteredJobs;
        this.totalItems = filteredJobs.length;
        this.totalPages = Math.ceil(this.totalItems / this.pageSize);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading jobs:', err);
        this.error = 'İş ilanları yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  loadCategories(): void {
    this.jobService.getCategories().subscribe({
      next: (data: string[]) => {
        this.categories = data;
      },
      error: (err: any) => {
        console.error('Error loading categories:', err);
      }
    });
  }

  viewJobDetails(jobId: number): void {
    this.loading = true;

    this.jobService.getJobById(jobId).subscribe({
      next: (job) => {
        this.selectedJob = job;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading job details:', err);
        this.error = 'İş ilanı detayları yüklenirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  confirmDeleteJob(job: Job): void {
    this.jobToDelete = job;
  }

  deleteJob(): void {
    if (!this.jobToDelete) return;

    this.loading = true;

    this.jobService.deleteJob(this.jobToDelete.id).subscribe({
      next: () => {
        // İş ilanını listeden kaldır
        this.jobs = this.jobs.filter(j => j.id !== this.jobToDelete!.id);

        // Modalları kapat
        this.jobToDelete = null;
        this.selectedJob = null;

        this.loading = false;
      },
      error: (err) => {
        console.error('Error deleting job:', err);
        this.error = 'İş ilanı silinirken bir hata oluştu.';
        this.loading = false;
      }
    });
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.loadJobs();
  }
}
