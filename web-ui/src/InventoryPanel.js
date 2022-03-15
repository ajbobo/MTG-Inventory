import './InventoryPanel.css'
import Filters from './Filters'
import editCTC from './EditCTC'
import React from 'react'
import DropdownButton from 'react-bootstrap/DropdownButton'
import Dropdown from 'react-bootstrap/Dropdown'
import Table from 'react-bootstrap/Table'
import OverlayTrigger from 'react-bootstrap/OverlayTrigger'

class InventoryPanel extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
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
        const setData = await this.props.scryfallApi("sets");

        var filteredSets = [];
        setData.data.forEach((set) => {
            if (!set.digital && (set.set_type === "core" || set.set_type === "expansion"))
                filteredSets.push(set);
        });

        this.setState({
            sets: filteredSets
        })
    }

    async getSetContents(code) {
        this.setState({ // Try to force a refresh that shows a loading message
            cards: [],
            filteredCards: [],
            loading: true
        });

        // Call Scryfall and get the cards for the specified set
        var curCardList = [];
        var page = 1;
        var needMore = true;
        while (needMore) {
            const fullData = await this.props.scryfallApi("cards/search?q=set:" + code + "&order=set&unique=prints", page);

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

        if (include && filters.qty) {
            const cardRecord = this.props.inventory.getCard(this.state.setCode, card.collector_number);
            const qty = this.props.inventory.getCardCount(cardRecord).total;
            // esLint doesn't like what I'm about to do, tell it to ignore the == and != 
            // eslint-disable-next-line
            if (filters.qty == 0 && qty != 0) // filters.qty will be a string, this allows comparisons between strings and ints
                include = false;
            else if (filters.qty === '<4')
                include = (qty < 4);
            else
                include = (qty >= filters.qty);
        }

        return include;
    }

    filtersChanged(filters) {
        if (!this.state.cards)
            return;

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

    displayCTCInventory(card) {
        const cardRecord = this.props.inventory.getCard(this.state.setCode, card.collector_number);
        const counts = this.props.inventory.getCardCount(cardRecord);

        return (
            <OverlayTrigger trigger='click' placement='right' overlay={(props) => editCTC(props, cardRecord, this.props.inventory)} rootClose='true'>
                <div className='QtyCell'>
                    {counts.total}
                    {counts.foil ? <img src='foil.png' alt='' title={'Foil: ' + counts.foil} /> : null}
                    {counts.other ? <img src='other.png' alt='' title={'Other printings: ' + counts.other} /> : null}
                </div>
            </OverlayTrigger>
        );
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
                                        <th width="80px">Cnt</th>
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
                                            <td>{this.displayCTCInventory(card)}</td>
                                            <td>{card.name}</td>
                                            <td>{this.props.convertTextToSymbols(card.color_identity ? card.color_identity : null)}</td>
                                            <td>{this.props.convertTextToSymbols(card.mana_cost ? card.mana_cost : card.card_faces ? card.card_faces[0].mana_cost : null)}</td>
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