import { CardTypeCount } from "./cardtypecount"
import { MTG_Card } from "./mtg_card"

export interface CardData {
    card: MTG_Card,
    cTCs: CardTypeCount[],
    totalCount: number,
    index: number
}