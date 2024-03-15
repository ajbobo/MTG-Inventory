import { Component } from '@angular/core';
import { TitleBarComponent } from '../title-bar/title-bar.component';
import { FilterPanelComponent } from '../filter-panel/filter-panel.component';
import { TablePanelComponent } from '../table-panel/table-panel.component';
import { MTG_Set } from '../models/mtg_set';
import { CardData } from '../models/carddata';
import { ChangesService } from '../changes.service';

@Component({
  selector: 'app-collection-panel',
  standalone: true,
  imports: [
    TitleBarComponent,
    FilterPanelComponent,
    TablePanelComponent
  ],
  templateUrl: './collection-panel.component.html',
  styleUrl: './collection-panel.component.scss'
})
export class CollectionPanelComponent {
  title = 'ng-web-ui';
  curSet?: MTG_Set;
  cardList: CardData[] = [];
  selectedCard?: CardData;
  
  focusFilter: boolean = false;

  constructor(
    private changes: ChangesService
  ) {
    this.changes.dataChanged.subscribe(v => this.onIsDirty(v));
  }

  onIsDirty(ev: boolean) {
    this.focusFilter = true;
    setTimeout(() => this.focusFilter = false, 200); // We need to set focusFilter back to false AFTER the focus changes
  }
}
