import { Component } from '@angular/core';
import { DeckContentsPanelComponent } from "../deck-contents-panel/deck-contents-panel.component";
import { DeckListPanelComponent } from "../deck-list-panel/deck-list-panel.component";
import { InventoryService } from '../inventory.service';

@Component({
    selector: 'app-decks-panel',
    standalone: true,
    templateUrl: './decks-panel.component.html',
    styleUrl: './decks-panel.component.scss',
    imports: [DeckContentsPanelComponent, DeckListPanelComponent]
})
export class DecksPanelComponent {

}
