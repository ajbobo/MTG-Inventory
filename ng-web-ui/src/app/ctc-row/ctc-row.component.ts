import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CardTypeCount } from '../models/cardtypecount';

@Component({
  selector: 'app-ctc-row',
  standalone: true,
  imports: [],
  templateUrl: './ctc-row.component.html',
  styleUrl: './ctc-row.component.css'
})
export class CtcRowComponent {
  @Input() ctc?: CardTypeCount;

  @Output() isDirty = new EventEmitter<boolean>();

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
