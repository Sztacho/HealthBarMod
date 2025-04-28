
# HealthBar Mod for Vintage Story
![HealthBarMod](https://github.com/Sztacho/HealthBarMod/blob/master/modicon.png)
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

---

## Configuration options

Edit the file **`VintagestoryData/ModConfig/HealthBarSettings.json`**.  
Every entry is optional – delete a line to restore its default value.

| Key | Default | Description |
|-----|:-------:|-------------|
| `BarWidth` | **66** | Total width of the bar (pixels on your screen). |
| `BarHeight` | **6.6** | Total height of the bar (pixels). |
| `VerticalOffset` | **22** | Vertical distance (pixels) above the mob’s head. Increase for tall entities. |
| `FadeInSpeed` | **0.3** | Seconds it takes to fully fade *in* after the bar becomes visible. |
| `FadeOutSpeed` | **0.5** | Seconds it takes to fade *out* after you stop looking / the timeout expires. |
| `LowHealthThreshold` | **0.25** | ≤ 25 % HP → bar switches to **`LowHealthColor`**. |
| `MidHealthThreshold` | **0.60** | ≤ 60 % HP (and > 25 %) → bar switches to **`MidHealthColor`**. |
| `LowHealthColor` | **"#FF4444"** | Hex colour when HP ≤ `LowHealthThreshold`. |
| `MidHealthColor` | **"#FFCC00"** | Hex colour when HP ≤ `MidHealthThreshold` but > `LowHealthThreshold`. |
| `FullHealthColor` | **"#44FF44"** | Hex colour when HP > `MidHealthThreshold`. |
| `FrameColor` | **"#CCCCCC"** | Hex colour of the stone-style border around the bar. |

**Tip:** Use any 6-digit hex (e.g. `#AABBCC`). The mod parses it automatically.  

