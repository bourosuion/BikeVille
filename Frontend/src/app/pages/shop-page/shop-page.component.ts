import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CartWishlistService } from '../../services/cart-wishlist.service';
import { LoginService } from '../../services/login.service';
import { Product } from '../../interfaces/Product';

@Component({
  selector: 'app-shop-page',
  standalone: true,
  templateUrl: './shop-page.component.html',
  imports: [FormsModule, CommonModule, HttpClientModule],
  styleUrls: ['./shop-page.component.css']
})
export class ShopPageComponent implements OnInit {
  isLoggedIn = false;

  isLoading: boolean = true;

  searchValue: string = '';

  priceFilter: number = 5000;
  minPrice: number = 0;
  maxPrice: number = 5000;

  products: Product[] = [];
  selectedProduct: any = null;

  cards: Card[] = [];

  //Variables for drop-down-tags menu
  categories = [
    {
      name: 'Bikes',
      items: ['Mountain Bikes', 'Road Bikes', 'Touring Bikes'],
      open: false
    },
    {
      name: 'Components',
      items: [
        'Handlebars', 'Bottom Brackets', 'Brakes', 'Chains', 'Cranksets',
        'Derailleurs', 'Forks', 'Headsets', 'Mountain Frames', 'Pedals',
        'Road Frames', 'Saddles', 'Touring Frames', 'Wheels'
      ],
      open: false
    },
    {
      name: 'Clothing',
      items: ['Bib-Shorts', 'Caps', 'Gloves', 'Jerseys', 'Shorts', 'Socks', 'Tights', 'Vests'],
      open: false
    },
    {
      name: 'Accessories',
      items: [
        'Bike Racks', 'Bike Stands', 'Bottles and Cages', 'Cleaners', 'Fenders',
        'Helmets', 'Hydration Packs', 'Lights', 'Locks', 'Panniers',
        'Pumps', 'Tires and Tubes'
      ],
      open: false
    },
  ];

  categoriesList = [
    "Bikes",
    "Components",
    "Clothing",
    "Accessories",
    "Mountain Bikes",
    "Road Bikes",
    "Touring Bikes",
    "Handlebars",
    "Bottom Brackets",
    "Brakes",
    "Chains",
    "Cranksets",
    "Derailleurs",
    "Forks",
    "Headsets",
    "Mountain Frames",
    "Pedals",
    "Road Frames",
    "Saddles",
    "Touring Frames",
    "Wheels",
    "Bib-Shorts",
    "Caps",
    "Gloves",
    "Jerseys",
    "Shorts",
    "Socks",
    "Tights",
    "Vests",
    "Bike Racks",
    "Bike Stands",
    "Bottles and Cages",
    "Cleaners",
    "Fenders",
    "Helmets",
    "Hydration Packs",
    "Lights",
    "Locks",
    "Panniers",
    "Pumps",
    "Tires and Tubes",
  ];

  selectedFilters: { [key: string]: string[] } = {};

  //Variables for color filters
  colors = [
    { name: 'Black', hex: '#000000' },
    { name: 'Blue', hex: '#0000FF' },
    { name: 'Grey', hex: '#808080' },
    { name: 'Multi', hex: 'linear-gradient(135deg, red, yellow, green, blue)' }, // Multi can use a gradient
    { name: 'Red', hex: '#FF0000' },
    { name: 'Silver', hex: '#C0C0C0' },
    { name: 'Silver/Black', hex: 'linear-gradient(90deg, #C0C0C0, #000000)' }, // Gradient for mixed colors
    { name: 'White', hex: '#FFFFFF' },
    { name: 'Yellow', hex: '#FFFF00' },
  ];

  selectedColors: string[] = [];

  //Variables for the sizes options
  availableSizes: string[] = [
    '38', '40', '42', '44', '46', '48',
    '50', '52', '54', '56', '58', '60', '70',
    'L', 'M', 'S', 'XL'
  ];

  // Object to track open sections
  openSections: { [key: string]: boolean } = {
    filters: false,
    colors: false,
    sizes: false,
    price: true,
  };

  //VARIABLES FOR FILTERED QUERY
  baseUrl: string = "https://localhost:7163/Products/GetFiltered";

  currentPage = 1;
  pageSize = 20
  categoryId: number = 0;
  subCategoryId: number = 0;
  //selectedColors is on top
  selectedSizes: { [key: string]: string[] } = {};

  constructor(private cdr: ChangeDetectorRef, private cartService: CartWishlistService, private loginService: LoginService) { }

  ngOnInit() {
    this.fetchProductsWithFilters();
  }

  // Move to the previous page
  goToPreviousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.fetchProductsWithFilters(); // Fetch new products for the previous page
    }
  }

  // Move to the next page
  goToNextPage() {
    this.currentPage++;
    this.fetchProductsWithFilters(); // Fetch new products for the next page
  }

  // Toggle the visibility of a section
  toggleSection(section: keyof OpenSections): void {
    this.openSections[section] = !this.openSections[section];
  }

  setWishlistedItems() {
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
        data.forEach((product: any) => {
          for (var i = 0; i < this.products.length; i++) {
            if (this.products[i].id == product.productId) {
              this.products[i].isWishlisted = true;
              continue;
            }
          }
        });
      })
      .catch(error => {
        console.error("Failed to fetch cart items:", error);
      });
  }

  fetchProductsWithFilters() {
    //Loading animation
    this.isLoading = true;

    const params = {
      SearchTerm: this.searchValue == "" ? null : this.searchValue,
      PageNumber: this.currentPage,
      PageSize: this.pageSize,
      CategoryId: this.categoryId == 0 ? null : this.categoryId,
      SubCategoryId: this.subCategoryId == 0 ? null : this.subCategoryId,
      SelectedColors: this.selectedColors,
      SelectedSizes: this.selectedSizes["sizes"] == undefined ? [] : this.selectedSizes["sizes"], //Set to empty if undefined
      PriceRange: [0, this.priceFilter]
    };

    // Convert arrays to the required format for the query string
    const queryParams = new URLSearchParams();

    Object.entries(params).forEach(([key, value]) => {
      if (Array.isArray(value)) {
        value.forEach(item => queryParams.append(key, String(item))); // Add each array element
      } else if (value !== null) {
        queryParams.append(key, String(value)); // Add scalar values
      }
    });

    const urlWithParams = `${this.baseUrl}?${queryParams.toString()}`;

    // Fetch request
    fetch(urlWithParams, {
      method: "GET",
      headers: {
        "Content-Type": "application/json"
      }
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then(data => {
        //Delete saved products
        this.cards = [];
        this.products = [];

        //Add filtered products
        data.products.forEach((item: any) => {
          this.products.push({
            id: item.productId,
            title: item.name,
            price: item.listPrice,
            description: item.description == undefined ? "Description not available" : item.description,
            image: item.largePhoto == null ? `data:image/gif;base64,${item.thumbNailPhoto}` : `data:image/gif;base64,${item.largePhoto}`,
            isWishlisted: false,
            quantity: 1,
            removing: false
          });

          this.cards.push({
            id: item.productId,
            title: item.name,
            price: item.listPrice,
            image: item.thumbNailPhoto == null ? `data:image/gif;base64,${item.largePhoto}` : `data:image/gif;base64,${item.thumbNailPhoto}`
          });
        });

        this.setWishlistedItems();
        this.isLoading = false;
      })
      .catch(error => {
        console.error("Error fetching filtered products:", error);
      });
  }

  toggleDropdown(category: any): void {
    category.open = !category.open;
  }

  selectFilterSize(categoryName: string, item: string): void {
    if (!this.selectedSizes[categoryName]) {
      this.selectedSizes[categoryName] = [];
    }

    const index = this.selectedSizes[categoryName].indexOf(item);

    if (index === -1) {
      // Add item to the filter
      this.selectedSizes[categoryName].push(item);
    } else {
      // Remove item from the filter
      this.selectedSizes[categoryName].splice(index, 1);
    }

    console.log('Selected Sizes:', this.selectedSizes["sizes"]);
    this.fetchProductsWithFilters();
  }

  selectFilterItem(categoryIndex: number, itemIndex: number, categoryName: string, item: string): void {
    if (!this.selectedFilters[categoryName]) {
      this.selectedFilters[categoryName] = [];
    }

    const index = this.selectedFilters[categoryName].indexOf(item);

    if (index === -1) {
      // Add item to the filter
      this.selectedFilters[categoryName].push(item);
    } else {
      // Remove item from the filter
      this.selectedFilters[categoryName].splice(index, 1);
    }

    categoryIndex++; //Products category starts from 1
    itemIndex = 0;
    this.categoriesList.forEach(category => {
      itemIndex++;
      if (category == item) {
        console.log('Category Index:', categoryIndex, 'Item Index:', itemIndex);

        this.categoryId = categoryIndex;
        this.subCategoryId = itemIndex;

        this.fetchProductsWithFilters();
        return;
      }
    });
  }

  isSelected(categoryName: string, item: string): boolean {
    return this.selectedFilters[categoryName]?.includes(item);
  }

  isSizeSelected(categoryName: string, item: string): boolean {
    return this.selectedSizes[categoryName]?.includes(item);
  }

  toggleColor(colorName: string): void {
    const index = this.selectedColors.indexOf(colorName);

    if (index === -1) {
      // Add color to selected list
      this.selectedColors.push(colorName);
    } else {
      // Remove color from selected list
      this.selectedColors.splice(index, 1);
    }

    console.log('Selected Colors:', this.selectedColors);
    this.fetchProductsWithFilters();
  }

  get filteredCards() {
    return this.cards.filter(card => card.title.toLowerCase().includes(this.searchValue.toLowerCase()) && card.price <= this.priceFilter);
  }

  applyFilters() {
    console.log('Filters applied:', {
      searchValue: this.searchValue,
      priceFilter: this.priceFilter
    });
  }

  // Open the popup with the product details
  openPopup(product: any) {
    this.selectedProduct = this.products.filter(prod => prod.id == product.id)[0];
    this.isLoggedIn = this.loginService.getLoginStatus();
  }

  // Close the popup
  closePopup(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal')) {
      this.selectedProduct = null;
    }
  }

  // Add to wishlist - TO ADD: check for presence in the wishlist
  addToWishlist(product: any) {
    if (this.loginService.getLoginStatus()) {
      // Get the heart button and add the "clicked" class to trigger the animation
      const heartButton = document.querySelector('.wishlist-button') as HTMLElement;

      //Remove item from wishlist
      if (heartButton.classList.contains("clicked")) {
        heartButton.classList.remove("clicked");

        const selectedProduct: any = this.products.find(p => p.id === product.id);
        if (selectedProduct != undefined) {
          selectedProduct.isWishlisted = false;
        }

        this.cartService.removeFromCart(selectedProduct);
      } else { //Add item to wishlist
        heartButton.classList.add('clicked');

        const selectedProduct: any = this.products.find(p => p.id === product.id);
        if (selectedProduct != undefined) {
          selectedProduct.isWishlisted = true;
        }

        this.cartService.addToCart({ id: product.id, title: product.title, price: product.price, quantity: 1, removing: false }, false);
      }
    }
  }

  // Add to cart - TO ADD: check for presence in the cart
  addToCart(product: any) {
    const addCartButton = document.getElementById("add-cart-button");

    //If logged-in add to cart with animation
    if (this.loginService.getLoginStatus()) {
      addCartButton?.classList.add("clicked");
      this.selectedProduct.isWishlisted = false;

      setTimeout(() => {
        addCartButton?.classList.remove("clicked");
      }, 1000);

      this.cartService.addToCart({ id: product.id, title: product.title, price: product.price, quantity: 1, removing: false }, true);
    }
  }
}

interface Card {
  id: BigInt,
  title: string,
  price: number,
  image: string
}

interface OpenSections {
  filters: boolean;
  colors: boolean;
  sizes: boolean;
  price: boolean;
}

interface Color {
  color: string
}