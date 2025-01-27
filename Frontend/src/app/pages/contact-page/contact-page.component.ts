import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-contact-page',
  standalone: true,
  templateUrl: './contact-page.component.html',
  imports: [CommonModule, FormsModule],
  styleUrls: ['./contact-page.component.css']
})
export class ContactPageComponent implements OnInit {
  contact = {
    name: '',
    email: '',
    message: ''
  };

  isModalVisible = false;
  modalSuccess = false;
  modalError = false;
  modalMessage = "Nothing here";


  constructor() { }

  ngOnInit() {
  }

  // Simulate the submission of the form (send to server logic can be added here)
  onSubmit() {
    if (this.contact.name && this.contact.email && this.contact.message) {
      // Normally, here you'd call an API to send the contact form data.
      console.log('Contact Form Submitted:', this.contact);

      this.showModalSuccess("Thank you for contacting us. We will get back to you shortly!");

      // Clear form after submission
      this.contact.name = '';
      this.contact.email = '';
      this.contact.message = '';

    } else {
      this.showModalError("Error, please complete all the fields!");
    }
  }

  closeModal(event: MouseEvent, isCloseButton: boolean = false) {
    if (isCloseButton) {
      event.stopPropagation();
    }
    if ((event.target as HTMLElement).classList.contains('modal') || isCloseButton) {
      this.isModalVisible = false;
    }
  }

  showModalSuccess(message: string) {
    this.modalMessage = message;
    this.modalSuccess = true;
    this.modalError = false;
    this.isModalVisible = true;
  }

  showModalError(message: string) {
    this.modalMessage = message
    this.modalError = true;
    this.modalSuccess = false;
    this.isModalVisible = true;
  }
}