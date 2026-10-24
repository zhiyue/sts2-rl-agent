using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Rngs;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Entities.Multiplayer;

public class NetFullCombatState : IPacketSerializable
{
	public struct CreatureState : IPacketSerializable
	{
		public ModelId? monsterId;

		public ulong? playerId;

		public int currentHp;

		public int maxHp;

		public int block;

		public List<PowerState> powers;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteBool(monsterId != null);
			if (monsterId != null)
			{
				writer.WriteModelEntry(monsterId);
			}
			writer.WriteBool(playerId.HasValue);
			if (playerId.HasValue)
			{
				writer.WriteULong(playerId.Value);
			}
			writer.WriteInt(currentHp);
			writer.WriteInt(maxHp);
			writer.WriteInt(block);
			writer.WriteList(powers);
		}

		public void Deserialize(PacketReader reader)
		{
			if (reader.ReadBool())
			{
				monsterId = reader.ReadModelIdAssumingType<MonsterModel>();
			}
			if (reader.ReadBool())
			{
				playerId = reader.ReadULong();
			}
			currentHp = reader.ReadInt();
			maxHp = reader.ReadInt();
			block = reader.ReadInt();
			powers = reader.ReadList<PowerState>();
		}

		public CreatureState Anonymized()
		{
			CreatureState result = this;
			result.playerId = (playerId.HasValue ? new ulong?(IdAnonymizer.Anonymize(playerId.Value)) : ((ulong?)null));
			return result;
		}
	}

	public struct PowerState : IPacketSerializable
	{
		public ModelId id;

		public int amount;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteModelEntry(id);
			writer.WriteInt(amount);
		}

		public void Deserialize(PacketReader reader)
		{
			id = reader.ReadModelIdAssumingType<PowerModel>();
			amount = reader.ReadInt();
		}
	}

	public struct OrbState : IPacketSerializable
	{
		public ModelId id;

		public int passive;

		public int evoke;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteModelEntry(id);
			writer.WriteInt(passive, 16);
			writer.WriteInt(evoke, 16);
		}

		public void Deserialize(PacketReader reader)
		{
			id = reader.ReadModelIdAssumingType<OrbModel>();
			passive = reader.ReadInt(16);
			evoke = reader.ReadInt(16);
		}

		public static OrbState From(OrbModel orb)
		{
			return new OrbState
			{
				id = orb.Id,
				passive = (int)orb.PassiveVal,
				evoke = (int)orb.EvokeVal
			};
		}
	}

	/// <summary>
	/// All the combat-relevant state from the Player class.
	/// Includes most state except:
	///  - Rewards and Shop RNG counters (stripped out in FromRun)
	///  - Rarity odds
	/// </summary>
	public struct PlayerState : IPacketSerializable
	{
		public ulong playerId;

		public ModelId characterId;

		public int turnNumber;

		public PlayerTurnPhase phase;

		public int energy;

		public int stars;

		public int maxPotionCount;

		public int gold;

		public List<CombatPileState> piles;

		public List<PotionState> potions;

		public List<RelicState> relics;

		public List<OrbState> orbs;

		public SerializablePlayerRngSet rngSet;

		public SerializableRelicGrabBag relicGrabBag;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteULong(playerId);
			writer.WriteModelEntry(characterId);
			writer.WriteInt(turnNumber);
			writer.WriteEnum(phase);
			writer.WriteInt(energy);
			writer.WriteInt(stars);
			writer.WriteInt(maxPotionCount);
			writer.WriteInt(gold);
			writer.WriteList(piles);
			writer.WriteList(potions);
			writer.WriteList(relics);
			writer.WriteList(orbs);
			writer.Write(rngSet);
			writer.Write(relicGrabBag);
		}

		public void Deserialize(PacketReader reader)
		{
			playerId = reader.ReadULong();
			characterId = reader.ReadModelIdAssumingType<CharacterModel>();
			turnNumber = reader.ReadInt();
			phase = reader.ReadEnum<PlayerTurnPhase>();
			energy = reader.ReadInt();
			stars = reader.ReadInt();
			maxPotionCount = reader.ReadInt();
			gold = reader.ReadInt();
			piles = reader.ReadList<CombatPileState>();
			potions = reader.ReadList<PotionState>();
			relics = reader.ReadList<RelicState>();
			orbs = reader.ReadList<OrbState>();
			rngSet = reader.Read<SerializablePlayerRngSet>();
			relicGrabBag = reader.Read<SerializableRelicGrabBag>();
		}

		public PlayerState Anonymized()
		{
			PlayerState result = this;
			result.playerId = IdAnonymizer.Anonymize(playerId);
			return result;
		}
	}

	public struct CombatPileState : IPacketSerializable
	{
		public PileType pileType;

		public List<CardState> cards;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteInt((int)pileType);
			writer.WriteList(cards);
		}

		public void Deserialize(PacketReader reader)
		{
			pileType = (PileType)reader.ReadInt();
			cards = reader.ReadList<CardState>();
		}

		public static CombatPileState From(CardPile pile)
		{
			CombatPileState result = new CombatPileState
			{
				pileType = pile.Type,
				cards = new List<CardState>()
			};
			foreach (CardModel card in pile.Cards)
			{
				result.cards.Add(CardState.From(card));
			}
			return result;
		}
	}

	public struct CardState : IPacketSerializable
	{
		public SerializableCard card;

		public ModelId? affliction;

		public int afflictionCount;

		public int? energyCost;

		public List<CardKeyword>? keywords;

		public void Serialize(PacketWriter writer)
		{
			writer.Write(card);
			writer.WriteBool(affliction != null);
			if (affliction != null)
			{
				writer.WriteModelEntry(affliction);
				writer.WriteInt(afflictionCount);
			}
			writer.WriteBool(energyCost.HasValue);
			if (energyCost.HasValue)
			{
				writer.WriteInt(energyCost.Value);
			}
			writer.WriteBool(keywords != null);
			if (keywords == null)
			{
				return;
			}
			writer.WriteInt(keywords.Count, 3);
			foreach (CardKeyword keyword in keywords)
			{
				writer.WriteEnum(keyword);
			}
		}

		public void Deserialize(PacketReader reader)
		{
			card = reader.Read<SerializableCard>();
			if (reader.ReadBool())
			{
				affliction = reader.ReadModelIdAssumingType<AfflictionModel>();
				afflictionCount = reader.ReadInt();
			}
			if (reader.ReadBool())
			{
				energyCost = reader.ReadInt();
			}
			if (reader.ReadBool())
			{
				keywords = new List<CardKeyword>();
				int num = reader.ReadInt(3);
				for (int i = 0; i < num; i++)
				{
					keywords.Add(reader.ReadEnum<CardKeyword>());
				}
			}
		}

		public static CardState From(CardModel card)
		{
			CardState result = new CardState
			{
				card = card.ToSerializable(),
				affliction = card.Affliction?.Id,
				afflictionCount = (card.Affliction?.Amount ?? 0)
			};
			IReadOnlySet<CardKeyword> readOnlySet = card.Keywords;
			if (readOnlySet.Count > 0)
			{
				result.keywords = new List<CardKeyword>();
				CardKeyword[] values = Enum.GetValues<CardKeyword>();
				foreach (CardKeyword item in values)
				{
					if (readOnlySet.Contains(item))
					{
						result.keywords.Add(item);
					}
				}
			}
			else
			{
				result.keywords = null;
			}
			int withModifiers = card.EnergyCost.GetWithModifiers(CostModifiers.All);
			if (withModifiers != card.EnergyCost.Canonical)
			{
				result.energyCost = withModifiers;
			}
			return result;
		}
	}

	public struct PotionState : IPacketSerializable
	{
		public ModelId id;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteModelEntry(id);
		}

		public void Deserialize(PacketReader reader)
		{
			id = reader.ReadModelIdAssumingType<PotionModel>();
		}
	}

	public struct RelicState : IPacketSerializable
	{
		public SerializableRelic relic;

		public void Serialize(PacketWriter writer)
		{
			writer.Write(relic);
		}

		public void Deserialize(PacketReader reader)
		{
			relic = reader.Read<SerializableRelic>();
		}
	}

	public List<uint> nextChoiceIds;

	public List<int> nextRewardIds;

	public uint? lastExecutedHookId;

	public uint? lastExecutedActionId;

	public List<CreatureState> Creatures { get; private set; } = new List<CreatureState>();

	public List<PlayerState> Players { get; private set; } = new List<PlayerState>();

	public SerializableRunRngSet Rng { get; private set; }

	public static NetFullCombatState FromRun(IRunState runState, GameAction? justFinishedAction)
	{
		NetFullCombatState netFullCombatState = new NetFullCombatState
		{
			nextChoiceIds = new List<uint>(),
			nextRewardIds = new List<int>(),
			Creatures = new List<CreatureState>(),
			Players = new List<PlayerState>(),
			Rng = runState.Rng.ToSerializable()
		};
		netFullCombatState.nextChoiceIds.AddRange(RunManager.Instance.PlayerChoiceSynchronizer.ChoiceIds);
		netFullCombatState.nextRewardIds.AddRange(RunManager.Instance.RewardsSetSynchronizer.GetNextRewardIds());
		netFullCombatState.lastExecutedHookId = ((justFinishedAction is GenericHookGameAction genericHookGameAction) ? new uint?(genericHookGameAction.HookId) : ((uint?)null));
		netFullCombatState.lastExecutedActionId = justFinishedAction?.Id;
		IReadOnlyList<Creature> readOnlyList = runState.Players[0].Creature.CombatState?.Creatures ?? Array.Empty<Creature>();
		foreach (Creature item3 in readOnlyList)
		{
			CreatureState item = new CreatureState
			{
				monsterId = item3.Monster?.Id,
				playerId = item3.Player?.NetId,
				currentHp = item3.CurrentHp,
				maxHp = item3.MaxHp,
				block = item3.Block,
				powers = new List<PowerState>()
			};
			foreach (PowerModel power in item3.Powers)
			{
				item.powers.Add(new PowerState
				{
					id = power.Id,
					amount = power.Amount
				});
			}
			netFullCombatState.Creatures.Add(item);
		}
		foreach (Player player in runState.Players)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			PlayerState item2 = new PlayerState
			{
				playerId = player.NetId,
				characterId = player.Character.Id,
				turnNumber = (playerCombatState?.TurnNumber ?? 0),
				phase = (playerCombatState?.Phase ?? PlayerTurnPhase.None),
				energy = (playerCombatState?.Energy ?? 0),
				stars = (playerCombatState?.Stars ?? 0),
				maxPotionCount = player.MaxPotionCount,
				gold = player.Gold,
				piles = new List<CombatPileState>(),
				potions = new List<PotionState>(),
				relics = new List<RelicState>(),
				orbs = new List<OrbState>(),
				rngSet = player.PlayerRng.ToSerializable(),
				relicGrabBag = player.RelicGrabBag.ToSerializable()
			};
			item2.rngSet.Counters.Remove(PlayerRngType.Rewards);
			item2.rngSet.Counters.Remove(PlayerRngType.Shops);
			if (playerCombatState != null && CombatManager.Instance.IsInProgress)
			{
				item2.piles.Add(CombatPileState.From(playerCombatState.Hand));
				item2.piles.Add(CombatPileState.From(playerCombatState.DrawPile));
				item2.piles.Add(CombatPileState.From(playerCombatState.DiscardPile));
				item2.piles.Add(CombatPileState.From(playerCombatState.ExhaustPile));
				item2.piles.Add(CombatPileState.From(playerCombatState.PlayPile));
				item2.orbs.AddRange(playerCombatState.OrbQueue.Orbs.Select(OrbState.From));
			}
			foreach (PotionModel potion in player.Potions)
			{
				item2.potions.Add(new PotionState
				{
					id = potion.Id
				});
			}
			foreach (RelicModel relic in player.Relics)
			{
				item2.relics.Add(new RelicState
				{
					relic = relic.ToSerializable()
				});
			}
			netFullCombatState.Players.Add(item2);
		}
		return netFullCombatState;
	}

	public void Serialize(PacketWriter writer)
	{
		writer.WriteList(Creatures);
		writer.WriteList(Players);
		writer.Write(Rng);
		writer.WriteInt(nextChoiceIds.Count);
		foreach (uint nextChoiceId in nextChoiceIds)
		{
			writer.WriteUInt(nextChoiceId);
		}
		writer.WriteInt(nextRewardIds.Count);
		foreach (int nextRewardId in nextRewardIds)
		{
			writer.WriteInt(nextRewardId);
		}
		writer.WriteBool(lastExecutedActionId.HasValue);
		if (lastExecutedActionId.HasValue)
		{
			writer.WriteUInt(lastExecutedActionId.Value);
		}
		writer.WriteBool(lastExecutedHookId.HasValue);
		if (lastExecutedHookId.HasValue)
		{
			writer.WriteUInt(lastExecutedHookId.Value);
		}
	}

	public void Deserialize(PacketReader reader)
	{
		Creatures = reader.ReadList<CreatureState>();
		Players = reader.ReadList<PlayerState>();
		Rng = reader.Read<SerializableRunRngSet>();
		int num = reader.ReadInt();
		nextChoiceIds = new List<uint>();
		for (int i = 0; i < num; i++)
		{
			nextChoiceIds.Add(reader.ReadUInt());
		}
		int num2 = reader.ReadInt();
		nextRewardIds = new List<int>();
		for (int j = 0; j < num2; j++)
		{
			nextRewardIds.Add(reader.ReadInt());
		}
		if (reader.ReadBool())
		{
			lastExecutedActionId = reader.ReadUInt();
		}
		if (reader.ReadBool())
		{
			lastExecutedHookId = reader.ReadUInt();
		}
	}

	public NetFullCombatState Anonymized()
	{
		return new NetFullCombatState
		{
			Creatures = Creatures.Select((CreatureState c) => c.Anonymized()).ToList(),
			Players = Players.Select((PlayerState p) => p.Anonymized()).ToList(),
			Rng = Rng,
			nextChoiceIds = nextChoiceIds,
			nextRewardIds = nextRewardIds,
			lastExecutedHookId = lastExecutedHookId,
			lastExecutedActionId = lastExecutedActionId
		};
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
		handler.AppendLiteral("Choice IDs: ");
		handler.AppendFormatted(string.Join(",", nextChoiceIds));
		stringBuilder3.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
		handler.AppendLiteral("Reward IDs: ");
		handler.AppendFormatted(string.Join(",", nextRewardIds));
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
		handler.AppendLiteral("Last executed action ID: ");
		handler.AppendFormatted(lastExecutedActionId);
		stringBuilder5.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder6 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("Last executed hook ID: ");
		handler.AppendFormatted(lastExecutedHookId);
		stringBuilder6.AppendLine(ref handler);
		foreach (CreatureState creature in Creatures)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(38, 2, stringBuilder2);
			handler.AppendLiteral("Creature with monster ID: ");
			handler.AppendFormatted(creature.monsterId);
			handler.AppendLiteral(" player ID: ");
			handler.AppendFormatted(creature.playerId);
			stringBuilder7.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(44, 4, stringBuilder2);
			handler.AppendLiteral("\tCurrent HP: ");
			handler.AppendFormatted(creature.currentHp);
			handler.AppendLiteral(" Max HP: ");
			handler.AppendFormatted(creature.maxHp);
			handler.AppendLiteral(" Block: ");
			handler.AppendFormatted(creature.block);
			handler.AppendLiteral(" Power count: ");
			handler.AppendFormatted(creature.powers.Count);
			stringBuilder8.AppendLine(ref handler);
			foreach (PowerState power in creature.powers)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder9 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(16, 2, stringBuilder2);
				handler.AppendLiteral("\tPower ");
				handler.AppendFormatted(power.id);
				handler.AppendLiteral(" Amount: ");
				handler.AppendFormatted(power.amount);
				stringBuilder9.AppendLine(ref handler);
			}
		}
		foreach (PlayerState player in Players)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
			handler.AppendLiteral("Player with ID: ");
			handler.AppendFormatted(player.playerId);
			handler.AppendLiteral(" Character: ");
			handler.AppendFormatted(player.characterId);
			stringBuilder10.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
			handler.AppendLiteral("\tTurn: ");
			handler.AppendFormatted(player.turnNumber);
			handler.AppendLiteral(" Phase: ");
			handler.AppendFormatted(player.phase);
			stringBuilder11.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder12 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(38, 4, stringBuilder2);
			handler.AppendLiteral("\tEnergy: ");
			handler.AppendFormatted(player.energy);
			handler.AppendLiteral(" Stars: ");
			handler.AppendFormatted(player.stars);
			handler.AppendLiteral(" Max Potions: ");
			handler.AppendFormatted(player.maxPotionCount);
			handler.AppendLiteral(" Gold: ");
			handler.AppendFormatted(player.gold);
			stringBuilder12.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder13 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(54, 4, stringBuilder2);
			handler.AppendLiteral("\tPile Count: ");
			handler.AppendFormatted(player.piles.Count);
			handler.AppendLiteral(" Potion Count: ");
			handler.AppendFormatted(player.potions.Count);
			handler.AppendLiteral(" Relic Count: ");
			handler.AppendFormatted(player.relics.Count);
			handler.AppendLiteral(" Orb Count: ");
			handler.AppendFormatted(player.orbs.Count);
			stringBuilder13.AppendLine(ref handler);
			foreach (CombatPileState pile in player.piles)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder14 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
				handler.AppendLiteral("\tPile ");
				handler.AppendFormatted(pile.pileType);
				stringBuilder14.AppendLine(ref handler);
				foreach (CardState card in pile.cards)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder15 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
					handler.AppendLiteral("\t\tSerialized card: ");
					handler.AppendFormatted(card.card);
					stringBuilder15.AppendLine(ref handler);
					if (card.affliction != null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder16 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
						handler.AppendLiteral("\t\t\tAffliction: ");
						handler.AppendFormatted(card.affliction);
						handler.AppendLiteral(" Affliction Count: ");
						handler.AppendFormatted(card.afflictionCount);
						stringBuilder16.AppendLine(ref handler);
					}
					if (card.keywords != null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder17 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
						handler.AppendLiteral("\t\t\tKeywords: ");
						handler.AppendFormatted(string.Join(",", card.keywords));
						stringBuilder17.AppendLine(ref handler);
					}
					if (card.energyCost.HasValue)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder18 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
						handler.AppendLiteral("\t\t\tEnergy Cost: ");
						object reference = card.energyCost;
						handler.AppendFormatted(string.Join(",", new ReadOnlySpan<object>(in reference)));
						stringBuilder18.AppendLine(ref handler);
					}
				}
			}
			foreach (PotionState potion in player.potions)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder19 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
				handler.AppendLiteral("\tPotion ");
				handler.AppendFormatted(potion.id);
				stringBuilder19.AppendLine(ref handler);
			}
			foreach (RelicState relic in player.relics)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder20 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(29, 3, stringBuilder2);
				handler.AppendLiteral("\tRelic ");
				handler.AppendFormatted(relic.relic.Id);
				handler.AppendLiteral(" Props: ");
				handler.AppendFormatted(relic.relic.Props);
				handler.AppendLiteral(" Floor added: ");
				handler.AppendFormatted(relic.relic.FloorAddedToDeck);
				stringBuilder20.AppendLine(ref handler);
			}
			foreach (OrbState orb in player.orbs)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder21 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(24, 3, stringBuilder2);
				handler.AppendLiteral("\tOrb ");
				handler.AppendFormatted(orb.id);
				handler.AppendLiteral(", passive: ");
				handler.AppendFormatted(orb.passive);
				handler.AppendLiteral(" evoke: ");
				handler.AppendFormatted(orb.evoke);
				stringBuilder21.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder22 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
			handler.AppendLiteral("Player RNG seed: ");
			handler.AppendFormatted(player.rngSet.Seed);
			stringBuilder22.AppendLine(ref handler);
			PlayerRngType[] values = Enum.GetValues<PlayerRngType>();
			foreach (PlayerRngType playerRngType in values)
			{
				if (player.rngSet.Counters.TryGetValue(playerRngType, out var value))
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder23 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
					handler.AppendLiteral("\tRNG counter ");
					handler.AppendFormatted(playerRngType);
					handler.AppendLiteral(": ");
					handler.AppendFormatted(value);
					stringBuilder23.AppendLine(ref handler);
				}
			}
			stringBuilder.AppendLine("Player relic grab bag:");
			RelicRarity[] values2 = Enum.GetValues<RelicRarity>();
			foreach (RelicRarity relicRarity in values2)
			{
				if (player.relicGrabBag.RelicIdLists.TryGetValue(relicRarity, out List<ModelId> value2))
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder24 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(10, 2, stringBuilder2);
					handler.AppendLiteral("\tRarity ");
					handler.AppendFormatted(relicRarity);
					handler.AppendLiteral(": ");
					handler.AppendFormatted(string.Join(",", value2.Select((ModelId m) => m.Entry)));
					stringBuilder24.AppendLine(ref handler);
				}
			}
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder25 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
		handler.AppendLiteral("RNG global seed: ");
		handler.AppendFormatted(Rng.Seed);
		stringBuilder25.AppendLine(ref handler);
		RunRngType[] values3 = Enum.GetValues<RunRngType>();
		foreach (RunRngType runRngType in values3)
		{
			if (Rng.Counters.TryGetValue(runRngType, out var value3))
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder26 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
				handler.AppendLiteral("\tRNG counter ");
				handler.AppendFormatted(runRngType);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(value3);
				stringBuilder26.AppendLine(ref handler);
			}
		}
		return stringBuilder.ToString();
	}
}
