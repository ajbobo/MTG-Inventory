import { collection, query, getDocs, enableIndexedDbPersistence, disableNetwork } from 'firebase/firestore'
import { hard_coded_inventory } from '../data/hard_coded.js'

class Inventory {
    cards = {};

    constructor(db) {
        console.log("Creating new Inventory object");
        this.db = db;

        this.setupCaching();
        this.populateCards();
    }

    setupCaching() {
        enableIndexedDbPersistence(this.db)
            .catch((err) => {
                if (err.code == 'failed-precondition') {
                    console.log("ERROR: Unable to enable caching because the app is option in multiple tabs");
                }
                else {
                    console.log("ERROR: Something happened during cache setup: " + err.code);
                    console.log(err);
                }
            });

        disableNetwork(this.db);
    }

    async populateCards() {
        // console.log("Reading user_inventory from Json file");
        console.log("Reading user_inventory from Firebase");
        const user_inventory_query = query(collection(this.db, "user_inventory"));

        const user_inventory = await getDocs(user_inventory_query);
        // const user_inventory = hard_coded_inventory;
        user_inventory.forEach((doc) => {
            const data = doc.data(); // Firebase-style
            // const data = doc; // JSON-style
            if (!this.cards[data.SetCode])
                this.cards[data.SetCode] = {};
            this.cards[data.SetCode][data.CollectorNumber.toString()] = {
                collectorNumber: data.CollectorNumber,
                counts: data.Counts,
                name: data.Name,
                setCode: data.SetCode
            };
        })
        console.log(this.cards);
    }

    getCardCount(setCode, collectorNumber) {
        const set = this.cards[setCode];
        if (set) {
            const card = set[collectorNumber.toString()];
            if (card) {
                let count = {total:0};
                card.counts.forEach((ctc) => {
                    // Go through each property of the CTC, count the total, foil, and whether there are others
                    for (let name of Object.keys(ctc)) {
                        switch (name) {
                            case "Count": count.total += ctc.Count; break;
                            case "Foil": if (!count.foil) count.foil = 0; count.foil += ctc.Count; break;
                            default: if (!count.other) count.other = true; break;
                        }
                    }
                });
                return count;
            }
        }
        return {total: 0};
    }
}

export default Inventory;