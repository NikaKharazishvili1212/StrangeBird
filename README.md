# Strange Bird
A Flappy Bird-inspired Unity game with skill systems, customization, and dynamic gameplay, showcasing clean, performant, and modular code.

## 🎮 Features
- **Core Gameplay**: Refined Flappy Bird mechanics with smooth physics.
- **Skill System**: 
  - *Luck*: Boosts coin spawn probability.
  - *Shield*: Temporary invincibility with cooldown.
- **Customization**: Unlockable birds, backgrounds, and obstacle styles.
- **Progression**: Coin collection and skill upgrades via shop system.
- **Dynamic Environment**: Day/night cycle and decorative birds.
- **Multiple Difficulties**: Easy, Medium, and Hard modes, affecting moving speed, Coin spawn chance and score gain over time.

## 🛠️ Technical Highlights
- **OOP & SOLID Principles**: Modular design with partial classes (`GameManager`, `MenuManager`) and `Movable` base class for single responsibility and extensibility.
- **Performance Optimization**:
  - Object pooling for `Bird`, `Coin`, and `Obstacle` to minimize instantiation.
  - `SpriteAtlas` and sprite sheets for optimized rendering.
  - `VInspector` `[Button]` attribute runs `FindObjectsOfType<Button>` in editor, avoiding runtime costs.
- **Helper Classes**:
  - `Constants` centralizes magic numbers for maintainability.
  - `Utils` with overloaded `PoolObject` methods for reusable pooling across `GameObject` and `Component` arrays.
- **Event-Driven Design**: `Player` events (`OnCoinTake`, `OnDeath`, `OnRespawn`) decouple logic.
- **External Plugin**: `VInspector` for enhanced Inspector organization (`[Tab]`, `[Button]`).
- **Robustness**: Null checks and debug warnings (e.g., in `Utils.Wait`).
- **Async Scene Loading**: Smooth transitions with progress UI.
- **Shop & Save System**: Persistent data via `PlayerPrefs` for cosmetics and skills.

## 🎥 Gameplay Video
https://youtu.be/A9y79YyFsdw

---