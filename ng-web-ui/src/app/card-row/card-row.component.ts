import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardData } from '../models/carddata';

import { CtcViewComponent } from '../ctc-view/ctc-view.component';
import { CardPanelComponent } from '../card-panel/card-panel.component';
import { InventoryService } from '../inventory.service';
import { CardTypeCount } from '../models/cardtypecount';
import { DecimalPipe } from '@angular/common';
import { ChangesService } from '../changes.service';

@Component({
  selector: 'app-card-row',
  standalone: true,
  imports: [
    CtcViewComponent,
    CardPanelComponent,
    DecimalPipe
],
  templateUrl: './card-row.component.html',
  styleUrl: './card-row.component.scss'
})
export class CardRowComponent {
  hasMouse: boolean = false;

  @Input() card?: CardData;

  @Input() expandedCard: number = -1;
  @Output() expandedCardChange = new EventEmitter<number>();

  constructor(
    private inventory: InventoryService,
    private changes: ChangesService
    ) {
      this.changes.cardChanged.subscribe(v => this.onCardChanged(v));
    }

  getRaritySymbol(): string {
    return `/assets/${this.card?.card!.rarity}.png`;
  }

  hasFoil(): boolean {
    var hasFoil = false;
    this.card?.ctCs?.forEach((c: CardTypeCount) => { if (c.cardType.indexOf('foil') > -1) hasFoil = true; })
    return hasFoil;
  }

  hasOther(): boolean {
    var hasOther = false;
    this.card?.ctCs?.forEach((c: CardTypeCount) => { if (c.cardType != 'Standard' && c.cardType != 'foil') hasOther = true; })
    return hasOther;
  }

  onClick(): void {
    if (this.expandedCard != this.card?.index!) {
      this.expandedCard = this.card?.index!
      this.hasMouse = false;
    }
    else {
      this.expandedCard = -1;
      this.hasMouse = false;
    }
    this.expandedCardChange.emit(this.expandedCard);
  }

  onCardChanged(card: CardData) {
    if (card != this.card)
      return;
  
  // The card has been changed; calculate its new totalCount, push its CTCs to the API
    var cnt = 0;
    var updatedCTCs: CardTypeCount[] = [];
    this.card?.ctCs?.forEach(v => {
      if (v.count > 0)
        updatedCTCs.push(v);
        cnt += v.count;
    });
    this.card!.ctCs = updatedCTCs;
    this.card!.totalCount = cnt;

    this.inventory.updateCardCTCs(this.card).subscribe();
  }
}
