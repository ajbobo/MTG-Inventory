import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChangesService {

  dataChanged = new Subject<boolean>(); // Components should subscribe to this

  constructor() { }

  changesMade() {
    this.dataChanged.next(true);
  }
}
