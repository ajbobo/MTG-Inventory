import { Component, Input } from '@angular/core';
import { CardData } from '../models/carddata';
import { InventoryService } from '../inventory.service';
import { CardRowComponent } from '../card-row/card-row.component';
import { NgForOf } from '@angular/common';
import { MTG_Set } from '../models/mtg_set';

@Component({
  selector: 'app-table-panel',
  standalone: true,
  imports: [CardRowComponent, NgForOf],
  templateUrl: './table-panel.component.html',
  styleUrl: './table-panel.component.css'
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

  cardList: CardData[] = [];

  constructor(
    private inventory: InventoryService
  ) { }

  ngOnInit(): void {
    this.getCardList();
  }

  getCardList(): void {
    this.inventory.getCardList().subscribe(s => {
      this.cardList = s;
      this.cardList.forEach((v, i) => v.index = i); // Set each card's display index number, so that they can be highlighted correctly
    });
  }
}
