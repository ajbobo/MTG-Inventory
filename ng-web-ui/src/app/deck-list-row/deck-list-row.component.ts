import { Component, Input } from '@angular/core';
import { DeckData } from '../models/deckdata';
import { NgIf } from '@angular/common';
import { DeckManagerService } from '../deck-manager.service';

@Component({
  selector: 'app-deck-list-row',
  standalone: true,
  imports: [
    NgIf
  ],
  templateUrl: './deck-list-row.component.html',
  styleUrl: './deck-list-row.component.scss'
})
export class DeckListRowComponent {
  @Input() deck?: DeckData;

  hasMouse: boolean = false;

  constructor(
    private deckManager: DeckManagerService
  ){}

  onClick(): void {
    console.log(`clicked on ${this.deck?.name}`)
    this.deckManager.ChangeDeck(this.deck!);
  }
}
