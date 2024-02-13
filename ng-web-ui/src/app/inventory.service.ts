import { Injectable } from '@angular/core';
import { MTG_Set } from './models/mtg_set';
import { CardData } from './models/carddata';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {

  constructor() { }

  getSetList() : MTG_Set[] {
    // TODO: This needs to be asynchronous and call the API
    return [
      {
        code: "pip",
        name: "Fallout",
        iconUrl: "https://svgs.scryfall.io/sets/pip.svg?1707714000"
      },
      {
        code: "clu",
        name: "Ravnica: Clue Edition",
        iconUrl: "https://svgs.scryfall.io/sets/clu.svg?1707714000"
      },
      {
        code: "mkm",
        name: "Murders at Karlov Manor",
        iconUrl: "https://svgs.scryfall.io/sets/mkm.svg?1707714000"
      }
    ]
  }
}
