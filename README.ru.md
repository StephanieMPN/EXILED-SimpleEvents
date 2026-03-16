# SimpleEvents — Event Framework для EXILED

[![GitHub Releases](https://img.shields.io/github/v/release/KtoOlegMongol/EXILED-SimpleEvents?include_prereleases&label=release&style=flat-square)](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases)
[![License](https://img.shields.io/badge/license-MIT-blue?style=flat-square)](LICENSE)
[![EXILED](https://img.shields.io/badge/EXILED-8.0%2B-brightgreen?style=flat-square)](https://github.com/Exiled-Team/EXILED)

SimpleEvents — это фреймворк событий, построенный поверх [EXILED](https://github.com/Exiled-Team/EXILED) для разработки плагинов SCP: Secret Laboratory. Расширяет стандартную систему событий EXILED типизированной подпиской, подпиской по имени и динамическими событиями — не ломая при этом ничего в самом EXILED.

---

## Зачем

Стандартная система событий EXILED хорошо работает, но привязывает тебя к конкретному типу EventArgs для каждого события. Если нужно:

- Подписаться на события по имени в рантайме без импорта исходной сборки
- Обмениваться событиями между плагинами без жёстких зависимостей
- Обрабатывать события, которые не планировались на этапе компиляции
- Строить конфиг-ориентированные или скриптовые слои поверх игровых событий

...нормального способа нет. SimpleEvents добавляет всё это поверх существующего пайплайна. Старые плагины не требуют изменений.

---

## Архитектура

```
Игра
 |
 v
EXILED  (Harmony патчи, стандартная система событий)
 |
 v
EventHooks  — подписывается на EXILED события как обычный плагин
 |
 v
ExiledEventBridge  — принимает EXILED EventArgs, вызывает EventArgsMapper
 |
 v
EventArgsMapper  — известные типы → типизированные SimpleEventArgs (без reflection)
                   неизвестные типы → DynamicEventArgs (кешированный reflection)
 |
 v
SimpleEventBus  — потокобезопасная рассылка типизированным и именованным обработчикам
 |
 v
Твои обработчики
```

SimpleEvents **не патчит игровой код напрямую**. Он подписывается на события EXILED как обычный плагин — полностью совместим со всем, что работает на сервере.

После обработки каждого события `IsAllowed` записывается обратно в оригинальный EXILED EventArgs — отмена события из SimpleEvents-обработчика реально отменяет его в игре. Оригинальный EXILED EventArgs доступен через `ev.OriginalArgs` для доступа к любым полям, не вынесенным в типизированную обёртку.

---

## Установка

Скачай `SimpleEvents.dll` со страницы [Releases](https://github.com/KtoOlegMongol/EXILED-SimpleEvents/releases) и положи в папку плагинов EXILED.

**Windows:** `%AppData%\EXILED\Plugins\`
**Linux:** `~/.config/EXILED/Plugins/`

Фреймворк инициализируется автоматически при запуске сервера.

**Конфиг** (генерируется автоматически при первом запуске):

```yaml
simple_events:
  is_enabled: true
  debug: false   # включи чтобы логировать каждое диспатченное событие в консоль
```

---

## Использование

### Подписка по типу

Самый прямой способ. Каст обрабатывается внутри — обработчик всегда получает конкретный тип.

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

Удобно для конфиг-ориентированной логики, скриптовых слоёв или коммуникации между плагинами. Регистр не учитывается.

```csharp
SimpleEventBus.Subscribe("player.damage", ev =>
{
    Log.Info($"Игрок получил урон. Отменено: {!ev.IsAllowed}");
});

SimpleEventBus.Subscribe("round.start", ev =>
{
    Log.Info("Раунд начался!");
});

SimpleEventBus.Subscribe("server.waiting", ev =>
{
    // Срабатывает когда сервер переходит в режим ожидания игроков.
    // Хорошее место для сброса состояния плагина между раундами.
});
```

### Отмена события

Установи `IsAllowed = false` на любом отменяемом событии. SimpleEvents запишет это обратно в EXILED EventArgs до того, как игра его обработает.

```csharp
SimpleEventBus.Subscribe<PlayerDyingEventArgs>(ev =>
{
    if (ev.Player.IsGodModeEnabled)
        ev.IsAllowed = false;
});
```

### Доступ к оригинальному EXILED EventArgs

Каждое замапленное событие содержит `OriginalArgs` — сырой EXILED EventArgs. Используй для доступа к полям, не вынесенным в обёртку SimpleEvents.

```csharp
SimpleEventBus.Subscribe<PlayerDamageEventArgs>(ev =>
{
    if (ev.OriginalArgs is HurtingEventArgs original)
    {
        // Полный доступ к EXILED damage handler
        Log.Info(original.DamageHandler.Type.ToString());
    }
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

```csharp
Action<SimpleEventArgs> namedHandler = ev => { /* ... */ };

SimpleEventBus.Subscribe("player.damage", namedHandler);
// Позже:
SimpleEventBus.Unsubscribe("player.damage", namedHandler);
```

Повторная подписка одного и того же делегата — no-op (идемпотентно), как у стандартных C# событий.

### Динамические события

EXILED события без выделенного класса SimpleEvents автоматически оборачиваются в `DynamicEventArgs`. Все читаемые свойства копируются в словарь через кешированный reflection.

```csharp
SimpleEventBus.Subscribe("dynamic.someexiledevent", ev =>
{
    if (ev is DynamicEventArgs dyn && dyn.Has("Player"))
        Log.Info(dyn.Get<Player>("Player")?.Nickname);
});
```

### Генерация собственных событий

Любой плагин может отправлять события через шину. Другие плагины, подписанные на SimpleEventBus, их получат.

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

### Коммуникация между плагинами

SimpleEvents позволяет плагинам общаться без взаимной зависимости. Плагин A отправляет событие; плагин B подписывается по имени без импорта сборки плагина A.

**Плагин A** (источник):
```csharp
var ev = new DynamicEventArgs { EventName = "myplugin.something_happened" };
ev["Data"] = someData;
SimpleEventBus.Emit(ev);
```

**Плагин B** (получатель):
```csharp
SimpleEventBus.Subscribe("myplugin.something_happened", ev =>
{
    var data = (ev as DynamicEventArgs)?.Get<SomeType>("Data");
    // обработка
});
```

---

## Поддерживаемые события

| Имя события       | Тип                         | Источник EXILED             | Отменяемое | Write-back           |
|-------------------|-----------------------------|------------------------------|------------|----------------------|
| `server.waiting`  | *(DynamicEventArgs)*        | `Server.WaitingForPlayers`  | Нет        | —                    |
| `round.start`     | `RoundStartedEventArgs`     | `Server.RoundStarted`       | Нет        | —                    |
| `round.end`       | `RoundEndedEventArgs`       | `Server.RoundEnded`         | Нет        | —                    |
| `player.damage`   | `PlayerDamageEventArgs`     | `Player.Hurting`            | Да         | `IsAllowed`          |
| `player.death`    | `PlayerDyingEventArgs`      | `Player.Dying`              | Да         | `IsAllowed`          |
| `player.spawn`    | `PlayerSpawningEventArgs`   | `Player.Spawning`           | Нет        | `Position`, `Rotation` |
| `item.pickup`     | `ItemPickingUpEventArgs`    | `Player.PickingUpItem`      | Да         | `IsAllowed`          |

Любое другое EXILED событие, прошедшее через bridge, генерирует `DynamicEventArgs` с именем `dynamic.<eventname>` и всеми читаемыми свойствами.

---

## Расширение маппера

Зарегистрируй свою фабрику для любого EXILED EventArgs типа. После регистрации этот тип маппируется без reflection.

```csharp
// В OnEnabled() своего плагина:
EventArgsMapper.Register<SomeExiledEventArgs>("my.event.name", ev => new DynamicEventArgs
{
    EventName = "my.event.name",
    ["Player"] = ev.Player,
    ["SomeValue"] = ev.SomeProperty,
});
```

---

## Производительность

- **Известные типы событий** (7 перечисленных выше) маппируются через заранее зарегистрированные фабричные функции — **ноль reflection** в рантайме.
- **Неизвестные типы** используют кешированные массивы `PropertyInfo[]` — reflection выполняется один раз на тип, а не на каждое событие.
- Dispatch в шине берёт снапшот списка обработчиков под лок, затем вызывает их вне лока — безопасно для многопоточных сценариев без лишней блокировки.
- Каждый вызов `Subscribe<T>` аллоцирует один wrapper-лямбда, который переиспользуется для всех последующих вызовов этой подписки.

---

## Потокобезопасность

`SimpleEventBus` потокобезопасен. Обработчики можно подписывать и отписывать из любого потока. Внутренний лок удерживается только для чтения/изменения списков — обработчики вызываются вне лока, дедлок невозможен.

`EventHooks.Subscribe`/`Unsubscribe` тоже потокобезопасны.

---

## Динамические обновления (hot-reload)

SimpleEvents полностью поддерживает горячую перезагрузку EXILED. При `OnDisabled` все хуки отписываются, шина очищается, Harmony-инстанс корректно анпатчится. При следующем `OnEnabled` всё инициализируется заново с нуля.

Если твой плагин использует SimpleEvents, отпишись от своих обработчиков в `OnDisabled`:

```csharp
public override void OnDisabled()
{
    SimpleEventBus.Unsubscribe<PlayerDamageEventArgs>(OnPlayerDamage);
    SimpleEventBus.Unsubscribe("round.start", OnRoundStart);
    base.OnDisabled();
}
```

---

## Требования

- [EXILED](https://github.com/Exiled-Team/EXILED) 8.0.0 или новее
- .NET Framework 4.8
- SCP: Secret Laboratory (версия, поддерживаемая твоей сборкой EXILED)

---

## Сборка из исходников

Укажи переменную окружения `EXILED_REFERENCES` — путь к папке `SCPSL_Data/Managed/` сервера SCP:SL:

```bash
git clone https://github.com/KtoOlegMongol/EXILED-SimpleEvents.git
cd EXILED-SimpleEvents
dotnet build -c Release
```

Готовый DLL будет в `bin/Release/`.

---

## Решение проблем

**События не срабатывают**
Убедись что `SimpleEvents.dll` находится в папке Plugins и при загрузке нет ошибок в консоли. Включи `debug: true` в конфиге — каждое диспатченное событие будет логироваться.

**`IsAllowed = false` не отменяет событие**
Только события с "Отменяемое: Да" в таблице поддерживают write-back. Для остальных игра не предоставляет механизм отмены.

**Изменения Damage/Position не применяются**
Только поля из колонки "Write-back" синхронизируются обратно в EXILED. Для остального — каст `ev.OriginalArgs` к оригинальному EXILED типу и модифицируй напрямую.

**`OriginalArgs` равен null**
Это происходит когда событие было отправлено напрямую через `SimpleEventBus.Emit(...)`, а не пришло через EXILED bridge.

---

## История изменений

### v1.1.0
- Исправлено: отсутствие `using System` вызывало ошибку компиляции на типе `Version`
- Исправлено: `HarmonyBootstrap.Initialize()` теперь анпатчит существующий Harmony-инстанс перед созданием нового (предотвращает утечку патчей при hot-reload)
- Исправлено: повторная подписка одного typed-обработчика больше не ломает WrapperCache — последующие вызовы идемпотентны
- Исправлено: флаг `EventHooks.subscribed` теперь защищён локом (был обычным bool, race condition при параллельных Subscribe/Unsubscribe)
- Исправлено: `EventArgsMapper.PropertyCache` теперь доступен под локом (был небезопасен при параллельных событиях неизвестных типов)
- Исправлено: `CanRead` теперь проверяется до `GetValue` в `BuildDynamic` (раньше проверялся после, мог бросить на write-only свойствах)
- Исправлено: `RoundEndedEventArgs.LeadingTeam` изменён с `int` на правильный enum `LeadingTeam`
- Добавлено: `OriginalArgs` теперь проставляется во всех методах bridge
- Добавлено: событие `server.waiting` (`Server.WaitingForPlayers`)
- Добавлено: опция `Debug` в конфиге теперь реально логирует активность dispatch в консоль
- Добавлено: свойство `RoundEndedEventArgs.WinningTeamName` для удобства

### v1.0.0
- Первый публичный релиз

---

## Лицензия

[MIT](LICENSE)
