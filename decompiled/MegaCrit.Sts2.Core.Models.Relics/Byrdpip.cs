using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models.Relics;

public sealed class Byrdpip : RelicModel
{
	private string _skin = SkinOptions[0];

	public override bool AddsPet => true;

	public override RelicRarity Rarity => RelicRarity.Event;

	public override bool HasUponPickupEffect => true;

	public override bool SpawnsPets => true;

	public static string[] SkinOptions => new string[4] { "version1", "version2", "version3", "version4" };

	[SavedProperty]
	public string Skin
	{
		get
		{
			return _skin;
		}
		set
		{
			AssertMutable();
			_skin = value;
		}
	}

	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<ByrdSwoop>();

	public override async Task AfterObtained()
	{
		Skin = new Rng((uint)(base.Owner.NetId + base.Owner.RunState.Rng.Seed)).NextItem(SkinOptions);
		List<CardModel> list = PileType.Deck.GetPile(base.Owner).Cards.Where((CardModel c) => c is ByrdonisEgg).ToList();
		if (CombatManager.Instance.IsInProgress)
		{
			list.AddRange(base.Owner.PlayerCombatState.AllCards.Where((CardModel c) => c is ByrdonisEgg));
		}
		foreach (CardModel item in list)
		{
			await CardCmd.TransformTo<ByrdSwoop>(item);
		}
		if (CombatManager.Instance.IsInProgress)
		{
			await SummonPet();
		}
	}

	public override async Task BeforeCombatStart()
	{
		await SummonPet();
	}

	private async Task SummonPet()
	{
		await PlayerCmd.AddPet<MegaCrit.Sts2.Core.Models.Monsters.Byrdpip>(base.Owner);
	}
}
