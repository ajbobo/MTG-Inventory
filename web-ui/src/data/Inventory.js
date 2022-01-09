import {collection, query, getDocs} from 'firebase/firestore'
import {hard_coded_inventory} from '../data/hard_coded.js'

class Inventory {
    cards = {};

    constructor(db) {
        console.log("Creating new Inventory object");
        this.db = db;

        this.populateCards();
    }

    async populateCards() {
        console.log("Reading user_inventory from Json file");
        // console.log("Reading user_inventory from Firebase");
        // const user_inventory_query = query(collection(this.db, "user_inventory"));

        // const user_inventory = await getDocs(user_inventory_query);
        const user_inventory = hard_coded_inventory;
        user_inventory.forEach((doc) => {
            const data = doc; //doc.data();
            if (!this.cards[data.SetCode])
                this.cards[data.SetCode] = {};
            this.cards[data.SetCode][data.Collector_Number.toString()] = {
                collector_number: data.Collector_Number,
                count: data.Count,
                name: data.Name,
                set_code: data.SetCode
            };
        })
        console.log(this.cards);
    }

    getCardCount(setCode, collector_number) {
        const set = this.cards[setCode];
        if (set){
            const card = set[collector_number.toString()];
            if (card)
                return card.count
        }
        return 0;
    }
}

export default Inventory;