## Developer & Contributions

- Adiguna S Ligawan (Game Developer & Systems Designer)
  <br>

## About

Dark Abyss is a top-down dungeon crawler inspired by classic 90s action RPGs like Diablo. Navigate through dark catacombs filled with deadly traps and relentless enemies. Master real-time combat with strategic movement, collect powerful items, and upgrade your character to survive increasingly challenging encounters. Face terrifying bosses in atmospheric dungeons where every step could be your last.
<br>

## Key Features

**Real-Time Combat System**: Fast-paced combat with attack combos, dodging, and blocking mechanics that reward skillful play and tactical positioning.

** Enemy AI**:  state machine driven enemies that patrol, chase, and attacks, creating dynamic and unpredictable encounters.

**Progressive Upgrade System**: Collect items and upgrades throughout your journey to enhance your character's abilities and survivability in the depths.

**Atmospheric Dungeon Crawling**: Explore procedurally-enhanced catacombs with strategic lighting, environmental traps, and hidden secrets.

<br>

<table>
  <tr>
    <td align="left" width="50%">
      <img width="100%" alt="gif1" src="https://github.com/yourusername/dark-abyss/blob/main/gameplay1.gif">
    </td>
    <td align="right" width="50%">
      <img width="100%" alt="gif2" src="https://github.com/yourusername/dark-abyss/blob/main/gameplay2.gif">
    </td>
  </tr>
</table>

## Scene Flow 

```mermaid
flowchart LR
  mm[Main Menu]
  gp[Gameplay]
  inv[Inventory]
  death[Death Screen]
  victory[Victory Screen]

  mm -- "Start Game" --> gp
  gp -- "Open Inventory" --> inv
  inv -- "Close" --> gp
  gp -- "Player Death" --> death
  gp -- "Boss Defeated" --> victory
  death -- "Retry" --> gp
  death -- "Main Menu" --> mm
  victory -- "Main Menu" --> mm

```

## Layer / Module Design 

```mermaid
---
config:
  theme: neutral
  look: neo
---
graph TD
    subgraph "Game Initialization"
        Start([Game Start])
        Boot[Boot Layer]
        SaveCheck{Save Data<br/>Exists?}
    end
    subgraph "Main Menu System"
        MM[Main Menu]
        Settings[Settings Menu]
        CharSelect[Character Select]
    end
    subgraph "Gameplay Flow"
        GP[Gameplay Scene]
        Combat[Combat System]
        Inventory[Inventory System]
        Upgrades[Upgrade System]
    end
    subgraph "Enemy Systems"
        Spawner[Enemy Spawner]
        AI[Enemy AI]
        StateMachine[State Machine]
        Boss[Boss Encounters]
    end
    subgraph "End States"
        Death[Death Screen]
        Victory[Victory Screen]
        Stats[Statistics Display]
    end
    Start --> Boot
    Boot --> SaveCheck
    SaveCheck -->|No Save| MM
    SaveCheck -->|Has Save| CharSelect
    MM -->|Play| CharSelect
    MM -->|Settings| Settings
    Settings --> MM
    CharSelect --> GP
    GP --> Combat
    GP --> Inventory
    GP --> Upgrades
    Combat --> Spawner
    Spawner --> AI
    AI --> StateMachine
    GP -->|Boss Room| Boss
    Combat -->|Player Death| Death
    Boss -->|Victory| Victory
    Death -->|Retry| GP
    Death -->|Main Menu| MM
    Victory --> Stats
    Stats -->|Main Menu| MM
    classDef initStyle fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef menuStyle fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef gameplayStyle fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef enemyStyle fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef endStyle fill:#ffebee,stroke:#b71c1c,stroke-width:2px
    class Start,Boot,SaveCheck initStyle
    class MM,Settings,CharSelect menuStyle
    class GP,Combat,Inventory,Upgrades gameplayStyle
    class Spawner,AI,StateMachine,Boss enemyStyle
    class Death,Victory,Stats endStyle

```

## Modules and Features

A top-down dungeon crawler with real-time combat, strategic movement, and survival mechanics powered by a comprehensive enemy AI system that creates intense dungeon crawling experiences reminiscent of classic 90s action RPGs.

| 📂 Name | 🎬 Scene | 📋 Responsibility |
|---------|----------|-------------------|
| **Canvas** | **UI System** | - Render all UI elements<br/>- Handle UI scaling and positioning<br/>- Manage UI layers and sorting |
| **GameManager** | **Persistent** | - Manage game state and flow<br/>- Handle scene transitions<br/>- Track player progress and statistics |
| **EnemyController** | **Gameplay** | - Control enemy movement and behavior<br/>- Handle enemy combat mechanics<br/>- Process AI decision making |
| **EnemySpawner** | **Gameplay** | - Spawn enemies at designated points<br/>- Manage spawn rates and patterns<br/>- Control enemy population limits |
| **EnemyStateMachineBehaviours** | **Gameplay** | - Define enemy AI states (Idle, Patrol, Chase, Attack)<br/>- Handle state transitions<br/>- Execute state-specific behaviors |
| **HealthBar** | **UI/Gameplay** | - Display player and enemy health<br/>- Update health visuals in real-time<br/>- Show damage feedback |
| **IsometricPlayerController** | **Gameplay** | - Handle player movement in isometric view<br/>- Process combat inputs (attack, dodge, block)<br/>- Manage player physics and collisions |
| **ItemPickup** | **Gameplay** | - Handle item collection mechanics<br/>- Apply item effects to player<br/>- Manage item spawning and despawning |
| **Items** | **Gameplay** | - Store item data and properties<br/>- Define item types (weapons, potions, upgrades)<br/>- Handle item interactions |
| **PlayerHealth** | **Gameplay** | - Track player health and damage<br/>- Handle death and respawn logic<br/>- Manage healing and damage reduction |
| **PlayerUpgrades** | **Gameplay** | - Store and apply character upgrades<br/>- Calculate stat modifiers<br/>- Handle upgrade progression system |

<br>

## Game Flow Chart

```mermaid
---
config:
  theme: redux
  look: neo
---
flowchart TD
  start([Game Start])
  start --> explore{Player Action}
  explore -->|"Move"| move[Navigate Dungeon]
  explore -->|"Attack"| combat[Enter Combat]
  explore -->|"Item"| pickup[Collect Item]
  
  move --> enemy{Enemy Detected?}
  enemy -->|Yes| aiCheck[Check AI State]
  enemy -->|No| cont1[Continue Exploring]
  
  aiCheck --> state{Enemy State}
  state -->|Idle| patrol[Start Patrol]
  state -->|Patrol| detect[Detection Check]
  state -->|Chase| chase[Pursue Player]
  state -->|Attack| attack[Execute Attack]
  
  detect --> spotted{Player Spotted?}
  spotted -->|Yes| chase
  spotted -->|No| patrol
  
  combat --> damage[Calculate Damage]
  damage --> health{Health Check}
  health -->|Player Dead| death[Death Screen]
  health -->|Enemy Dead| loot[Drop Loot]
  health -->|Both Alive| combat
  
  pickup --> upgrade{Item Type}
  upgrade -->|Health| heal[Restore HP]
  upgrade -->|Weapon| equip[Equip Weapon]
  upgrade -->|Upgrade| boost[Apply Stat Boost]
  
  loot --> pickup
  heal --> cont2[Continue]
  equip --> cont2
  boost --> cont2
  
  cont1 --> boss{Boss Room?}
  cont2 --> boss
  boss -->|Yes| bossFight[Boss Battle]
  boss -->|No| explore
  
  bossFight --> bossHealth{Boss Defeated?}
  bossHealth -->|Yes| victory[Victory Screen]
  bossHealth -->|No| bossFight
  
  death --> retry{Retry?}
  retry -->|Yes| start
  retry -->|No| menu[Main Menu]
  victory --> menu

```

<br>

## Event Signal Diagram

```mermaid
classDiagram
    %% --- Core Combat ---
    class IsometricPlayerController {
        +OnMove(direction: Vector2)
        +OnAttack()
        +OnDodge()
        +OnBlock()
        +OnItemPickup(item: Item)
    }

    class EnemyController {
        +OnPlayerDetected()
        +OnPlayerLost()
        +OnTakeDamage(amount: float)
        +OnDeath()
    }

    class EnemyStateMachine {
        +OnStateEnter(state: string)
        +OnStateExit(state: string)
        +OnStateTransition(from: string, to: string)
    }

    class EnemySpawner {
        +OnSpawnEnemy(type: string)
        +OnWaveComplete()
        +OnAllEnemiesDefeated()
    }

    %% --- Systems ---
    class GameManager {
        +OnGameStart()
        +OnPlayerDeath()
        +OnBossDefeated()
        +OnLevelComplete()
    }

    class PlayerHealth {
        +OnHealthChanged(current: float, max: float)
        +OnDamaged(amount: float)
        +OnHealed(amount: float)
        +OnDeath()
    }

    class PlayerUpgrades {
        +OnUpgradeUnlocked(upgrade: string)
        +OnStatIncreased(stat: string, amount: float)
    }

    class ItemPickup {
        +OnItemCollected(item: Item)
        +OnItemSpawned(position: Vector2)
    }

    %% --- Relations ---
    IsometricPlayerController --> EnemyController : triggers
    EnemyController --> EnemyStateMachine : controls
    EnemySpawner --> EnemyController : creates
    GameManager --> EnemySpawner : manages
    IsometricPlayerController --> PlayerHealth : updates
    ItemPickup --> PlayerUpgrades : applies
    PlayerHealth --> GameManager : notifies

```

<br>




![Dark Abyss Demo](https://raw.githubusercontent.com/yourusername/dark-abyss/main/DarkAbyssSlide.png)
