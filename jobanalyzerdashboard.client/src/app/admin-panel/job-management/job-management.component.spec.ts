import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobManagementComponent } from './job-management.component';

describe('JobManagementComponent', () => {
  let component: JobManagementComponent;
  let fixture: ComponentFixture<JobManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JobManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
