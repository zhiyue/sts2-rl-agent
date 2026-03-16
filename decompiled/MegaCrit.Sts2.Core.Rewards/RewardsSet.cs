using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Rewards;

public class RewardsSet
{
	private bool _allowEmptyRewards;

	public AbstractRoom? Room { get; private set; }

	public Player Player { get; }

	public List<Reward> Rewards { get; } = new List<Reward>();

	public RewardsSet(Player player)
	{
		Player = player;
	}

	public RewardsSet EmptyForRoom(AbstractRoom room)
	{
		Room = room;
		_allowEmptyRewards = true;
		return this;
	}

	public RewardsSet WithRewardsFromRoom(AbstractRoom room)
	{
		Room = room;
		if (room.RoomType == RoomType.Boss && Player.RunState.CurrentActIndex >= Player.RunState.Acts.Count - 1)
		{
			return this;
		}
		if (!TryGenerateTutorialRewards(Player, room))
		{
			Rewards.AddRange(GenerateRewardsFor(Player, room));
		}
		if (Room is CombatRoom combatRoom && combatRoom.ExtraRewards.TryGetValue(Player, out List<Reward> value))
		{
			Rewards.AddRange(value);
		}
		return this;
	}

	public RewardsSet WithCustomRewards(List<Reward> rewards)
	{
		Rewards.AddRange(rewards);
		return this;
	}

	public async Task<List<Reward>> GenerateWithoutOffering()
	{
		List<Reward> initialRewards = Rewards.ToList();
		foreach (Reward reward in Rewards)
		{
			await reward.Populate();
		}
		IEnumerable<AbstractModel> modifiers = Hook.ModifyRewards(Player.RunState, Player, Rewards, Room);
		foreach (Reward item in Rewards.Except(initialRewards))
		{
			if (!item.IsPopulated)
			{
				await item.Populate();
			}
		}
		await Hook.AfterModifyingRewards(Player.RunState, modifiers);
		Rewards.Sort((Reward x, Reward y) => x.RewardsSetIndex.CompareTo(y.RewardsSetIndex));
		return Rewards;
	}

	public async Task Offer()
	{
		if (Player.Creature.IsDead)
		{
			return;
		}
		await GenerateWithoutOffering();
		bool isTerminal = Room is CombatRoom;
		if (Rewards.Count <= 0 && !isTerminal && !_allowEmptyRewards)
		{
			return;
		}
		await Hook.BeforeRewardsOffered(Player.RunState, Player, Rewards);
		if (!Rewards.All((Reward r) => r.IsPopulated) && Rewards.Any((Reward r) => r.IsPopulated))
		{
			Log.Warn("Some rewards are populated and others are not when calling RewardsCmd.Offer! This might lead to hooks getting called twice");
		}
		if (!LocalContext.IsMe(Player))
		{
			return;
		}
		if (TestMode.IsOn)
		{
			foreach (Reward reward in Rewards)
			{
				await reward.OnSelectWrapper();
			}
		}
		else
		{
			NRewardsScreen nRewardsScreen = NRewardsScreen.ShowScreen(isTerminal, Player.RunState);
			nRewardsScreen.SetRewards(Rewards);
			await nRewardsScreen.ClosedTask;
		}
	}

	private List<Reward> GenerateRewardsFor(Player player, AbstractRoom room)
	{
		if (RunManager.Instance == null)
		{
			throw new InvalidOperationException("Only valid during a run.");
		}
		List<Reward> list = new List<Reward>();
		if (!(room is CombatRoom combatRoom))
		{
			if (!(room is TreasureRoom))
			{
				throw new InvalidOperationException("Tried to generate a reward for invalid room type: " + room.GetType().Name);
			}
		}
		else
		{
			switch (room.RoomType)
			{
			case RoomType.Monster:
				if (combatRoom.GoldProportion > 0f)
				{
					list.Add(new GoldReward((int)Math.Round((float)combatRoom.Encounter.MinGoldReward * combatRoom.GoldProportion), (int)Math.Round((float)combatRoom.Encounter.MaxGoldReward * combatRoom.GoldProportion), player));
				}
				RollForPotionAndAddTo(list, player, room.RoomType);
				list.Add(new CardReward(CardCreationOptions.ForRoom(player, room.RoomType), 3, player));
				break;
			case RoomType.Elite:
				list.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
				RollForPotionAndAddTo(list, player, room.RoomType);
				list.Add(new CardReward(CardCreationOptions.ForRoom(player, room.RoomType), 3, player));
				list.Add(new RelicReward(player));
				break;
			case RoomType.Boss:
				list.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
				RollForPotionAndAddTo(list, player, room.RoomType);
				list.Add(new CardReward(CardCreationOptions.ForRoom(player, room.RoomType), 3, player));
				break;
			}
		}
		return list;
	}

	private void RollForPotionAndAddTo(ICollection<Reward> rewards, Player player, RoomType roomType)
	{
		PotionRewardOdds potionReward = player.PlayerOdds.PotionReward;
		AscensionManager ascensionManager = RunManager.Instance.AscensionManager;
		if (potionReward.Roll(player, ascensionManager, roomType))
		{
			rewards.Add(new PotionReward(player));
		}
	}

	private bool TryGenerateTutorialRewards(Player player, AbstractRoom room)
	{
		if (player.UnlockState.NumberOfRuns == 0 && player.UnlockState.EpochUnlockCount() == 0 && player.Character is Ironclad && room is CombatRoom combatRoom)
		{
			int num = player.RunState.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> p) => p).Count((MapPointHistoryEntry e) => e.Rooms.FindIndex((MapPointRoomHistoryEntry r) => r.RoomType == RoomType.Monster) >= 0);
			if (room.RoomType == RoomType.Monster && num <= 7)
			{
				CardModel[][] array = new CardModel[7][]
				{
					new CardModel[3]
					{
						player.RunState.CreateCard<SetupStrike>(player),
						player.RunState.CreateCard<Tremble>(player),
						player.RunState.CreateCard<BloodWall>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<Breakthrough>(player),
						player.RunState.CreateCard<Inflame>(player),
						player.RunState.CreateCard<Anger>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<IronWave>(player),
						player.RunState.CreateCard<Dismantle>(player),
						player.RunState.CreateCard<Cinder>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<Stomp>(player),
						player.RunState.CreateCard<ShrugItOff>(player),
						player.RunState.CreateCard<Armaments>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<Thunderclap>(player),
						player.RunState.CreateCard<SetupStrike>(player),
						player.RunState.CreateCard<Rage>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<BattleTrance>(player),
						player.RunState.CreateCard<TrueGrit>(player),
						player.RunState.CreateCard<Uppercut>(player)
					},
					new CardModel[3]
					{
						player.RunState.CreateCard<Bloodletting>(player),
						player.RunState.CreateCard<Whirlwind>(player),
						player.RunState.CreateCard<Tremble>(player)
					}
				};
				Rewards.Add(new GoldReward(10, 20, player));
				PotionModel potionModel = num switch
				{
					3 => ModelDb.Potion<FirePotion>().ToMutable(), 
					5 => ModelDb.Potion<StrengthPotion>().ToMutable(), 
					7 => ModelDb.Potion<EnergyPotion>().ToMutable(), 
					_ => null, 
				};
				if (potionModel != null)
				{
					Rewards.Add(new PotionReward(potionModel, player));
				}
				Rewards.Add(new CardReward(array[num - 1], CardCreationSource.Encounter, player));
				return true;
			}
			if (room.RoomType == RoomType.Elite)
			{
				switch (player.RunState.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> l) => l).Count((MapPointHistoryEntry e) => e.MapPointType == MapPointType.Elite))
				{
				case 1:
				{
					CardModel[] cardsToOffer2 = new CardModel[3]
					{
						player.RunState.CreateCard<Bludgeon>(player),
						player.RunState.CreateCard<Pyre>(player),
						player.RunState.CreateCard<EvilEye>(player)
					};
					Rewards.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
					Rewards.Add(new PotionReward(ModelDb.Potion<BlockPotion>().ToMutable(), player));
					Rewards.Add(new RelicReward(ModelDb.Relic<Vajra>().ToMutable(), player));
					Rewards.Add(new CardReward(cardsToOffer2, CardCreationSource.Encounter, player));
					return true;
				}
				case 2:
				{
					CardModel[] cardsToOffer = new CardModel[3]
					{
						player.RunState.CreateCard<Pillage>(player),
						player.RunState.CreateCard<Rampage>(player),
						player.RunState.CreateCard<FlameBarrier>(player)
					};
					Rewards.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
					Rewards.Add(new RelicReward(ModelDb.Relic<OrnamentalFan>().ToMutable(), player));
					Rewards.Add(new CardReward(cardsToOffer, CardCreationSource.Encounter, player));
					return true;
				}
				}
			}
			else if (room.RoomType == RoomType.Boss && player.RunState.MapPointHistory.SelectMany((IReadOnlyList<MapPointHistoryEntry> l) => l).Count((MapPointHistoryEntry e) => e.MapPointType == MapPointType.Boss) == 1)
			{
				CardModel[] cardsToOffer3 = new CardModel[3]
				{
					player.RunState.CreateCard<PrimalForce>(player),
					player.RunState.CreateCard<DemonForm>(player),
					player.RunState.CreateCard<Thrash>(player)
				};
				Rewards.Add(new GoldReward(combatRoom.Encounter.MinGoldReward, combatRoom.Encounter.MaxGoldReward, player));
				Rewards.Add(new CardReward(cardsToOffer3, CardCreationSource.Encounter, player));
				return true;
			}
		}
		return false;
	}
}
