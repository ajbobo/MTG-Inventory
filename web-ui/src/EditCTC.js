import './EditCTC.css'
import React from 'react'
import Popover from 'react-bootstrap/Popover';
import PopoverHeader from 'react-bootstrap/PopoverHeader';
import PopoverBody from 'react-bootstrap/PopoverBody';

function formatCTC(ctc) {
    var description = '';
    for (let prop of Object.keys(ctc)) {
        if (prop != 'Count')
            description += (description.length > 0 ? " | " : "") + prop;
    }

    return (
        <div>
            <p>{description.length > 0 ? description : 'Standard'}  {ctc.Count}</p>
            {/* Add buttons here */}
        </div>
    );
}

function editCTC(card) {
    if (!card)
        return null; // TODO: This should still open the Popover, but then the user should be able to add new cards/types

    return (
        <Popover id='edit_ctc'>
            <PopoverHeader>{card.name}</PopoverHeader>
            <PopoverBody>{card.counts.map((ctc) => formatCTC(ctc))}</PopoverBody>
        </Popover>
    );
}

export default editCTC;