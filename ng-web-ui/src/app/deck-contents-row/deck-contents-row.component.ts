import { Component, Input } from '@angular/core';
import { DeckCardCount } from '../models/deckcardcount';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-deck-contents-row',
  standalone: true,
  imports: [
    NgIf
  ],
  templateUrl: './deck-contents-row.component.html',
  styleUrl: './deck-contents-row.component.scss'
})
export class DeckContentsRowComponent {
  @Input() card?: DeckCardCount
}
