import './EditCTC.css'
import React from 'react'
import Popover from 'react-bootstrap/Popover';
import PopoverBody from 'react-bootstrap/PopoverBody';

var _inventory = null;

function formatCTC(ctc, card, index) {
    var description = '';
    for (let prop of Object.keys(ctc)) {
        if (prop !== 'Count')
            description += (description.length > 0 ? " | " : "") + prop;
    }

    return (
        <tr key={index}>
            <td>{description.length > 0 ? description : "Standard"}</td>
            <td style={{ width: "25px", textAlign: "center" }}><b>{ctc.Count}</b></td>
            <td>
                <button className="CTCButton" onClick={() => addOne(ctc, card)}>+</button>
                <button className="CTCButton" onClick={() => subtractOne(ctc, card)}>-</button>
                <button className="CTCButton" onClick={() => setToFour(ctc, card)}>=</button>
                <button className="CTCButton" onClick={() => deleteCTC(ctc, card)}>X</button>
            </td>
        </tr>
    )
}

function editCTC(props, card, inventory) {
    // console.log("editCTC() - (" + card.collectorNumber + ")");
    _inventory = inventory;
    
    return (
        <Popover id='edit_ctc' {...props}>
            <PopoverBody>
                <table className="CTCTable">
                    <tbody>
                        {card.counts.map((ctc, index) => formatCTC(ctc, card, index))}
                    </tbody>
                </table>
            </PopoverBody>
        </Popover>
    );
}

function addOne(ctc, card) {
    ctc.Count += 1;
    if (_inventory)
        _inventory.updateInventory(card, ctc);
}

function subtractOne(ctc, card) {
    ctc.Count = Math.max(ctc.Count - 1, 0);
    if (_inventory)
        _inventory.updateInventory(card, ctc);
}

function setToFour(ctc, card) {
    ctc.Count = 4;
    if (_inventory)
        _inventory.updateInventory(card, ctc);
}

function deleteCTC(ctc, card) {
    alert("Delete not implemented yet");
}

export default editCTC;