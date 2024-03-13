import { Component, Input } from '@angular/core';
import { MTG_Card } from '../models/mtg_card';


@Component({
  selector: 'app-card-panel',
  standalone: true,
  imports: [],
  templateUrl: './card-panel.component.html',
  styleUrl: './card-panel.component.scss'
})
export class CardPanelComponent {
  @Input() curCard?: MTG_Card;
}
