# Color Bus Sim Prototype

Unity prototype for a color-based bus routing and passenger transfer game loop.

## Overview

This project simulates buses moving on spline paths, picking up and dropping off color-coded passengers at stops.

The core loop is:

1. Spawn buses from data.
2. Spawn passengers from bus quotas.
3. Seed each bus with initial onboard passengers.
4. Start buses by player click.
5. Let buses auto-drive, avoid collisions, and interact with stops.

## Core Gameplay Logic

- **Bus spawning**: `BusManager` reads `BusDataContainer` and instantiates one bus per `BusData` entry.
- **Passenger spawning**: passengers are spawned according to each bus type's configured count and then shuffled.
- **Initial loadout**: each bus receives 3 initial passengers.
- **Player input**: clicking a bus starts its movement.
- **Bus movement**: each bus follows a spline path with acceleration/deceleration.
- **Obstacle handling**: buses raycast ahead and stop when blocked by another bus.
- **Stop interaction**:
  - Pick up passengers from stops matching bus color/type.
  - Drop off matching passengers to typed stops of other colors.
  - If a stop is empty (`None`), buses can seed it with the best candidate passenger type.

## Main Scripts

- `Assets/Scripts/BusManager.cs`
  - Startup orchestration and per-frame bus updates.
  - Global tracking of occupied stop types.
  - Broadcasts stop-type changes to buses.

- `Assets/Scripts/BaseBus.cs`
  - Movement along spline.
  - Click-to-start behavior.
  - Raycast-based obstacle and stop detection.
  - Pickup/drop/seed logic and onboard passenger state.

- `Assets/Scripts/PassengerStop.cs`
  - Stop type state and waiting passenger list.
  - Type change events when becoming occupied/empty.
  - Passenger transfer to buses.

- `Assets/Scripts/PassengerObject.cs`
  - Passenger visual object and type.
  - DOTween jump animation during transfers.

- `Assets/Scripts/BusDataContainer.cs`
  - ScriptableObject data model for bus setup (type, color, prefab, capacity, materials, counts).

- `Assets/Scripts/BaseBusObject.cs`
  - Shared `PassengerType` enum and `IBus` interface.

## Data-Driven Setup

`BusDataContainer` controls most tuning:

- bus prefab
- passenger prefab
- bus type/color
- max bus count/capacity
- stop material

This allows quick iteration of balancing and content without changing core logic code.

## Event Flow

- `PassengerStop` emits stop-type changes.
- `BusManager` listens and updates a global occupied stop-type list.
- `BusManager` broadcasts the updated list to buses.
- `BaseBus` uses this list to avoid claiming empty stops with duplicate active types.

## Current Prototype Notes

- Restart is implemented via scene reload.
- Endgame/win/loss conditions are not fully implemented yet.
- Some scripts include placeholders or in-progress logic (`TestScript`, commented sections).

## Dependencies

- Unity
- DOTween (used for passenger transfer animation)

## How to Run

1. Open project in Unity.
2. Open the main gameplay scene.
3. Press Play.
4. Click buses to start movement and observe stop interactions.

