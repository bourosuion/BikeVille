import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Product } from '../interfaces/Product';

@Injectable({
  providedIn: 'root'
})
export class CartWishlistService {
  // Create a BehaviorSubject to hold cart items
  private cartItemsSubject = new BehaviorSubject<Product[]>([]);
  private wishlistItemsSubject = new BehaviorSubject<Product[]>([]);

  // Observable to subscribe to changes
  cartItems$ = this.cartItemsSubject.asObservable();
  wishlistItems$ = this.wishlistItemsSubject.asObservable();

  constructor() {
    this.fetchCartItems();
    this.loadWishList();
  }

  // Method to fetch cart items from the server and update the cartItemsSubject
  fetchCartItems() {
    const token = localStorage.getItem("authToken");

    fetch('https://localhost:7163/CustomerProducts?isCart=true', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        //Parse response as JSON
        return response.json();
      })
      .then(data => {
        //Transform the data into the desired format
        const formattedCartItems: Product[] = data.map((product: any) => ({
          id: product.productId,
          title: product.name,
          price: product.listPrice,
          description: product.description == undefined ? "Description not available" : product.description,
          image: product.largePhoto == null ? `data:image/gif;base64,${product.thumbNailPhoto}` : `data:image/gif;base64,${product.largePhoto}`,
          isWishlisted: false,
          quantity: 1,
          removing: false
        }));

        //Update the BehaviorSubject with the transformed data
        this.cartItemsSubject.next(formattedCartItems);
      })
      .catch(error => {
        console.error("Failed to fetch cart items:", error);
      });
  }

  loadWishList() {
    const token = localStorage.getItem("authToken");

    fetch('https://localhost:7163/CustomerProducts?isCart=false', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        //Parse response as JSON
        return response.json();
      })
      .then(data => {
        console.log(data);
        //Transform the data into the desired format
        const formattedWishlistItems = data.map((product: any) => ({
          id: product.productId,
          title: product.name,
          price: product.listPrice,
          description: product.description == undefined ? "Description not available" : product.description,
          image: product.largePhoto == null ? `data:image/gif;base64,${product.thumbNailPhoto}` : `data:image/gif;base64,${product.largePhoto}`,
          isWishlisted: true,
          quantity: 1,
          removing: false
        }));

        this.wishlistItemsSubject.next(formattedWishlistItems);
      })
      .catch(error => {
        console.error("Failed to fetch cart items:", error);
      });
  }

  addToCart(item: any, isCart: boolean) {
    const currentCart = this.cartItemsSubject.value;
    const token = localStorage.getItem("authToken");

    const itemExists = currentCart.some(cartItem => cartItem.id === item.id);

    //If we are adding to cart
    if (isCart) {
      if (itemExists) {
        //Modify the item In_Cart status
        this.modifyCart(item, isCart);
        return;
      } else {
        fetch('https://localhost:7163/CustomerProducts/' + item.id + '?isCart=true', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
          },
        })
          .then(response => {
            if (!response.ok) {
              throw new Error(`HTTP error! Status: ${response.status}`);
            }
          })
          .then(() => {
            //Add to the current cart array
            this.cartItemsSubject.next([...currentCart, item]);
          })
          .catch(error => {
            console.log("Impossible to add item to the cart: " + error);
          });
        //Modify the item In_Cart status
        this.modifyCart(item, isCart);
      }
      return;
    }

    if (!isCart) {
      //Check if the product already exists in wishlist
      const currentWishlist = this.wishlistItemsSubject.value;
      const wishlistItemExists = currentWishlist.some(cartItem => cartItem.id === item.id);

      if (wishlistItemExists) {
        this.modifyWishlist(item, isCart);
        return;
      } else {
        fetch('https://localhost:7163/CustomerProducts/' + item.id + '?isCart=false', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
          },
        })
          .then(response => {
            if (!response.ok) {
              throw new Error(`HTTP error! Status: ${response.status}`);
            }
          })
          .then(() => {
            const currentWishlist = this.wishlistItemsSubject.value;
            this.wishlistItemsSubject.next([...currentWishlist, item]);
          })
          .catch(error => {
            console.log("Impossible to add item to the wishlist: " + error);
          });
        this.modifyWishlist(item, isCart);
      }
      return;
    }
  }

  modifyWishlist(item: any, isCart: boolean) {
    const currentCart = this.cartItemsSubject.value;
    const token = localStorage.getItem("authToken");

    //Modify the item In_Cart status
    fetch('https://localhost:7163/CustomerProducts/' + item.id + (isCart ? '?isCart=true' : '?isCart=false'), {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
      })
      .then(() => {
        if (isCart) {
          const updatedWishlist = this.wishlistItemsSubject.value.filter(wishlistItem => wishlistItem.id !== item.id);
          this.wishlistItemsSubject.next(updatedWishlist);
        } else {
          const updatedCart = this.cartItemsSubject.value.filter(cartItem => cartItem.id !== item.id);
          this.cartItemsSubject.next(updatedCart);
        }
      })
      .catch(error => {
        console.log("Impossible to modify product: " + error);
      });
  }

  modifyCart(item: any, isCart: boolean) {
    const currentCart = this.cartItemsSubject.value;
    const token = localStorage.getItem("authToken");

    //Modify the item In_Cart status
    fetch('https://localhost:7163/CustomerProducts/' + item.id + (isCart ? '?isCart=true' : '?isCart=false'), {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
      })
      .then(() => {
        if (isCart) {
          const updatedWishlist = this.wishlistItemsSubject.value.filter(wishlistItem => wishlistItem.id !== item.id);
          this.wishlistItemsSubject.next(updatedWishlist);
        } else {
          const updatedCart = this.cartItemsSubject.value.filter(cartItem => cartItem.id !== item.id);
          this.cartItemsSubject.next(updatedCart);
        }
      })
      .catch(error => {
        console.log("Impossible to modify product: " + error);
      });
  }

  //Get the item and increase/decrease the quantity by "amount"
  modifyQuantity(item: any, increaseAmount: number): boolean {
    const currentCart = this.cartItemsSubject.value;

    // Check if the item is already in the cart based on a unique identifier (e.g., productId)
    const itemExists = currentCart.some(cartItem => cartItem.id === item.id);

    // If the item doesn't exist, add it to the cart
    if (itemExists) {
      const token = localStorage.getItem("authToken");

      console.log();
      fetch('https://localhost:7163/CustomerProducts/' + item.id + "/quantity", {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`,
        },
        body: JSON.stringify(item.quantity + increaseAmount)
      })
        .then(response => {
          if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
          }

          // Only parse as JSON if content exists
          const contentType = response.headers.get("Content-Type");
          if (contentType && contentType.includes("application/json")) {
            return response.json(); // Parse the JSON if the content type matches
          } else {
            return null; // Handle responses with no content
          }
        })
        .then(data => {
          if (data) {
            console.log(data); // Handle the parsed JSON data
          } else {
            console.log("No JSON content in the response.");
          }
        })
        .catch(error => {
          console.log("Impossible to update quantity: " + error);
        });

      return true;
    }

    return false;
  }

  // Method to remove items from the cart
  removeFromCart(item: any): boolean {
    console.log(item);
    const token = localStorage.getItem("authToken"); // Get the auth token from localStorage

    fetch("https://localhost:7163/CustomerProducts/" + item.id, {
      method: 'DELETE', // Specify the HTTP method as DELETE
      headers: {
        'Content-Type': 'application/json', // Optional: Specify content type
        'Authorization': `Bearer ${token}`, // Add Authorization header with Bearer token
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`); // Handle any error responses
        }
        console.log(`Item with ID ${item.id} deleted successfully.`);
        const updatedCart = this.cartItemsSubject.value.filter(cartItem => cartItem.id !== item.id);
        this.cartItemsSubject.next(updatedCart);
        return true;
      })
      .catch(error => {
        console.error("Failed to delete the item from the cart:", error); // Handle fetch errors
        return false;
      });

    return false;
  }

  // Method to get the current cart items
  getCartItems() {
    return this.cartItemsSubject.value;
  }
}