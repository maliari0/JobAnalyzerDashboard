import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobListComponentComponent } from './job-list-component.component';

describe('JobListComponentComponent', () => {
  let component: JobListComponentComponent;
  let fixture: ComponentFixture<JobListComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JobListComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobListComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
