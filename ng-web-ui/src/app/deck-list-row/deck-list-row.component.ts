import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-deck-list-row',
  standalone: true,
  imports: [],
  templateUrl: './deck-list-row.component.html',
  styleUrl: './deck-list-row.component.scss'
})
export class DeckListRowComponent {
  @Input() name: string = "";
}
