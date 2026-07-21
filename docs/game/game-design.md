# Minimap - Game design

Note: Minimap is a working title

## Game summary and primary features

- A 2D action/strategy game in the Vampire Survivor / roguelite genre
- Single screen world, no scrolling (rectangular hex arena — see [features/map-layout.md](features/map-layout.md))
- 1-4 player, local-coop (lobby join → hex arena; see [features/lobby.md](features/lobby.md))
- Retro pixel art graphics
- Hex grid world with mutable cell types
- Realtime cartesian movement (screen-axis input); hex walls and other characters are solid colliders with slide
- Characters belong to factions; hostiles are other factions ([features/factions.md](features/factions.md))
- Health, damage, and missile combat ([features/health.md](features/health.md), [features/damage.md](features/damage.md), [features/combat.md](features/combat.md))
- AI characters wander (floor goals + navigation steering) and shoot hostiles ([features/ai.md](features/ai.md))
- Dense, close quarters tactics
- Minimally procedurally generated world
- World transforms over the course of each playthrough

## Goal

To create a game I can play with my sons which is both chill and tense and has a variety of things to do and strategies to try.

## Setting

Takes place in my CompuQuest universe, focusing on office workers fighting supernatural horrors, laced with a 90s computer theme.
