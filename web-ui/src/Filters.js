import './Filters.css'
import React from 'react';
import Form from 'react-bootstrap/Form'
import FormGroup from 'react-bootstrap/FormGroup';
import FormLabel from 'react-bootstrap/FormLabel';
import FormSelect from 'react-bootstrap/FormSelect';
import FormText from 'react-bootstrap/FormText';
import FormCheck from 'react-bootstrap/FormCheck'
import Row from 'react-bootstrap/Row'
import Col from 'react-bootstrap/Col'

class Filters extends React.Component {
    constructor(props) {
        super(props);

        this.OnChanged = props.OnChanged;

        this.state = {
            filters: {}
        }
    }

    OnChanged = (filters) => {
        console.log("Not Overwritten");
        console.log(filters);
    }

    filtersToString(filters) {
        let res = "< ";
        if (filters.color) {
            res += "Color:" + filters.color + " ";
        }
        if (filters.rarity) {
            res += "Rarity:" + filters.rarity + " ";
        }
        if (filters.qty) {
            res += "Qty:" + filters.qty + " ";
        }
        res += ">";

        return res;
    }

    updateSingleFilter(filterStr, value) {
        let res = filterStr;
        if (res && res.indexOf(value) >= 0) { // The value is already selected; clear it
            res = res.replace(value, '');
        }
        else if (res) { // The value is new; add it to the list
            res += value;
        }
        else { // There are no selected values right now; create the list
            res = value;
        }
        return res;
    }

    updateFilters(changeRequest) {
        let filters = this.state.filters;

        if (changeRequest === 'all') {
            filters = {}; // TODO: This needs to be called from a button, which also needs to reset the checkboxes/etc.
        }
        else if (changeRequest.startsWith('color:')) {
            const color = changeRequest.at(-1).toUpperCase(); // Get the last character in the string
            filters.color = this.updateSingleFilter(filters.color, color);
        }
        else if (changeRequest.startsWith('rarity:')) {
            const rarity = changeRequest.at(-1).toUpperCase(); // Get the last character in the string
            filters.rarity = this.updateSingleFilter(filters.rarity, rarity);
        }
        else if (changeRequest.startsWith('qty:')) {
            filters.qty = changeRequest.substring(4);
        }

        this.setState({
            filters: filters
        })

        this.OnChanged(filters);
    }

    render() {
        return (
            <Form className='Filters'>
                <Row>
                    <Col sm='auto'>
                        <FormGroup controlId="formColor">
                            <Row>
                                <Col>
                                    <FormCheck type="checkbox" label="White" onClick={() => this.updateFilters("color:W")} />
                                    <FormCheck type="checkbox" label="Blue" onClick={() => this.updateFilters("color:U")} />
                                    <FormCheck type="checkbox" label="Black" onClick={() => this.updateFilters("color:B")} />
                                </Col>
                                <Col>
                                    <FormCheck type="checkbox" label="Red" onClick={() => this.updateFilters("color:R")} />
                                    <FormCheck type="checkbox" label="Green" onClick={() => this.updateFilters("color:G")} />
                                    <FormCheck type="checkbox" label="Colorless" onClick={() => this.updateFilters("color:N")} />
                                </Col>
                            </Row>
                        </FormGroup>
                    </Col>

                    <Col sm='auto'>
                        <FormGroup controlId="formRarity">
                            <div>
                                <FormCheck type="checkbox" label="Common" onClick={() => this.updateFilters("rarity:C")} />
                                <FormCheck type="checkbox" label="Uncommon" onClick={() => this.updateFilters("rarity:U")} />
                                <FormCheck type="checkbox" label="Rare" onClick={() => this.updateFilters("rarity:R")} />
                                <FormCheck type="checkbox" label="Mythic" onClick={() => this.updateFilters("rarity:M")} />
                            </div>
                        </FormGroup>
                    </Col>

                    <Col sm='auto'>
                        <FormGroup as={Row} controlId="formQty">
                            <FormLabel column sm='auto'>Quantity</FormLabel>
                            <Col>
                                <FormSelect onChange={(evt) => this.updateFilters("qty:" + evt.target.value)}>
                                    <option value=''></option>
                                    <option value='0'>0</option>
                                    <option value='1'>1+</option>
                                    <option value='4'>4+</option>
                                </FormSelect>
                            </Col>
                        </FormGroup>
                    </Col>
                </Row>

                <Row>
                    <FormText style={{"textAlign":"center"}}>Current Filters: {this.filtersToString(this.state.filters)}</FormText>
                </Row>
            </Form>
        )
    }
}

export default Filters;