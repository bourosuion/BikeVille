import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ShopPageComponent } from '../pages/shop-page/shop-page.component';
import { CommonModule } from '@angular/common';
import { CartWishlistService } from '../services/cart-wishlist.service';
import { LoginService } from '../services/login.service';
import { Product } from '../interfaces/Product';

@Component({
  selector: 'header-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './header-bar.component.html',
  styleUrls: ['./header-bar.component.css']
})
export class HeaderBarComponent implements OnInit {
  isDropdownVisible = false;

  isModalOpen = false;
  selectedIndex: number = -1;

  isCardOpen: boolean = false;
  isLoggedIn: boolean = false;

  hasError: boolean = false;
  errorMessage = "An error has occured!";

  // Mock cart data
  cartItems: Product[] = [];

  // Animated total price
  animatedTotalPrice: string = "0.00";

  constructor(private router: Router, private cartService: CartWishlistService, private loginService: LoginService) { }

  ngOnInit() {
    document.addEventListener("scroll", () => {
      const header = document.getElementById("header");
      if (header) { // Ensure header is not null
        if (window.scrollY > 50) {
          header.classList.add("rounded");
        } else {
          header.classList.remove("rounded");
        }
      }
    });

    this.cartService.cartItems$.subscribe(items => {
      this.cartItems = items;
      this.updateAnimatedTotalPrice();
    });

    this.isLoggedIn = this.loginService.getLoginStatus();

    this.loginService.testServerConnection("https://localhost:7163/api/HealthCheck/database").then(isConnected => {
      if (!isConnected) {
        this.hasError = true;
        this.errorMessage = "Unable to get the information from the database!";
      } else {
        this.loginService.testServerConnection("https://localhost:7163/api/HealthCheck").then(isConnected => {
          if (!isConnected) {
            this.hasError = true;
            this.errorMessage = "Unable to reach the server!";
          }
        });
      }
    });
  }

  toggleDropdown() {
    this.isDropdownVisible = !this.isDropdownVisible;
  }

  @HostListener('document:click', ['$event'])
  closeDropdown(event: Event) {
    const target = event.target as HTMLElement;
    if (!target.closest('.account-container')) {
      this.isDropdownVisible = false;
    }
  }

  // Update animated price smoothly
  updateAnimatedTotalPrice() {
    const currentTotal = parseFloat(this.animatedTotalPrice);
    const targetTotal = this.totalPrice;
    const duration = 300; // Animation duration in ms
    const startTime = performance.now();

    const animate = (time: number) => {
      const elapsed = time - startTime;
      const progress = Math.min(elapsed / duration, 1); // Progress from 0 to 1
      const interpolatedValue = currentTotal + (targetTotal - currentTotal) * progress;

      this.animatedTotalPrice = interpolatedValue.toFixed(2);

      if (progress < 1) {
        requestAnimationFrame(animate);
      }
    };

    requestAnimationFrame(animate);
  }

  // Increase item quantity
  increaseQuantity(index: number) {
    if (this.cartService.modifyQuantity(this.cartItems[index], +1)) {
      console.log("Update item: " + this.cartItems[index]);
      this.cartItems[index].quantity += 1;
      this.updateAnimatedTotalPrice();
    }
  }

  // Decrease item quantity
  decreaseQuantity(index: number) {
    if (this.cartItems[index].quantity > 1) {
      if (this.cartService.modifyQuantity(this.cartItems[index], -1)) {
        this.cartItems[index].quantity -= 1;
        this.updateAnimatedTotalPrice();
      }
    } else {
      alert('Quantity must be at least 1.');
    }
  }

  removeItem(index: number) {
    this.selectedIndex = index;
    this.isModalOpen = true;
  }

  //Wait for user input for deletion
  deleteItem() {
    if (!this.cartService.removeFromCart(this.cartItems[this.selectedIndex])) {
      return;
    }

    this.selectedIndex = -1;
    this.isModalOpen = false;

    const item = this.cartItems[this.selectedIndex];

    // Add a `removing` property to the item to trigger the animation
    item.removing = true;

    // Wait for the animation to finish before actually removing the item
    setTimeout(() => {
      this.cartItems.splice(this.selectedIndex, 1);
      this.updateAnimatedTotalPrice();
    }, 300); // Duration should match the CSS animation duration
  }

  moveToWishlist() {
    this.cartService.addToCart(this.cartItems[this.selectedIndex], false);
  }

  // Calculate total price
  get totalPrice(): number {
    return this.cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  // Toggle the card's visibility
  toggleCard() {
    this.isLoggedIn = this.loginService.getLoginStatus();
    this.isCardOpen = !this.isCardOpen;
  }

  // Close the card
  closeCard() {
    this.isCardOpen = false;
  }

  // Handle checkout
  goToCheckout() {
    this.closeCard(); // Close the cart
  }

  //Handle log-out
  logout() {
    localStorage.removeItem("authToken");
    this.isLoggedIn = this.loginService.getLoginStatus();
    this.router.navigate(["/home"]);
  }
}