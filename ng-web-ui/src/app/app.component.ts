import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { TitleBarComponent } from './title-bar/title-bar.component';
import { FilterPanelComponent } from './filter-panel/filter-panel.component';
import { TablePanelComponent } from './table-panel/table-panel.component';
import { CardPanelComponent } from './card-panel/card-panel.component';
import { HttpClient } from '@angular/common/http';
import { MTG_Set } from './models/mtg_set';
import { CardData } from './models/carddata';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    TitleBarComponent,
    FilterPanelComponent,
    TablePanelComponent,
    CardPanelComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  // title = 'ng-web-ui';
  // curSet?: MTG_Set;
  // cardList: CardData[] = [];
  // selectedCard?: CardData;

  // focusFilter: boolean = false;

  // onIsDirty(ev: boolean) {
  //   this.focusFilter = true;
  //   setTimeout(() => this.focusFilter = false, 200); // We need to set focusFilter back to false AFTER the focus changes
  // }

}
