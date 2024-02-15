import { Component, Input } from '@angular/core';
import { MTG_Card } from '../models/mtg_card';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-card-panel',
  standalone: true,
  imports: [NgIf],
  templateUrl: './card-panel.component.html',
  styleUrl: './card-panel.component.css'
})
export class CardPanelComponent {
  @Input() curCard?: MTG_Card;
}
