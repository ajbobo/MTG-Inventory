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
  private countFilter: string = '';
  private priceFilter: string = '';
  private rarityFilter: string = '';

  constructor(private http: HttpClient) { }

  getSetList(): Observable<MTG_Set[]> {
    const url: string = `${this.apiUrl}/Sets`
    console.log(`SetList URL: ${url}`);
    return this.http.get<MTG_Set[]>(url); // This should have error handling and maybe logging
  }

  changeActiveSet(set: MTG_Set): void {
    this.curSet = set;
  }

  setCountFilter(filter: string): void {
    this.countFilter = filter != 'All' ? filter : '';
  }

  setPriceFilter(filter: string): void {
    this.priceFilter = filter != 'All' ? filter.replace("$", '') : '';
  }

  setRarityFilter(filter: string): void {
    this.rarityFilter = filter != 'All' ? filter.toUpperCase().charAt(0) : '';
  }

  getCardList(): Observable<CardData[]> {
    if (this.curSet) {
      var url: string = `${this.apiUrl}/Collection/${this.curSet?.code}?`
      if (this.countFilter.length > 0) url += `&count=${this.countFilter}`
      if (this.priceFilter.length > 0) url += `&price=${this.priceFilter}`
      if (this.rarityFilter.length > 0) url += `&rarity=${this.rarityFilter}`
      console.log(`CardList URL: ${encodeURI(url)}`);
      return this.http.get<CardData[]>(encodeURI(url)); // This should have error handling and maybe logging
    }

    return new Observable<CardData[]>();
  }

}
