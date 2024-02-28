import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { DeckData } from './models/deckdata';

@Injectable({
  providedIn: 'root'
})
export class DeckManagerService {

  selectedDeckChanged = new Subject<DeckData>()

  constructor() { }

  ChangeDeck(deck: DeckData): void {
    this.selectedDeckChanged.next(deck);
  }

}
