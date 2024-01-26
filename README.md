# Andy's Mods
Personal list of custom-made mods for Lethal Company, currently we serve:
\
*Command in chat - what you need to type in the ingame chat in order to disable/enable the particular mod.*
### [ServerSide] Lethal Turrets
\
Turrets can target enemies too. Hell let loose!
\
**Command in chat**: "/turrets".
### [ClientSide] Enable/Disable Mod
\
Lets the player enable/disable mods from this collection.
\
**Command in chat**: N/A.
### [ClientSide/ServerSide] Quick Switch With 1/2/3/4
\
Quickly switch between item slots with 1/2/3/4 buttons! No more scrolling with the mouse.
\
**Command in chat**: "/quick".
### [ServerSide] Lethal Landmines
\
Landmines can be triggered (read - exploded) by enemies. Nobody is safe!
\
**Command in chat**: "/mines"
### [ClientSide] Rare Bonk
\
10% chance to bonk anyone with shovel. With power comes great responsibility!
\
**Command in chat**: "/bonk"
### [ClientSide] One Of Us, Kinda
\
Masks now provide protection against enemies (no detection, no damage), but beware, don't become one of them!
\
**Command in chat**: "/mask"
### [ServerSide] Optimal Sells
\
New terminal command to optimize one's scrap sells to the company! Type *sell* in the terminal to quickly place minimum required scrap on the counter and hit the quota. Type *sell -all* to quickly place all your items on the desk counter of the the company for sell.
\
**Command in chat**: "/sell"
# Note
If you are using this mod with GameMasters, be aware chat commands won't work. This is due to how GameMasters work.
\
__By default, all mods are enabled__.
# Release v1.8.0:
- Updates:
    - Mod Manager now supports configuration file and display neatly a message when mod is either enabled/disabled. Plugin configuration file can be found under "BepInEx\config" folder. Look for the file "AndysModsPlugin.cfg" (make sure to launch the game at least once, otherwise it won't be there);
    - Completely new icon for the mod, made by my dear friend @bunnymoon!
- Fixes:
    - Optimize Sells will now sell scrap that lies in the ship and not in the player's hands or inventory. This addresses the issue when selling all stuff causes the player to have corrupted inventory state, and inability to drop the phantom scrap item.
    - Lethal Turrets has a new layer mask for physics casting. This should address the issue when turret targets the enemy through the wall.
    - Optimal Sells was incorrectly marked as ClientSide mod due to copy-paste error. It is ServerSide mod.
# Release v1.7.0:
- **[ServerSide] Optimal Sells**
\
New terminal command to optimize one's scrap sells to the company! Type *sell* in the terminal to quickly place minimum required scrap on the counter and hit the quota. Type *sell -all* to quickly place all your items on the desk counter of the the company for sell.
- Fixes:
    - Command chat for One Of Us, Kinda was expected "/masked" instead of "/mask". Oops.
# Release v1.6.0:
- **[ClientSide] One Of Us, Kinda**
\
Masks now provide protection against enemies (no detection, no damage), but beware, don't become one of them!
- Fixes:
    - [As per suggestion in Github](https://github.com/DrFeederino/AndysModsPlugin/issues/1), Lethal Mines now don't trigger on the ghosty girls. _Thank you for the feedback, Diffle!_
# Release v1.5.2:
- Fixes:
    - This mod is fully compatible with v47 (aka v49 in game).
    - Lethal Landmines has seen some reworks under the hood.
# Release v1.5.1 (Happy New Year update):
- Fixes:
    - Lethal Turrets now synchronize between server and clients properly and should work flawlessly.
    - Quick Switch has been reworked to synchronize item switching between clients.
# Release v1.5.0 (Happy New Year update):
- **[ServerSide] Lethal Turrets (previously known as Turrets Are No Joke)**
\
Turrets can target enemies too. Hell let loose!
- Fixes:
    - Rare Bonk has seen some tweaks under the hood. As a a result, it now plays the original shovel sound as well as bonk sound.
    - Quick Switch was causing player's desynchronization among host <-> client and client <-> client (e.g. Player A could have a shovel in hands, but nobody would see that because due to internal game network logic). This was resolved. 
    - Mods for turrets and landmines have been renamed to "Lethal Turrets" and "Lethal Landmines" respectively. 
# Release v1.4.0:
- [ClientSide] Enable/Disable Mod
\
Let the player enable/disable mods. To enable/disable mod, type in the chat:\
/quickswitch - toggles quick item switch mod,\
/bonk - toggles Rare Bonk mod,\
/landmine - toggles landmine mod.
- Fixes:
    - Rare Bonk is now [ServerSide] and all players can now hear it! Let the BONK begin!
# Release v1.3.0:
- [ClientSide] Quick Switch With 1/2/3/4
\
Added ability to quickly switch between item slots with 1/2/3/4 buttons! No more scrolling with the mouse.
# Release v1.2.1:
- Fixes:
    - Markdown and typos are fixed here and there.
    - Rare Bonk failed to hook up to shovels, resulting in the broken game. Apologies.
- Known issues:
    - ~~Rare bonk not playing for other players. Investigating possible solutions. For now, it is marked as [ClientSide].~~ Fixed in 1.4.0 and above.
# Release v1.2.0:
- [ClientSide] Rare Bonk
\
10% chance to bonk anyone with shovel. With power comes great responsibility!
- [ServerSide] Landmines Are No Joke
\
Landmines can be triggered (read - exploded) by enemies. Nobody is safe!
# Work in very hard progress
- [ServerSide] Masked Enemies are helpful?
\
Masked guys can bring loot too. Cute but dangerous!
- [ClientSide] Bring it on, Mask!
\
You can A/D to release yourself from the grabby hands of masked guys. Zap-gun now stuns them! Hope you can make it in one piece!
- [ServerSide] Chest to rule them all!
\
Adds a purchasable chest. It can store all the scrap you brought in the ship. It shows the full price of all sellable items.
- [ServerSide] Is Company fr?
\
Items can arrive broken from the shop and break during use for a short-term.