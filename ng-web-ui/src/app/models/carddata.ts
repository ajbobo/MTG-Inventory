import { CardTypeCount } from "./cardtypecount"
import { MTG_Card } from "./mtg_card"

export interface CardData {
    Card: MTG_Card,
    CTCs: CardTypeCount[]
}