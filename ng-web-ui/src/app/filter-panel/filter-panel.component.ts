import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { NgbDropdownModule, NgbTypeaheadConfig, NgbTypeaheadModule, NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';
import { CardData } from '../models/carddata';
import { FormsModule } from '@angular/forms';
import { Observable, OperatorFunction, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { NgFor, NgIf } from '@angular/common';
import { InventoryService } from '../inventory.service';

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [
    NgbDropdownModule,
    FormsModule,
    NgbTypeaheadModule,
    NgIf,
    NgFor
  ],
  templateUrl: './filter-panel.component.html',
  styleUrl: './filter-panel.component.css'
})
export class FilterPanelComponent {
  private _cardList: CardData[] = []
  @Input() set cardList(value: CardData[]) {
    this._cardList = value;
    this.card = null;
  }
  get cardList(): CardData[] {
    return this._cardList;
  }
  @Output() cardListChange = new EventEmitter<CardData[]>();

  @Input() selectedCard?: CardData;
  @Output() selectedCardChange = new EventEmitter<CardData>();

  @ViewChild('cardSearch') searchInput?: ElementRef; // Finds the element with the #cardSearch tag

  // These are used as the display text in the dropdowns
  countFilter: string = 'All';
  priceFilter: string = 'All';
  rarityFilter: string = 'All';

  constructor(config: NgbTypeaheadConfig, private inventory: InventoryService) {
    config.showHint = true;
  }

  card: any; // Holds the card that is found by the search box

  formatter = (card: CardData) => card.card.name;

  search: OperatorFunction<string, readonly CardData[]> = (text$: Observable<string>) =>
    text$.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      map((term) =>
        // term.length < 2 ? [] : this.cardList.filter((v) => v.card.name.toLowerCase().indexOf(term.toLowerCase()) > -1).slice(0, 10),
        term.length < 2 ? [] : this.cardList.filter((v) => v.card.name.toLowerCase().startsWith(term.toLowerCase())).slice(0, 10),
      ),
    );

  onCountChange(ev: string): void {
    this.countFilter = ev;
    this.inventory.setCountFilter(ev);
    this.updateCardList()
  }

  onPriceChange(ev: string): void {
    this.priceFilter = ev;
    this.inventory.setPriceFilter(ev);
    this.updateCardList()
  }

  onRarityChange(ev: string): void {
    this.rarityFilter = ev;
    this.inventory.setRarityFilter(ev);
    this.updateCardList()
  }

  updateCardList() {
    this.inventory.getCardList().subscribe(s => {
      console.log("Updating CardList in FilterPanel");
      this.cardList = s;
      this.cardList.forEach((v, i) => v.index = i); // Set each card's display index number, so that they can be highlighted correctly
      this.cardListChange.emit(this.cardList);
    });
  }

  onSelectItem(ev: NgbTypeaheadSelectItemEvent): void {
    this.selectedCard = ev.item;
    this.selectedCardChange.emit(this.selectedCard);
  }

  clearSelectedCard(): void {
    this.searchInput!.nativeElement.value = '';
    this.selectedCard = undefined;
    this.selectedCardChange.emit(undefined);
  }
}
