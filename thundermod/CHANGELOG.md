# 2.0.2

- Pulling sale value directly from terminal, as I always should have been.
  - Enables compatibility with ButteryFixes with no compat checks needed.
- Mentioning suits fix in ButteryFixes and how to let it work with this mod added as well.

# 2.0.1

- Fixing Player #[N] showing up when no one is in a slot.
- Adding \&colon; to the slogan parsing.
- Added a quick sanitization to the slogans where a missing or invalid weight is replaced with the previous weight in the list plus 1, allowing for quick strictly monotomically more common setting of slogans.
- Until I know how to do mod data loading, quota ad count will always be set to zero on save file loads.

# 2.0.0

- Adding probabilities to all the ad triggers.
- Last day until quota probabilities are also available separately.
- Resetting the ad wait interval on landing.
- Adding a Max Ads Per Quota setting.
- Adding slogan variables and modifiers:
  - {me} for the current player
  - {player} for a random player
  - {here} for the current planet
  - {planet} for a random planet
  - {product} for the advertised product
  - \&comma; to show a comma
  - and / to split the slogan between the bottom and top text with the first part overriding the product name that's usually displayed

# 1.2.0

- Using ads slogans that are furniture only in vanilla to tools with no sale or low sale values.
- Slogan values switched from cumulative distribution to weight function, like the ones used to select interiors on moons, monster spawns, and scrap spawns.

# 1.1.0

- Add item and furniture (combined) blacklist to prevent errors (suits break ads permanently in vanilla (v72))
- Add customizable ad slogans, defaults to vanilla.

# 1.0.1

- Fix dll version stuff

# 1.0.0

- Adding a server side config. LethalConfig is helpful but not required.
- Ads now randomly select after the first time which attempts to play a vanilla ad.
- Can play or speed up ads on player hurt or player death.

# 0.1.0

- Turns out there's more than one thing that needs to be reset each ad.
- Runs ads every 30 seconds.

# 0.0.1

- Tell the game it hasn't run an ad yet.