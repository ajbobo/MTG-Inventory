import { Component, Input } from '@angular/core';
import { CardData } from '../models/carddata';
import { NgForOf, NgIf } from '@angular/common';
import { CtcRowComponent } from '../ctc-row/ctc-row.component';

@Component({
  selector: 'app-ctc-view',
  standalone: true,
  imports: [
    NgIf, 
    NgForOf,
    CtcRowComponent
  ],
  templateUrl: './ctc-view.component.html',
  styleUrl: './ctc-view.component.css'
})
export class CtcViewComponent {
  @Input() curCard?: CardData;
}
