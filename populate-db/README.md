# Populate DB

This is a simple C# project that used the Scryfall API to populate a Firestore database with the list of available MTG sets.
It only includes CORE and EXPANSION sets.

Scryfall has a lot of fields for each set. The following fields are stored in the DB:
- Name
- Code
- Release date
- Icon Uri