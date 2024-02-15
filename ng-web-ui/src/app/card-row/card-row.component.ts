import { Component, Input } from '@angular/core';
import { CardData } from '../models/carddata';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-card-row',
  standalone: true,
  imports: [NgIf],
  templateUrl: './card-row.component.html',
  styleUrl: './card-row.component.css'
})
export class CardRowComponent {
  @Input() card?: CardData;
}
