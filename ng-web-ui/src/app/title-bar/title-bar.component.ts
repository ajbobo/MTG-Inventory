import { Component, EventEmitter, Input, Output } from '@angular/core';
import { InventoryService } from '../inventory.service';
import { MTG_Set } from '../models/mtg_set';

import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-title-bar',
  standalone: true,
  imports: [NgbDropdownModule],
  templateUrl: './title-bar.component.html',
  styleUrl: './title-bar.component.scss'
})
export class TitleBarComponent {
  setCode: string = "";
  setList: MTG_Set[] = [];
  @Input() curSet?: MTG_Set;
  @Output() curSetChange = new EventEmitter<MTG_Set>();


  constructor(
    private inventory: InventoryService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.route.params.subscribe(p => this.setCode = p['setCode']);
  }

  ngOnInit(): void {
    this.inventory.getSetList().subscribe(s => {
      this.setList = s
      if (this.setCode) {
        const selSet: MTG_Set | undefined = this.setList.find(s => s.code.toLowerCase() === this.setCode.toLowerCase());
        if (selSet)
          this.changeCurrentSet(selSet);
        else // An invalid setCode was provided
          this.router.navigate(['/notfound']);
      }
    });
  }

  getSetDisplayName(): string {
    if (this.curSet?.name)
      return this.curSet.name;
    return 'Choose a Set';
  }

  getSetIcon(): string {
    if (this.curSet?.iconUrl)
      return this.curSet.iconUrl;
    return 'https://svgs.scryfall.io/sets/planeswalker.svg'
  }

  routeTo(set: MTG_Set) {
    this.router.navigate(['/collection', set.code])
    this.changeCurrentSet(set);
  }

  changeCurrentSet(set: MTG_Set): void {
    this.curSet = set;
    this.curSetChange.emit(this.curSet);
    this.inventory.changeActiveSet(set);
  }
}
