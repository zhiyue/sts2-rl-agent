

# Agente RL de STS2

Un agente de aprendizaje por refuerzo para **Slay the Spire 2**, construido sobre un simulador de combate sin cabeza de alto rendimiento y un entorno de entrenamiento de Gymnasium. Incluye un mod de puente en C# para conectar el agente entrenado con el juego real.

## Arquitectura

```
+-----------------------------------------------------------------------+
|  Headless Python Simulator (sts2_env/)                                |
|                                                                       |
|  +----------------+  +----------------+  +---------------------------+|
|  | Core Engine    |  | Game Content   |  | Gym Environments          ||
|  | combat.py      |  | 577 cards      |  | combat_env.py  (single)  ||
|  | creature.py    |  | 260 powers     |  | run_env.py     (full run)||
|  | hooks.py       |  | 121 monsters   |  | observation.py (131-dim) ||
|  | damage.py      |  | 290 relics     |  | action_space.py(61/100)  ||
|  | rng.py         |  | 63 potions     |  | reward.py                ||
|  +-------+--------+  +-------+--------+  +-----------+--------------+|
|          |                    |                        |              |
|          +--------------------+------------------------+              |
+-----------------------------------------------------------------------+
           |                                             |
           v                                             v
+---------------------+                  +--------------------------+
| Training Pipeline   |                  | Bridge to Real Game      |
| MaskablePPO (SB3)   |                  | bridge_mod/ (C#/Godot)|
| train_combat.py     |------model------>| agent_runner.py (Python) |
| train_full_run.py   |                  | TCP JSON protocol        |
+---------------------+                  +--------------------------+
```

## Estadísticas del Proyecto

| Métrica | Valor |
|--------|-------|
| Archivos fuente | 133 Python + C# |
| Líneas de código | ~50.000 |
| Funciones de prueba | 408 |
| Cartas implementadas | 577 |
| Poderes implementados | 260 |
| Monstruos implementados | 121 |
| Reliquias implementadas | 290 |
| Pociones implementadas | 63 |
| Personajes jugables | 5 (Ironclad, Silent, Defect, Necrobinder, Regent) |
| Velocidad de simulación | ~1.200 combates/seg, ~28.000 pasos/seg |
| Tasa de victoria en combate (PPO entrenado) | ~92% (Acto 1 Ironclad) |

## Inicio Rápido

### Requisitos Previos

- **Python 3.11+** (se recomienda 3.12)
- **pip** (incluido con Python)
- Para el entrenamiento: se recomienda una GPU compatible con CUDA, pero no es obligatoria
- Para el puente al juego real: SDK de .NET 9, Godot 4.5.1 Mono, Slay the Spire 2 (Steam)

### Instalación

```bash
git clone <repo-url>
cd sts2-rl-agent

# Core simulator only
pip install -e .

# With training dependencies (PyTorch, SB3, sb3-contrib)
pip install -e ".[train]"

# With dev dependencies (pytest)
pip install -e ".[dev]"
```

### Ejecutar Benchmark

Mida el rendimiento de la simulación con acciones aleatorias:

```bash
python scripts/benchmark.py
```

Salida esperada en un CPU moderno:

```
Episodes:       1000
Total steps:    28101
Time:           0.78s
Episodes/sec:   1276
Steps/sec:      28101
```

### Jugar una Partida Completa en la Terminal

Comience desde el evento inicial, elija nodos del mapa, libere combates, reciba recompensas y continúe la partida desde la línea de comandos:

```bash
python -m sts2_env.cli.play_run
```

Opciones útiles:

```bash
python -m sts2_env.cli.play_run --character Silent --seed 123 --ascension 0
```

Sin `--character`, la CLI le pedirá que elija un personaje primero. Dentro del juego, ingrese el número junto a una acción. Atajos: `a` toma la primera acción listada, `c` confirma u omite, y `q` sale.

### Jugar una Partida Completa en el Navegador

Ejecute la interfaz web local:

```bash
python -m sts2_env.web.play_run --port 8765
```

Luego abra <http://127.0.0.1:8765/>. La interfaz del navegador utiliza la misma lógica de `RunManager` que la versión de terminal, pero presenta la partida como pantallas clicables para el mapa, combate, eventos, recompensas, tienda, sitios de descanso, tesoros y reliquias de jefes.

### Entrenar un Agente de Combate

Entrene un agente MaskablePPO en encuentros de combate individual:

```bash
python scripts/train_combat.py \
    --total-timesteps 500000 \
    --n-envs 4 \
    --output-dir output/combat_ppo
```

Indicadores principales:

| Indicador | Predeterminado | Descripción |
|------|---------|-------------|
| `--total-timesteps` | 500.000 | Pasos totales del entorno |
| `--n-envs` | 4 | Entornos paralelos (utiliza núcleos de CPU) |
| `--lr` | 3e-4 | Tasa de aprendizaje |
| `--batch-size` | 256 | Tamaño del minibatch |
| `--n-steps` | 2048 | Pasos por rollout por entorno |
| `--output-dir` | output/combat_ppo | Directorio para guardar modelos y registros |

### Entrenar un Agente de Partida Completa

Entrene un agente que gestione una partida completa (combate + mapa + recompensas + eventos):

```bash
python scripts/train_full_run.py \
    --total-timesteps 1000000 \
    --act-count 1 \
    --n-envs 4 \
    --output-dir output/run_ppo
```

El indicador `--act-count` controla cuántos actos por episodio (1 = solo Acto 1, 3 = juego completo).

### Conectar al Juego Real

Después del entrenamiento, ejecute el agente contra el juego real:

1. Compile e instale el mod de puente (consulte [docs/MOD_BUILD_GUIDE.md](docs/MOD_BUILD_GUIDE.md))
2. Inicie Slay the Spire 2
3. Ejecute el agente:

```bash
python -m sts2_env.bridge.agent_runner \
    --model-path output/combat_ppo/best_model/best_model.zip \
    --verbose
```

Consulte [docs/AGENT_USAGE_GUIDE.md](docs/AGENT_USAGE_GUIDE.md) para más detalles.

## Estructura del Proyecto

```
sts2-rl-agent/
|-- pyproject.toml                 # Package config, dependencies
|-- scripts/
|   |-- benchmark.py               # Throughput benchmark
|   |-- train_combat.py            # Combat-only training
|   +-- train_full_run.py          # Full-run training
|
|-- sts2_env/                      # Python package (headless simulator)
|   |-- core/                      # Combat engine
|   |   |-- combat.py              # CombatState (turn flow, card play)
|   |   |-- creature.py            # Creature (HP, block, powers)
|   |   |-- hooks.py               # Central hook dispatch (~25 hooks)
|   |   |-- damage.py              # Damage/block calculation pipelines
|   |   |-- enums.py               # CardId, PowerId, IntentType, etc.
|   |   |-- constants.py           # Game constants from decompiled source
|   |   +-- rng.py                 # Seeded RNG
|   |
|   |-- cards/                     # Card definitions (577 cards)
|   |   |-- base.py                # CardInstance class
|   |   |-- effects.py             # 12 composable effect primitives
|   |   |-- registry.py            # Card ID -> effect dispatch
|   |   |-- ironclad.py            # Ironclad cards
|   |   |-- silent.py              # Silent cards
|   |   |-- defect.py              # Defect cards
|   |   |-- necrobinder.py         # Necrobinder cards
|   |   |-- regent.py              # Regent cards
|   |   |-- colorless.py           # Colorless cards
|   |   +-- status.py              # Status/Curse cards
|   |
|   |-- powers/                    # Status effects (260 powers)
|   |   |-- base.py                # PowerInstance base class
|   |   |-- common.py              # Core powers (Strength, Vulnerable, etc.)
|   |   |-- damage_modifiers.py    # Damage pipeline hooks
|   |   |-- block_modifiers.py     # Block pipeline hooks
|   |   |-- card_play_effects.py   # On-card-play triggers
|   |   |-- damage_reactions.py    # Thorns, reactive powers
|   |   |-- duration.py            # Tick-down / duration powers
|   |   |-- turn_effects.py        # Start/end of turn triggers
|   |   +-- monster.py             # Monster-specific powers
|   |
|   |-- monsters/                  # Monster AI (121 monsters)
|   |   |-- state_machine.py       # MoveState, RandomBranch, ConditionalBranch
|   |   |-- intents.py             # Intent types
|   |   |-- act1_weak.py           # Act 1 weak encounters
|   |   |-- act1.py                # Act 1 monsters
|   |   |-- act2.py                # Act 2 monsters
|   |   |-- act3.py                # Act 3 monsters
|   |   +-- act4.py                # Act 4 monsters
|   |
|   |-- relics/                    # Relic effects (290 relics)
|   |-- potions/                   # Potion effects (63 potions)
|   |-- orbs/                      # Orb mechanics (Defect)
|   |-- characters/                # Character starting states
|   |-- encounters/                # Encounter definitions (88 encounters)
|   |-- events/                    # Event decision trees (68 events)
|   |-- map/                       # Map generation algorithm
|   |-- run/                       # Full-run state management
|   |   |-- run_manager.py         # Run loop (map -> room -> rewards)
|   |   |-- run_state.py           # Persistent run state
|   |   |-- rewards.py             # Card/gold/potion rewards
|   |   |-- shop.py                # Shop system
|   |   |-- rest_site.py           # Rest site (heal/upgrade)
|   |   +-- events.py              # Event handler
|   |
|   |-- gym_env/                   # Gymnasium environments
|   |   |-- combat_env.py          # Single-combat env (Discrete(61))
|   |   |-- run_env.py             # Full-run env (Discrete(100))
|   |   |-- observation.py         # State -> 131-dim float32 vector
|   |   |-- action_space.py        # Action encoding + masking
|   |   +-- reward.py              # Reward shaping
|   |
|   +-- bridge/                    # Real-game connection
|       |-- client.py              # TCP client
|       |-- protocol.py            # Message types, phases
|       |-- state_adapter.py       # Game JSON -> observation vector
|       +-- agent_runner.py        # Main agent loop
|
|-- bridge_mod/                    # C# Bridge Mod (Godot project)
|   |-- STS2BridgeMod.csproj       # Build config (Godot.NET.Sdk/4.5.1)
|   |-- MainFile.cs                # Entry point, Harmony patches
|   |-- BridgeServer.cs            # TCP server
|   |-- RlAutoSlayer.cs            # AutoSlay-based automation
|   |-- RlCombatHandler.cs         # Combat decision handler
|   |-- RlMapHandler.cs            # Map navigation handler
|   |-- RlCardSelector.cs          # Card selection handler
|   +-- RlCardRewardScreenHandler.cs
|
|-- tests/                         # 14 test files, 408 test functions
|-- docs/                          # Documentation
|   |-- GAME_BRIDGE_REFERENCE.md   # Bridge architecture and protocol
|   |-- AUTOSLAY_BRIDGE.md         # AutoSlay-based bridge design
|   |-- GAME_SYSTEMS_REFERENCE.md  # Game mechanics reference
|   |-- CARDS_REFERENCE.md         # All 577 cards
|   |-- POWERS_REFERENCE.md        # All 260 powers
|   |-- MONSTERS_REFERENCE.md      # All 121 monsters
|   +-- RELICS_REFERENCE.md        # All 290 relics
|
|-- RESEARCH.md                    # Research notes and prior work
+-- DECOMPILED_ARCHITECTURE.md     # Decompiled C# architecture analysis
```

## Cobertura de Contenido del Juego

| Tipo de Contenido | Total en el Juego | Implementado | Cobertura |
|-------------|-----------|-------------|----------|
| Cartas | 577 | 577 | 100% |
| Poderes (efectos de estado) | 260 | 260 | 100% |
| Monstruos | 121 | 121 | 100% |
| Reliquias | 290 | 290 | 100% |
| Pociones | 63 | 63 | 100% |
| Enfrentamientos | 88 | 88 | 100% |
| Eventos | 68 | 68 | 100% |
| Personajes | 5 + 2 | 5 | 100% (jugables) |
| Actos | 4 | 4 | 100% |

## Cómo Funciona

### Enfoque de Dos Fases

Siguiendo las lecciones de la comunidad de RL de STS1, este proyecto utiliza una estrategia de dos fases:

1. **Simulador sin cabeza** (para entrenamiento): Una reescritura en Python puro de las mecánicas de combate y partida de STS2, verificada contra el código C# descompilado. Ejecuta a ~1.200 combates/segundo — lo suficientemente rápido para millones de episodios de entrenamiento.

2. **Mod de puente** (para validación): Un mod en C# que se conecta al juego real a través de Harmony, expone el estado mediante TCP e inyecta las decisiones del agente. Incluye parches de velocidad de 5 a 10 veces para una evaluación más rápida en el juego real.

### Algoritmo de RL

- **MaskablePPO** de sb3-contrib (Stable Baselines 3)
- **Enmascaramiento de acciones inválidas**: En cada paso, el entorno proporciona una máscara booleana que indica qué acciones son legales (cartas jugables, objetivos válidos). Las acciones ilegales se ponen a cero antes de muestrear la política.
- **Observación**: Vector float32 de 131 dimensiones que codifica el estado del jugador, las cartas de la mano, los resúmenes de los mazos y el estado del enemigo.
- **Espacio de acción**: Discrete(61) para combate (fin de turno + 10 autoaplicado + 50 con objetivo), Discrete(100) para partida completa.

### Diseño de Recompensas

**Entorno de combate:**
- Victoria: +1.0
- Derrota: -1.0
- Pérdida de PV: pequeña penalización negativa (fomenta un juego eficiente)

**Entorno de partida completa:**
- Ganar la partida: +1.0
- Muerte: -1.0
- Modelado opcional: pequeñas bonificaciones por progreso de piso y finalización de acto.

## Documentación

| Documento | Descripción |
|----------|-------------|
| [README.md](README.md) | Este archivo |
| [RESEARCH.md](RESEARCH.md) | Notas de investigación, trabajos previos, selección de algoritmo |
| [DECOMPILED_ARCHITECTURE.md](DECOMPILED_ARCHITECTURE.md) | Análisis de C# descompilado para el simulador |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Guía de contribución, configuración de desarrollo, adición de contenido |
| [docs/SIMULATOR_ARCHITECTURE.md](docs/SIMULATOR_ARCHITECTURE.md) | Arquitectura interna del simulador Python |
| [docs/TRAINING_GUIDE.md](docs/TRAINING_GUIDE.md) | Guía integral de entrenamiento RL |
| [docs/PROTOCOL.md](docs/PROTOCOL.md) | Protocolo de comunicación del puente TCP |
| [docs/KNOWN_ISSUES.md](docs/KNOWN_ISSUES.md) | Problemas y limitaciones conocidos actuales |
| [docs/MOD_BUILD_GUIDE.md](docs/MOD_BUILD_GUIDE.md) | Cómo compilar e instalar el mod de puente |
| [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) | Problemas comunes y soluciones |
| [docs/GAME_BRIDGE_REFERENCE.md](docs/GAME_BRIDGE_REFERENCE.md) | Arquitectura del puente y notas de diseño |
| [docs/AUTOSLAY_BRIDGE.md](docs/AUTOSLAY_BRIDGE.md) | Diseño del puente basado en AutoSlay |
| [docs/GAME_SYSTEMS_REFERENCE.md](docs/GAME_SYSTEMS_REFERENCE.md) | Referencia de mecánicas del juego |

## Licencia

Este proyecto es con fines de investigación y educativos. Slay the Spire 2 es propiedad de Mega Crit Games.

## Reconocimientos

- [decapitate-the-spire](https://github.com/jahabrewer/decapitate-the-spire) -- Simulador sin cabeza de STS1, inspiración arquitectónica
- [spire-codex](https://github.com/ptrlrd/spire-codex) -- Pipeline de extracción de datos de STS2
- [CommunicationMod](https://github.com/ForgottenArbiter/CommunicationMod) -- Diseño del protocolo de puente al juego de STS1
- [BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) -- Marco de mods de STS2
- [Stable Baselines 3](https://github.com/DLR-RM/stable-baselines3) -- Marco de entrenamiento RL
