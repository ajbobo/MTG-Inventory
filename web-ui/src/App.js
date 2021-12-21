import './App.css';
import React from 'react';
import { initializeApp } from 'firebase/app';
import { getFirestore, collection, getDocs, query, orderBy } from 'firebase/firestore';

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

class SetTable extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            sets: []
        };
    }

    componentDidMount() {
        const getSets = async (db) => {
            const setsCollection = collection(db, 'sets');
            const setsSnapshot = await getDocs(query(setsCollection, orderBy('Date', 'desc')));
            var res = setsSnapshot.docs.map(doc => doc.data());
            this.setState({ sets: res });
        };

        getSets(db);
    }

    render() {
        return (
            <table className="simple_table">
                <thead>
                    <tr>
                        <th>Code</th>
                        <th>Set Name</th>
                        <th>Icon</th>
                    </tr>
                </thead>
                <tbody>
                    {this.state.sets.map((r) => (
                        <tr>
                            <td>{r.Code}</td>
                            <td>{r.Name}</td>
                            <td><img src={r.Icon_Uri} /></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        )
    }
}

function App() {
    return (
        <div>
            <h1>React testing here</h1>
            <SetTable />
        </div>
    );
}

export default App;
