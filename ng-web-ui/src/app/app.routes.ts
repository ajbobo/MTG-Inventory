import { Routes } from '@angular/router';
import { CollectionPanelComponent } from './collection-panel/collection-panel.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    { path: 'collection/:setCode/:cardNumber', component: CollectionPanelComponent },
    { path: 'collection/:setCode', component: CollectionPanelComponent },
    { path: 'collection', component: CollectionPanelComponent },
    { path: 'login', component: LoginComponent },
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '**', component: PageNotFoundComponent}
];
