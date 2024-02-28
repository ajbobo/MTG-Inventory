import { Component } from '@angular/core';
import { DeckContentsRowComponent } from "../deck-contents-row/deck-contents-row.component";
import { DeckListRowComponent } from "../deck-list-row/deck-list-row.component";
import { InventoryService } from '../inventory.service';
import { NgForOf } from '@angular/common';
import { DeckData } from '../models/deckdata';

@Component({
    selector: 'app-deck-list-panel',
    standalone: true,
    templateUrl: './deck-list-panel.component.html',
    styleUrl: './deck-list-panel.component.scss',
    imports: [
        DeckContentsRowComponent,
        DeckListRowComponent,
        NgForOf
    ]
})
export class DeckListPanelComponent {
    deckList: DeckData[] = [];

    constructor(
        private inventory: InventoryService,
    ) { }

    ngOnInit(): void {
        this.getDeckList();
    }

    getDeckList(): void {
        console.log("Getting list of Decks");
        var index: number = 0;
        this.inventory.getDeckList().subscribe(p => {
            this.deckList = p;
            this.deckList.forEach(d => d.index = index++);
        });
    }
}
