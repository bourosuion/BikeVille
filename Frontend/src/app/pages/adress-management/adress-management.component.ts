// src/app/address-manager/address-manager.component.ts
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { Observable, Subscription } from 'rxjs';
import { Address } from '../../interfaces/Address';
import { AddressService } from '../../services/address.service';

@Component({
  selector: 'app-adress-management',
  standalone: true,
  templateUrl: './adress-management.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
  ],
  styleUrls: ['./adress-management.component.css']
})
export class AdressManagementComponent implements OnInit, OnDestroy {
  addresses!: Observable<Address[] | undefined>;
  selectedAddress: Address | null = null;
  showModal = false;
  isEditing = false;
  loading = true;
  error: string | null = null;

  newAddress: Address = {
    id: 0,
    addressLine1: '',
    addressLine2: '',
    city: '',
    stateProvince: '',
    countryRegion: '',
    postalCode: '',
  };

  private errorSubscription!: Subscription;
  private loadingSubscription!: Subscription;

  constructor(private addressService: AddressService) {
  }

  ngOnInit() {
    this.addresses = this.addressService.addresses$;
    this.errorSubscription = this.addressService.error$.subscribe(error => {
      this.error = error;
    });
    this.loadingSubscription = this.addressService.loading$.subscribe(loading => {
      this.loading = loading;
    });
    this.addressService.loadAddresses();
  }

  ngOnDestroy(): void {
    this.errorSubscription.unsubscribe();
    this.loadingSubscription.unsubscribe();
  }


  selectAddress(address: Address) {
    this.selectedAddress = address; // Copy address
    this.isEditing = true;
    this.showModal = true;
  }

  addAddress() {
    this.newAddress = {
      id: 0,
      addressLine1: '',
      addressLine2: '',
      city: '',
      stateProvince: '',
      countryRegion: '',
      postalCode: '',
    };
    this.isEditing = false;
    this.showModal = true;
  }

  async saveAddress() {
    let addressToSave: Address;
    if (this.isEditing) {
      if (this.selectedAddress == null) {
        console.error("selectedAddress is null")
        return;
      }
      addressToSave = this.selectedAddress;
    }
    else {
      addressToSave = this.newAddress;
    }
    await this.addressService.saveAddress(addressToSave, this.isEditing);
    this.closeModal();
  }

  async closeModal() {
    this.showModal = false;
    this.selectedAddress = null;
    this.newAddress = {
      id: 0,
      addressLine1: '',
      addressLine2: '',
      city: '',
      stateProvince: '',
      countryRegion: '',
      postalCode: '',
    };
  }

  async deleteAddress(address: Address) {
    await this.addressService.deleteAddress(address);
    this.closeModal();
  }
}