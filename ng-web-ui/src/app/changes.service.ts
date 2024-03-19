import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { CardData } from './models/carddata';

@Injectable({
  providedIn: 'root'
})
export class ChangesService {

  cardChanged = new Subject<CardData>(); // Components should subscribe to this

  constructor() { }

  changeCard(card: CardData) {
    this.cardChanged.next(card);
  }
}
