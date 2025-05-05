import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SettingsManagementComponent } from './settings-management.component';

describe('SettingsManagementComponent', () => {
  let component: SettingsManagementComponent;
  let fixture: ComponentFixture<SettingsManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SettingsManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SettingsManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
