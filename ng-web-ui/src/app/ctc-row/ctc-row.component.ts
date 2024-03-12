import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardTypeCount } from '../models/cardtypecount';
import { NgFor } from '@angular/common';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-ctc-row',
  standalone: true,
  imports: [
    NgFor,
    NgbDropdownModule,
    MatButtonModule
  ],
  templateUrl: './ctc-row.component.html',
  styleUrl: './ctc-row.component.scss'
})
export class CtcRowComponent {
  typeList: string[] = ['Standard', 'foil', 'foil | prerelease', 'Spanish', 'Autographed'];

  @Input() ctc?: CardTypeCount;

  @Output() isDirty = new EventEmitter<boolean>();

  setCTCType(type: string, ev: MouseEvent){
    if (this.ctc) {
      this.ctc.cardType = type;
      this.isDirty.emit(true);
    }

    ev.stopPropagation();
  }

  setCount(ctc: CardTypeCount, count: number) {
    ctc.count = count;

    this.isDirty.emit(true);
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
