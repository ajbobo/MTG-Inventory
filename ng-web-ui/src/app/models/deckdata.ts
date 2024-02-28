import { DeckCardCount } from "./deckcardcount";

export interface DeckData {
    key: string;
    name: string;
    cards: DeckCardCount[];
}