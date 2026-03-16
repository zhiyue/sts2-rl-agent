using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace MegaCrit.Sts2.Core.Models.Cards;

public sealed class FiendFire : CardModel
{
	protected override IEnumerable<DynamicVar> CanonicalVars => new global::_003C_003Ez__ReadOnlySingleElementList<DynamicVar>(new DamageVar(7m, ValueProp.Move));

	public override IEnumerable<CardKeyword> CanonicalKeywords => new global::_003C_003Ez__ReadOnlySingleElementList<CardKeyword>(CardKeyword.Exhaust);

	protected override IEnumerable<string> ExtraRunAssetPaths => NGroundFireVfx.AssetPaths;

	public FiendFire()
		: base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
		List<CardModel> list = PileType.Hand.GetPile(base.Owner).Cards.ToList();
		int cardCount = list.Count;
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		float scale = 0.8f;
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(cardCount).FromCard(this)
			.Targeting(cardPlay.Target)
			.BeforeDamage(delegate
			{
				NGroundFireVfx nGroundFireVfx = NGroundFireVfx.Create(cardPlay.Target);
				if (nGroundFireVfx == null)
				{
					return Task.CompletedTask;
				}
				SfxCmd.Play("event:/sfx/characters/attack_fire");
				nGroundFireVfx.Scale = Vector2.One * scale;
				NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nGroundFireVfx);
				scale += 0.1f;
				return Task.CompletedTask;
			})
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		base.DynamicVars.Damage.UpgradeValueBy(3m);
	}
}
