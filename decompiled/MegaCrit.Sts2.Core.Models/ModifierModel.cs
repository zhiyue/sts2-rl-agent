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

/// <summary>
/// Run-lifetime model which alters the run for daily and custom runs.
/// </summary>
public abstract class ModifierModel : AbstractModel
{
	private const string _locTable = "modifiers";

	private RunState? _runState;

	public override bool ShouldReceiveCombatHooks => true;

	/// <summary>
	/// If true, the player's deck will be cleared before AfterRunInitialized is called.
	/// </summary>
	public virtual bool ClearsPlayerDeck => false;

	public virtual IEnumerable<IHoverTip> HoverTips => Array.Empty<IHoverTip>();

	public virtual LocString Title => new LocString("modifiers", base.Id.Entry + ".title");

	public virtual LocString Description => new LocString("modifiers", base.Id.Entry + ".description");

	public virtual LocString NeowOptionTitle => Title;

	public virtual LocString NeowOptionDescription => Description;

	/// <summary>
	/// Get the additional text that this modifier should add to the "heal" option at the rest site.
	/// Returns null for modifiers that add no additional text.
	/// </summary>
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

	/// <summary>
	/// Called after a new run is created with the modifier.
	/// Only called for new runs, not loaded ones.
	/// </summary>
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

	/// <summary>
	/// Called after a run is loaded with the modifier.
	/// </summary>
	public void OnRunLoaded(RunState runState)
	{
		AssertMutable();
		_runState = runState;
		AfterRunLoaded(runState);
	}

	/// <summary>
	/// If this returns a non-null function, the modifier is presented as an option at Neow.
	/// The function is called when the option is chosen. The option will have the modifier's title and description.
	/// </summary>
	/// <param name="eventModel">The event model for which the option should be generated.</param>
	public virtual Func<Task>? GenerateNeowOption(EventModel eventModel)
	{
		return null;
	}

	/// <summary>
	/// A special hook for modifiers that is called right after the run is created for the first time.
	/// Used for initializing things like the relic bags.
	/// Note that this is not called when a run is loaded from save.
	/// </summary>
	protected virtual void AfterRunCreated(RunState runState)
	{
	}

	/// <summary>
	/// A special hook for modifiers that is called right after the run is loaded.
	/// Used for initializing things that are not serialized like base room odds.
	/// Note that this is not called when a run is created from scratch without loading.
	/// </summary>
	protected virtual void AfterRunLoaded(RunState runState)
	{
	}

	/// <summary>
	/// Returns true if this is equivalent to the passed modifier.
	/// By default, two ModifierModels are considered equivalent if they are of the same type. The CharacterCards modifier
	/// overrides this to ensure that the character property is equal as well.
	/// </summary>
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
