import './Filters.css'
import React from 'react';
import Button from 'react-bootstrap/Button'
import Form from 'react-bootstrap/Form'
import FormGroup from 'react-bootstrap/FormGroup';
import FormLabel from 'react-bootstrap/FormLabel';
import FormSelect from 'react-bootstrap/FormSelect';
import FormControl from 'react-bootstrap/FormControl'
import FormCheck from 'react-bootstrap/FormCheck'
import Row from 'react-bootstrap/Row'
import Col from 'react-bootstrap/Col'

class Filters extends React.Component {
    constructor(props) {
        super(props);
    }

    OnApply() {
        alert("You pressed Apply");
    }

    render() {
        return (
            <Form className='Filters mb-3'>
                <FormGroup className="mb-3" controlId="formSet">
                    <Row>
                        <Col sm={1}>
                            <FormLabel column>Set</FormLabel>
                        </Col>
                        <Col>
                            <FormSelect column id="frmSelectSet">
                                <option>DOM</option>
                                <option>MID</option>
                                <option>VOW</option>
                            </FormSelect>
                        </Col>
                    </Row>
                </FormGroup>

                <FormGroup className="mb-3 " controlId="formName">
                    <Row>
                        <Col sm={1}>
                            <FormLabel column>Name</FormLabel>
                        </Col>
                        <Col>
                            <FormControl type="text" />
                        </Col>
                    </Row>
                </FormGroup>

                <Row>
                    <Col>
                        <FormGroup className="mb-3 " controlId="formColor">
                            <Row>
                                <FormLabel>Color</FormLabel>
                            </Row>
                            <Row>
                                <Col sm={3}>
                                    <FormCheck type="checkbox" label="White" />
                                    <FormCheck type="checkbox" label="Blue" />
                                    <FormCheck type="checkbox" label="Black" />
                                </Col>
                                <Col sm={3}>
                                    <FormCheck type="checkbox" label="Red" />
                                    <FormCheck type="checkbox" label="Green" />
                                    <FormCheck type="checkbox" label="Colorless" />
                                </Col>
                            </Row>
                        </FormGroup>
                    </Col>

                    <Col>
                        <FormGroup className="mb-3 " controlId="formRarity">
                            <FormLabel>Rarity</FormLabel>
                            <div className='mb-3'>
                                <FormCheck type="checkbox" label="Common" />
                                <FormCheck type="checkbox" label="Uncommon" />
                                <FormCheck type="checkbox" label="Rare" />
                                <FormCheck type="checkbox" label="Mythic" />
                            </div>
                        </FormGroup>
                    </Col>
                </Row>

                <FormGroup className="mb-3 " controlId="formQty">
                    <Row>
                        <Col sm={1}>
                            <FormLabel column>Quantity</FormLabel>
                        </Col>
                        <Col sm={2}>
                            <FormSelect>
                                <option></option>
                                <option>0</option>
                                <option>1+</option>
                                <option>4+</option>
                            </FormSelect>
                        </Col>
                    </Row>
                </FormGroup>

                <Button onClick={() => this.OnApply()}>Apply</Button>
            </Form>
        )
    }
}

export default Filters;