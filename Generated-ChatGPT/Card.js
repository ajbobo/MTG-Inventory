import React from 'react';

function Card({ card }) {
  return (
    <div className="card">
      <img src={card.imageUrl} alt={card.name} />
      <div className="card-details">
        <h2>{card.name}</h2>
        <p>Set: {card.set}</p>
        <p>Collector Number: {card.collectorNumber}</p>
        <p>Price: ${card.price}</p>
        {card.priceFoil ? <p>Foil Price: ${card.priceFoil}</p> : null}
      </div>
    </div>
  );
}

export default Card;
