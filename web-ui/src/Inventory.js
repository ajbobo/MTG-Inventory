import './Inventory.css'
import Filters from './Filters'
import React from 'react';

class Inventory extends React.Component{
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className="Inventory">
                <Filters />
                <h1>Actual inventory table here</h1>
            </div>
        )
    }
}

export default Inventory;