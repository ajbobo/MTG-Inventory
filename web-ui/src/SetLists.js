import './SetLists.css'
import React from 'react';
import { collection, getDocs, query, orderBy } from 'firebase/firestore';
import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import Button from 'react-bootstrap/Button'

class SetLists extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            db: this.props.db,
            sets: []
        };
    }

    componentDidMount() {
        // Should I just make an API call to Scryfall instead?
        // That way I wouldn't have to update my database when new sets are released
        const getSets = async (db) => {
            const setsCollection = collection(db, 'sets');
            const setsSnapshot = await getDocs(query(setsCollection, orderBy('Date', 'desc')));
            var res = setsSnapshot.docs.map(doc => doc.data());
            this.setState({ sets: res });
        };

        getSets(this.state.db);
    }

    getSetContents(code) {
        const getCards = async (db) => {
            const setCollection = collection(db, code);
            const setSnapshot = await getDocs(setCollection);
            if (setSnapshot.size === 0) {
                this.setState({
                    noCards: true
                });
            }
            else {
                this.setState({
                    noCards: null
                });
            }
        }

        getCards(this.state.db);
    }

    selectSet(code, name, iconUri) {
        this.setState({
            setCode: code,
            setName: name,
            iconUri: iconUri
        });

        this.getSetContents(code);
    }

    populateSet() {
        // Make Scryfall calls to get the cards, add them to the database?
        // Or just make the API call, and join it with inventory from the db?
    }

    render() {
        return (
            <div className="SetLists">
                <div className="SetHeader">
                    <h3>
                        {this.state.iconUri ? <img src={this.state.iconUri} width="50px" height="50px" alt="" /> : null}
                        {this.state.setName ? this.state.setName : "No set selected"}
                        {this.state.iconUri ? <img src={this.state.iconUri} width="50px" height="50px" alt="" /> : null}
                    </h3>
                    <DropdownButton id="set-dropdown" title="Choose a Set">
                        {this.state.sets.map((r) => (
                            <Dropdown.Item href="#" onClick={() => { this.selectSet(r.Code, r.Name, r.Icon_Uri) }}>
                                <img src={r.Icon_Uri} alt="" /> {r.Name}
                            </Dropdown.Item>
                        ))}
                    </DropdownButton>
                </div>
                <div>
                    {this.state.noCards ?
                        <div>
                            <p>There are no cards in the set</p>
                            <Button onClick={this.populateSet()}>Get Card List</Button>
                        </div>
                        :
                        null
                    }
                </div>
            </div>
        )
    }
}

export default SetLists;