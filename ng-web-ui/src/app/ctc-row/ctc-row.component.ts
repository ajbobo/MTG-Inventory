import { Component, Input } from '@angular/core';
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
}
