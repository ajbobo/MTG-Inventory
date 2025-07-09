import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-resetpassword',
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    CommonModule
  ],
  templateUrl: './resetpassword.component.html',
  styleUrl: './resetpassword.component.scss'
})
export class ResetpasswordComponent {
  username: string = '';
  invalidUsername: boolean = false;

  constructor(
    private titleService: Title
  ){ 
    this.titleService.setTitle("MTG Inventory - Reset Password");
  }

  onSubmit(event: Event): void {
    this.invalidUsername = this.username.length <= 0;
  }

}
