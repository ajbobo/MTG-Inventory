import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { NgbDropdownModule, NgbTypeaheadConfig, NgbTypeaheadModule, NgbTypeaheadSelectItemEvent } from '@ng-bootstrap/ng-bootstrap';
import { CardData } from '../models/carddata';
import { FormsModule } from '@angular/forms';
import { Observable, OperatorFunction, debounceTime, distinctUntilChanged, map } from 'rxjs';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  imports: [
    NgbDropdownModule,
    FormsModule,
    NgbTypeaheadModule,
    NgIf
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

  @Input() selectedCard?: CardData;
  @Output() selectedCardChange = new EventEmitter<CardData>();

  card: any;
  @ViewChild('cardSearch') searchInput?: ElementRef; // Finds the element with the #cardSearch tag

  constructor(config: NgbTypeaheadConfig) {
		config.showHint = true;
	}

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
