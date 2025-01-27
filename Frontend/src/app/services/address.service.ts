// src/app/services/address.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Address } from '../interfaces/Address';

@Injectable({
  providedIn: 'root'
})
export class AddressService {
  private addressesSubject = new BehaviorSubject<Address[]>([]);
  addresses$ = this.addressesSubject.asObservable();
  private errorSubject = new BehaviorSubject<string | null>(null);
  error$ = this.errorSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  constructor() {
  }

  loadAddresses() {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    const token = localStorage.getItem('authToken');
    fetch('https://localhost:7163/Address', {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
      })
      .then(data => {
        if (data && Array.isArray(data)) {
          const formattedAddresses = data.map(item => ({
            id: item.addressId,
            addressLine1: item.addressLine1,
            addressLine2: item.addressLine2,
            city: item.city,
            stateProvince: item.stateProvince,
            countryRegion: item.countryRegion,
            postalCode: item.postalCode,
          }));
          this.addressesSubject.next(formattedAddresses)
        } else {
          this.errorSubject.next('Invalid data format received from the API.');
        }
      })
      .catch(error => {
        console.error('Error fetching addresses:', error);
        this.errorSubject.next(
          'Failed to load addresses. Please login and try again or check your network.'
        );
      })
      .finally(() => {
        this.loadingSubject.next(false);
      });
  }

  async saveAddress(address: Address, isEditing: boolean): Promise<Address | null> {
    this.errorSubject.next(null);
    const method = isEditing ? 'PUT' : 'POST';
    const url = isEditing
      ? `https://localhost:7163/Address/${address.id}`
      : 'https://localhost:7163/Address';
    try {
      const response = await fetch(url, {
        method: method,
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem("authToken")}`,
        },
        body: JSON.stringify(address),
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      if (response.status === 204) {
        return null;
      }
      const savedAddress = await response.json() as Address;

      const currentAddresses = this.addressesSubject.value;
      if (isEditing) {
        const index = currentAddresses.findIndex(a => a.id === savedAddress.id);
        if (index > -1) {
          currentAddresses[index] = { ...savedAddress };
        }
      } else {
        currentAddresses.push({ ...savedAddress });
      }
      this.addressesSubject.next([...currentAddresses]);
      return savedAddress;
    } catch (error) {
      console.error('Error saving address:', error);
      this.errorSubject.next('Failed to save address. Please try again.');
      return null;
    }
  }


  async deleteAddress(address: Address): Promise<boolean> {
    this.errorSubject.next(null);
    const token = localStorage.getItem('authToken');
    try {
      const response = await fetch(`https://localhost:7163/Address/${address.id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const currentAddresses = this.addressesSubject.value;
      const index = currentAddresses.findIndex(a => a.id === address.id);
      if (index > -1) {
        currentAddresses.splice(index, 1);
      }
      this.addressesSubject.next([...currentAddresses]);
      return true;
    }
    catch (error) {
      console.error('Error deleting address:', error);
      this.errorSubject.next('Failed to delete address. Please try again.');
      return false;
    }
  }

  getAddresses() {
    return this.addressesSubject.value;
  }
}