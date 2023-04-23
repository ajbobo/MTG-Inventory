import React, { useState, useEffect } from 'react';
import CardList from './CardList';

function CollectionPage() {
  const [sets, setSets] = useState([]);
  const [selectedSet, setSelectedSet] = useState(null);
  const [cards, setCards] = useState([]);

  useEffect(() => {
    fetch('/api/sets')
      .then(res => res.json())
      .then(data => setSets(data))
      .catch(err => console.error(err));
  }, []);

  useEffect(() => {
    if (selectedSet) {
      fetch(`/api/collection/${selectedSet}`)
        .then(res => res.json())
        .then(data => setCards(data))
        .catch(err => console.error(err));
    }
  }, [selectedSet]);

  const handleSetChange = (event) => {
    setSelectedSet(event.target.value);
  }

  return (
    <div className="collection-page">
      <h1>My Collection</h1>
      <div className="set-dropdown">
        <label htmlFor="set-select">Select a Set:</label>
        <select name="set" id="set-select" value={selectedSet} onChange={handleSetChange}>
          <option value="">--Select a Set--</option>
          {sets.map(set => (
            <option key={set.setCode} value={set.setCode}>{set.setName}</option>
          ))}
        </select>
      </div>
      {selectedSet ? <CardList cards={cards} /> : null}
    </div>
  );
}

export default CollectionPage;
