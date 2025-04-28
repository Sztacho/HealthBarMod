
# HealthBar Mod for Vintage Story
![HealthBarMod](https://github.com/Sztacho/HealthBarMod/blob/1.0.0/modicon.png)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)

A lightweight, client-side mod that shows every creature’s (and optionally players’) health in a clean, game-style bar above the head.  
The bar scales with distance, changes colour as HP drops, animates smoothly when damage is taken, and features a subtle dark background with a stone-frame border that fits the Vintage Story aesthetic.

---

## Features

| Feature | Details |
|---------|---------|
| **Clean pixel-art bar** | Stone-frame border, dark backdrop, colour-shifting fill (green → yellow → red). |
| **Distance scaling** | Bar, frame **and text** scale down the farther you stand, avoiding HUD clutter. |
| **Smooth damage animation** | Fill slides to the new value instead of snapping, for a polished look. |
| **Configurable thresholds & colours** | Set low/mid/full HP colours and threshold percentages. |
| **Fade-in / fade-out** | The bar appears only when you look at the mob (or on damage), then fades away. |
| **Optional player bar** | Can show HP over other players (disabled by default). |
| **Tiny footprint** | Pure client code – no world data, no network traffic, safe to add or remove anytime. |

---

## Screenshots
*(insert your in-game screenshots here – the icon serve only as branding)*

---

## Installation

1. Download **HealthBar v1.0.1.zip** from the Releases tab or the VS Mod DB.
2. Drop the zip into your `Mods/` folder (no need to unzip).
3. Start the game – the mod loads automatically and is client-side only.

> **Multiplayer:** Works on any server; other players need the mod installed to see bars on their own screen.

---

## Configuration

After first launch a file `HealthBarSettings.json` appears in `VintagestoryData/ModConfig/`.  
Open it in any text editor; example defaults:

```jsonc
{
  "BarWidth":           100,
  "BarHeight":          12,
  "VerticalOffset":     20,      // pixels above mob head
  "FadeInSpeed":        0.15,    // seconds
  "FadeOutSpeed":       0.25,
  "LowHealthColor":     "#c62828",
  "MidHealthColor":     "#ffd54f",
  "FullHealthColor":    "#66bb6a",
  "LowHealthThreshold": 0.25,    // ≤ 25 % = red
  "MidHealthThreshold": 0.60,    // ≤ 60 % = yellow
  "FrameColor":         "#cccccc",
  "ShowOnPlayers":      false
}
