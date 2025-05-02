import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-unauthorized-component',
  standalone: false,
  templateUrl: './unauthorized-component.component.html',
  styleUrl: './unauthorized-component.component.css'
})
export class UnauthorizedComponentComponent {
  constructor(private router: Router) { }

  goHome(): void {
    this.router.navigate(['/']);
  }
}
