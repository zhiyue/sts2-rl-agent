# AutoSlay Bridge — 基于游戏内置自动化系统的 RL Agent 接入方案

## 发现

STS2 内置了 `AutoSlay` 系统（`MegaCrit.Sts2.Core.AutoSlay`），一个完整的自动化游玩框架，
可以自动完成整局 run。它被 `IsReleaseGame()` 守卫阻止在发布版中使用。

## 新方案 vs 旧方案

| | 旧方案 (TCP + JSON) | 新方案 (AutoSlay Hook) |
|---|---|---|
| 状态序列化 | 手写 JSON 序列化 | 直接访问游戏对象 |
| 动作注入 | 反射 + CallDeferred | 直接调用 CardCmd/PlayerCmd |
| UI 处理 | 手动处理所有界面 | AutoSlay 已处理所有界面 |
| 错误恢复 | 手写 | AutoSlay 的 Watchdog |
| 代码量 | ~2500 行 C# | ~200 行 C# |

## 实现步骤

### Step 1: Patch IsReleaseGame
```csharp
[HarmonyPatch(typeof(NGame), nameof(NGame.IsReleaseGame))]
static class UnlockAutoSlay
{
    static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}
```

### Step 2: 替换 CombatRoomHandler
替换随机打牌为 RL agent 决策：
```csharp
public class RlCombatHandler : IRoomHandler
{
    // 接收 CombatState，通过 TCP 发给 Python agent
    // Python 返回 card_index + target_index
    // 调用 CardCmd.AutoPlay(ctx, card, target)
}
```

### Step 3: 实现 ICardSelector
替换随机选牌为 RL agent 决策：
```csharp
public class RlCardSelector : ICardSelector
{
    // 所有选牌决策（奖励、升级、变换等）通过 TCP 发给 Python
    Task<IEnumerable<CardModel>> GetSelectedCards(...)
    CardModel? GetSelectedCardReward(...)
}
```

### Step 4: 替换 MapScreenHandler
替换固定选路为 RL agent 决策。

## 关键 API

| API | 用途 |
|---|---|
| `CardCmd.AutoPlay(ctx, card, target)` | 打牌（绕过 UI） |
| `PlayerCmd.EndTurn(player, false)` | 结束回合 |
| `potionModel.EnqueueManualUse(target)` | 使用药水 |
| `CardSelectCmd.UseSelector(selector)` | 注入选牌逻辑 |
| `UiHelper.Click(button)` | 点击 UI 按钮 |
| `WaitHelper.Until(condition, ct)` | 等待条件满足 |
| `RunManager.Instance.DebugOnlyGetState()` | 获取 RunState |
| `CombatManager.Instance` | 获取战斗状态 |
| `NOverlayStack.Instance.Peek()` | 当前界面 |

## 线程安全
所有操作必须在 Godot 主线程。AutoSlay 用 `TaskHelper.RunSafely()` 运行 async Task。

## 启动方式
Mod 加载后，在主菜单自动启动 AutoSlayer，替换其决策 handler 为 RL agent。
或者通过 TCP 信号触发启动。
