# 0041. Placed object draw order

Status: accepted
Date: 2026-11-10

## Context
A map draws its tile layers, its placed objects and the player by `DrawOrderKey`: by layer (Z), then tiles before
sprites, then the sprite's base (its bottom edge, lower on screen in front), then placement order. An object's
above-player zones (a roof, a tree's leaves) are drawn again as a canopy on the canopy layer (`MapLayers.CanopyZOf`,
at least 5, the player on 1), so the player walks under them whatever the object's layer. Until this ADR the canopies
sorted among themselves by base alone: a tree on -3 drew its leaves under the roof of a house on -4 whenever the
house's bottom edge was lower on screen, although the tree's body drew over the house's walls. The game
(`MapDrawList`) and the editor (`MapDrawing`) each built the list, which ADR 0012 flagged to watch. Plan
`2026-11-10-map-fixes-banner-and-folders`, decisions 520-522; the developer's houses and trees sit on -3 to -5, under
the player's layer, and their wish is that a higher layer draws over a lower one, canopies included.

## Options
### A. Today's rule
- Pros: nothing changes on existing maps.
- Cons: the layer is honored for bodies and ignored for canopies; the developer's bug.

### B. Layers win for bodies only
- Pros: no change to the canopy layer.
- Cons: that is today's rule; the roof still covers the higher tree's leaves.

### C. A depth offset per placement, or a sort line per object definition
- Pros: forced perspective on the player's own layer, where the player sorts by feet among the objects.
- Cons: a second knob beside the layer, which the developer doesn't want; their objects never share the player's layer.
  Kept in the backlog for the day a case needs it.

### D. One builder in Core; a sprite sorts by the layer it comes from (chosen)
- Pros: one rule read by the game, the editor's drawing and its hit test; among canopies the object's layer decides, so
  the layer is the whole model among objects; bodies are unchanged.
- Cons: wherever two overlapping objects on different layers both have canopies and the lower layer's object has the
  lower base, their canopies swap on existing maps.

## Decision
Core's `MapDrawOrder` is the only place the parts are made and sorted (decision 520): the game fills its list with the
player, the editor fills its list without, the editor's hit test asks for an object's key. A `DrawOrderKey` carries the
layer the part comes from, a canopy's being its object's (`ForCanopy`); on one Z, tiles come before sprites, sprites
order by the layer they come from, then their base, then placement order (decision 521). A canopy's item belongs to its
object's layer, so the editor dims and locks it with its object (decision 522).

## Consequences
- A tree on -3 draws its leaves over the roof of a house on -4, always; two trees on one layer still sort by base.
- A canopy still draws over the player, so over every body below the canopy layer: a tree behind a house (on a lower
  layer) shows its leaves over the house's walls where they overlap, as before. Only a sort rule per object would change
  that (the backlog's depth offset).
- Content and save formats are untouched.
- ADR 0012's watch on the two copies of the draw order is closed: a visual difference between the game and the editor
  now means a drawer, not the order.
