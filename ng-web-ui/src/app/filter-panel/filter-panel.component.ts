import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { NgbDropdownModule, NgbTypeaheadConfig, NgbTypeaheadModule, NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';
import { CardData } from '../models/carddata';
import { FormsModule } from '@angular/forms';
import { Observable, OperatorFunction, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { NgFor, NgIf } from '@angular/common';
import { InventoryService } from '../inventory.service';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [
    NgbDropdownModule,
    FormsModule,
    NgbTypeaheadModule,
    NgIf,
    NgFor,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './filter-panel.component.html',
  styleUrl: './filter-panel.component.scss'
})
export class FilterPanelComponent {
  @ViewChild('cardSearch') searchInput?: ElementRef; // Finds the element with the #cardSearch tag

  @Input() set needsFocus(focus: boolean) {
    if (focus) {
      this.searchInput?.nativeElement.focus();
      this.searchInput?.nativeElement.select();
    }
  }

  private _cardList: CardData[] = []
  @Input() set cardList(value: CardData[]) {
    this._cardList = value;
    this.card = null;
    this.displaySelectedCard();
  }
  get cardList(): CardData[] {
    return this._cardList;
  }
  @Output() cardListChange = new EventEmitter<CardData[]>();

  @Input() selectedCard?: CardData;
  @Output() selectedCardChange = new EventEmitter<CardData>();

  // These are used as the display text in the dropdowns
  countFilter: string = 'All';
  priceFilter: string = 'All';
  rarityFilter: string = 'All';

   // The values from the URL, if provided
  setCode: string = "";
  cardNumber: string = "";

  constructor(
    config: NgbTypeaheadConfig, 
    private inventory: InventoryService, 
    private router: Router,
    private route: ActivatedRoute) {
    config.showHint = true;
    this.route.params.subscribe(p => { 
      this.setCode = p['setCode']; 
      this.cardNumber = p['cardNumber']; 
    });
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
    this.router.navigate(['/collection', this.selectedCard?.card.setCode, this.selectedCard?.card.collectorNumber]);
    this.selectedCardChange.emit(this.selectedCard);
  }

  clearSelectedCard(): void {
    this.searchInput!.nativeElement.value = '';
    this.selectedCard = undefined;
    this.router.navigate(['/collection', this.setCode]);
    this.selectedCardChange.emit(undefined);
  }
  
  private displaySelectedCard() {
    if (this.cardNumber) {
      const selCard: CardData | undefined = this._cardList.find(x => x.card.collectorNumber === this.cardNumber);
      if (selCard) {
        this.selectedCard = selCard;
        this.selectedCardChange.emit(this.selectedCard);
        if (this.searchInput)
          this.searchInput.nativeElement.value = this.selectedCard.card.name;
      }
      else if (this._cardList.length > 0) {
        this.router.navigate(['/notfound']);
      }
    }
  }

}
