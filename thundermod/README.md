I'm guessing you play Lethal Company to get ads, so this will increase the nmber of ads introduced in v70.

There are a number of settings. You can set the max number of ads per day, whether to play an ad as soon as possible on landing, the delay between ads, play ad or reroll timing on deaths, play ad or reroll timing on damage, the earliest time an ad can play, and the latest time an ad can play. And all of these triggers can be set to have a chance of happening instead of being guaranteed, if that is more your style!

Any time a reroll happens, the ad will happen sooner (or the same time), never later.

Also, ads now play if you're the last one left as well, which means they'll appear in solo play as well.

*New*: Customize the ad slogans! Format is [slogan]:[weight], and comma separated, where weight is the unnormalized chance that slogan will be rolled. The odds do not have to add up to 100, nor does the order matter. And there are a number of extra options that will be outlined below.

# Options

In full detail:

### Max ads per day
Pretty self explanatory. Lets you cap the number that will play between trips to orbit.

### Max ads per quota
Also pretty self explanatory. Number of ads that can play between each time a quota period is started. To mirror vanilla, set this to 1.

### Chance to play on landing
Each time you land on a planet, this rolls. Setting it to 0% will prevent this ad, and mirror vanilla results.

### Chance to play on landing on last day
Same as the previous but adjusts the chance on the last day. Vanilla like is still 0%.

### Chance to play at least one ad without other triggers
If the chance to play on landing rolls no ads, this tells the game whether to schedule one upon landing. The vanilla value is 33%.

### Chance to play at least one ad without other triggers on last day
The name might be too long.... But this is the alternate probability used on the last day. Vanilla uses 60% here.

### Chance to schedule an ad after one is played
After one ad plays, this tells the game how likely it should be to setup a second one to play at a random time between now and the last time an ad can play during a day. To set this like vanilla, set it to 0%.

### Chance to schedule an ad after one is played on last day
The same as above but altered value for the final day of the quota.

### Earliest time to show ad
A normalized in game time between 0 and 1 where 0 is roughly when the ship lands, and 1 is roughly when the ship leaves (technically it's 0.996)

### Latest time to show ad
Another normalized in game time that caps the max time that can be rolled for an ad to play. Technically, LC will not play ads after 0.9f (unless there's a race condition, but we're not worried about that here), but the value can be set to 1.0 despite this.

### Minimum time between ads seconds
Adjusts the real time between ads playing. Triggers can happen as much as they want, but each ad will only play this close to each other.

### Play ad on death
The type of trigger that occurs when a player death occurs. The three settings are Immediately, RerollNext, and None. Immediately sets the time for the next ad to be as soon as possible, which is overwritten in part only by the minimum time between ads setting. RerollNext will set or update the next ad time to a random time between now and the latest time to show ad, where updates can only speed up the next ad, and should next push one later into the day.

### Chance to play ad on death
This adds a chance to the death trigger chosen, so you can have more control over the ad timings.

### Chance to play ad on death on last day
The chance will switch to this on the last day of the quota.

### Play ad on hurt
This is the same as Play ad on death but will roll each time a player gets hurt.

### Chance to play ad on hurt
Similarly, this adds a chance for the hurt trigger chosen.

### Chance to play ad on hurt last day
Similarly, this overrides the chance on the last day of the quota.

### Blacklist
Ads to never play. Contains all vanilla suits by default as Lethal Company (in vanilla) will have its ad routines crash and never fix themselves until the game is restarted if any suit ad tries to play.
- Note: If you have ButterFixes, suits will work and you can remove them all from the blacklist.

### Sales text
The text to use during ads. If a chosen item has a sale of 20% or less, or no sale, this will set the text instead. Very customizable.

No really. Each slogan can have its own weighted chance, and the total weight doesn't have to add up to 100 because that would be hard to maintain over time. And each slogan can have special elements that alter how they work. As of now, those elements are five variables that autopopulate and an option to overwrite the top text.

Using the following:
- {me}
  - Will put the current player's in game name into the ad.
- {player}
  - Will put a random player's in game name into the ad.
- {here}
  - Will put the current planet's name into the ad.
- {planet}
  - Will put a random planet's name into the ad.
- {product}
  - Will put the advertised items name into the ad.
- \&comma;
  - Will put a comma into the ad (as it would otherwise split the entries in the raw config, this is required)
- \&colon;
  - Will put a colon into the ad (as it would otherwise split the entry from the weight in the raw config, this is required)
- /
  - Will split the entry into top text (the left side of the entry) and bottom text.

All together you can make an entry like "Hey {me}\&comma; get {player} one while you're on {here}/{planet} doesn't sell {product}s!:100" to really confuse your friends.

#### Quick settings

If you enter a list of strings, or add a string in without a weight, it will set a weight for you of the previous weight plus one, with a minimum automatic value of one. This way you can enter things quickly, starting with the rarest one.

# Miscellanious fixes

- By default the game gets the ad sale value wrong on clients. This mod fixes that.
  - ButteryFixes fixes this too, but these patches no longer conflict.
- If a suit ad tries to play, something null reference exceptions and then no ad can ever play again. A custom ad selection is forced every time to prevent this, with configurable blacklist.
  - If you're using ButterFixes, it actually fixes the suit ads so they can be played again. In that case, you might want to remove the suits from the ads blacklist.

# Known issues

- Ad count per quota does not save and will reset every time the file is loaded.

# Compatibility

- Should work with ButterFixes as of version 2.0.2
  - You can remove suits from the blacklist if and only if Zeekerss fixes them, or you have ButteryFixes installed as well.
- Actually works with RemoveAds, but that defeats the purpose.
- As far as I know, everything is compatible, but let me know if anything comes up.

# Support

This isn't guaranteed to work, but it was running well for us for a while.
If you run into any issues, please let me know on the [github](https://github.com/cdusold/lethal-moreads) or [discord](https://discord.com/channels/1168655651455639582/1379569936703160340).

# Recommendations

Pairs very well with [TomatoBird's AdRevenue](https://thunderstore.io/c/lethal-company/p/Tomatobird/AdRevenue/), though you likely want to decrease the per ad payout since there's so many of them. Capping the number of ads per day and dropping the ad on landing isn't a bad idea either.