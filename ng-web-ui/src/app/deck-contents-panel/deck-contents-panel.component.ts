import { Component } from '@angular/core';
import { DeckManagerService } from '../deck-manager.service';
import { DeckData } from '../models/deckdata';
import { NgFor, NgForOf, NgIf } from '@angular/common';
import { DeckContentsRowComponent } from '../deck-contents-row/deck-contents-row.component';
import { InventoryService } from '../inventory.service';

@Component({
  selector: 'app-deck-contents-panel',
  standalone: true,
  imports: [
    NgIf,
    NgForOf,
    DeckContentsRowComponent
  ],
  templateUrl: './deck-contents-panel.component.html',
  styleUrl: './deck-contents-panel.component.scss'
})
export class DeckContentsPanelComponent {
  deck?: DeckData;

  private deckChangeSub;

  constructor(
    private deckManager: DeckManagerService,
    private inventory: InventoryService
  ) {
    this.deckChangeSub = this.deckManager.selectedDeckChanged.subscribe(d => this.onChangedDeck(d))
  }

  ngOnDestroy() {
    this.deckChangeSub.unsubscribe(); // If we don't do this, the object will subscribe multiple times
  }

  onChangedDeck(deck: DeckData): void {
    console.log(`Changed deck -> ${deck.name}`);
    this.inventory.getSingleDeck(deck.key).subscribe(d => {
      this.deck = d[0];
    });
  }
}
