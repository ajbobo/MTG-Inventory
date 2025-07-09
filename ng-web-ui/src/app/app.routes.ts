import { Routes } from '@angular/router';
import { CollectionPanelComponent } from './collection-panel/collection-panel.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ResetpasswordComponent } from './resetpassword/resetpassword.component';

export const routes: Routes = [
    { path: 'collection/:setCode/:cardNumber', component: CollectionPanelComponent },
    { path: 'collection/:setCode', component: CollectionPanelComponent },
    { path: 'collection', component: CollectionPanelComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'resetpassword', component: ResetpasswordComponent },
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '**', component: PageNotFoundComponent}
];
