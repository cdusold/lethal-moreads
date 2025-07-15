I'm guessing you play Lethal Company to get ads, so this will increase the nmber of ads introduced in v70.

There are a number of settings. You can set the max number of ads per day, whether to play an ad as soon as possible on landing, the delay between ads, play ad or reroll timing on deaths, play ad or reroll timing on damage, the earliest time an ad can play, and the latest time an ad can play.

Any time a reroll happens, the ad will happen sooner (or the same time), never later.

Also, ads now play if you're the last one left as well, which means they'll appear in solo play as well.

*New*: Customize the ad slogans! Format is [slogan]:[weight], and comma separated, where weight is the unnormalized chance that slogan will be rolled. The odds do not have to add up to 100, nor does the order matter.

# Miscellanious fixes

- By default the game gets the ad sale value wrong on clients. This mod fixes that.
- If a suit ad tries to play, something null reference exceptions and then no ad can ever play again. A custom ad selection is forced every time to prevent this, with configurable blacklist.

# Support

This isn't guaranteed to work, but it was running well for us for a while.
If you run into any issues, please let me know on the [github](https://github.com/cdusold/lethal-moreads) or [discord](https://discord.com/channels/1168655651455639582/1379569936703160340).

# Recommendations

Pairs very well with [TomatoBird's AdRevenue](https://thunderstore.io/c/lethal-company/p/Tomatobird/AdRevenue/), though you likely want to decrease the per ad payout since there's so many of them. Capping the number of ads per day and dropping the ad on landing isn't a bad idea either.