import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    CommonModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  username: string = '';
  invalidUsername: boolean = false;
  password1: string = '';
  invalidPassword: boolean = false;
  password2: string = '';
  passwordMismatch: boolean = false;

  constructor(
    private titleService: Title
  ){ 
    this.titleService.setTitle("MTG Inventory - Register");
  }

  onSubmit(event: Event) {
    this.invalidUsername = !this.checkUsername();
    this.invalidPassword = !this.checkPassword();
    this.passwordMismatch = this.password1 !== this.password2;
  }

  checkUsername(): boolean {
    // TODO: Use a regular expression here
    return this.username.length > 0;
  }

  checkPassword(): boolean {
    // TODO: Use a regular expression here
    return this.password1.length >= 8;
  }
}
