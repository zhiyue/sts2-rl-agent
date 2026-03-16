using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class DeathBlowIntent : SingleAttackIntent
{
	protected override string IntentPrefix => "DEATH_BLOW";

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_death_blow.tres";

	public override IntentType IntentType => IntentType.DeathBlow;

	public override Texture2D GetTexture(IEnumerable<Creature> targets, Creature owner)
	{
		string imagePath = ImageHelper.GetImagePath(SpritePath);
		return PreloadManager.Cache.GetTexture2D(imagePath);
	}

	public override string GetAnimation(IEnumerable<Creature> targets, Creature owner)
	{
		return _cachedAnimationName ?? (_cachedAnimationName = IntentPrefix.ToLowerInvariant());
	}

	public DeathBlowIntent(Func<decimal> damageCalc)
		: base(damageCalc)
	{
	}
}
