import './App.css';
import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import Tabs from 'react-bootstrap/Tabs';
import Tab from 'react-bootstrap/Tab';
import { initializeApp } from 'firebase/app';
import { getFirestore } from 'firebase/firestore';
import Inventory from './Inventory';
import SetLists from './SetLists';
import Prices from './Prices';

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
    }

    componentDidMount() {
        this.getGlobalSymbols();
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

    convertTextToSymbols = (text) => { // This has to be declared like this so that "this" is the App, not the tab that is calling it
        if (!text)
            return null;

        let splitText = text.split(/\{|\}/); // Split on { or } - It leaves some empty elements, but separates the right values
        return (
            <div className="CastingCost">
                {splitText.map((sym, index) => (
                    (sym && sym.length > 0) ? <img key={index} src={this.state.symbols["{" + sym + "}"]} alt={"{" + sym + "}"}/> : null // The {} were removed in the split, so they need to be put back
                ))}
            </div>
        )
    }

    render() {
        return (
            <div>
                <h1>Magic: The Gathering Inventory</h1>
                <Tabs defaultActiveKey="setlists" id="mainScreen" className="mb-3">
                    <Tab eventKey="inventory" title="Inventory"><Inventory/></Tab>
                    <Tab eventKey="setlists" title="Set Lists"><SetLists db={db} scryfallApi={this.scryfallApi} convertTextToSymbols={this.convertTextToSymbols}/></Tab>
                    <Tab eventKey="prices" title="Prices"><Prices/></Tab>
                </Tabs>
            </div>
        );
    }
}

export default App;
