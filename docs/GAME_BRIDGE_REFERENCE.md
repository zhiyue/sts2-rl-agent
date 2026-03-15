# STS2 真实游戏桥接 — 技术方案

## 概述

将训练好的 RL agent 连接到真实 STS2 游戏，需要两个组件：
1. **C# Mod** (游戏侧) — 通过 Harmony hook 暴露游戏状态，接收动作指令
2. **Python 客户端** (Agent 侧) — 连接 Mod，运行模型推理

## 架构

```
STS2 游戏进程 (Godot + .NET 8)          Python Agent 进程
┌─────────────────────────────┐   TCP   ┌──────────────────┐
│  STS2BridgeMod (.dll)       │◄───────►│  sts2_client.py  │
│                             │  JSON   │                  │
│  ┌───────────────────────┐  │         │  ┌────────────┐  │
│  │ TcpListener (端口9002) │  │ ──状态─►│  │ 状态解析   │  │
│  │ 后台线程              │  │         │  │ → 观测向量  │  │
│  └───────────────────────┘  │         │  └────────────┘  │
│                             │         │        ↓         │
│  ┌───────────────────────┐  │         │  ┌────────────┐  │
│  │ Harmony Hooks:        │  │         │  │ MaskablePPO│  │
│  │  CombatManager        │  │         │  │ 模型推理   │  │
│  │  PlayCardAction       │  │         │  └────────────┘  │
│  │  ActionQueueSet       │  │         │        ↓         │
│  └───────────────────────┘  │         │  ┌────────────┐  │
│                             │ ◄─动作──│  │ 动作编码   │  │
│  ┌───────────────────────┐  │         │  │ → JSON cmd │  │
│  │ Superfast Patches:    │  │         │  └────────────┘  │
│  │  3-10x 动画加速       │  │         │                  │
│  │  跳过等待             │  │         │                  │
│  └───────────────────────┘  │         │                  │
└─────────────────────────────┘         └──────────────────┘
```

## 第一部分: C# Mod

### 项目配置

参考 [lamali292/sts2_example_mod](https://github.com/lamali292/sts2_example_mod):

```xml
<!-- STS2BridgeMod.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <PlatformTarget>x64</PlatformTarget>
  </PropertyGroup>
  <ItemGroup>
    <!-- 从游戏 data_sts2_windows_x86_64/ 目录引用 -->
    <Reference Include="0Harmony">
      <HintPath>$(STS2GamePath)/data_sts2_windows_x86_64/0Harmony.dll</HintPath>
    </Reference>
    <Reference Include="GodotSharp">
      <HintPath>$(STS2GamePath)/data_sts2_windows_x86_64/GodotSharp.dll</HintPath>
    </Reference>
    <Reference Include="sts2">
      <HintPath>$(STS2GamePath)/data_sts2_windows_x86_64/sts2.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

### Mod 入口

```csharp
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("Initialize")]
public class BridgeMod
{
    public static void Initialize()
    {
        var harmony = new Harmony("sts2.bridge.rl");
        harmony.PatchAll();

        // 启动 TCP 服务器
        BridgeServer.Instance.Start(port: 9002);
        Logger.Log("STS2 Bridge Mod initialized on port 9002");
    }
}
```

### TCP 服务器

```csharp
// 后台线程运行 TcpListener
public class BridgeServer
{
    private TcpListener _listener;
    private TcpClient _client;
    private bool _running;

    public void Start(int port)
    {
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.Start();
        _running = true;
        Task.Run(AcceptLoop);
    }

    private async Task AcceptLoop()
    {
        while (_running)
        {
            _client = await _listener.AcceptTcpClientAsync();
            await HandleClient(_client);
        }
    }
}
```

### 状态序列化 (当 ActionQueue idle 时)

关键: 只在游戏等待玩家输入时发送状态。

```csharp
// 需要 hook 的关键点:
// 1. CombatManager — 回合开始/结束
// 2. ActionQueueSet — 队列空闲时读取状态
// 3. PlayCardAction — 注入打牌动作

// 状态 JSON 格式 (参考 STS1 CommunicationMod):
{
    "type": "game_state",
    "phase": "COMBAT_PLAY",  // COMBAT_PLAY, COMBAT_END_TURN, MAP, EVENT, SHOP, REST, CARD_REWARD, ...
    "combat_state": {
        "player": {
            "hp": 70, "max_hp": 80, "block": 5,
            "energy": 3, "max_energy": 3,
            "powers": [{"id": "STRENGTH", "amount": 2}]
        },
        "hand": [
            {"id": "STRIKE_IRONCLAD", "cost": 1, "type": "Attack", "target": "AnyEnemy", "upgraded": false}
        ],
        "draw_pile_count": 5,
        "discard_pile_count": 2,
        "exhaust_pile_count": 0,
        "enemies": [
            {"id": "NIBBIT", "hp": 35, "max_hp": 44, "block": 0,
             "intent": "ATTACK", "intent_damage": 12, "intent_hits": 1,
             "powers": []}
        ],
        "round": 2
    },
    "available_actions": ["PLAY", "END_TURN", "POTION"],
    "run_state": {
        "floor": 5, "act": 1, "gold": 120,
        "deck": [...], "relics": [...], "potions": [...]
    }
}
```

### 动作注入

```csharp
// 从 TCP 接收动作后，通过 CallDeferred 回到主线程执行
public void HandleAction(string actionJson)
{
    var action = JsonConvert.DeserializeObject<BridgeAction>(actionJson);

    // 必须在主线程执行游戏动作
    Godot.Callable.From(() => {
        switch (action.Type)
        {
            case "PLAY":
                // 模拟打牌: 找到手牌, 找到目标, 创建 PlayCardAction
                InjectPlayCard(action.CardIndex, action.TargetIndex);
                break;
            case "END_TURN":
                // 模拟结束回合
                InjectEndTurn();
                break;
            case "CHOOSE":
                // 非战斗选择 (卡牌奖励, 事件选项, 地图节点等)
                InjectChoice(action.ChoiceIndex);
                break;
            case "POTION":
                InjectPotionUse(action.PotionSlot, action.TargetIndex);
                break;
        }
    }).CallDeferred();
}
```

### Superfast 加速 (集成 STS2_Superfast_Mod 方案)

```csharp
// Hook Cmd.CustomScaledWait 减少所有等待时间
[HarmonyPatch]
class SpeedPatch
{
    [HarmonyPatch(typeof(Cmd), "CustomScaledWait")]
    [HarmonyPrefix]
    static void Prefix(ref float fastSeconds, ref float standardSeconds)
    {
        fastSeconds *= 0.1f;    // 10x 加速
        standardSeconds *= 0.1f;
    }
}

// Hook Spine 动画速度
[HarmonyPatch]
class AnimSpeedPatch
{
    [HarmonyPatch(typeof(MegaAnimationState), "SetTimeScale")]
    [HarmonyPrefix]
    static void Prefix(ref float timeScale)
    {
        timeScale *= 5.0f;  // 5x 动画速度
    }
}
```

### Mod 安装

```
Slay the Spire 2/
└── mods/
    └── STS2BridgeMod/
        ├── STS2BridgeMod.dll      # 编译后的 mod
        ├── STS2BridgeMod.pck      # Godot 资源包 (最小化)
        └── mod_manifest.json      # {"pck_name":"STS2BridgeMod","name":"STS2 Bridge"}
```

## 第二部分: Python 客户端

### 连接与通信

```python
# sts2_client.py
import socket
import json

class STS2GameClient:
    def __init__(self, host="127.0.0.1", port=9002):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.connect((host, port))
        self.buffer = b""

    def receive_state(self) -> dict:
        """接收游戏状态 JSON"""
        while b"\n" not in self.buffer:
            self.buffer += self.sock.recv(4096)
        line, self.buffer = self.buffer.split(b"\n", 1)
        return json.loads(line)

    def send_action(self, action: dict):
        """发送动作指令"""
        self.sock.sendall(json.dumps(action).encode() + b"\n")

    def play_card(self, card_index: int, target_index: int = -1):
        self.send_action({"type": "PLAY", "card_index": card_index, "target_index": target_index})

    def end_turn(self):
        self.send_action({"type": "END_TURN"})

    def choose(self, choice_index: int):
        self.send_action({"type": "CHOOSE", "choice_index": choice_index})

    def use_potion(self, slot: int, target_index: int = -1):
        self.send_action({"type": "POTION", "slot": slot, "target_index": target_index})
```

### Agent 运行循环

```python
# run_agent.py
from sb3_contrib import MaskablePPO
from sts2_client import STS2GameClient
from sts2_env.gym_env.observation import encode_observation
from sts2_env.gym_env.action_space import decode_action, compute_action_mask

def run_agent(model_path: str, host: str = "127.0.0.1", port: int = 9002):
    model = MaskablePPO.load(model_path)
    client = STS2GameClient(host, port)

    while True:
        state = client.receive_state()

        if state["phase"] == "COMBAT_PLAY":
            # 战斗中: 用训练好的模型决策
            obs = encode_observation(state["combat_state"])
            mask = compute_action_mask(state["combat_state"])
            action, _ = model.predict(obs, action_masks=mask, deterministic=True)
            decoded = decode_action(int(action), state["combat_state"])

            if decoded["type"] == "end_turn":
                client.end_turn()
            else:
                client.play_card(decoded["card_index"], decoded.get("target_index", -1))

        elif state["phase"] == "CARD_REWARD":
            # 卡牌奖励: 暂用简单启发式
            client.choose(pick_best_card(state))

        elif state["phase"] == "MAP":
            # 地图选路: 暂用简单启发式
            client.choose(pick_map_node(state))

        elif state["phase"] in ("EVENT", "SHOP", "REST"):
            # 其他: 暂用简单启发式
            client.choose(0)  # 默认第一个选项
```

## 第三部分: 实现步骤

### Phase 1: 最小可用 (纯战斗)
1. 创建 C# mod 项目 (参考 sts2_example_mod)
2. 实现 TCP 服务器
3. Hook CombatManager, 序列化战斗状态
4. 实现打牌/结束回合的动作注入
5. Python 客户端 + 已训练的 combat 模型
6. **验证**: agent 能在真实游戏中打一场战斗

### Phase 2: 全流程
1. 扩展状态序列化 (地图/事件/商店/篝火/卡牌奖励)
2. 扩展动作注入 (地图选择/购买/篝火选项)
3. 训练全流程 RunEnv 模型
4. **验证**: agent 能完成一整局 run

### Phase 3: 高性能
1. 集成 Superfast 加速 (3-10x)
2. 测试 --headless 模式
3. 批量运行收集数据
4. 在真实游戏数据上 fine-tune 模型

## 关键参考项目

| 项目 | 说明 | 链接 |
|------|------|------|
| sts2_example_mod | STS2 Mod 项目模板 | [GitHub](https://github.com/lamali292/sts2_example_mod) |
| STS2_Superfast_Mod | 加速 Mod (Harmony patches) | [GitHub](https://github.com/jidon333/STS2_Superfast_Mod) |
| QuickRestart | 状态操作参考 | [GitHub](https://github.com/freude916/sts2-quickRestart) |
| CommunicationMod (STS1) | 协议设计参考 | [GitHub](https://github.com/ForgottenArbiter/CommunicationMod) |
| spirecomm (STS1) | Python 客户端参考 | [GitHub](https://github.com/ForgottenArbiter/spirecomm) |
| TelnetTheSpire (STS1) | TCP 方案参考 | [GitHub](https://github.com/cdaymand/TelnetTheSpire) |
| UndoAndRedo | 状态快照参考 | [NexusMods](https://www.nexusmods.com/slaythespire2/mods/16) |
| BaseLib-StS2 | Mod 基础库 | [GitHub](https://github.com/Alchyr/BaseLib-StS2) |

## 注意事项

1. **线程安全**: 游戏状态读取和动作注入必须在 Godot 主线程，TCP 通信在后台线程
2. **稳定性检测**: 只在 ActionQueue idle 时读取状态和注入动作
3. **Modded 存档隔离**: 加 mod 后使用独立存档，不影响正常游戏
4. **Mac ARM64**: 自带的 0Harmony.dll 有 bug，需要替换修补版
5. **游戏更新**: STS2 在 Early Access，每次更新可能需要重新适配 Harmony patches
