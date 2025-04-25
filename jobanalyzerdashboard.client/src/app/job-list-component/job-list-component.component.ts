import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Job } from '../models/job.model';
import { JobFilter, JobService, JobStats } from '../services/job.service';

@Component({
  selector: 'app-job-list-component',
  standalone: false,
  templateUrl: './job-list-component.component.html',
  styleUrl: './job-list-component.component.css'
})
export class JobListComponentComponent implements OnInit {
  jobs: Job[] = [];
  loading = false;
  error = '';
  stats: JobStats | null = null;
  categories: string[] = [];
  tags: string[] = [];

  // Filtre seçenekleri
  filter: JobFilter = {};
  selectedCategory: string = '';
  selectedTag: string = '';
  selectedQualityScore: number | null = null;
  selectedActionSuggestion: string = '';
  showOnlyApplied: boolean = false;
  sortBy: string = 'date';
  sortDirection: string = 'desc';

  constructor(private jobService: JobService, private router: Router) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadTags();
    this.loadStats();
    this.loadJobs();
  }

  loadJobs(): void {
    this.loading = true;
    this.error = '';

    // Filtre nesnesini oluştur
    this.updateFilter();

    this.jobService.getJobs(this.filter).subscribe({
      next: (data) => {
        this.jobs = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'İş ilanları yüklenirken bir hata oluştu.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadCategories(): void {
    this.jobService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Kategoriler yüklenirken hata oluştu:', err);
      }
    });
  }

  loadTags(): void {
    this.jobService.getTags().subscribe({
      next: (data) => {
        this.tags = data;
      },
      error: (err) => {
        console.error('Etiketler yüklenirken hata oluştu:', err);
      }
    });
  }

  loadStats(): void {
    this.jobService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
      },
      error: (err) => {
        console.error('İstatistikler yüklenirken hata oluştu:', err);
      }
    });
  }

  updateFilter(): void {
    this.filter = {};

    if (this.selectedCategory) {
      this.filter.category = this.selectedCategory;
    }

    if (this.selectedTag) {
      this.filter.tag = this.selectedTag;
    }

    if (this.selectedQualityScore !== null) {
      this.filter.minQualityScore = this.selectedQualityScore;
    }

    if (this.selectedActionSuggestion) {
      this.filter.actionSuggestion = this.selectedActionSuggestion;
    }

    if (this.showOnlyApplied) {
      this.filter.isApplied = true;
    }

    this.filter.sortBy = this.sortBy;
    this.filter.sortDirection = this.sortDirection;
  }

  applyFilters(): void {
    this.loadJobs();
  }

  resetFilters(): void {
    this.selectedCategory = '';
    this.selectedTag = '';
    this.selectedQualityScore = null;
    this.selectedActionSuggestion = '';
    this.showOnlyApplied = false;
    this.sortBy = 'date';
    this.sortDirection = 'desc';

    this.loadJobs();
  }

  viewJobDetails(jobId: number): void {
    this.router.navigate(['/job', jobId]);
  }

  getQualityScoreClass(score: number): string {
    if (score >= 4) return 'high-quality';
    if (score >= 3) return 'medium-quality';
    return 'low-quality';
  }

  getActionSuggestionClass(action: string): string {
    switch (action.toLowerCase()) {
      case 'sakla': return 'action-store';
      case 'bildir': return 'action-notify';
      case 'ilgisiz': return 'action-ignore';
      default: return '';
    }
  }

  getActionSuggestionText(action: string): string {
    switch (action.toLowerCase()) {
      case 'sakla': return 'Sakla';
      case 'bildir': return 'Bildir';
      case 'ilgisiz': return 'İlgisiz';
      default: return action;
    }
  }
}
