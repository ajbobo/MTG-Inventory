import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardData } from '../models/carddata';
import { InventoryService } from '../inventory.service';
import { CardRowComponent } from '../card-row/card-row.component';
import { NgForOf, NgIf } from '@angular/common';
import { MTG_Set } from '../models/mtg_set';

@Component({
  selector: 'app-table-panel',
  standalone: true,
  imports: [
    CardRowComponent,
    NgForOf,
    NgIf],
  templateUrl: './table-panel.component.html',
  styleUrl: './table-panel.component.scss'
})
export class TablePanelComponent {
  expandedCard: number = -1;

  private _curSet?: MTG_Set;
  @Input() set curSet(value: MTG_Set | undefined) {
    this._curSet = value;
    this.getCardList();
  }
  get curSet(): MTG_Set | undefined {
    return this._curSet;
  }

  @Input() cardList: CardData[] = [];
  @Output() cardListChange = new EventEmitter<CardData[]>();

  @Input() selectedCard?: CardData;

  @Output() isDirty = new EventEmitter<boolean>();

  constructor(
    private inventory: InventoryService
  ) { }

  ngOnInit(): void {
    this.getCardList();
  }

  ngOnDestroy(): void {
    this.inventory.changeActiveSet(undefined);
    this._curSet = undefined;
    this.cardList = [];
    this.cardListChange.emit(this.cardList);
  }

  getCardList(): void {
    this.inventory.getCardList().subscribe(s => {
      this.cardList = s;
      this.cardList.forEach((v, i) => v.index = i); // Set each card's display index number, so that they can be highlighted correctly
      this.cardListChange.emit(this.cardList);
    });
  }

  onIsDirty(ev: boolean) {
    this.cardListChange.emit(this.cardList); // Refresh anything bound to the cardList
    this.isDirty.emit(ev);
  }
}
