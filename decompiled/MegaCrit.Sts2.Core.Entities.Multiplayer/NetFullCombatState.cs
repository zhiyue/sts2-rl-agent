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

	public struct PlayerState : IPacketSerializable
	{
		public ulong playerId;

		public ModelId characterId;

		public int energy;

		public int stars;

		public int maxStars;

		public int maxPotionCount;

		public int gold;

		public List<CombatPileState> piles;

		public List<PotionState> potions;

		public List<RelicState> relics;

		public List<OrbState> orbs;

		public SerializablePlayerRngSet rngSet;

		public SerializablePlayerOddsSet oddsSet;

		public SerializableRelicGrabBag relicGrabBag;

		public void Serialize(PacketWriter writer)
		{
			writer.WriteULong(playerId);
			writer.WriteModelEntry(characterId);
			writer.WriteInt(energy);
			writer.WriteInt(stars);
			writer.WriteInt(maxStars);
			writer.WriteInt(maxPotionCount);
			writer.WriteInt(gold);
			writer.WriteList(piles);
			writer.WriteList(potions);
			writer.WriteList(relics);
			writer.WriteList(orbs);
			writer.Write(rngSet);
			writer.Write(oddsSet);
			writer.Write(relicGrabBag);
		}

		public void Deserialize(PacketReader reader)
		{
			playerId = reader.ReadULong();
			characterId = reader.ReadModelIdAssumingType<CharacterModel>();
			energy = reader.ReadInt();
			stars = reader.ReadInt();
			maxStars = reader.ReadInt();
			maxPotionCount = reader.ReadInt();
			gold = reader.ReadInt();
			piles = reader.ReadList<CombatPileState>();
			potions = reader.ReadList<PotionState>();
			relics = reader.ReadList<RelicState>();
			orbs = reader.ReadList<OrbState>();
			rngSet = reader.Read<SerializablePlayerRngSet>();
			oddsSet = reader.Read<SerializablePlayerOddsSet>();
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
			return new CardState
			{
				card = card.ToSerializable(),
				affliction = card.Affliction?.Id,
				afflictionCount = (card.Affliction?.Amount ?? 0),
				keywords = ((card.Keywords.Count > 0) ? card.Keywords.ToList() : null)
			};
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
			Creatures = new List<CreatureState>(),
			Players = new List<PlayerState>(),
			Rng = runState.Rng.ToSerializable()
		};
		netFullCombatState.nextChoiceIds.AddRange(RunManager.Instance.PlayerChoiceSynchronizer.ChoiceIds);
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
			PlayerState item2 = new PlayerState
			{
				playerId = player.NetId,
				characterId = player.Character.Id,
				energy = (player.PlayerCombatState?.Energy ?? 0),
				stars = (player.PlayerCombatState?.Stars ?? 0),
				maxPotionCount = player.MaxPotionCount,
				gold = player.Gold,
				piles = new List<CombatPileState>(),
				potions = new List<PotionState>(),
				relics = new List<RelicState>(),
				orbs = new List<OrbState>(),
				rngSet = player.PlayerRng.ToSerializable(),
				oddsSet = player.PlayerOdds.ToSerializable(),
				relicGrabBag = player.RelicGrabBag.ToSerializable()
			};
			if (player.PlayerCombatState != null && CombatManager.Instance.IsInProgress)
			{
				item2.piles.Add(CombatPileState.From(player.PlayerCombatState.Hand));
				item2.piles.Add(CombatPileState.From(player.PlayerCombatState.DrawPile));
				item2.piles.Add(CombatPileState.From(player.PlayerCombatState.DiscardPile));
				item2.piles.Add(CombatPileState.From(player.PlayerCombatState.ExhaustPile));
				item2.piles.Add(CombatPileState.From(player.PlayerCombatState.PlayPile));
				item2.orbs.AddRange(player.PlayerCombatState.OrbQueue.Orbs.Select(OrbState.From));
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
		handler = new StringBuilder.AppendInterpolatedStringHandler(25, 1, stringBuilder2);
		handler.AppendLiteral("Last executed action ID: ");
		handler.AppendFormatted(lastExecutedActionId);
		stringBuilder4.AppendLine(ref handler);
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(23, 1, stringBuilder2);
		handler.AppendLiteral("Last executed hook ID: ");
		handler.AppendFormatted(lastExecutedHookId);
		stringBuilder5.AppendLine(ref handler);
		foreach (CreatureState creature in Creatures)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(38, 2, stringBuilder2);
			handler.AppendLiteral("Creature with monster ID: ");
			handler.AppendFormatted(creature.monsterId);
			handler.AppendLiteral(" player ID: ");
			handler.AppendFormatted(creature.playerId);
			stringBuilder6.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(44, 4, stringBuilder2);
			handler.AppendLiteral("\tCurrent HP: ");
			handler.AppendFormatted(creature.currentHp);
			handler.AppendLiteral(" Max HP: ");
			handler.AppendFormatted(creature.maxHp);
			handler.AppendLiteral(" Block: ");
			handler.AppendFormatted(creature.block);
			handler.AppendLiteral(" Power count: ");
			handler.AppendFormatted(creature.powers.Count);
			stringBuilder7.AppendLine(ref handler);
			foreach (PowerState power in creature.powers)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(16, 2, stringBuilder2);
				handler.AppendLiteral("\tPower ");
				handler.AppendFormatted(power.id);
				handler.AppendLiteral(" Amount: ");
				handler.AppendFormatted(power.amount);
				stringBuilder8.AppendLine(ref handler);
			}
		}
		foreach (PlayerState player in Players)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
			handler.AppendLiteral("Player with ID: ");
			handler.AppendFormatted(player.playerId);
			handler.AppendLiteral(" Character: ");
			handler.AppendFormatted(player.characterId);
			stringBuilder9.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(51, 5, stringBuilder2);
			handler.AppendLiteral("\t Energy: ");
			handler.AppendFormatted(player.energy);
			handler.AppendLiteral(" Stars: ");
			handler.AppendFormatted(player.stars);
			handler.AppendLiteral(" Max Stars: ");
			handler.AppendFormatted(player.maxStars);
			handler.AppendLiteral(" Max Potions: ");
			handler.AppendFormatted(player.maxPotionCount);
			handler.AppendLiteral(" Gold: ");
			handler.AppendFormatted(player.gold);
			stringBuilder10.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder11 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(54, 4, stringBuilder2);
			handler.AppendLiteral("\tPile Count: ");
			handler.AppendFormatted(player.piles.Count);
			handler.AppendLiteral(" Potion Count: ");
			handler.AppendFormatted(player.potions.Count);
			handler.AppendLiteral(" Relic Count: ");
			handler.AppendFormatted(player.relics.Count);
			handler.AppendLiteral(" Orb Count: ");
			handler.AppendFormatted(player.orbs.Count);
			stringBuilder11.AppendLine(ref handler);
			foreach (CombatPileState pile in player.piles)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder12 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
				handler.AppendLiteral("\tPile ");
				handler.AppendFormatted(pile.pileType);
				stringBuilder12.AppendLine(ref handler);
				foreach (CardState card in pile.cards)
				{
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder13 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
					handler.AppendLiteral("\t\tSerialized card: ");
					handler.AppendFormatted(card.card);
					stringBuilder13.AppendLine(ref handler);
					if (card.affliction != null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder14 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
						handler.AppendLiteral("\t\t\tAffliction: ");
						handler.AppendFormatted(card.affliction);
						handler.AppendLiteral(" Affliction Count: ");
						handler.AppendFormatted(card.afflictionCount);
						stringBuilder14.AppendLine(ref handler);
					}
					if (card.keywords != null)
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder15 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
						handler.AppendLiteral("\t\t\tKeywords: ");
						handler.AppendFormatted(string.Join(",", card.keywords));
						stringBuilder15.AppendLine(ref handler);
					}
				}
			}
			foreach (PotionState potion in player.potions)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder16 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(8, 1, stringBuilder2);
				handler.AppendLiteral("\tPotion ");
				handler.AppendFormatted(potion.id);
				stringBuilder16.AppendLine(ref handler);
			}
			foreach (RelicState relic in player.relics)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder17 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(29, 3, stringBuilder2);
				handler.AppendLiteral("\tRelic ");
				handler.AppendFormatted(relic.relic.Id);
				handler.AppendLiteral(" Props: ");
				handler.AppendFormatted(relic.relic.Props);
				handler.AppendLiteral(" Floor added: ");
				handler.AppendFormatted(relic.relic.FloorAddedToDeck);
				stringBuilder17.AppendLine(ref handler);
			}
			foreach (OrbState orb in player.orbs)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder18 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(24, 3, stringBuilder2);
				handler.AppendLiteral("\tOrb ");
				handler.AppendFormatted(orb.id);
				handler.AppendLiteral(", passive: ");
				handler.AppendFormatted(orb.passive);
				handler.AppendLiteral(" evoke: ");
				handler.AppendFormatted(orb.evoke);
				stringBuilder18.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder19 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
			handler.AppendLiteral("Player RNG seed: ");
			handler.AppendFormatted(player.rngSet.Seed);
			stringBuilder19.AppendLine(ref handler);
			List<KeyValuePair<PlayerRngType, int>> list = player.rngSet.Counters.ToList();
			foreach (KeyValuePair<PlayerRngType, int> item in list)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder20 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
				handler.AppendLiteral("\tRNG counter ");
				handler.AppendFormatted(item.Key);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(item.Value);
				stringBuilder20.AppendLine(ref handler);
			}
			stringBuilder.AppendLine("Player Odds:");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder21 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("\tCard rarity:");
			handler.AppendFormatted(player.oddsSet.CardRarityOddsValue);
			stringBuilder21.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder22 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(15, 1, stringBuilder2);
			handler.AppendLiteral("\tPotion reward:");
			handler.AppendFormatted(player.oddsSet.PotionRewardOddsValue);
			stringBuilder22.AppendLine(ref handler);
			stringBuilder.AppendLine("Player relic grab bag:");
			List<KeyValuePair<RelicRarity, List<ModelId>>> list2 = player.relicGrabBag.RelicIdLists.ToList();
			foreach (KeyValuePair<RelicRarity, List<ModelId>> item2 in list2)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder23 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(10, 2, stringBuilder2);
				handler.AppendLiteral("\tRarity ");
				handler.AppendFormatted(item2.Key);
				handler.AppendLiteral(": ");
				handler.AppendFormatted(string.Join(",", item2.Value.Select((ModelId m) => m.Entry)));
				stringBuilder23.AppendLine(ref handler);
			}
		}
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder24 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
		handler.AppendLiteral("RNG global seed: ");
		handler.AppendFormatted(Rng.Seed);
		stringBuilder24.AppendLine(ref handler);
		List<KeyValuePair<RunRngType, int>> list3 = Rng.Counters.ToList();
		foreach (KeyValuePair<RunRngType, int> item3 in list3)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder25 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(15, 2, stringBuilder2);
			handler.AppendLiteral("\tRNG counter ");
			handler.AppendFormatted(item3.Key);
			handler.AppendLiteral(": ");
			handler.AppendFormatted(item3.Value);
			stringBuilder25.AppendLine(ref handler);
		}
		return stringBuilder.ToString();
	}
}
