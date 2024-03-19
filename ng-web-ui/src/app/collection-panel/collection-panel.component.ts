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
}
