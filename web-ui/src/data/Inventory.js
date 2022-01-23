import { collection, query, getDocs } from 'firebase/firestore'
import { hard_coded_inventory } from '../data/hard_coded.js'

class Inventory {
    cards = {};

    constructor(db) {
        console.log("Creating new Inventory object");
        this.db = db;

        // this.setupCaching();
        this.populateCards();
    }

    // Reenable this when ready to work with Firebase again
    // setupCaching() {
    //     enableIndexedDbPersistence(this.db)
    //         .catch((err) => {
    //             if (err.code == 'failed-precondition') {
    //                 console.log("ERROR: Unable to enable caching because the app is option in multiple tabs");
    //             }
    //             else {
    //                 console.log("ERROR: Something happened during cache setup: " + err.code);
    //                 console.log(err);
    //             }
    //         })
    // }

    async populateCards() {
        console.log("Reading user_inventory from Json file");
        // console.log("Reading user_inventory from Firebase");
        // const user_inventory_query = query(collection(this.db, "user_inventory"));

        // const user_inventory = await getDocs(user_inventory_query);
        const user_inventory = hard_coded_inventory;
        user_inventory.forEach((doc) => {
            // const data = doc.data(); // Firebase-style
            const data = doc; // JSON-style
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
                    count.total += ctc.Count;
                    if (ctc.Foil) {
                        if (!count.foil) count.foil = 0;
                        count.foil += ctc.Count;
                    }
                    if (ctc.PreRelease) {
                        if (!count.prerelease) count.prerelease = 0;
                        count.prerelease += ctc.Count;
                    }
                    if (ctc.Spanish) {
                        if (!count.spanish) count.spanish = 0;
                        count.spanish += ctc.Count;
                    }
                });
                return count;
            }
        }
        return {total: 0};
    }
}

export default Inventory;