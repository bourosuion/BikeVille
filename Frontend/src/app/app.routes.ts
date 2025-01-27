import { Routes } from '@angular/router';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { ShopPageComponent } from './pages/shop-page/shop-page.component';
import { AboutPageComponent } from './pages/about-page/about-page.component';
import { ContactPageComponent } from './pages/contact-page/contact-page.component';
import { WishlistPageComponent } from './pages/wishlist-page/wishlist-page.component';
import { AdressManagementComponent } from './pages/adress-management/adress-management.component';

export const routes: Routes = [
    { path: "", component: HomePageComponent },
    { path: "login", component: LoginPageComponent },
    { path: "shop", component: ShopPageComponent },
    { path: "about", component: AboutPageComponent },
    { path: "contact", component: ContactPageComponent },
    { path: "wishlist", component: WishlistPageComponent },
    { path: "addresses", component: AdressManagementComponent }
];
