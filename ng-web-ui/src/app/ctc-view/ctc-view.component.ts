import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardData } from '../models/carddata';
import { CardTypeCount } from '../models/cardtypecount';
import { JsonPipe, NgForOf, NgIf } from '@angular/common';
import { CtcRowComponent } from '../ctc-row/ctc-row.component';

@Component({
  selector: 'app-ctc-view',
  standalone: true,
  imports: [
    NgIf,
    NgForOf,
    CtcRowComponent,
    JsonPipe
  ],
  templateUrl: './ctc-view.component.html',
  styleUrl: './ctc-view.component.css'
})
export class CtcViewComponent {
  @Input() curCard?: CardData;

  @Output() isDirty = new EventEmitter<boolean>();

  onIsDirty(ev: boolean) {
    this.isDirty.emit(ev);
  }

  addNewCTC(ev: MouseEvent) {
    console.log("Adding CTC")
    if (!this.curCard?.ctCs)
      this.curCard!.ctCs = [];

      var ctc: CardTypeCount = { cardType: (this.curCard?.ctCs.length == 0 ? "Standard" : "foil"), count: 1 };

    this.curCard?.ctCs.push(ctc);
    this.curCard!.totalCount++;

    ev.stopPropagation();
  }
}
