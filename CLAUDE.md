# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a WPF-based strategy/simulation game built with .NET 10.0 targeting Windows. The game features resource management, building construction with upgrades, a research system, and a tile-based isometric map with camera controls.

## Build and Run Commands

```powershell
# Build the project
dotnet build anartsgame/anartsgame.csproj

# Run the application
dotnet run --project anartsgame/anartsgame.csproj

# Build for release
dotnet build anartsgame/anartsgame.csproj -c Release

# Clean build artifacts
dotnet clean anartsgame/anartsgame.csproj
```

The application can also be built and run through Visual Studio by opening `anartsgame/anartsgame.csproj`.

## Architecture

### MVVM Pattern

The application follows the Model-View-ViewModel pattern:

- **Models** (`models/`): Core game logic and data structures
  - `GameState`: Central state container with resources, buildings, research, map, and camera data
  - `Building`: Building entities with production, investment, and batch production systems
  - `Research`: Research system with progress tracking
  - `GameMap` and `Tile`: Tile-based map system with isometric rendering support
  
- **ViewModels** (`viewmodels/`): Presentation logic and data binding
  - `BaseViewModel`: Base class implementing `INotifyPropertyChanged` with `SetProperty` helper
  - `RelayCommand`: Command pattern implementation for UI actions
  - Specific ViewModels for each view (MainMenuViewModel, SettingsViewModel, etc.)

- **Views** (`views/`): XAML UI definitions
  - `SplashView`: Initial loading screen
  - `MainMenuView`: Main menu navigation
  - `NewGameSetupView`: Game initialization settings
  - `GameView`: Main gameplay interface
  - `SettingsView`: Application settings

### Navigation Flow

The application uses a view-swapping pattern in `MainWindow`:

1. **Startup**: Window fades in (1.5s animation)
2. **Splash Screen**: `SplashView` displays until `LoadingComplete` event fires
3. **Main Menu**: `MainMenuView` fades in (1s animation) after splash completes

Views are swapped by clearing `ContentGrid.Children` and adding the new view. Transitions use `DoubleAnimation` with `QuadraticEase` easing functions.

### State Management and Serialization

The `GameState` class serves as the central state container. All game entities have corresponding data classes (e.g., `Building` ↔ `BuildingData`) with bidirectional conversion methods:

- `FromBuilding(Building)` / `FromResearch(Research)` / `FromGameMap(GameMap)`: Convert runtime objects to serializable data
- `ToBuilding()` / `ToResearch()` / `ToGameMap()`: Reconstruct runtime objects from data

This pattern enables save/load functionality and state snapshots.

### Game Systems

**Building System**:
- Buildings have levels, production progress, and position on the map
- **Investment System**: Buildings can accept resource investments to upgrade (with cooldown and progress tracking)
- **Batch Production**: Buildings can queue batch production jobs with input/output resources

**Research System**:
- Research items have types, progress tracking, and completion states
- Only one research can be active at a time (`CurrentResearch` in `GameState`)

**Resource System**:
- Resources stored as `Dictionary<ResourceType, int>` in `GameState`
- Resource types defined by `ResourceType` enum

**Map System**:
- Tile-based grid with `TileType` enum
- Tiles have walkability, position, and offset properties (for isometric rendering)
- Camera system with scale and translation for pan/zoom

**Game Speed Control**:
- `GameSpeed` property in `GameState` (multiplier for game time)
- `ElapsedTime` tracks total game time

### Services

**SettingsService** (`services/`):
- Singleton pattern (`SettingsService.Instance`)
- Manages application settings like fullscreen mode
- Called during `MainWindow.OnLoaded` to apply initial settings

### Resources

**Custom Font**:
- `styles/Watc2.TTF` is embedded as a resource
- Reference in XAML using the font family name

**Music Files**:
- Located in `music/` directory (`.ogg` and `.mp3` formats)
- Automatically copied to output directory (`CopyToOutputDirectory: PreserveNewest`)
- Access at runtime from the application directory

## Code Conventions

- **Namespace**: All code uses `anartsgame` as the root namespace with subfolders (e.g., `anartsgame.models`, `anartsgame.viewmodels`)
- **Nullable Reference Types**: Enabled project-wide (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Property Change Notification**: Use `SetProperty` from `BaseViewModel` for automatic `INotifyPropertyChanged` implementation
- **Async Patterns**: UI animations use `TaskCompletionSource` to await animation completion

## Common Development Patterns

### Adding a New View

1. Create XAML file in `views/` (e.g., `NewView.xaml`)
2. Create corresponding ViewModel in `viewmodels/` (e.g., `NewViewModel.cs` inheriting from `BaseViewModel`)
3. Set `DataContext` in view's code-behind or XAML
4. Add navigation logic in calling view/ViewModel using `RelayCommand`
5. Update `MainWindow` or parent view to handle view swapping with animations

### Adding a New Game Entity

1. Create model class in `models/` with game logic
2. Create corresponding data class (e.g., `EntityData`) for serialization
3. Implement `FromEntity(Entity)` static method and `ToEntity()` instance method
4. Add to `GameState` as appropriate (list, dictionary, or single property)
5. Update any save/load logic to include the new entity

### Working with Animations

Use the established pattern from `MainWindow`:
```csharp
var animation = new DoubleAnimation
{
    From = startValue,
    To = endValue,
    Duration = TimeSpan.FromSeconds(duration),
    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
};
element.BeginAnimation(PropertyToAnimate, animation);
```

For awaitable animations:
```csharp
var tcs = new TaskCompletionSource();
animation.Completed += (s, e) => tcs.SetResult();
element.BeginAnimation(PropertyToAnimate, animation);
await tcs.Task;
```

## Important Notes

- This is a Windows-only application (WPF requires Windows)
- The game uses an isometric tile rendering system (note `OffsetX`/`OffsetY` in `Tile`)
- All ViewModels should inherit from `BaseViewModel` for consistent property change notification
- The application starts with a splash screen - ensure any initialization logic completes before firing `LoadingComplete` event
- Music files must be in the `music/` directory to be included in the build output
