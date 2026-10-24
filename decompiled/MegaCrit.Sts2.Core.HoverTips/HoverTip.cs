using Godot;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.HoverTips;

public record struct HoverTip : IHoverTip
{
	public string? Title { get; private set; }

	public Texture2D? Icon { get; private set; }

	public string Description { get; private set; }

	public string Id { get; set; }

	public bool IsSmart { get; set; }

	public bool IsDebuff { get; set; }

	public bool IsInstanced { get; set; }

	public AbstractModel? CanonicalModel { get; private set; }

	/// <summary>
	/// Defines whether or now we want to word wrap the description text on the hover tip node
	/// or if we want the text to stretch the HoverTip horizontally. This is useful for situations
	/// like our map point entry hover tips in the run history screen, that can be pretty long.
	/// </summary>
	public bool ShouldOverrideTextOverflow { get; set; }

	public HoverTip(LocString description, Texture2D? icon = null)
	{
		Title = null;
		IsSmart = false;
		IsDebuff = false;
		IsInstanced = false;
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = "LocString with Description=" + description.LocTable + "." + description.LocEntryKey;
		Description = description.GetFormattedText();
		Icon = icon;
	}

	public HoverTip(LocString title, LocString description, Texture2D? icon = null)
	{
		IsSmart = false;
		IsDebuff = false;
		IsInstanced = false;
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = $"LocString with Title={title.LocTable}.{title.LocEntryKey} and Description={description.LocTable}.{description.LocEntryKey}";
		Title = title.GetFormattedText();
		Description = description.GetFormattedText();
		Icon = icon;
	}

	public HoverTip(LocString title, string description, Texture2D? icon = null)
	{
		IsSmart = false;
		IsDebuff = false;
		IsInstanced = false;
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = "LocString with Title=" + title.LocTable + "." + title.LocEntryKey;
		Title = title.GetFormattedText();
		Description = description;
		Icon = icon;
	}

	public HoverTip(AfflictionModel affliction, LocString description)
	{
		IsSmart = false;
		IsInstanced = false;
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = affliction.Id.ToString();
		Title = affliction.Title.GetFormattedText();
		Description = description.GetFormattedText();
		Icon = null;
		IsDebuff = true;
	}

	public HoverTip(OrbModel orb, LocString description)
	{
		IsSmart = false;
		IsDebuff = false;
		IsInstanced = false;
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = orb.Id.ToString();
		Title = orb.Title.GetFormattedText();
		Description = description.GetFormattedText();
		Icon = orb.Icon;
	}

	/// <summary>
	/// Create a HoverTip from a PowerModel.
	/// </summary>
	/// <param name="power">Power to create the HoverTip for.</param>
	/// <param name="description">string to use in the description field.</param>
	/// <param name="isSmart">
	///     Whether or not the power description is "smart".
	///     For example, when the Strength HoverTip is displayed on the Body Slam card, it should be "dumb" because it's
	///     just referring to the concept of Strength, rather than a specific amount.
	///     On the flip side, when the Strength HoverTip is displayed when hovering over a creature with 3 Strength, it
	///     should be "smart" because it's referring to an instance of 3 Strength.
	/// </param>
	public HoverTip(PowerModel power, string description, bool isSmart)
	{
		CanonicalModel = null;
		ShouldOverrideTextOverflow = false;
		Id = power.Id.ToString();
		Title = power.Title.GetFormattedText();
		Description = description;
		Icon = power.Icon;
		IsDebuff = power.Type == PowerType.Debuff;
		IsInstanced = power.InstanceType != PowerInstanceType.None;
		IsSmart = isSmart;
	}

	public void SetCanonicalModel(AbstractModel model)
	{
		model.AssertCanonical();
		CanonicalModel = model;
	}

	/// <summary>
	/// We prefer to align HoverTips to be on the right-hand side of the hovered element. However, if we can't fit the HoverTip we left align.
	/// </summary>
	public static HoverTipAlignment GetHoverTipAlignment(Node2D node, float threshold = 0.75f)
	{
		if (!(node.GlobalPosition.X > node.GetViewport().GetVisibleRect().Size.X * threshold))
		{
			return HoverTipAlignment.Right;
		}
		return HoverTipAlignment.Left;
	}

	public static HoverTipAlignment GetHoverTipAlignment(Control node, float threshold = 0.75f)
	{
		if (!(node.GlobalPosition.X > node.GetViewport().GetVisibleRect().Size.X * threshold))
		{
			return HoverTipAlignment.Right;
		}
		return HoverTipAlignment.Left;
	}
}
