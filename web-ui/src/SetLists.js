import './SetLists.css'
import React from 'react';
import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import Table from 'react-bootstrap/Table'

class SetLists extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            db: this.props.db,
            sets: [],
            cards: [],
            loading: false
        };
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

    async componentDidMount() {
        this.getSetInfo();
    }

    async getSetInfo() {
        // Call Scryfall and get the sets that I'm interested in
        const setData = await this.scryfallApi("sets");

        var filteredSets = [];
        setData.data.forEach((set) => {
            if (!set.digital && (set.set_type === "core" || set.set_type === "expansion"))
                filteredSets.push(set);
        });

        console.log("filtered sets");
        console.log(filteredSets);

        this.setState({
            sets: filteredSets
        })
    }

    async getSetContents(code) {
        // Call Scryfall and get the cards for the specified set
        this.setState({ // Try to force a refresh that shows a loading message
            cards: [],
            loading: true
        });

        var curCardList = [];
        var page = 1;
        var needMore = true;
        while (needMore) {
            const fullData = await this.scryfallApi("cards/search?q=set:" + code + "&order=set&unique=prints", page);

            fullData.data.forEach(card => curCardList.push(card));

            if (fullData.has_more)
                page++;
            else
                needMore = false
        }

        this.setState({
            cards: curCardList,
            loading: false
        });
    }

    selectSet(code, name, iconUri) {
        this.setState({
            setCode: code,
            setName: name,
            iconUri: iconUri
        });

        this.getSetContents(code);
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
                            <Dropdown.Item href="#" onClick={() => { this.selectSet(r.code, r.name, r.icon_svg_uri) }}>
                                <img src={r.icon_svg_uri} alt="" /> {r.name}
                            </Dropdown.Item>
                        ))}
                    </DropdownButton>
                </div>
                <div>
                    {this.state.loading ? <h3>loading cards...</h3> : null}
                </div>
                <div>
                    {(this.state.cards && this.state.cards.length) > 0 ?
                        <Table striped bordered hover>
                            <thead>
                                <th>#</th>
                                <th>Card Name</th>
                                <th>Rarity</th>
                            </thead>
                            <tbody>
                                {this.state.cards.map((card) => (
                                    <tr>
                                        <td>{card.collector_number}</td>
                                        <td>{card.name}</td>
                                        <td>{card.rarity}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </Table>
                        :
                        null
                    }
                </div>
            </div>
        )
    }
}

export default SetLists;