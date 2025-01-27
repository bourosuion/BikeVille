import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoginService {

  constructor() { }

  async testServerConnection(url: string): Promise<boolean> {
    try {
      const response = await fetch(url, { method: 'GET' }); // Minimal request
      if (response.ok) {
        console.log("Server is reachable!");
        return true;
      } else {
        console.error("Server responded with a status:", response.status);
        return false;
      }
    } catch (error) {
      console.error("Error connecting to the server:", error);
      return false;
    }
  }

  getLoginStatus(): boolean {
    // Check if AuthToken is present in localStorage
    const authToken = localStorage.getItem('authToken');

    //Check if token is expired (1 day)
    const tokenDateString = localStorage.getItem("authTokenDate");

    //Return if token date is null
    if (tokenDateString == null) {
      return false;
    }

    const tokenDate = new Date(tokenDateString);
    const date = new Date(Date.now());

    //Check token and date validity
    if (authToken && tokenDate.getDay() == date.getDay() && tokenDate.getMonth() == tokenDate.getMonth() && tokenDate.getFullYear() == date.getFullYear()) {
      return true;  // User is logged in
    } else {
      return false; // User is not logged in
    }
  }

  authenticateCustomer(email: string, password: string): Promise<any> {
    return new Promise((resolve, reject) => {
      fetch('https://localhost:7163/LoginJwt/AuthenticateCustomer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      })
        .then(response => {
          if (!response.ok) {
            reject(new Error("Invalid Password or Email"));
            return;
          } else {
            return response.json();
          }
        })
        .then(data => {
          resolve(data); // Pass the data back to the caller
        })
        .catch(error => {
          reject(error); // Pass the error to the caller
        });
    });
  }

  registerCustomer(
    emailAddress: string,
    password: string,
    firstName: string,
    lastName: string
  ): Promise<any> {
    return new Promise((resolve, reject) => {
      fetch('https://localhost:7163/Customers/register-customer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          emailAddress,
          password,
          firstName,
          lastName,
        }),
      })
        .then(response => {
          if (!response.ok) {
            reject(new Error('Failed to register customer'));
          }
          return response.json();
        })
        .then(data => {
          resolve(data); // Pass the response data to the caller
        })
        .catch(error => {
          reject(error); // Pass the error to the caller
        });
    });
  }
}
