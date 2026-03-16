# SimpleEvents — Event Framework for EXILED

[![GitHub Releases](https://img.shields.io/github/v/release/KtoOlegMongol/EXILED-SimpleEvents?include_prereleases&label=release&style=flat-square)](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![EXILED](https://img.shields.io/badge/EXILED-8.0%2B-brightgreen?style=flat-square)](https://github.com/Exiled-Team/EXILED)

**[Читать на русском](README.ru.md)**

SimpleEvents is an event framework built on top of [EXILED](https://github.com/Exiled-Team/EXILED) for SCP: Secret Laboratory plugin development. It extends the standard EXILED event system with typed subscriptions, name-based subscriptions, and dynamic event support — without changing anything about how EXILED itself works.

---

## Why

The built-in EXILED event system is solid, but it ties you to a specific EventArgs type for every event. If you want to:

- Subscribe to events by name at runtime without importing the originating assembly
- Share events between plugins without tight coupling
- Handle events you didn't plan for at compile time
- Build config-driven or scripting layers on top of game events

...there's no clean way to do it out of the box. SimpleEvents adds all of that on top of the existing pipeline. Your old plugins don't need to change. New ones get a richer API.

---

## Architecture

```
Game
 |
 v
EXILED  (Harmony patches, existing event system)
 |
 v
EventHooks  — subscribes to EXILED events like a normal plugin
 |
 v
ExiledEventBridge  — receives EXILED EventArgs, calls EventArgsMapper
 |
 v
EventArgsMapper  — known types → typed SimpleEventArgs (zero reflection)
                   unknown types → DynamicEventArgs (cached reflection)
 |
 v
SimpleEventBus  — thread-safe dispatch to typed and named handlers
 |
 v
Your handlers
```

SimpleEvents does **not** patch game code directly. It hooks EXILED's exposed events like any normal plugin, which means it's fully compatible with everything else running on your server.

After the bus processes each event, `IsAllowed` is written back to the original EXILED EventArgs — so cancelling an event from a SimpleEvents handler actually cancels it in-game. The original EXILED EventArgs is also available via `ev.OriginalArgs` for any properties not exposed by the typed wrapper.

---

## Installation

Download `SimpleEvents.dll` from the [Releases](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases) page and place it in your EXILED plugins folder.

**Windows:** `%AppData%\EXILED\Plugins\`
**Linux:** `~/.config/EXILED/Plugins\`

The framework initializes automatically when the server starts. No setup required.

**Config** (auto-generated on first run):

```yaml
simple_events:
  is_enabled: true
  debug: false   # set to true to log every dispatched event to console
```

---

## Usage

### Subscribe by type

The most direct way. The cast is handled internally — your handler always receives the concrete type.

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

Useful for scripting layers, config-driven logic, or inter-plugin communication. Name matching is case-insensitive.

```csharp
SimpleEventBus.Subscribe("player.damage", ev =>
{
    Log.Info($"A player took damage. Cancelled: {!ev.IsAllowed}");
});

SimpleEventBus.Subscribe("round.start", ev =>
{
    Log.Info("New round!");
});

SimpleEventBus.Subscribe("server.waiting", ev =>
{
    // Fires when the server enters waiting-for-players state.
    // Good place to reset plugin state between rounds.
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

### Access the original EXILED EventArgs

Every mapped event has an `OriginalArgs` property containing the raw EXILED EventArgs object. Use it to access fields not exposed by the SimpleEvents wrapper.

```csharp
SimpleEventBus.Subscribe<PlayerDamageEventArgs>(ev =>
{
    if (ev.OriginalArgs is HurtingEventArgs original)
    {
        // Access the full EXILED damage handler
        Log.Info(original.DamageHandler.Type.ToString());
    }
});
```

### Unsubscribe

```csharp
Action<PlayerDamageEventArgs> handler = ev => { /* ... */ };

SimpleEventBus.Subscribe(handler);
// Later:
SimpleEventBus.Unsubscribe(handler);
```

Named subscriptions work the same — keep a reference to the delegate.

```csharp
Action<SimpleEventArgs> namedHandler = ev => { /* ... */ };

SimpleEventBus.Subscribe("player.damage", namedHandler);
// Later:
SimpleEventBus.Unsubscribe("player.damage", namedHandler);
```

Subscribing the same delegate instance twice is a no-op (idempotent), matching standard C# event behaviour.

### Dynamic events

EXILED events without a dedicated SimpleEvents class are automatically wrapped in `DynamicEventArgs`. All readable properties are copied into a key/value bag using cached reflection.

```csharp
SimpleEventBus.Subscribe("dynamic.someexiledevent", ev =>
{
    if (ev is DynamicEventArgs dyn && dyn.Has("Player"))
        Log.Info(dyn.Get<Player>("Player")?.Nickname);
});
```

### Emit your own events

Push events through the bus from anywhere in your plugin. Other plugins subscribed to SimpleEventBus will receive them.

```csharp
// Named, no data — fire and forget
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

### Inter-plugin communication

SimpleEvents makes it easy for plugins to talk to each other without hard dependencies. Plugin A emits an event; Plugin B subscribes to it by name without importing Plugin A's assembly.

**Plugin A** (producer):
```csharp
var ev = new DynamicEventArgs { EventName = "myplugin.something_happened" };
ev["Data"] = someData;
SimpleEventBus.Emit(ev);
```

**Plugin B** (consumer):
```csharp
SimpleEventBus.Subscribe("myplugin.something_happened", ev =>
{
    var data = (ev as DynamicEventArgs)?.Get<SomeType>("Data");
    // handle it
});
```

---

## Supported events

| Event name        | Typed class                 | EXILED source            | Cancellable | Write-back           |
|-------------------|-----------------------------|--------------------------|-------------|----------------------|
| `server.waiting`  | *(DynamicEventArgs)*        | `Server.WaitingForPlayers` | No        | —                    |
| `round.start`     | `RoundStartedEventArgs`     | `Server.RoundStarted`    | No          | —                    |
| `round.end`       | `RoundEndedEventArgs`       | `Server.RoundEnded`      | No          | —                    |
| `player.damage`   | `PlayerDamageEventArgs`     | `Player.Hurting`         | Yes         | `IsAllowed`          |
| `player.death`    | `PlayerDyingEventArgs`      | `Player.Dying`           | Yes         | `IsAllowed`          |
| `player.spawn`    | `PlayerSpawningEventArgs`   | `Player.Spawning`        | No          | `Position`, `Rotation` |
| `item.pickup`     | `ItemPickingUpEventArgs`    | `Player.PickingUpItem`   | Yes         | `IsAllowed`          |

Any other EXILED event dispatched through the bridge produces a `DynamicEventArgs` with the name `dynamic.<eventname>` and all readable properties copied in.

---

## Extending the mapper

Register your own factory for any EXILED EventArgs type. This runs at zero reflection cost for that type going forward.

```csharp
// In your plugin's OnEnabled():
EventArgsMapper.Register<SomeExiledEventArgs>("my.event.name", ev => new DynamicEventArgs
{
    EventName = "my.event.name",
    ["Player"] = ev.Player,
    ["SomeValue"] = ev.SomeProperty,
});
```

---

## Performance

- **Known event types** (the 7 listed above) are mapped using pre-registered factory functions — zero reflection at runtime.
- **Unknown event types** use cached `PropertyInfo[]` arrays — reflection runs once per type, not once per event.
- The bus dispatch takes a snapshot of the handler list under a lock, then invokes handlers outside the lock — safe for concurrent scenarios without blocking.
- Each `Subscribe<T>` call allocates one wrapper lambda that is reused for all future dispatches of that subscription.

---

## Thread safety

`SimpleEventBus` is thread-safe. Handlers can be subscribed and unsubscribed from any thread. The internal lock is held only to read/modify the handler lists — handlers are invoked outside the lock, so you won't deadlock even if a handler subscribes to another event.

`EventHooks.Subscribe`/`Unsubscribe` are also thread-safe.

---

## Dynamic updates (hot-reload)

SimpleEvents fully supports EXILED's hot-reload system. On `OnDisabled`, all hooks are unsubscribed, the bus is cleared, and the Harmony instance is cleanly unpatched. On the next `OnEnabled`, everything reinitializes from scratch.

If your plugin uses SimpleEvents, unsubscribe your handlers in `OnDisabled`:

```csharp
public override void OnDisabled()
{
    SimpleEventBus.Unsubscribe<PlayerDamageEventArgs>(OnPlayerDamage);
    SimpleEventBus.Unsubscribe("round.start", OnRoundStart);
    base.OnDisabled();
}
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

## Troubleshooting

**Events aren't firing**
Make sure `SimpleEvents.dll` is in your Plugins folder and that no startup errors appear in the server console when it loads. Enable `debug: true` in the config to see every dispatched event logged.

**IsAllowed = false isn't cancelling the event**
Only events marked "Cancellable: Yes" in the table above support write-back. For others, the game doesn't expose cancellation.

**My handler fires but changes to Damage/Position aren't applied**
Only the properties listed in the "Write-back" column are synced back to EXILED. For anything else, cast `ev.OriginalArgs` to the original EXILED type and modify it directly.

**`OriginalArgs` is null**
This happens when an event was emitted directly via `SimpleEventBus.Emit(...)` rather than coming from the EXILED bridge.

---

## Changelog

### v1.1.0
- Fixed: missing `using System` caused compile error on `Version` type
- Fixed: `HarmonyBootstrap.Initialize()` now unpatches existing Harmony instance before creating a new one (prevents patch leak on hot-reload)
- Fixed: double-subscribing the same typed handler no longer corrupts the wrapper cache — subsequent calls are now idempotent
- Fixed: `EventHooks.subscribed` flag is now protected by a lock (was a plain bool, race condition on concurrent Subscribe/Unsubscribe)
- Fixed: `EventArgsMapper.PropertyCache` is now accessed under a lock (was unsafe for concurrent events of unknown types)
- Fixed: `CanRead` is now checked before `GetValue` in `BuildDynamic` (was checked after, could throw on write-only properties)
- Fixed: `RoundEndedEventArgs.LeadingTeam` changed from `int` to proper `LeadingTeam` enum — previous version silently discarded type information
- Added: `OriginalArgs` is now populated on all bridge dispatch methods
- Added: `server.waiting` event (`Server.WaitingForPlayers`)
- Added: `Debug` config option now actually logs dispatch activity to console
- Added: `RoundEndedEventArgs.WinningTeamName` convenience property

### v1.0.0
- Initial release

---

## License

[MIT](LICENSE)
