import { Injectable } from '@angular/core';
import { MTG_Set } from './models/mtg_set';
import { CardData } from './models/carddata';
import { Observable, of } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private apiUrl: string = '/api'; // The proxy.conf.json file connects this to the full URL
  private curSet?: MTG_Set;

  constructor(private http: HttpClient) { }

  getSetList(): Observable<MTG_Set[]> {
    const url: string = `${this.apiUrl}/Sets`
    console.log(`SetList URL: ${url}`);
    return this.http.get<MTG_Set[]>(url); // This should have error handling and maybe logging
  }

  changeActiveSet(set: MTG_Set): void {
    this.curSet = set;
  }

  getCardList(): Observable<CardData[]> {
    if (this.curSet) {
      const url: string = `${this.apiUrl}/Collection/${this.curSet?.code}`
      console.log(`CardList URL: ${url}`);
      return this.http.get<CardData[]>(url); // This should have error handling and maybe logging
    }

    return new Observable<CardData[]>();
  }

}
