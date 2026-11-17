# Art sources

Source files the editor imports from (`gifs/`: animated GIFs turned into PNG strips by the Enemies page's Import...). The game never reads this folder.
`content/art/art-sources.json` lists the art imported from them, under the source it came from.

`digits/New-DigitStrips.ps1` renders the battle's damage digit strips (`content/art/fonts/DamageDigits.png` and
`CriticalDigits.png`) from the Cormorant Garamond font the game ships; run it by hand when their look changes.

`backgrounds/New-PlaceholderAnimatedBackground.ps1` makes the placeholder animated battle background
(`content/art/backgrounds/ForestPulse.png`, decision 910) from `Forest.png`: four 960 x 540 frames in a 2 x 2 grid, a
brightness pulse; it refuses to replace the PNG without `-Force`.
