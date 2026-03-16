# SimpleEvents — Event Framework for EXILED

[![GitHub Releases](https://img.shields.io/github/v/release/KtoOlegMongol/EXILED-SimpleEvents?include_prereleases&label=release&style=flat-square)](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![EXILED](https://img.shields.io/badge/EXILED-8.0%2B-brightgreen?style=flat-square)](https://github.com/Exiled-Team/EXILED)

SimpleEvents is an event framework built on top of [EXILED](https://github.com/Exiled-Team/EXILED) for SCP: Secret Laboratory plugin development. It extends the standard EXILED event system with typed subscriptions, name-based subscriptions, and dynamic event support — without changing anything about how EXILED itself works.

---

## Why

The built-in EXILED event system is solid, but it ties you to a specific EventArgs type for every event. If you want to subscribe to events by name at runtime, share events between plugins without hard dependencies, or handle events you didn't plan for — there's no clean way to do it out of the box.

SimpleEvents adds all of that on top of the existing pipeline. Your old plugins don't need to change. New ones can use the richer API.

---

## How it works

```
Game
 |
 v
EXILED  (Harmony patches, existing event handlers)
 |
 v
SimpleEvents Bridge  — maps EXILED EventArgs to SimpleEventArgs
 |
 v
SimpleEventBus  — dispatches to typed and named handlers
 |
 v
Your handlers
```

SimpleEvents hooks into EXILED's events like any other plugin. It does not patch game code directly and does not interfere with other subscribers.

---

## Installation

Download `SimpleEvents.dll` from the [Releases](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases) page and place it in your EXILED plugins folder.

**Windows:** `%AppData%\EXILED\Plugins\`
**Linux:** `~/.config/EXILED/Plugins/`

The framework initializes automatically when the server starts. No setup required.

---

## Usage

### Subscribe by type

```csharp
using SimpleEvents.Core;
using SimpleEvents.Core.Events;

SimpleEventBus.Subscribe<PlayerDamageEventArgs>(ev =>
{
    if (ev.Attacker != null && ev.Attacker.IsScp)
        ev.Damage *= 2f;
});
```

### Subscribe by name

```csharp
SimpleEventBus.Subscribe("player.damage", ev =>
{
    Log.Info($"A player took damage. Event cancelled: {!ev.IsAllowed}");
});

SimpleEventBus.Subscribe("round.start", ev =>
{
    Log.Info("Round started");
});
```

### Cancel an event

Set `IsAllowed = false` on any cancellable event. SimpleEvents writes it back to the original EXILED EventArgs before the game processes it.

```csharp
SimpleEventBus.Subscribe<PlayerDyingEventArgs>(ev =>
{
    if (ev.Player.IsGodModeEnabled)
        ev.IsAllowed = false;
});
```

### Unsubscribe

```csharp
Action<PlayerDamageEventArgs> handler = ev => { /* ... */ };

SimpleEventBus.Subscribe(handler);
// Later:
SimpleEventBus.Unsubscribe(handler);
```

Named subscriptions work the same way — keep a reference to your delegate.

### Dynamic events

EXILED events that don't have a dedicated SimpleEvents class are automatically wrapped in a `DynamicEventArgs` with all readable properties copied in:

```csharp
SimpleEventBus.Subscribe("dynamic.someexiledevent", ev =>
{
    if (ev is DynamicEventArgs dyn && dyn.Has("Player"))
        Log.Info(dyn.Get<Player>("Player")?.Nickname);
});
```

### Emit your own events

You can push events through the bus from anywhere in your plugin:

```csharp
// Named, no data
SimpleEventBus.Emit("scp.ability.use");

// Dynamic with data
var ev = new DynamicEventArgs { EventName = "scp.ability.cooldown" };
ev["ScpPlayer"] = player;
ev["Cooldown"] = 30f;
SimpleEventBus.Emit(ev);

// Typed
SimpleEventBus.Emit(new PlayerDamageEventArgs
{
    EventName = "player.damage",
    Player = target,
    Damage = 50f,
});
```

---

## Supported events

| Event name      | Typed class                 | EXILED source          | Cancellable |
|-----------------|-----------------------------|------------------------|-------------|
| `player.damage` | `PlayerDamageEventArgs`     | `Player.Hurting`       | Yes         |
| `player.death`  | `PlayerDyingEventArgs`      | `Player.Dying`         | Yes         |
| `player.spawn`  | `PlayerSpawningEventArgs`   | `Player.Spawning`      | No          |
| `round.start`   | `RoundStartedEventArgs`     | `Server.RoundStarted`  | No          |
| `round.end`     | `RoundEndedEventArgs`       | `Server.RoundEnded`    | No          |
| `item.pickup`   | `ItemPickingUpEventArgs`    | `Player.PickingUpItem` | Yes         |

Any other EXILED event dispatched through the bridge produces a `DynamicEventArgs` with the name `dynamic.<eventname>`.

---

## Extending the mapper

Register a custom factory for any EXILED EventArgs type before the bus processes it:

```csharp
EventArgsMapper.Register<SomeCoolEventArgs>("my.event", ev => new DynamicEventArgs
{
    EventName = "my.event",
    ["Player"] = ev.Player,
});
```

---

## Requirements

- [EXILED](https://github.com/Exiled-Team/EXILED) 8.0.0 or later
- .NET Framework 4.8
- SCP: Secret Laboratory (version supported by your EXILED build)

---

## Building from source

Set the `EXILED_REFERENCES` environment variable to your SCP:SL server's `SCPSL_Data/Managed/` folder, then:

```bash
git clone https://github.com/KtoOlegMongol/EXILED-SimpleEvents.git
cd EXILED-SimpleEvents
dotnet build -c Release
```

The output DLL will be in `bin/Release/`.

---

## License

[MIT](LICENSE)
