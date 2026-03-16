using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace MegaCrit.Sts2.Core.Models;

public abstract class ModifierModel : AbstractModel
{
	private const string _locTable = "modifiers";

	private RunState? _runState;

	public override bool ShouldReceiveCombatHooks => true;

	public virtual bool ClearsPlayerDeck => false;

	public virtual IEnumerable<IHoverTip> HoverTips => Array.Empty<IHoverTip>();

	public virtual LocString Title => new LocString("modifiers", base.Id.Entry + ".title");

	public virtual LocString Description => new LocString("modifiers", base.Id.Entry + ".description");

	public virtual LocString NeowOptionTitle => Title;

	public virtual LocString NeowOptionDescription => Description;

	protected LocString? AdditionalRestSiteHealText => LocString.GetIfExists("modifiers", base.Id.Entry + ".additionalRestSiteHealText");

	public Texture2D Icon
	{
		get
		{
			if (ResourceLoader.Exists(IconPath))
			{
				return PreloadManager.Cache.GetTexture2D(IconPath);
			}
			return PreloadManager.Cache.GetTexture2D(MissingIconPath);
		}
	}

	protected virtual string IconPath => ImageHelper.GetImagePath("packed/modifiers/" + base.Id.Entry.ToLowerInvariant() + ".png");

	private static string MissingIconPath => ImageHelper.GetImagePath("powers/missing_power.png");

	protected RunState RunState => _runState ?? throw new InvalidOperationException("Modifier was never initialized!");

	public void OnRunCreated(RunState runState)
	{
		AssertMutable();
		_runState = runState;
		if (ClearsPlayerDeck)
		{
			foreach (Player player in runState.Players)
			{
				player.Deck.Clear();
			}
		}
		AfterRunCreated(runState);
	}

	public void OnRunLoaded(RunState runState)
	{
		AssertMutable();
		_runState = runState;
		AfterRunLoaded(runState);
	}

	public virtual Func<Task>? GenerateNeowOption(EventModel eventModel)
	{
		return null;
	}

	protected virtual void AfterRunCreated(RunState runState)
	{
	}

	protected virtual void AfterRunLoaded(RunState runState)
	{
	}

	public virtual bool IsEquivalent(ModifierModel other)
	{
		if (base.IsCanonical == other.IsCanonical)
		{
			return GetType() == other.GetType();
		}
		return false;
	}

	public ModifierModel ToMutable()
	{
		AssertCanonical();
		return (ModifierModel)MutableClone();
	}

	public SerializableModifier ToSerializable()
	{
		AssertMutable();
		return new SerializableModifier
		{
			Id = base.Id,
			Props = SavedProperties.From(this)
		};
	}

	public static ModifierModel FromSerializable(SerializableModifier serializable)
	{
		ModifierModel modifierModel = SaveUtil.ModifierOrDeprecated(serializable.Id).ToMutable();
		serializable.Props?.Fill(modifierModel);
		return modifierModel;
	}
}
