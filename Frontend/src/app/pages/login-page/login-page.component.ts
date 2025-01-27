import { CommonModule } from '@angular/common';
import { Component, OnInit, Renderer2 } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LoginService } from '../../services/login.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.css']
})
export class LoginPageComponent implements OnInit {
  hasConnectionError = false;

  isLoading = false;
  isSuccess = false;
  isError = false;

  modalSuccess = false;
  modalError = false;

  isShifted = false;
  isFadingOut = false;
  isFadingIn = false;

  isModalVisible = false;

  loginButtonText = 'Log In';
  modalMessage = "Nothing here";
  illustrationSrc = 'assets/images/login.svg';

  //Data binded from the login form
  email: string = "";
  password: string = "";

  //Data binded from the signin form
  firstName: string = "";
  lastName: string = "";
  signEmail: string = "";
  signPassword: string = "";
  signPasswordConfirm: string = "";

  constructor(private renderer: Renderer2, private loginService: LoginService) { }

  ngOnInit() {
    this.loginService.testServerConnection("https://localhost:7163/api/HealthCheck").then(isConnected => {
      if (!isConnected) {
        this.hasConnectionError = true;
      }
    });
  }

  onLoginClick(event: Event): void {
    event.preventDefault();

    this.isLoading = true; // Set loading state to true
    this.loginButtonText = 'Logging In...';

    this.loginService.authenticateCustomer(this.email, this.password)
      .then(data => {
        // Save the token in localStorage
        localStorage.setItem('authToken', data.token);

        // Save the token date
        const date = new Date(Date.now());
        localStorage.setItem('authTokenDate', date.toISOString());

        // Update UI states
        this.isLoading = false;
        this.isSuccess = true;
        this.loginButtonText = 'Login Successful';

        // Reset the button text after 2 seconds
        setTimeout(() => {
          this.isSuccess = false;
          this.loginButtonText = 'Log In';
        }, 2000);

        // Optionally reload the page
        window.location.reload();
      })
      .catch((error: any) => {
        console.error('Error during login:', error);

        // Handle error states
        this.isLoading = false;
        this.isError = true;
        this.loginButtonText = 'Invalid Password or Email';

        setTimeout(() => {
          this.isError = false;
          this.loginButtonText = 'Log In';
        }, 2000);
      });
  }

  onSigninClick(event: Event) {
    event.preventDefault();

    //Return with error if the any of the inputs are missing
    if (this.signEmail == "" || this.signPassword == "" || this.firstName == "" || this.lastName == "") {
      this.showModal("All the fields must be completed", false);
      return;
    }

    if (!(this.signEmail.includes("@") && this.signEmail.includes("."))) {
      this.showModal("The email is not inserted correctly!", false);
      return;
    }

    //Return with error if the passwords are not equal
    if (this.signPassword != this.signPasswordConfirm) {
      this.showModal("The passwords inserted are not the same", false);
      return;
    }

    this.loginService.registerCustomer(this.signEmail, this.signPassword, this.firstName, this.lastName)
      .then(() => {
        this.showModal('Account registered successfully!', true);
      })
      .catch(error => {
        console.error('Registration error:', error);
        this.showModal('Email already exists!', false);
      });
  }

  closeModal() {
    this.isModalVisible = false;
  }

  showModal(message: string, success: boolean) {
    this.modalMessage = message;
    this.modalSuccess = success;
    this.modalError = !success;
    this.isModalVisible = true;
  }

  onSignInLinkClick(event: Event): void {
    event.preventDefault();
    this.toggleShiftState(true);
    this.changeIllustration('assets/images/signin.svg');
  }

  onLoginLinkClick(event: Event): void {
    event.preventDefault();
    this.toggleShiftState(false);
    this.changeIllustration('assets/images/login.svg');
  }

  private toggleShiftState(shifted: boolean): void {
    this.isShifted = shifted;
  }

  private changeIllustration(newSrc: string): void {
    this.isFadingOut = true;

    setTimeout(() => {
      this.illustrationSrc = newSrc;
      this.isFadingOut = false;
      this.isFadingIn = true;

      setTimeout(() => {
        this.isFadingIn = false;
      }, 800);
    }, 800);
  }
}
