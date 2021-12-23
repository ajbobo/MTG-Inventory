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
        this.state = {
            curTab: "inventory"
        };
    }

    chooseTab(tabName) {
        console.log("chooseTab -> tabName:" + tabName);
        this.setState({
            curTab:tabName
        })
    }

    render() {
        return (
            <div>
                <h1>Magic: The Gathering Inventory</h1>
                <Tabs defaultActiveKey="inventory" id="mainScreen" className="mb-3">
                    <Tab eventKey="inventory" title="Inventory"><Inventory/></Tab>
                    <Tab eventKey="setlists" title="Set Lists"><SetLists db={db}/></Tab>
                    <Tab eventKey="prices" title="Prices"><Prices/></Tab>
                </Tabs>
            </div>
        );
    }
}

export default App;
