# SimpleEvents — Event Framework для EXILED

[![GitHub Releases](https://img.shields.io/github/v/release/KtoOlegMongol/EXILED-SimpleEvents?include_prereleases&label=release&style=flat-square)](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![EXILED](https://img.shields.io/badge/EXILED-8.0%2B-brightgreen?style=flat-square)](https://github.com/Exiled-Team/EXILED)

SimpleEvents — это фреймворк событий, построенный поверх [EXILED](https://github.com/Exiled-Team/EXILED) для разработки плагинов SCP: Secret Laboratory. Расширяет стандартную систему событий EXILED: добавляет типизированную подписку, подписку по имени и динамические события — не ломая при этом ничего в самом EXILED.

---

## Зачем это нужно

Стандартная система событий EXILED хорошо работает, но привязывает тебя к конкретному типу EventArgs для каждого события. Если нужно подписаться на события по имени в рантайме, делиться событиями между плагинами без жёстких зависимостей или обрабатывать события, которые не планировал заранее — нормального способа нет.

SimpleEvents добавляет всё это поверх существующего пайплайна. Старые плагины работают без изменений, новые получают расширенный API.

---

## Как это работает

```
Игра
 |
 v
EXILED  (Harmony патчи, стандартные обработчики)
 |
 v
SimpleEvents Bridge  — преобразует EXILED EventArgs → SimpleEventArgs
 |
 v
SimpleEventBus  — рассылает по типизированным и именованным обработчикам
 |
 v
Твои обработчики
```

SimpleEvents подписывается на события EXILED как обычный плагин. Игровой код напрямую не патчит и другим подписчикам не мешает.

---

## Установка

Скачай `SimpleEvents.dll` со страницы [Releases](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases) и положи в папку плагинов EXILED.

**Windows:** `%AppData%\EXILED\Plugins\`
**Linux:** `~/.config/EXILED/Plugins/`

Фреймворк инициализируется автоматически при запуске сервера. Никакой настройки не требуется.

---

## Использование

### Подписка по типу

```csharp
using SimpleEvents.Core;
using SimpleEvents.Core.Events;

SimpleEventBus.Subscribe<PlayerDamageEventArgs>(ev =>
{
    // Удвоить урон от SCP
    if (ev.Attacker != null && ev.Attacker.IsScp)
        ev.Damage *= 2f;
});
```

### Подписка по имени

```csharp
SimpleEventBus.Subscribe("player.damage", ev =>
{
    Log.Info($"Игрок получил урон. Отменено: {!ev.IsAllowed}");
});

SimpleEventBus.Subscribe("round.start", ev =>
{
    Log.Info("Раунд начался");
});
```

### Отмена события

Установи `IsAllowed = false` на любом отменяемом событии. SimpleEvents сам запишет это обратно в EXILED EventArgs до того, как игра обработает его.

```csharp
SimpleEventBus.Subscribe<PlayerDyingEventArgs>(ev =>
{
    if (ev.Player.IsGodModeEnabled)
        ev.IsAllowed = false;
});
```

### Отписка

```csharp
Action<PlayerDamageEventArgs> handler = ev => { /* ... */ };

SimpleEventBus.Subscribe(handler);
// Позже:
SimpleEventBus.Unsubscribe(handler);
```

Именованные подписки работают так же — сохрани ссылку на делегат.

### Динамические события

События EXILED без выделенного класса SimpleEvents автоматически оборачиваются в `DynamicEventArgs` — все читаемые свойства копируются в словарь:

```csharp
SimpleEventBus.Subscribe("dynamic.someexiledevent", ev =>
{
    if (ev is DynamicEventArgs dyn && dyn.Has("Player"))
        Log.Info(dyn.Get<Player>("Player")?.Nickname);
});
```

### Генерация собственных событий

```csharp
// Только имя, без данных
SimpleEventBus.Emit("scp.ability.use");

// Динамическое с данными
var ev = new DynamicEventArgs { EventName = "scp.ability.cooldown" };
ev["ScpPlayer"] = player;
ev["Cooldown"] = 30f;
SimpleEventBus.Emit(ev);

// Типизированное
SimpleEventBus.Emit(new PlayerDamageEventArgs
{
    EventName = "player.damage",
    Player = target,
    Damage = 50f,
});
```

---

## Поддерживаемые события

| Имя события     | Тип                         | Источник EXILED        | Отменяемое |
|-----------------|-----------------------------|------------------------|------------|
| `player.damage` | `PlayerDamageEventArgs`     | `Player.Hurting`       | Да         |
| `player.death`  | `PlayerDyingEventArgs`      | `Player.Dying`         | Да         |
| `player.spawn`  | `PlayerSpawningEventArgs`   | `Player.Spawning`      | Нет        |
| `round.start`   | `RoundStartedEventArgs`     | `Server.RoundStarted`  | Нет        |
| `round.end`     | `RoundEndedEventArgs`       | `Server.RoundEnded`    | Нет        |
| `item.pickup`   | `ItemPickingUpEventArgs`    | `Player.PickingUpItem` | Да         |

Любое другое EXILED событие, прошедшее через bridge, генерирует `DynamicEventArgs` с именем `dynamic.<eventname>`.

---

## Расширение маппера

Зарегистрируй свою фабрику для любого EXILED EventArgs типа:

```csharp
EventArgsMapper.Register<SomeCoolEventArgs>("my.event", ev => new DynamicEventArgs
{
    EventName = "my.event",
    ["Player"] = ev.Player,
});
```

---

## Требования

- [EXILED](https://github.com/Exiled-Team/EXILED) 8.0.0 или новее
- .NET Framework 4.8
- SCP: Secret Laboratory (версия, поддерживаемая твоей сборкой EXILED)

---

## Сборка из исходников

Укажи переменную окружения `EXILED_REFERENCES` — путь к папке `SCPSL_Data/Managed/` сервера SCP:SL, затем:

```bash
git clone https://github.com/KtoOlegMongol/EXILED-SimpleEvents.git
cd EXILED-SimpleEvents
dotnet build -c Release
```

Готовый DLL будет в `bin/Release/`.

---

## Лицензия

[MIT](LICENSE)
