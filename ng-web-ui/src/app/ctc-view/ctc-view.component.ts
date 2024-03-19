import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardData } from '../models/carddata';
import { CardTypeCount } from '../models/cardtypecount';
import { JsonPipe } from '@angular/common';
import { CtcRowComponent } from '../ctc-row/ctc-row.component';
import { ChangesService } from '../changes.service';

@Component({
  selector: 'app-ctc-view',
  standalone: true,
  imports: [
    CtcRowComponent,
    JsonPipe
],
  templateUrl: './ctc-view.component.html',
  styleUrl: './ctc-view.component.scss'
})
export class CtcViewComponent {
  @Input() curCard?: CardData;

  constructor(
    private changes: ChangesService
  ){ }

  addNewCTC(ev: MouseEvent) {
    console.log("Adding CTC")
    if (!this.curCard?.ctCs)
      this.curCard!.ctCs = [];

      var ctc: CardTypeCount = { cardType: (this.curCard?.ctCs.length == 0 ? "Standard" : "foil"), count: 1 };

    this.curCard?.ctCs.push(ctc);
    this.curCard!.totalCount++;

    this.changes.changeCard(this.curCard!);

    ev.stopPropagation();
  }
}
