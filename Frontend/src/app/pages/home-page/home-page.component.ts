import {
  Component,
  OnInit,
  ElementRef,
  Renderer2,
  ViewChildren,
  QueryList,
  OnDestroy,
  AfterViewInit,
  HostListener
} from '@angular/core';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.css']
})
export class HomePageComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChildren('circle') circles!: QueryList<ElementRef>;
  private videoContainer: HTMLElement | null = null;
  private animationFrameId: number | null = null;
  private scrollThreshold = 50; // The scroll position at which we apply the `video-scrolled` class
    private videoElement!: HTMLVideoElement;
    private hasInteracted = false;

  constructor(private renderer: Renderer2) { }

  ngOnInit() {
    this.videoContainer = document.querySelector(".video-container");
    this.startAnimation();
      this.addVideoScrollClass();
  }

  ngAfterViewInit() {
    this.videoElement = document.querySelector('video') as HTMLVideoElement;
    if (this.videoElement) {
      this.videoElement.load();
      // Attach event listener to the document body, to start the video if the user interacts with the body.
       document.body.addEventListener('click', this.handleUserInteraction);
      setTimeout(() => {
            if (!this.hasInteracted) {
                 this.playVideo();
             }
      }, 100);
    }
  }

  @HostListener('document:keydown')
   onKeyDown() {
      this.handleUserInteraction();
  }


   handleUserInteraction = () => {
       if (!this.hasInteracted) {
           this.playVideo();
            this.hasInteracted = true;
            document.body.removeEventListener('click', this.handleUserInteraction);
       }
    }


     playVideo() {
         if (this.videoElement) {
              this.videoElement.play().catch(error => {
                  console.error("Autoplay was prevented:", error);
           });
         }
    }


   addVideoScrollClass() {
      if (this.videoContainer) { // Ensure header is not null
        if (window.scrollY > this.scrollThreshold) {
          this.videoContainer.classList.add("video-scrolled");
        } else {
          this.videoContainer.classList.remove("video-scrolled");
        }
      }
    }

  ngOnDestroy() {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  startAnimation(): void {
    const animateCircles = () => {
      const scrollY = window.scrollY;

      this.circles.forEach((circleRef, index) => {
        const circle = circleRef.nativeElement;
        const speed = (index + 1) * 0.1;
        const translateY = scrollY * -speed;

        this.renderer.setStyle(
          circle,
          'transform',
          `translateY(${translateY}px)`
        );
      });

        this.addVideoScrollClass();
      this.animationFrameId = requestAnimationFrame(animateCircles);
    };

    this.animationFrameId = requestAnimationFrame(animateCircles);
  }
}