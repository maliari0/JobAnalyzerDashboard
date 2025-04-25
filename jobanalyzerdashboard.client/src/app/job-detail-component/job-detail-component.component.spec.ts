import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobDetailComponentComponent } from './job-detail-component.component';

describe('JobDetailComponentComponent', () => {
  let component: JobDetailComponentComponent;
  let fixture: ComponentFixture<JobDetailComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [JobDetailComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobDetailComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
