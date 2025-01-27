import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-about-page',
  standalone: true,
  imports: [ CommonModule ],
  templateUrl: './about-page.component.html',
  styleUrls: ['./about-page.component.css']
})
export class AboutPageComponent implements OnInit {
  imageUrl = 'assets/images/experience_bike.jpg'; // Replace with your image URL
  backgroundPosition = 'center';

  constructor() { }

  ngOnInit() {
  }

  onMouseMove(event: MouseEvent) {
    const container = event.currentTarget as HTMLElement;
    const rect = container.getBoundingClientRect();

    // Calculate mouse position relative to the container
    const xPercent = ((event.clientX - rect.left) / rect.width) * 100;
    const yPercent = ((event.clientY - rect.top) / rect.height) * 100;

    // Set the background position based on mouse position
    this.backgroundPosition = `${xPercent}% ${yPercent}%`;
  }
}
