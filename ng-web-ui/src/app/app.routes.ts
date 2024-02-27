import { Routes } from '@angular/router';
import { CollectionPanelComponent } from './collection-panel/collection-panel.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { DecksPanelComponent } from './decks-panel/decks-panel.component';

export const routes: Routes = [
    { path: 'decks', component: DecksPanelComponent},
    { path: 'collection/:setCode/:cardNumber', component: CollectionPanelComponent },
    { path: 'collection/:setCode', component: CollectionPanelComponent },
    { path: 'collection', component: CollectionPanelComponent },
    { path: '', redirectTo: 'collection', pathMatch: 'full' },
    { path: '**', component: PageNotFoundComponent}
];
