import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TitleBarComponent } from './title-bar/title-bar.component';
import { FilterPanelComponent } from './filter-panel/filter-panel.component';
import { TablePanelComponent } from './table-panel/table-panel.component';
import { CardPanelComponent } from './card-panel/card-panel.component';
import { HttpClient } from '@angular/common/http';
import { MTG_Set } from './models/mtg_set';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    TitleBarComponent,
    FilterPanelComponent,
    TablePanelComponent,
    CardPanelComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'ng-web-ui';
  curSet?: MTG_Set;

}
