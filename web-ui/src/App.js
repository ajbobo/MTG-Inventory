import './App.css';
import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import { initializeApp } from 'firebase/app';
import { getFirestore } from 'firebase/firestore';
import InventoryPanel from './InventoryPanel';
import Inventory from './data/Inventory';

const firebaseConfig = {
    apiKey: "AIzaSyBfAgbzeYJxOdG97bi6l8VdxTN9JUNHeMg",
    authDomain: "mtg-inventory-9d4ca.firebaseapp.com",
    projectId: "mtg-inventory-9d4ca",
    storageBucket: "mtg-inventory-9d4ca.appspot.com",
    messagingSenderId: "1044780533087",
    appId: "1:1044780533087:web:5125824c422e8662e422ea"
};

const fbapp = initializeApp(firebaseConfig);
const db = getFirestore(fbapp);

class App extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            inventory: {}
        };
    }

    componentDidMount() {
        this.getGlobalSymbols();
        this.loadUserInventory();
    }

    async scryfallApi(endpoint, page) {
        var Url = "https://api.scryfall.com/" + endpoint + (page ? "&page=" + page : "");

        console.log("Scryfall API call: " + Url);
        return fetch(Url)
            .then(res => res.json())
            .then(data => {
                console.log("Scryfall result");
                console.log(data);
                return data;
            })
            .catch(error => console.log(error));
    }

    async getGlobalSymbols() {
        const symbolData = await this.scryfallApi("symbology");

        var symbols = {};
        symbolData.data.forEach((sym) => {
            symbols[sym.symbol] = sym.svg_uri;
        })
        this.setState({
            symbols: symbols
        })
    }

    loadUserInventory() {
        let inventory = new Inventory(db);

        this.setState({
            inventory: inventory
        });
    }

    cleanUpStyles(eventKey) {
        // This feels like a very hacky way to hide/show the SetList table, but it's what I can figure out that works
        var setlist = document.getElementById("mainScreen-tabpane-setlists");
        if (eventKey === "setlists")
            setlist.style.display = "flex";
        else
            setlist.style.display = null;
    }

    convertTextToSymbols = (text) => { // This has to be declared like this so that "this" is the App, not the tab that is calling it
        if (!text)
            return null;

        let splitText = (text instanceof Array ? text : text.split(/\{|\}/)); // Split strings on { or } - It leaves some empty elements, but separates the right values
        return (
            <div className="CastingCost">
                {splitText.map((sym, index) => {
                    var name = "{" + sym + "}"; // The {} were removed in the split, so they need to be put back
                    return (sym && sym.length > 0) ? this.state.symbols[name] ? <img key={index} src={this.state.symbols[name]} alt={name} /> : sym : null
                })}
            </div>
        )
    }

    render() {
        return (
            <div className="flexprep">
                <h1 className="TitleBanner">Magic: The Gathering Inventory</h1>
                <InventoryPanel inventory={this.state.inventory} scryfallApi={this.scryfallApi} convertTextToSymbols={this.convertTextToSymbols} />
            </div>
        );
    }
}

export default App;
