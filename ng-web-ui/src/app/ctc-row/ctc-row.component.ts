import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardTypeCount } from '../models/cardtypecount';

import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ChangesService } from '../changes.service';

@Component({
  selector: 'app-ctc-row',
  standalone: true,
  imports: [
    NgbDropdownModule
],
  templateUrl: './ctc-row.component.html',
  styleUrl: './ctc-row.component.scss'
})
export class CtcRowComponent {
  typeList: string[] = ['Standard', 'foil', 'foil | prerelease', 'Spanish', 'Autographed'];

  @Input() ctc?: CardTypeCount;

  constructor(
    private changes: ChangesService
  ) {}

  setCTCType(type: string, ev: MouseEvent){
    if (this.ctc) {
      this.ctc.cardType = type;
      this.changes.changesMade();
    }

    ev.stopPropagation();
  }

  setCount(ctc: CardTypeCount, count: number) {
    ctc.count = count;

    this.changes.changesMade();
  }

  IncrementCTC(ctc: CardTypeCount | undefined, ev: MouseEvent) {
    if (ctc === undefined)
      return;

    this.setCount(ctc, ctc.count + 1);

    ev.stopPropagation();
  }

  DecrementCTC(ctc: CardTypeCount | undefined, ev: MouseEvent) {
    if (ctc === undefined)
      return;

    this.setCount(ctc, Math.max(0, ctc.count - 1));

    ev.stopPropagation();
  }

  SetCTCto4(ctc: CardTypeCount | undefined, ev: MouseEvent) {
    if (ctc === undefined)
      return;

    this.setCount(ctc, 4);

    ev.stopPropagation();
  }

  DeleteCTC(ctc: CardTypeCount | undefined, ev: MouseEvent) {
    if (ctc === undefined)
      return;

    this.setCount(ctc, 0);

    ev.stopPropagation();
  }
}
