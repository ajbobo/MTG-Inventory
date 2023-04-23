Here are the API endpoints:

* **/cards**
    * Get a list of all cards.
* **/cards/add**
    * Add a new card.
* **/cards/update**
    * Update an existing card.
* **/cards/delete**
    * Delete a card.
* **/sets**
    * Get a list of all sets.
* **/sets/add**
    * Add a new set.
* **/sets/update**
    * Update an existing set.
* **/sets/delete**
    * Delete a set.
* **/cards/bysetcode**
    * Get a list of cards in the specified set.

All of these endpoints use the HTTP GET method. The responses are JSON objects.

For example, the following request will get a list of all cards:

```
GET /cards
```

The response will be a JSON object with an array of Card objects.

The following request will add a new card:

```
POST /cards
```

The request body should be a JSON object with the following properties:

* **name:** The name of the card.
* **manaCost:** The mana cost of the card.
* **type:** The type of the card.

The response will be a JSON object with the new Card object.

The following request will update an existing card:

```
PUT /cards/123
```

The request body should be a JSON object with the following properties:

* **name:** The new name of the card.
* **manaCost:** The new mana cost of the card.
* **type:** The new type of the card.

The response will be a JSON object with the updated Card object.

The following request will delete a card:

```
DELETE /cards/123
```

The response will be a JSON object with the deleted Card object.

The following request will get a list of cards in the specified set:

```
GET /cards/bysetcode/M21
```

The response will be a JSON object with an array of Card objects.

I hope this helps! Let me know if you have any other questions.