import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CartWishlistService } from '../../services/cart-wishlist.service';
import { Product } from '../../interfaces/Product';

@Component({
  selector: 'app-wishlist-page',
  standalone: true,
  templateUrl: './wishlist-page.component.html',
  imports: [CommonModule],
  styleUrls: ['./wishlist-page.component.css']
})
export class WishlistPageComponent implements OnInit {
  title = 'Wishlist';

  products: Product[] = [];

  constructor(private cartService: CartWishlistService) { }

  ngOnInit() {
    this.cartService.wishlistItems$.subscribe(items => {
      this.products = items;
    });
  }

  moveToCart(product: Product) {
    this.cartService.addToCart(product, true);
  }

  deleteProduct(product: Product) {
    this.cartService.removeFromCart(product);

    this.products = this.products.filter(cartItem => cartItem.id !== product.id);
  }
}