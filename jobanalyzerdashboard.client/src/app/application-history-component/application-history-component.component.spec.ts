import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ApplicationHistoryComponentComponent } from './application-history-component.component';

describe('ApplicationHistoryComponentComponent', () => {
  let component: ApplicationHistoryComponentComponent;
  let fixture: ComponentFixture<ApplicationHistoryComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ApplicationHistoryComponentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ApplicationHistoryComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
