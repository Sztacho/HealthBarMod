
# HealthBar Mod for Vintage Story
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

A lightweight, client-side mod that shows every creature’s (and optionally players’) health in a clean, game-style bar above the head.  
The bar scales with distance, changes colour as HP drops, animates smoothly when damage is taken, and features a subtle dark background with a stone-frame border that fits the Vintage Story aesthetic.

---

## Features


| Feature | Details |
|---------|---------|
| **Stylish pixel bar** | Stone-frame border, dark backdrop, colour-shifting fill (green → yellow → red). |
| **Distance-aware scaling** | Bar **and text** shrink as you step back, keeping the HUD tidy. |
| **Smooth damage animation** | Fill slides to the new value instead of snapping. |
| **Fade-in / fade-out** | Appears only when you target the entity, then fades out. |
| **Configurable everything** | Colours, thresholds, size, fade speed, vertical offset… all in JSON. |
| **Tiny footprint** | Pure client mod. Safe to add or remove at any time. |


## Screenshots
![HealthBarModScreen1](https://github.com/Sztacho/HealthBarMod/blob/master/ScreenShots/screen1.png)
![HealthBarModScreen2](https://github.com/Sztacho/HealthBarMod/blob/master/ScreenShots/screen2.png)
![HealthBarModScreen3](https://github.com/Sztacho/HealthBarMod/blob/master/ScreenShots/screen3.png)

---

## Installation

1. Download **HealthBar v1.0.1.zip** from the Releases tab or the VS Mod DB.
2. Drop the zip into your `Mods/` folder (no need to unzip).
3. Start the game – the mod loads automatically and is client-side only.

> **Multiplayer:** Works on any server; other players need the mod installed to see bars on their own screen.