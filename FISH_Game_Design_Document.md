# FISH! --- Game Design Document & Unity Development Handoff

**Document purpose:** This document is the current source of truth for
the FISH! project. It is written to onboard a new ChatGPT conversation
and support implementation through Codex in Unity.

**Project status:** Design scope and core gameplay direction are
defined. Actual implementation status must be verified against the
current Unity project before anyone describes a feature as completed.

------------------------------------------------------------------------

## 1. Project Overview

  -----------------------------------------------------------------------
  Item                                Details
  ----------------------------------- -----------------------------------
  Game title                          FISH!

  Genre                               3D Fishing, Fish-Collecting RPG,
                                      Turn-Based Strategy

  Platform                            PC

  Engine                              Unity

  Game mode                           Single-player

  Development target                  One-month development period

  Primary focus                       Fishing, fish collection, and a
                                      short story-driven battle
                                      progression
  -----------------------------------------------------------------------

### Game concept

FISH! is a small 3D fishing and fish-collecting game set in a fictional
coastal barangay in Negros, Philippines. Players explore a compact
shoreline, discover fish found in Philippine waters through a timing-based
fishing minigame, and form a collection of aquatic companions. They learn
local fish names and habitats while investigating a disturbance threatening
the community's shoreline.

Collected fish can be selected for a team and used in separate,
story-triggered turn-based battles. These battles are events in which
the player protects the island from hostile fish and, ultimately, an
environmental boss.

A Fish Encyclopedia provides short information about collected species,
their habitats, and aquatic biodiversity. The game aims to combine
casual fishing, collection, light strategy, and basic marine
conservation awareness.

### Project goal

Develop a small but enjoyable 3D fishing and fish-collecting game that
introduces players to aquatic biodiversity through fishing, collection,
and simple turn-based battles. The game also aims to promote awareness
of marine conservation through educational information about collected
fish.

------------------------------------------------------------------------

## 2. Design Principles and Scope

The project must be achievable within approximately one month.
Prioritize a complete, playable, and polished core experience over a
large feature set.

### Core design principles

1.  **Fishing and battles are separate activities.** Fishing is the
    primary collection activity. Battles are story events and are not
    triggered by fishing itself.
2.  **Battles are progression-based events.** Players unlock encounters
    by completing objectives. Do not make players wait for real-world or
    in-game time.
3.  **Collected fish support battle preparation.** Players catch fish
    through the fishing system and select from their collection when
    preparing a battle team.
4.  **Use one compact environment.** Do not build a large open world or
    multiple islands.
5.  **Reuse systems.** All story encounters should use the same battle
    system and scene where practical. Change enemy data, dialogue, and
    difficulty rather than building separate battle mechanics.
6.  **Keep educational content lightweight.** Use concise fish
    information rather than complex ecosystem simulations.

### Explicitly out of scope for the initial version

-   Multiple islands or a large open world.
-   Multiple fishing minigames.
-   Complex fishing physics or a line-tension simulation.
-   A large fish catalog.
-   Fish breeding, evolution, or complex progression trees.
-   Player leveling, equipment, and extensive RPG progression.
-   Complex quests, dialogue trees, or NPC schedules.
-   Multiple major bosses.
-   Multiple boss phases or complicated boss mechanics.
-   Advanced enemy AI.
-   Multiplayer, online trading, or online rankings.
-   Complex ecosystem, population, or environmental simulations.
-   Real-time or in-game countdowns that force players to wait.

If time becomes limited, prioritize a functional fishing and collection
loop, then the battle progression, then polish and optional presentation
details.

------------------------------------------------------------------------

## 3. Game Setting

FISH! takes place in one small coastal environment within a fictional
barangay in Negros, Philippines. This location serves as the main area
for fishing, basic exploration, story events, and battle encounters.
The barangay name is to be decided; the game does not recreate a real
community or claim to document its specific customs.

### Environment requirements

-   One compact beach or shoreline.
-   One designated fishing spot.
-   A small surrounding environment with water and basic scenery.
-   A clearly marked or recognizable location for story encounters.
-   A final encounter location for the environmental boss.

The environment should be visually coherent and easy to navigate. It
does not need multiple biomes or separate maps.

### Philippine setting and localization direction

Localize through the setting, species information, and everyday coastal
details. Keep the existing fishing, collection, battle, and progression
systems. Localization should replace planned content rather than add new
systems or expand the map.

#### Minimum localization target

| Element | Implementation | Scope limit |
| --- | --- | --- |
| Setting | One fictional coastal barangay in Negros | Use the existing compact coastal map |
| Signature prop | One decorative bangka near the shoreline | Static scenery; no sailing or boat physics |
| Shelter and dock | Simple coastal shelter and wooden fishing dock | Adapt the already planned hut and dock |
| Fish | Approximately five verified species found in Philippine waters, with local and English names | Keep the existing fish count and collection system |
| Language | Mostly English with a few natural Filipino or Hiligaynon expressions | No language selector, full translation, or voice acting required |
| Conservation story | Explain why the community cares about its waters and how litter and habitat damage threaten them | Use existing introductions, notifications, and ending |

Optional finishing details include a barangay-name sign and a fishing rod
or basket beside the shelter. Reuse existing props where possible.
A populated village, additional NPC models, shops, fishing economy,
cultural minigames, and mythology-based encounters are not required.
Do not add national symbols or unrelated cultural decorations simply to
signal that the setting is Philippine.

#### Collection and fantasy framing

Present collecting as recording species and befriending fish companions.
Keep the timing-based catch mechanic and persistent team collection;
this framing does not require a separate release, trust, or friendship
system. Battles are stylized fantasy, not instructions for handling or
fighting real animals. Battle stats and abilities are fictional gameplay
data and must be distinguishable from biological facts.

#### Local species and language accuracy

Select exact species before finalizing local names, habitat descriptions,
or models. Verify occurrence in Philippine waters and suitability for
the represented coastal habitat. Local names can refer to several species
and vary between places; record the language or regional usage rather
than assuming a one-to-one mapping. The final five species remain to be
researched and selected; no species list is approved by this update.

Use concise, sourced facts. Do not invent conservation status, local
customs, or biological details. Have a fluent speaker review Filipino or
Hiligaynon text before finalizing it. Clear English is the fallback when
a local expression cannot be verified.

#### Production order

1. Build the functional beach, fishing spot, and core loop with placeholders.
2. Adapt the existing hut and dock to the chosen setting and add one bangka.
3. Verify the five species and populate their shared data and encyclopedia.
4. Add short community-focused story text and a few reviewed local expressions.
5. Add optional signs and props only after the complete game is playable.

### Recommended environment art direction

Use a **stylized, low-poly tropical coastal style** with a warm,
welcoming atmosphere. Aim for the visual feel of a polished indie
adventure game rather than realism. The environment should feel colorful
and inviting while keeping the fish, fishing spot, and important
interactable objects easy to recognize.

Useful visual references include:

- **Animal Crossing:** welcoming atmosphere, simplified shapes, and
  approachable scenery.
- **Pokémon:** readable creature designs and clear visual silhouettes.
- **Alba: A Wildlife Adventure:** nature-focused environments and a
  conservation theme.

Use these games as general inspiration only. Do not copy their
characters, assets, or exact visual designs.

#### Environment layout

Build one small, connected coastal area with three visually distinct
sections. These are areas within the same environment, not separate
levels or maps.

1. **Main beach and player area**
   - Open sandy ground with enough space for the player to move.
   - A few palm trees, tropical plants, grass, and rocks.
   - A small hut, sign, or other recognizable landmark.
   - A few decorative props such as driftwood, shells, and crates.

2. **Fishing area**
   - A small wooden dock or pier beside the water.
   - A clearly identifiable fishing interaction point.
   - Rocks, reeds, or aquatic plants near the shoreline.
   - Optional fish silhouettes beneath the water, if they are easy to
     implement and do not distract from the fishing minigame.

3. **Story encounter area**
   - A recognizable rocky or quieter section of the shoreline.
   - Enough open space for the player to approach an encounter marker.
   - A distinct final encounter location for the environmental boss.
   - Use slightly darker or more muted environmental details during the
     boss event to communicate that the area is polluted or damaged.

Keep the paths and important locations easy to understand. Players
should be able to identify the fishing spot and encounter locations
without searching through a large environment.

#### Color palette

Use a consistent, soft tropical palette. These are suggested starting
colors and can be adjusted to match the existing character model.

| Element | Suggested color |
| --- | --- |
| Ocean | `#77CDBB` |
| Shallow water | `#A8E2D5` |
| Sand | `#F3D69A` |
| Vegetation | `#83B879` |
| Deep vegetation | `#477B56` |
| Wood | `#D9A66F` |
| Cream and light details | `#F4E8D0` |
| Coral accents | `#E99A79` |
| Dark accents | `#536A70` |

For the environmental boss event, introduce a limited amount of darker
greens, muted blues, and polluted-looking details. Keep these effects
localized so the main environment remains visually appealing.

#### Blender modeling direction

Create simple, recognizable models that share the same proportions and
level of detail. Prioritize clear silhouettes over small surface
details.

Suggested environment assets:

-   One small wooden fishing dock.
-   A few palm tree variations.
-   Three to five rock variations.
-   Simple grass and tropical plant models.
-   A small hut or environmental landmark.
-   Driftwood, shells, crates, and fishing-related props.
-   Simple coral or aquatic plant models for shoreline decoration.
-   A recognizable prop or landmark for the final encounter area.

Reuse assets by changing their rotation, scale, and placement. Create
small material variations instead of modeling many nearly identical
objects. Keep geometry and materials simple so the environment remains
manageable within the one-month development period.

#### Matching the existing player character

The player character should guide the environment's final visual style.
Keep the character's existing design unless there is a clear visual
problem that needs to be fixed.

A smooth or rounded character model can work well in a low-poly
environment. The character does not need to be remodeled with faceted
geometry. Instead, create consistency through:

-   Similar color saturation and warmth.
-   Simple, clean material colors.
-   Rounded or simplified forms in nearby props.
-   Consistent lighting and shadow softness.
-   A similar overall level of visual detail.

Avoid making the environment extremely realistic if the character is
stylized. Also avoid adding excessive detail to the character simply to
match the environment.

If the existing Blender character uses separate materials for different
geometry or body parts, keep that workflow where practical. Use a small,
consistent material palette for the environment rather than introducing
complex texturing workflows that are unnecessary for the project.

#### Unity rendering direction

-   Keep the render pipeline already used by the Unity project. Do not
    switch pipelines just for visual effects.
-   Use soft directional sunlight and gentle ambient lighting.
-   A slightly warm lighting setup can support the tropical setting.
-   Use moderate, soft shadows where performance allows.
-   Use a simple ocean material with a clear distinction between
    shallow and deeper water.
-   Keep the third-person camera and environment scale readable.
-   Use minimal post-processing. Avoid effects that make the scene
    difficult to see or reduce performance.
-   Outline effects are optional and should only be added if they improve
    object readability.

#### Environment art MVP

For the first complete build, prioritize:

1. One compact beach and shoreline.
2. One fishing dock and functional fishing spot.
3. One small hut or recognizable landmark.
4. Three to four palm tree variations.
5. Three to five rock variations.
6. A small set of grass and tropical plants.
7. One ocean material.
8. One story encounter location.
9. One final boss location within the same environment.

Do not spend time building a large island, multiple biomes, detailed
underwater areas, or large numbers of decorative assets. Complete the
playable environment first, then add decoration if time remains.

------------------------------------------------------------------------

## 4. Main Gameplay Loop

The overall gameplay combines two separate activities:

**Explore → Fish and Collect → Progress Story → Prepare Team → Battle
Encounter → Continue Story**

### Fishing and collection activity

1.  Explore the coastal environment.
2.  Approach and interact with the fishing spot.
3.  Complete the fishing minigame.
4.  Catch a fish.
5.  Add the fish to the collection.
6.  View its information in the Fish Encyclopedia.
7.  Repeat fishing when the player wants to collect more fish or prepare
    a stronger team.

### Story and battle activity

1.  Complete the current progression objective.
2.  Unlock a story event.
3.  View a short event introduction.
4.  Approach or interact with the encounter location.
5.  Prepare or confirm the battle team.
6.  Enter the turn-based battle.
7.  Win the battle to unlock the next story event.

Fishing does not automatically start a battle. A successful fishing
attempt should result in a fish catch, not a battle encounter.

------------------------------------------------------------------------

## 5. Core Gameplay Systems

### 5.1 Player movement and interaction

Players use basic third-person movement to explore the small coastal
environment.

Required functionality:

-   Basic movement and camera control.
-   Collision with the environment.
-   Interaction with the fishing spot.
-   Interaction with story encounter markers or locations.
-   Clear prompts showing when an interaction is available.

Keep movement and interaction simple. Do not add unnecessary traversal
mechanics.

### 5.2 Fishing system

Fishing is the primary gameplay feature and the main way players obtain
fish.

#### Fishing flow

1.  The player approaches the designated fishing spot.
2.  The player interacts with it.
3.  The fishing minigame interface appears.
4.  A moving indicator travels across a bar.
5.  The player presses the required button when the indicator reaches
    the target zone.
6.  Successful timing catches a fish.
7.  The result is added to the player's collection and shown on screen.
8.  An unsuccessful attempt ends without adding a fish, allowing the
    player to try again.

#### Timing-based minigame

Use a simple timing bar rather than a complex fishing simulation.

Potential difficulty adjustments:

-   Indicator movement speed.
-   Target-zone size.
-   Different target-zone settings for fish rarity, if this is feasible.

For the first implementation, one functional timing system is more
important than multiple difficulty variations.

### 5.3 Fish collection

Caught fish are stored in the player's collection.

#### Initial content target

Start with approximately five fish species. The team can adjust the
number if modeling, animation, or implementation time becomes limited.

Each fish should have data for:

-   Unique ID.
-   Display name, using a verified local name where available.
-   English common name.
-   Scientific name for the exact species.
-   Local-name language or regional usage, where relevant.
-   Reference notes or URLs for species facts and local names (development metadata).
-   Icon or image.
-   3D model reference, if used in the battle scene.
-   Rarity.
-   Health.
-   Attack.
-   Defense.
-   Speed.
-   Habitat.
-   Short description.
-   One educational fact.
-   Special ability name and basic effect, if used in combat.

Use a consistent data structure for all fish. In Unity, a data-driven
approach such as ScriptableObjects is recommended if it fits the
existing project architecture. Do not introduce a new architecture
without first inspecting the project.

#### Rarity

Suggested initial categories:

-   Common.
-   Uncommon.
-   Rare.

Rarity can affect how frequently a fish appears or how difficult it is
to catch. Keep the distribution and balancing simple.

### 5.4 Fish Encyclopedia

The Fish Encyclopedia displays information about fish the player has
collected.

Each entry should include:

-   Local name, English name, and fish image.
-   Scientific name and regional naming note where useful.
-   Habitat.
-   Short description.
-   One educational fact.

New entries become available when the corresponding fish is caught.
Uncaught fish may appear as hidden or undiscovered entries, depending on
implementation time.

The encyclopedia should use the same fish data as the collection and
battle systems wherever practical. Avoid maintaining duplicate
information in separate systems.

------------------------------------------------------------------------

## 6. Turn-Based Battle System

Battles are separate story events. They do not happen during fishing and
are not random encounters.

The player uses collected fish to protect the island from hostile
aquatic creatures and the environmental boss.

### 6.1 Team formation

-   The player can select up to three fish for a battle team.
-   Surviving team members are displayed in opposing formations in a
    compact 3D coastal water arena, facing the other team.
-   One fish acts at a time. Alternate individual actions between sides,
    starting with the player. Each side cycles through its own team order
    and skips defeated fish.
-   Smaller teams cycle more frequently. Defeated fish leave their visual
    slots empty and cannot act or be targeted.
-   If the player has no surviving fish, the battle ends in defeat.
-   The player should be able to review or confirm the team before an
    encounter.

For the initial version, avoid complex turn-order calculations and
simultaneous actions.

### 6.2 Battle sequence

1.  Battle introduction displays the opponent.
2.  The player's selected team and the opponent are presented.
3.  The player's turn begins.
4.  The player selects an action for the acting fish and chooses a living
    enemy target for Attack or an offensive Special.
5.  The action is executed and the result is displayed.
6.  The next surviving opponent takes its turn, targeting the next player
    fish due to act when using an offensive action.
7.  The game checks whether either side has been defeated.
8.  If both sides have surviving fish, the next player fish's turn begins.
9.  When one side has no remaining fish, the result screen appears.

### 6.3 Player actions

The player has three basic actions:

**Attack** - Deals damage to the chosen surviving opposing fish. - Damage can be
calculated from the active fish's Attack and the opponent's Defense. -
Keep the initial damage formula simple and consistent.

**Defend** - Reduces incoming damage until the defender's next individual
turn. The effect cannot stack and expires when that turn begins.

**Special Ability** - Each fish can use its species' ability once per
battle attempt. Offensive specials use a chosen enemy target. Self-healing
affects the acting fish and is capped at maximum HP.

Canceling target selection returns to action selection without consuming
a turn or spending the special. Resolve damage and defeat immediately,
then present the action feedback before enabling the next input.

### 6.4 Opponent behavior

Use basic automated opponent behavior.

For the initial version, opponents can choose between attacking and
defending. A special ability can be added only if it is straightforward
to implement.

Do not build advanced enemy AI, complex behavior trees, or elaborate
status-effect systems.

### 6.5 Battle conditions

**Victory:** All opposing fish have been defeated.

**Defeat:** All fish in the player's team have been defeated.

After defeat, allow the player to retry the encounter. Do not
permanently remove collected fish.

After victory, update story progression and unlock the next event.

### 6.6 Battle presentation

The battle interface should show:

-   Both formations in shallow clear water, viewed with an elevated
    diagonal camera.
-   Acting fish and selected target, with distinct labels and highlights.
-   Each member's name and current/max HP, including defeated HUD entries.
-   Attack button.
-   Defend button.
-   Special Ability button.
-   Short action or damage feedback.
-   Victory or defeat result.

Use simple transitions and animations. Reuse the same battle interface
for all encounters.

------------------------------------------------------------------------

## 7. Story Progression and Battle Events

FISH! uses a progression-based event system. Battles unlock after
players complete specific objectives, rather than through random
encounters or real-time countdowns.

The player does not need to wait for an event timer. Completing an
objective makes the next event available immediately.

### Opening: The disturbance

**Trigger:** New game.

The player arrives at the fictional barangay's shoreline. A short text
introduction explains the community's connection to the sea and why the
player wants to protect it; an animated NPC is not required. Something
unusual is happening in the surrounding waters. Fish have been acting
strangely, and litter and damaged shoreline details hint at the disturbance.

The player learns:

-   Basic movement.
-   How to interact with the fishing spot.
-   How to use the fishing minigame.
-   How to collect fish.

**Objective:** Catch at least three fish.

This gives the player an initial collection and enough options to begin
forming a battle team.

### Event 1: First Disturbance

**Unlock condition:** The player has caught at least three fish.

A hostile wild fish appears near the coastal area. It behaves
aggressively because of the environmental disturbance.

Event sequence:

1.  The player receives a short story notification or sees the hostile
    fish appear.
2.  The player approaches or interacts with the encounter location.
3.  An encounter animation or brief introduction plays.
4.  The player confirms their battle team.
5.  The first battle begins.

**Objective:** Defeat the first hostile fish.

**Purpose:** Introduce the battle system. This encounter should be easy
and teach players how to attack, defend, and use a special ability.

### Event 2: Growing Threat

**Unlock condition:** The player wins Event 1.

After the first battle, another disturbance occurs. A stronger wild fish
appears, suggesting that the problem is getting worse.

Event sequence:

1.  A short story event introduces the stronger opponent.
2.  The player approaches the marked encounter location.
3.  The encounter introduction plays.
4.  The player confirms their team.
5.  The second battle begins.

**Objective:** Defeat the stronger wild fish.

**Purpose:** Provide a greater challenge and encourage players to use
their collected fish strategically.

### Final Event: The Environmental Boss

**Unlock condition:** The player wins Event 2.

The source of the disturbance appears: the environmental boss.

The boss represents accumulated damage caused by human activities,
particularly marine pollution, overfishing, and habitat destruction.

Event sequence:

1.  A short boss introduction plays.
2.  The player receives an opportunity to prepare or change their team.
3.  The player interacts with the final encounter location.
4.  The boss battle begins.
5.  The player defeats the boss.
6.  A short ending scene shows the coastal environment beginning to
    recover.

**Objective:** Defeat the environmental boss and protect the island.

### Progression rules

-   Events unlock immediately after their required objectives are
    completed.
-   Players can retry battles after losing.
-   Fishing remains available between encounters.
-   Players can collect additional fish before starting the final boss.
-   The final boss should not start automatically as soon as it unlocks.
    Let the player choose when to initiate it.
-   Defeating the boss triggers the ending sequence.

### Progression table

  -----------------------------------------------------------------------
  Stage                   Unlock condition        Player objective
  ----------------------- ----------------------- -----------------------
  Introduction            New game                Learn basic controls

  Fishing and collection  Introduction completed  Catch at least 3 fish

  First encounter         3 fish caught           Win the first battle

  Second encounter        First battle won        Win the second battle

  Final boss              Second battle won       Defeat the boss

  Ending                  Boss defeated           View the ending
  -----------------------------------------------------------------------

### Recommended implementation

Use a simple progression manager or equivalent system to track the
current game stage. A small enum is sufficient for the initial version:

``` csharp
public enum GameProgress
{
    Introduction,
    Fishing,
    FirstEncounter,
    SecondEncounter,
    FinalBoss,
    Completed
}
```

Track relevant values separately, such as:

``` csharp
int totalFishCaught = 0;
bool firstBattleWon = false;
bool secondBattleWon = false;
bool bossDefeated = false;
```

These are conceptual examples, not instructions to create duplicate
state if the project already has a suitable save or progression system.

When a fish is caught, update the collection and fish-caught count. When
a battle is won, update the corresponding progression state. The game
then unlocks the next event.

Use the project's existing architecture where possible. Avoid creating
multiple competing sources of truth for progression.

------------------------------------------------------------------------

## 8. Environmental Boss and Conservation Message

The game features one main environmental boss. It represents the
accumulated effects of human-caused environmental damage.

Its appearance can combine visual elements such as:

-   Plastic waste.
-   Discarded fishing nets.
-   Marine debris.
-   Damaged coral or other corrupted aquatic elements.

Its attacks can symbolically represent pollution, entanglement, and
habitat damage.

For the initial version, the boss only needs:

-   Higher health than normal opponents.
-   Stronger attacks.
-   One unique special ability.
-   A short introduction.
-   A victory sequence.

Do not implement multiple phases, multiple bosses, or complex
environmental simulations.

After defeating the boss, show a simple visual change suggesting that
the coastal environment is recovering. This can be a short scene or a
before-and-after environment state. No ecosystem simulation is required. Reuse the same barangay shoreline:
hide selected litter props, restore warmer colors, and show a short
community thank-you message. The ending should suggest the beginning of
recovery; defeating a fantasy boss does not instantly solve real marine
pollution or habitat damage.

------------------------------------------------------------------------

## 9. Educational Integration and SDG 14

FISH! supports **Sustainable Development Goal 14: Life Below Water**,
which focuses on conserving and sustainably using oceans, seas, and
marine resources.

The educational purpose is mainly delivered through the Fish
Encyclopedia and the environmental boss storyline. Local relevance comes
from verified species found in Philippine waters, their local names,
and a fictional coastal community's concern for its shoreline. Do not
claim a specific real barangay has the problems depicted in the game.

Players can learn about:

-   Aquatic and marine species.
-   Natural fish habitats.
-   Aquatic biodiversity.
-   The importance of protecting marine ecosystems.
-   Marine pollution.
-   Responsible fishing and conservation.

Educational information should be short, clear, and appropriate for a
casual game. Use reliable references when writing species facts and
conservation information. Do not invent conservation statuses or
biological details.

The game connects particularly with SDG 14's goals related to reducing
marine pollution, protecting marine ecosystems, and promoting
sustainable fishing.

------------------------------------------------------------------------

## 10. User Interface Requirements

The initial version should include these interfaces:

### Main Menu

-   Start Game.
-   Settings, if time permits.
-   Exit.

### Gameplay HUD

-   Basic interaction prompt.
-   Fishing spot prompt.
-   Event notification when a story event unlocks.

### Fishing Minigame

-   Timing bar.
-   Moving indicator.
-   Target zone.
-   Success or failure feedback.

### Fish Collection

-   List or grid of collected fish.
-   Fish images and names.
-   Rarity indicators.
-   Fish information access.

### Fish Information / Encyclopedia

-   Fish description.
-   Habitat.
-   Educational fact.
-   Basic attributes.

### Team Selection

-   Display available collected fish.
-   Allow selection of up to three fish.
-   Show basic stats.
-   Confirm battle team.

### Battle Interface

-   Player and opponent fish.
-   Health indicators.
-   Attack, Defend, and Special Ability buttons.
-   Action feedback.
-   Victory and defeat display.

### Event Presentation

-   Short event introduction.
-   Current objective.
-   Encounter prompt.
-   Boss introduction and ending sequence.

Keep the UI simple and consistent. Avoid building separate complex
screens for each encounter.

------------------------------------------------------------------------

## 11. Minimum Viable Product (MVP)

The MVP should provide a complete playable experience from starting the
game to finishing the final boss encounter.

### Required features

1.  One small 3D coastal environment.
2.  Basic third-person player movement.
3.  One functional fishing spot.
4.  One timing-based fishing minigame.
5.  Approximately five collectible fish species.
6.  Basic fish rarity and attributes.
7.  A functional fish collection menu.
8.  A simple Fish Encyclopedia.
9.  A team selection system supporting up to three fish.
10. One reusable turn-based battle system.
11. Attack, Defend, and Special Ability actions.
12. Basic opponent behavior.
13. Victory and defeat conditions.
14. Two normal story battle encounters.
15. One final environmental boss encounter.
16. Progression-based event unlocking.
17. Basic UI and game flow.
18. A short ending after the boss is defeated.

### Priority order

If development time becomes limited, implement in this order:

**Priority 1 --- Core playable loop** - Player movement. - Fishing spot
interaction. - Fishing minigame. - Catching and storing fish. - Fish
collection display.

**Priority 2 --- Battle foundation** - Fish data and stats. - Team
selection. - Battle scene and turn sequence. - Attack and Defend. -
Victory and defeat conditions.

**Priority 3 --- Complete game progression** - Progression manager. -
First and second encounter. - Final boss encounter. - Retry and event
unlocking. - Ending sequence.

**Priority 4 --- Educational content and polish** - Fish Encyclopedia. -
Special abilities. - Event dialogue and transitions. - Visual and audio
polish. - Bug fixing and balancing.

Do not delay the core playable loop to polish optional features.

------------------------------------------------------------------------

## 12. One-Month Development Plan

### Week 1: Environment and Fishing Foundation

-   Inspect and organize the existing Unity project.
-   Create or complete the small coastal environment.
-   Implement player movement and camera.
-   Set up the fishing spot and interaction.
-   Develop the basic timing-based fishing minigame.

**Target result:** The player can move to the fishing spot and complete
a fishing attempt.

### Week 2: Fishing and Collection

-   Complete fishing success and failure logic.
-   Add approximately five fish species.
-   Implement fish-catching and collection data.
-   Build the fish collection interface.
-   Add basic fish rarity and attributes.

**Target result:** The player can catch fish and view them in the
collection.

### Week 3: Battle System

-   Implement fish team selection.
-   Develop the reusable turn-based battle sequence.
-   Add Attack and Defend actions.
-   Add simple Special Abilities if the core battle works.
-   Implement opponent actions, health, damage, and battle results.

**Target result:** The player can select collected fish and complete a
functional battle.

### Week 4: Progression and Finalization

-   Implement progression-based event unlocking.
-   Add the first and second encounters.
-   Add the final boss encounter.
-   Add the Fish Encyclopedia.
-   Connect the game flow and UI.
-   Add the ending sequence.
-   Test, fix bugs, balance, and prepare the final build.

**Target result:** A complete playable game from introduction through
the final boss.

This schedule is a target, not a reason to keep adding features. If a
feature threatens the final build, simplify or remove it.

------------------------------------------------------------------------

## 13. Unity and Codex Implementation Handoff

This section is intended for a new ChatGPT conversation that will help
implement the project through Codex.

### Instructions for the new ChatGPT conversation

You are assisting with the development of FISH!, a Unity 3D
single-player fishing and fish-collecting game. Treat this document as
the current design source of truth.

Your role is to help inspect, plan, implement, debug, and review the
existing Unity project with Codex.

Follow these rules:

1.  **Inspect before changing.** First examine the existing project
    structure, Unity version if available, scenes, scripts, packages,
    prefabs, and current implementation.
2.  **Do not assume the project is empty.** Identify existing systems
    and reuse or improve them instead of replacing working code.
3.  **Verify implementation status.** Separate implemented, partially
    implemented, broken, and not-yet-implemented features.
4.  **Preserve existing working functionality.** Avoid broad rewrites
    unless there is a clear technical reason.
5.  **Work incrementally.** Implement one small system or milestone at a
    time. Explain the intended changes before making large structural
    changes.
6.  **Keep the one-month scope.** Do not introduce unapproved features
    such as multiple islands, complex quests, multiple bosses, breeding,
    multiplayer, or advanced simulations.
7.  **Keep fishing and battles separate.** Fishing catches fish. Story
    progression triggers battle events. A successful fishing attempt
    must not automatically start a battle.
8.  **Use progression-based events.** Do not add real-time or in-game
    countdowns. Unlock events through objectives and battle results.
9.  **Reuse the battle system.** Normal encounters and the final boss
    should use the same battle framework with different enemy data and
    difficulty.
10. **Avoid unnecessary dependencies.** Do not add packages or
    frameworks without explaining why they are needed.
11. **Use maintainable, beginner-readable code.** Prefer clear naming,
    small responsibilities, and simple systems over unnecessary
    abstraction.
12. **Test each milestone.** Provide clear Unity Editor testing steps
    and identify any required scene, prefab, Inspector, or asset setup.
13. **Do not claim something works without verification.** State what
    was changed and what still needs to be tested in Unity.
14. **Ask before making major design changes.** If a technical
    limitation requires changing the game design, explain the trade-offs
    and request a decision.
15. **Keep localization lightweight.** Follow Section 3: one fictional
    Negros coastal barangay, a decorative bangka, verified local fish
    information, and concise community-focused text. Do not introduce
    boat controls, a larger village, new NPC systems, or unverified facts.

### First task for Codex

Before implementing anything:

1.  Inspect the repository and Unity project structure.
2.  Identify the Unity version and relevant packages from project files.
3.  List scenes, scripts, prefabs, ScriptableObjects, and existing
    gameplay systems.
4.  Identify what is already implemented for movement, fishing, fish
    collection, UI, combat, and progression.
5.  Identify broken, duplicated, or disconnected systems.
6.  Compare the current implementation with this GDD.
7.  Produce a concise implementation audit and a prioritized task list
    for the remaining month.

Do not start by rewriting the project. First understand the existing
codebase and report the safest implementation path.

### Expected implementation milestones

-   **Milestone 1:** Functional movement, fishing spot interaction, and
    timing minigame.
-   **Milestone 2:** Fish data, catching, collection, and encyclopedia.
-   **Milestone 3:** Team selection and reusable turn-based battle.
-   **Milestone 4:** Progression manager, two normal encounters, and
    final boss.
-   **Milestone 5:** UI integration, ending, testing, and final build.

------------------------------------------------------------------------

## 14. Current Design Decisions --- Quick Reference

  Topic                         Current decision
  ----------------------------- -----------------------------------------------------------
  World                         One compact shoreline in a fictional Negros coastal barangay
  Localization                  Local fish names, one decorative bangka, coastal shelter, brief local expressions
  Collection framing            Record species and befriend fish; battles are stylized fantasy
  Fishing                       Separate activity and primary collection method
  Fishing minigame              Timing-based bar
  Initial fish content          Approximately five species
  Fish collection               Collection menu and Fish Encyclopedia
  Battle trigger                Story events unlocked by progression
  Battle frequency              Two normal encounters and one final boss
  Team size                     Up to three collected fish
  Active fighters               One player fish and one opponent fish at a time
  Player actions                Attack, Defend, Special Ability
  Opponent AI                   Basic attack/defend behavior
  Boss                          One environmental boss
  Boss phases                   Not required
  Timer                         No real-time or in-game countdown
  Defeat handling               Allow battle retry; do not remove collected fish
  Final boss                    Player chooses when to start after unlocking
  Ending                        Short sequence showing the coastal environment recovering
  Primary educational feature   Fish Encyclopedia
  Primary SDG                   SDG 14: Life Below Water
  Development target            One month

------------------------------------------------------------------------

## 15. Final Project Direction

FISH! should be a small but complete fishing and fish-collecting
adventure with a natural Philippine coastal identity. Localize through
content and presentation while retaining the one-month scope.

Fishing gives players a reason to explore and collect different aquatic
species. The collection system allows players to discover fish
information and prepare a team. Story-triggered battles give the
collected fish a second purpose: protecting the island from increasingly
serious threats.

The environmental boss provides a clear final objective and connects the
game's story to marine conservation.

The priority is not to build a large RPG. It is to deliver a polished,
playable experience with:

-   One compact coastal environment.
-   One satisfying fishing minigame.
-   A small but meaningful fish collection.
-   A simple encyclopedia.
-   A reusable turn-based battle system.
-   Two story encounters and one final boss.
-   A clear beginning, progression, and ending.

**Build a small complete game before considering additional features.**
