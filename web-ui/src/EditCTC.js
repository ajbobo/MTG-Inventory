import './EditCTC.css'
import React from 'react'
import Popover from 'react-bootstrap/Popover';
import PopoverBody from 'react-bootstrap/PopoverBody';

function formatCTC(ctc) {
    var description = '';
    for (let prop of Object.keys(ctc)) {
        if (prop != 'Count')
            description += (description.length > 0 ? " | " : "") + prop;
    }

    return (
        <tr>
            <td>{description.length > 0 ? description : "Standard"}</td>
            <td style={{width:"25px", textAlign:"center"}}><b>{ctc.Count}</b></td>
            <td>
                <button class="CTCButton">+</button>
                <button class="CTCButton">-</button>
                <button class="CTCButton">=</button>
                <button class="CTCButton">X</button>
            </td>
        </tr>
    )
}

function editCTC(card) {
    return (
        <Popover id='edit_ctc'>
            <PopoverBody>
                <table class="CTCTable">
                    {card.counts.map((ctc) => formatCTC(ctc))}
                </table>
            </PopoverBody>
        </Popover>
    );
}

export default editCTC;