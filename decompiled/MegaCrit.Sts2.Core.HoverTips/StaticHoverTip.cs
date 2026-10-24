namespace MegaCrit.Sts2.Core.HoverTips;

/// <summary>
/// HoverTip tags that don't correspond to a model (i.e. powers/relics)
/// See <see cref="M:MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.Static(MegaCrit.Sts2.Core.HoverTips.StaticHoverTip,MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar[])" /> for example usage.
/// </summary>
public enum StaticHoverTip
{
	None = 0,
	Channeling = 2,
	Evoke = 3,
	Transform = 4,
	Block = 5,
	Fatal = 6,
	Energy = 7,
	Stun = 8,
	CardReward = 9,
	Forge = 10,
	SummonDynamic = 12,
	SummonStatic = 13,
	ReplayDynamic = 14,
	ReplayStatic = 15,
	Cook = 16
}
