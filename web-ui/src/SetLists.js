import './SetLists.css'
import React from 'react';
import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import Table from 'react-bootstrap/Table'

class SetLists extends React.Component {
    constructor(props) {
        super(props);
        this.scryfallApi = props.scryfallApi;
        this.convertTextToSymbols = props.convertTextToSymbols;
        this.state = {
            db: this.props.db,
            selectedSet: "Choose a Set",
            sets: [],
            cards: [],
            loading: false
        };
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
                needMore = false;

            await new Promise(r => setTimeout(r, 100)); // Add a short pause between API calls to not overload Scryfall's servers
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
            iconUri: iconUri,
            selectedSet: name
        });

        this.getSetContents(code);
    }

    render() {
        return (
            <div className="SetLists">
                <div className="SetHeader">
                    {this.state.iconUri ? <img src={this.state.iconUri} width="50px" height="50px" alt="" /> : null}
                    <DropdownButton id="set-dropdown" title={this.state.selectedSet}>
                        {this.state.sets.map((r, index) => (
                            <Dropdown.Item key={index} href="#" onClick={() => { this.selectSet(r.code, r.name, r.icon_svg_uri) }}>
                                <img src={r.icon_svg_uri} alt="" /> {r.name}
                            </Dropdown.Item>
                        ))}
                    </DropdownButton>
                    {this.state.iconUri ? <img src={this.state.iconUri} width="50px" height="50px" alt="" /> : null}
                </div>
                <div>
                    {this.state.loading ? <h3>loading cards...</h3> : null}
                </div>
                <div className="CardTable">
                    {(this.state.cards && this.state.cards.length) > 0 ?
                        <Table striped hover bordered size="sm">
                            <thead>
                                <tr>
                                    <th width="35px">#</th>
                                    <th>Card Name</th>
                                    <th width="115px">Casting Cost</th>
                                    <th width="115px">Rarity</th>
                                </tr>
                            </thead>
                            <tbody>
                                {this.state.cards.map((card, index) => (
                                    <tr key={index}>
                                        <td>{card.collector_number}</td>
                                        <td>{card.name}</td>
                                        <td>{this.convertTextToSymbols(card.mana_cost ? card.mana_cost : card.card_faces ? card.card_faces[0].mana_cost : null)}</td>
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