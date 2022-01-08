import './InventoryPanel.css'
import Filters from './Filters'
import React from 'react';
import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import Table from 'react-bootstrap/Table'

class InventoryPanel extends React.Component {
    constructor(props) {
        super(props);
        this.scryfallApi = props.scryfallApi;
        this.convertTextToSymbols = props.convertTextToSymbols;
        this.state = {
            db: this.props.db,
            selectedSet: "Choose a Set",
            selectedCard: null,
            sets: [],
            cards: [],
            filteredCards: [],
            filters: {},
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

        // console.log("filtered sets");
        // console.log(filteredSets);

        this.setState({
            sets: filteredSets
        })
    }

    async getSetContents(code) {
        // Call Scryfall and get the cards for the specified set
        this.setState({ // Try to force a refresh that shows a loading message
            cards: [],
            filteredCards: [],
            loading: true
        });

        var curCardList = [];
        var page = 1;
        var needMore = true;
        while (needMore) {
            const fullData = await this.scryfallApi("cards/search?q=set:" + code + "&order=set&unique=prints", page);

            fullData.data.forEach(card => { if (!card.digital) curCardList.push(card) });

            if (fullData.has_more)
                page++;
            else
                needMore = false;

            await new Promise(r => setTimeout(r, 100)); // Add a short pause between API calls to not overload Scryfall's servers
        }

        this.setState({
            cards: curCardList,
            filteredCards: curCardList,
            loading: false
        });

        this.filtersChanged(this.state.filters);
    }

    selectSet(code, name, iconUri) {
        this.setState({
            setCode: code,
            setName: name,
            iconUri: iconUri,
            selectedSet: name,
            selectedCard: null
        });

        this.getSetContents(code);
    }

    selectCard(card) {
        this.setState({
            selectedCard: card
        });
    }

    checkFilters(card, filters) {
        if (!card)
            return false;

        let include = true;
        if (filters.rarity) {
            include = filters.rarity.indexOf(card.rarity.toUpperCase()[0]) >= 0;
        }

        if (include && filters.color) {
            // Check for colors
            let hasColor = false; // Only one color in the color identity needs to be in the filters
            card.color_identity.forEach((color) => {
                hasColor |= filters.color.indexOf(color.toUpperCase()) >= 0;
            });
            include = hasColor;

            // Check for colorless
            if (card.color_identity.length === 0) {
                include = filters.color.indexOf("N") >= 0;
            }
        }
        return include;
    }

    filtersChanged(filters) {
        if (!this.state.cards)
            return;

        // console.log("Changed filters");
        // console.log(filters);

        let filtered = [];

        this.state.cards.forEach((card) => {
            if (this.checkFilters(card, filters))
                filtered.push(card);
        })

        this.setState({
            filteredCards: filtered,
            filters: filters,
            selectedCard: (this.checkFilters(this.state.selectedCard, filters) ? this.state.selectedCard : null),
        })
    }

    render() {
        return (
            <div className="InventoryPanel">
                <div className="InventoryHeader">
                    {this.state.iconUri ? <img src={this.state.iconUri} alt="" /> : null}
                    <DropdownButton id="set-dropdown" title={this.state.selectedSet} height="400px">
                        {this.state.sets.map((r, index) => (
                            <Dropdown.Item key={index} href="#" onClick={() => { this.selectSet(r.code, r.name, r.icon_svg_uri) }}>
                                <img src={r.icon_svg_uri} alt="" /> {r.name}
                            </Dropdown.Item>
                        ))}
                    </DropdownButton>
                    {this.state.iconUri ? <img src={this.state.iconUri} alt="" /> : null}
                </div>
                <Filters sets={this.state.sets} OnChanged={(filters) => this.filtersChanged(filters)} />
                {this.state.loading ? <div><h3>loading cards...</h3></div> : null}
                {(this.state.filteredCards && this.state.filteredCards.length) > 0 ?
                    <div className="CardInfoPanel">
                        <div className="CardTable">
                            <Table striped hover bordered size="sm">
                                <thead>
                                    <tr>
                                        <th width="35px">#</th>
                                        <th>Card Name</th>
                                        <th width="115px">Color Identity</th>
                                        <th width="115px">Casting Cost</th>
                                        <th width="35px">Rarity</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {this.state.filteredCards.map((card, index) => (
                                        <tr key={index} onClick={() => { this.selectCard(card) }} className={this.state.selectedCard === card ? "Selected" : ""}>
                                            <td>{card.collector_number}</td>
                                            <td>{card.name}</td>
                                            <td>{this.convertTextToSymbols(card.color_identity ? card.color_identity : null)}</td>
                                            <td>{this.convertTextToSymbols(card.mana_cost ? card.mana_cost : card.card_faces ? card.card_faces[0].mana_cost : null)}</td>
                                            <td><img className="Rarity" src={card.rarity + ".png"} title={card.rarity} alt={card.rarity} /></td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </div>
                        {this.state.selectedCard ?
                            this.state.selectedCard.image_uris ?
                                <div className="CardPanel">
                                    <img src={this.state.selectedCard.image_uris.normal} alt="" />
                                </div>
                                :
                                this.state.selectedCard.card_faces ?
                                    <div className="CardPanel">
                                        <img src={this.state.selectedCard.card_faces[0].image_uris.normal} alt="" />
                                        <img src={this.state.selectedCard.card_faces[1].image_uris.normal} alt="" />
                                    </div>
                                    :
                                    null
                            :
                            null
                        }
                    </div>
                    :
                    null
                }
            </div>
        )
    }
}

export default InventoryPanel;