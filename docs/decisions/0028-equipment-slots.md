# 0028. Equipment slots

Status: accepted
Date: 2026-10-27

## Context
A member wears two things today, a weapon and an armor, and `Items/EquipmentSlot` is both what an item is and where it
goes. The developer wants ARPG-style gear (decision 298): Weapon, Off-hand, Head, Chest, Hands, Feet, Amulet and two
Rings, the two rings being one kind of item worn twice, and the loot plan after this one will roll affixes onto items
of every kind. The Equipment page, the save and the rules (only the weapon gives the basic attack) all name slots, so
"what an item is" and "where it is worn" must come apart. Equipping takes the copy out of the inventory (ADR 0003) and
anyone wears anything (decision 165).

## Options
### A. Nine slots as a closed Core enum; an item declares a kind; the ring kind fits two slots
- Pros: the Equipment page's rows, the save's keys and the rules name a slot each, with a compiler behind them; a ring
  is one kind, so the same ring twice is nothing special (decision 329); a kind that fits two slots is one table.
- Cons: a tenth slot is a code change (an enum member, a fit, a label, a row).

### B. Slots defined in content (`rules.json` lists slots and kinds)
- Pros: a new slot without a rebuild.
- Cons: the page's layout, the save shape and the rules (one weapon gives the attack, the off-hand holds no second
  weapon) name slots anyway, so content would only add a second place where the list lives.

### C. Kind only; a member holds up to two rings as a list
- Pros: no ring slots to name.
- Cons: "which ring do I replace" needs a list index, the page's rows and the save need an order anyway, and taking a
  ring from another member (decision 137) must say which one.

## Decision
A. `Items/EquipmentSlot` is where a member wears something: Weapon, OffHand, Head, Chest, Hands, Feet, Amulet,
FirstRing, SecondRing. `Items/EquipmentKind` is what an item is worn as: Weapon, OffHand, Head, Chest, Hands, Feet,
Amulet, Ring. `Items/EquipmentFit` says which slots a kind fits: a ring both ring slots, every other kind its own slot.
Today's armor is the Chest kind.

Equipping names the slot (`PartyEquipment.TryEquip(member, item, slot)`); an item whose kind doesn't fit the slot is
refused. Anyone wears anything (decision 165), and taking a worn item from another member works per slot (decision 137).
Only the weapon gives the basic attack; the off-hand holds stats-only items, a shield or a focus, never a second weapon
(decision 322). The same ring may be worn in both ring slots (decision 329).

Saves key worn items by slot (`"worn": { "weapon": …, "firstRing": … }`), format 4; older formats are refused with no
migration, as ADR 0022 decides until the first release.

## Consequences
- A tenth slot is an enum member, an entry in the fit table, a label in `ui.json` and a row on the Equipment page.
- Two-handed weapons, out of scope (decision 322), would be a fit rule that empties the off-hand.
- Every item in content declares a `kind`; the validator refuses one without, and a weapon's damage only on the weapon
  kind.
- The Equipment page lists nine rows, so its look is done once, after the slots (E2).
