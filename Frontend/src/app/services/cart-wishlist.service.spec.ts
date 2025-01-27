/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { CartService } from './cart-wishlist.service';

describe('Service: Cart', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CartService]
    });
  });

  it('should ...', inject([CartService], (service: CartService) => {
    expect(service).toBeTruthy();
  }));
});
