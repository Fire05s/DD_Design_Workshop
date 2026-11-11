# Dungeon Dilemma Contributors

## How to Commit to this Repository
Save your changes and use either command line or Github Desktop to push to the branch you are working on. For merges to main, open a request and wait for it to be checked and approved, unless otherwise specified.

## Art
All art goes in the Art folder. To implement a sprite into the game. You must first update its type to be `Sprite (2D and GUI)`. Set the pixels per unit to match the sprite size. Set the filter mode to `Point` and set compression to none. If the sprite image contains multiple sprites, set the type to `Multiple` and open the sprite editor. Click slice at the top and separate the sprites. You can also slice them manually. At the point, you can save and push the new assets to your branch.

To add sprites in game, click on the game object, scroll down on the inspector panel, and add a `Sprite Renderer` component. There will be a sprite parameter where you can drag and drop the sprite asset from the project folder.

## Audio
All SFX and Music should go inside the `Assets > Audio > SFX | Music` folder.

To add SFX, the associated Game Object or prefab should have an audio component to add the SFX to. For adding music, find the Music Manager Prefab (`Assets > Prefabs > Audio Objects`) and add another entry into the Music Tracks dropdown / list. Give a name and attach the music clip for the new music track.

To actually play the correct sounds, that will have to done in code by going into the script where you'd like a certain sound to be placed and calling `MusicManager.Instance` or `SoundFXManager.Instance` to play the sound that you'll like.

For the purposes of this project, audio managers should already exist in the scene, and/or they are transferred from other parts of the game, e.g. MusicManager from the `MainLobby` to the `MainGame` scene.

Finally, as this is a multiplayer project, if you have added a non-local sound (primarily SFX), go to the `Assets` folder and add your audio clip to the `Network Assets` object in that folder. This is necessary so that all players can actually hear a sound that is played over the network.

## Design
### Equipments and Upgrades
To add new items (Equipment and Upgrades), the process is almost exactly the same for both. For adding or modifying existing Challenger or Dungeon Boss Equipment and/or Upgrade stats, go to `Assets > Scriptables > Equipment | Upgrades > Challenger | Dungeon Boss`. These folders hold the scriptable objects that describe the stats, abilities, and other miscellaneous info these items have. To create a new one, right click inside the folder and hit `Create > Item > Equipment | Upgrade`.

To add abilities to items, it's highly recommended to reference an existing item's ability, but in a nutshell, a scriptable object of the ability will need to be created along with the application of as many pre-defined interfaces as necessary (`IConditionalEffect`, `ILimitedAbility`, `IOnHitEffect`, `IRepeatedEffect`, `IStatusGiver`, and `ITimedAbility`). These effects can be mixed and matched and are fully dependent on that nature of the ability being created.

After an item ability has been created and given a unique name, go back to the Unity editor and find `Assets > Scriptables > Equipment | Upgrades > Abilities` and create the scriptable object for the ability by right clicking inside the folder and hitting `Create > Item > Equipment Ability | Upgrade Upgrade Ability > [YOUR ABILITY]`. Drag or select this scriptable object ability into the associated Equipment or Upgrade scriptable object.

Finally, to ensure this new Equipment or Upgrade is actually added to the Shop and the game, go to the `REFERENCE_XX` scriptable object in the `Challenger | Dungeon Boss` folder of the scriptable objects and add another entry including this new Equipment or Upgrade scriptable object. Additionally, because this is a multiplayer project, you must add new networked data to the `Network Assets` object in the `Assets` folder. Go to the `Network Assets` object, create a new entry in the Assets dropdown / list, and add the new Equipment or Upgrade scriptable object (not the ability scriptable object).

### Playable Characters
Playable Characters (Challengers and the Dungeon Boss Boss Phase) utilize a state machine that allows for the chaining of "sub-attacks" for their abilities. The damage multipliers and other modifiers for these sub-attacks can be found in their prefabs and child game objects (`Assets > Prefabs > Playable Characters`). The base stats for these characters can be found in `Assets > Scriptables > Challenger | Dungeon Boss Stats`.

If attempting to add a new character or new attack, more substantial programming knowledge of the game codebase and state machine may be necessary. If reusing an existing sub-attack, simply adding a component of the relevant attack (check `Assets > Scripts > PlayerLogic > StateMachine > States` for a full list) should do the job.

### Traps & Minions
Traps and Minions are similar in that they are both "Buildables," but understandably differ in how they function. Traps are solid game objects that are self-contained and interact with specific mechanics related to themselves. Minions mimic Playable Characters in their use of the state machine but have AI-directed attacks. Neither Traps nor Minions have scriptable objects related to their stats. Instead, their base stats and other relevant information will be found solely on their prefabs `Assets > Prefabs > Traps and Minions`.

If attempting to add a new Trap, more substantial programming knowledge of the game codebase may be necessary. You can look for existing implementations of traps in `Assets > Scripts > Traps`.

If attempting to add a new Minion or new attack to a Minion, more substantial programming knowledge of the game codebase and state machine may be necessary. If reusing an existing sub-attack, simply adding a component of the relevant attack (check `Assets > Scripts > PlayerLogic > StateMachine > States` for a full list) should do the job. Stats will be found on the `Synced Stats` component, and additional scaling may be found on the `Enemy` component.

After a new Trap or Minion has been created, that Trap or Minion will need to have a "Buildable" scriptable object made for it so that it can be acquired in the game. This can be done by going into `Assets > Scriptables > Buildables > Traps | Minions`, right clicking in the folder, selecting `Create > Item > New Buildable Item`, and filling out all relevant information there. This buildable item will show up in the shop for the Dungeon Boss to purchase.

Finally, to ensure this new Trap or Minion is actually added to the Shop and the game, go to the `REFERENCE_XX` scriptable object in the `Traps | Minions` folder of the `Scriptables > Buildables` and add another entry including this new Trap or Minion scriptable object. Additionally, because this is a multiplayer project, you must add new networked data to the `Network Assets` object in the `Assets` folder. Go to the `Network Assets` object, create a new entry in the Assets dropdown / list, and add the new Trap or Minion scriptable object.

### Projectiles, Explosions, and "Cursed Ground"
Projectiles and other instantiated attacks are relatively self-contained. Create a prefab and add the associated instantiated attack component (find them in `Assets > Scripts > Instantiated Attacks`). Projectiles need a projectile scriptable object for their stats, so this will need to be created in `Assets > Scriptables > Projectile Stats`, right clicking, and selecting `Create > Entities > Projectile Data` (if this doesn't show up, just copy an existing projectile scriptable object and replace its data accordingly). Add these instantiated attack prefabs (and projectile data if relevant) to the associated attack.

### Status Effects
Status Effects require a multi-step process to be created.

First to create their effect, it's highly recommended to reference an existing status effect's ability, but in a nutshell, a class will need to be created with the effect and apply `IDamageStatus` (for damaging and healing) and/or `IBuffStatus` (for stat buffs and debuffs). These effects can be mixed and matched and are fully dependent on that nature of the ability being created. Neither of these could also be applied for purely cosmetic "status" effects / VFX.

Next, we'll need to add this new status effect to the game's `StatusEffectType` enum (found in `Assets > Scripts > Combat Unified (Priestess)`) allowing for it to be understood and applied by the rest of the game. This is done by adding another entry to the enum with your status effect's name.

Then, to actually apply the status effect, it will need to be parsed by `StatusParser` (found in `Assets > Scripts > Combat Unified (Priestess)`). This is done by adding another case to the switch statement and mapping the newly added `StatusEffectType` value to return your new status effect's object.

(If you are also implementing how this status effect is applied, you will have to find either the relevant equipment/upgrade ability, attack, and/or mechanic to return the `StatusEffectType.[STATUS]`, add it to the list of status effects applied in `Assets/Scripts/PlayerLogic/HealthHandler`'s `TakeDamagerServerRpc` function, and/or use `HealthHandler`'s other status effect related functions to apply the status effect.)

To give this status effect its actual visuals (if necessary), create a prefab of the status effect's visuals in `Assets > Prefabs > Statuses`, modify Particle System component(s) as necessary, and attach a `Particle Visuals` script to the root object of the prefab, giving a duration to the status effect aligning with the status effect's duration.

Finally, to map this status effect to its visual, navigate to `Assets > Scriptables`, find the `Status Effect Prefab Map`, add another entry, and choose your new `StatusEffectType` and its associated visual prefab.

## Programming
Please take care to comment or add documentation to your code if necessary. Try to write code that is self explanatory so that comments are unneeded. Do not nest if statements or loops. Please give variables sensible names. Naming conventions follow standard C# naming conventions (https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names). All merges to main should be reviewed. Open a pull request instead of directly merging.

To test any changes you make, use the Multiplayer Play Mode package to run 2 or more instances of the editor, set the main editor's tag to "Host," and open Steam in the background. Testing can be done either through the main menu (`MainLobby` scene) or directly through the game (`MainGame` scene). Be warned that this package is memory intensive.

**See the [Reference File](REFERENCE.md) for more programming-related resources.**

## UI/UX
UI in Dungeon Dilemma (mostly) uses our own UI generation system. All UI Screens are found in `Assets > Resources > Avocado > Screens`. Anything in screens will be pre-rendered as an "Avocado Screen." All sub-folders within this Screens folder will be initialized by Avocado.

When making an Avocado Screen, a couple of features are necessary.
1. The Avocado Screen must be a prefab located within the `Screens` folder.
2. A root object with a `Avocado.UiScreen` derived script.
3. An EventSystem object underneath this root object.
4. A Canvas object underneath this root object from which all the main UI elements are located under.

UI assets should follow the same instructions as other sprites. To ensure your sprite resizes properly, drag the green lines inside the sprite editor to represent the borders and center properly. This prevents the edges and corners from being stretched improperly when resizing your UI asset. To see your sprite in game, add a `canvas` object or find an existing one. Make a new `image` or `panel` object as a child of the canvas and drag your sprite into the corresponding parameter. Resize the image or panel using the rectangle tool.

To add a new text object, edit either the TMP object included with the UI element or add a standalone object. It will have a textbox and parameters. Be sure to select the correct font (`Jersey10 SDF` or one of its other variants made for this project).

The following UI elements will be used most frequently: `Image`, `Button`, `Slider`.

For implementing UI functionality, you will use the `Avocado.UiScreen` derived script attached to the root object of the screen prefab. to understand how this works, take a look at `Assets > Scripts > Avocado > Core` for the "core" functions of the system or another Screen's implementation. At a high level, Avocado works by synthesizing all text, images, buttons, etc. on created UI screens for quick and easy reference in an `Avocado.UiScreen` script.

## Writing
To add shop dialogue into the game, find the relevant sub-folder inside the `Scriptables` folder. Right click inside the folder and hit `Create > Dialogue > Shop`. Shop Dialog Data is broken down into Challenger (C) and Dungeon (DB) Shop Open and Buy lines and Upgrade Shop Open and Buy lines. Tick as many boxes for however many cases your specific dialog line should apply to. After you have created a new dialog line, add that scriptable object to the `REFERENCE_SD` scriptable object in the same folder. This will allow your dialog line to also show up in the game alongside everything else!

## Note
Last Updated July 3, 2026.
