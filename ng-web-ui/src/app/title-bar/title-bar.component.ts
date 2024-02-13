import { Component } from '@angular/core';
import { InventoryService } from '../inventory.service';
import { MTG_Set } from '../models/mtg_set';
import { NgFor } from '@angular/common';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-title-bar',
  standalone: true,
  imports: [NgFor, NgbDropdownModule],
  templateUrl: './title-bar.component.html',
  styleUrl: './title-bar.component.css'
})
export class TitleBarComponent {
  setList: MTG_Set[] = [];
  curSet?: MTG_Set;

  constructor(
    private inventory: InventoryService
  ) { }

  ngOnInit(): void {
    this.setList = this.inventory.getSetList();
  }

  getSetDisplayName(): string {
    if (this.curSet?.name)
      return this.curSet.name;
    return 'Choose Set';
  }

  getSetIcon(): string {
    if (this.curSet?.iconUrl)
      return this.curSet.iconUrl;
    return 'https://svgs.scryfall.io/sets/planeswalker.svg'
  }

  changeCurrentSet(set: MTG_Set): void {
    this.curSet = set;
  }
}
