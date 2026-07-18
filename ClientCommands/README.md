# ClientCommands

An SCP: Secret Laboratory **EXILED** plugin that adds a set of fun client-side console commands and an automatic candy distribution system.

---

## Requirements

| Requirement | Version |
|-------------|---------|
| SCP: Secret Laboratory | Latest |
| EXILED | **8.x** |
| .NET Framework | **4.8** |

---

## Features

### Client Commands

All commands are typed in the **client game console** (default key: `` ` ``/`~`) with a leading dot.

| Command | Description |
|---------|-------------|
| `.help` | Lists all available plugin commands. |
| `.kill` | Kills yourself with a visual explosion. No nearby damage. |
| `.suicide` | Alias for `.kill`. |
| `.roll` | Rolls 1–6. Roll a **6** to win a random candy! |
| `.coinflip` | Flip a coin — **Heads** or **Tails**. |
| `.msg <text>` | Shows your message above your head for 6 s. Max 60 chars, 10 s cooldown. |

### Automatic Candy

Every **300 seconds** all alive players automatically receive one random SCP:SL candy. A broadcast notification is shown to each recipient.

**Available candy types:** Red · Green · Blue · Purple · Rainbow · Yellow · Pink

---

## Installation

1. Build the project:
   ```
   dotnet build -c Release
   ```
2. Copy `bin/Release/net48/ClientCommands.dll` to your server's EXILED plugins folder:
   ```
   ~/.config/EXILED/Plugins/ClientCommands.dll
   ```
3. Start/restart the server. EXILED will generate a default config on first run.

---

## Configuration

The config is generated at `~/.config/EXILED/Configs/<port>-config.yml` under the `cc` key.

```yaml
cc:
  is_enabled: true
  debug: false

  candy:
    is_enabled: true
    interval: 300        # seconds between each candy handout
    give_on_roll_six: true

  message:
    duration: 6          # seconds the .msg text is visible above the head
    cooldown: 10         # per-player cooldown in seconds
    max_length: 60       # maximum characters (after stripping rich-text tags)
```

---

## Project Structure

```
ClientCommands/
├── Plugin.cs                 # Plugin entry point, lifecycle management
├── Config.cs                 # Typed config (IConfig + sub-configs)
├── EventHandlers.cs          # Event subscriptions, candy coroutine, shared helpers
├── Commands/
│   ├── HelpCommand.cs        # .help
│   ├── KillCommand.cs        # .kill
│   ├── SuicideCommand.cs     # .suicide
│   ├── RollCommand.cs        # .roll
│   ├── CoinFlipCommand.cs    # .coinflip
│   └── MessageCommand.cs     # .msg <text>
├── ClientCommands.csproj
└── README.md
```

---

## Notes

- The explosion created by `.kill` / `.suicide` is **purely visual** — it uses `ExplosionUtils.ServerSpawnEffect` which spawns the particle effect only; no `Grenade` or damage object is created, so nearby players take no damage.
- `.msg` works by temporarily appending the text to the sender's `DisplayNickname`, making it visible to all players who can see their nametag. The original nickname is restored automatically after the configured duration.
- The automatic candy coroutine starts with each round and is stopped and cleaned up on round end or plugin disable.
