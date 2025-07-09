import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    CommonModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username: string = '';
  invalidUsername: boolean = false;
  password: string = '';
  invalidPassword: boolean = false;

  constructor(
    private titleService: Title
  ){ 
    this.titleService.setTitle("MTG Inventory - Login");
  }

  onSubmit(event: Event): void {
    this.invalidUsername = this.username.length <= 0;
    this.invalidPassword = this.password.length <= 0;
  }

  // checkUsername(): boolean {
  //   // TODO: Should check against a regular expression
  //   return this.username.length > 0;
  // }

  // checkPassword(): boolean {
  //   // TODO: Should check againts a regular expression
  //   return this.password.length > 0;
  // }
}
