namespace MegaCrit.Sts2.Core.Runs;

public enum CardRarityOddsType
{
	/// <summary>
	/// Sentinel value, should not be used.
	/// </summary>
	None,
	/// <summary>
	/// Rolls for rarity as if we were generating cards for a regular encounter.
	/// Note that this does not mean that the reward was generated from an encounter. Use this in conjunction with
	/// CardCreationSource to determine that.
	/// This roll method should be used in cases where you are generating a reward that should weight more towards
	/// commons than rares (which is usually the case). Be careful not to use this or the other encounter types if you
	/// are limiting the rarity in the pool passed to the generation.
	/// </summary>
	RegularEncounter,
	/// <summary>
	/// Rolls for rarity as if we were generating cards for an elite encounter.
	/// Be careful not to use this or the other encounter types if you are limiting the rarity in the pool passed to the
	/// generation.
	/// </summary>
	EliteEncounter,
	/// <summary>
	/// Rolls for rarity as if we were generating cards for an boss encounter (i.e. only rare cards).
	/// Be careful not to use this or the other encounter types if you are limiting the rarity in the pool passed to the
	/// generation.
	/// </summary>
	BossEncounter,
	/// <summary>
	/// Rolls for rarity as if we were generating cards for showing in the shop.
	/// </summary>
	Shop,
	/// <summary>
	/// Forces rolling all cards in the pool unweighted - rarity is not taken into account.
	/// Use this if you are limiting the rarity in the pool passed to the generation.
	/// </summary>
	Uniform
}
