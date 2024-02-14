import { Injectable } from '@angular/core';
import { MTG_Set } from './models/mtg_set';
import { CardData } from './models/carddata';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl: string = 'https://mtg-inventory.azurewebsites.net/api'

  constructor(private http: HttpClient) { }

  getSetList() : Observable<MTG_Set[]> {
    const url: string = `${this.apiUrl}/Sets`
    return this.http.get<MTG_Set[]>(url); // This should have error handling and maybe logging

    // TODO: This needs to call the API
    // return of([
    //   {
    //     code: "pip",
    //     name: "Fallout",
    //     iconUrl: "https://svgs.scryfall.io/sets/pip.svg?1707714000"
    //   },
    //   {
    //     code: "clu",
    //     name: "Ravnica: Clue Edition",
    //     iconUrl: "https://svgs.scryfall.io/sets/clu.svg?1707714000"
    //   },
    //   {
    //     code: "mkm",
    //     name: "Murders at Karlov Manor",
    //     iconUrl: "https://svgs.scryfall.io/sets/mkm.svg?1707714000"
    //   }
    // ])
  }
}
