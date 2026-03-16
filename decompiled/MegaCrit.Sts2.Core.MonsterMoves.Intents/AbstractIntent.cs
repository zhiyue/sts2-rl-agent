using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public abstract class AbstractIntent
{
	protected const string _locTable = "intents";

	protected string? _cachedAnimationName;

	public abstract IntentType IntentType { get; }

	public virtual bool HasIntentTip => true;

	protected virtual LocString? IntentLabelFormat => null;

	protected abstract string IntentPrefix { get; }

	protected abstract string? SpritePath { get; }

	public virtual IEnumerable<string> AssetPaths
	{
		get
		{
			if (SpritePath != null)
			{
				return new global::_003C_003Ez__ReadOnlySingleElementList<string>(ImageHelper.GetImagePath(SpritePath));
			}
			return Array.Empty<string>();
		}
	}

	protected LocString IntentTitle => new LocString("intents", IntentPrefix + ".title");

	public virtual LocString GetIntentLabel(IEnumerable<Creature> targets, Creature owner)
	{
		return IntentLabelFormat ?? new LocString("intents", "FORMAT_EMPTY");
	}

	public virtual Texture2D? GetTexture(IEnumerable<Creature> targets, Creature owner)
	{
		if (string.IsNullOrEmpty(SpritePath))
		{
			return null;
		}
		string imagePath = ImageHelper.GetImagePath(SpritePath);
		return PreloadManager.Cache.GetTexture2D(imagePath);
	}

	public virtual string GetAnimation(IEnumerable<Creature> targets, Creature owner)
	{
		return _cachedAnimationName ?? (_cachedAnimationName = IntentPrefix.ToLowerInvariant());
	}

	public HoverTip GetHoverTip(IEnumerable<Creature> targets, Creature owner)
	{
		Creature[] targets2 = targets.ToArray();
		return new HoverTip(IntentTitle, GetIntentDescription(targets2, owner), GetTexture(targets2, owner));
	}

	protected virtual LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		LocString locString = new LocString("intents", IntentPrefix + ".description");
		CombatState? combatState = owner.CombatState;
		locString.Add("IsMultiplayer", combatState != null && combatState.RunState.Players.Count > 1);
		return locString;
	}
}
