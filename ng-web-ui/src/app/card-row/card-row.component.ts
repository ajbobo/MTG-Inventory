import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardData } from '../models/carddata';
import { NgIf } from '@angular/common';
import { CtcViewComponent } from '../ctc-view/ctc-view.component';
import { CardPanelComponent } from '../card-panel/card-panel.component';

@Component({
  selector: 'app-card-row',
  standalone: true,
  imports: [
    NgIf,
    CtcViewComponent,
    CardPanelComponent
  ],
  templateUrl: './card-row.component.html',
  styleUrl: './card-row.component.css'
})
export class CardRowComponent {
  hasMouse: boolean = false;
  @Input() card?: CardData;
  @Input() expandedCard: number = -1;
  @Output() expandedCardChange = new EventEmitter<number>();

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
}
