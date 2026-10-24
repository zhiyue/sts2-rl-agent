using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace MegaCrit.Sts2.Core.Nodes.HoverTips;

[ScriptPath("res://src/Core/Nodes/HoverTips/NHoverTipCardContainer.cs")]
public class NHoverTipCardContainer : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the 'LayoutResizeAndReposition' method.
		/// </summary>
		public static readonly StringName LayoutResizeAndReposition = "LayoutResizeAndReposition";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const string _cardHoverTipScenePath = "res://scenes/ui/card_hover_tip.tscn";

	private const float _padding = 4f;

	private IEnumerable<Control> Tips => GetChildren().OfType<Control>();

	public void Add(CardHoverTip cardTip)
	{
		Control control = PreloadManager.Cache.GetScene("res://scenes/ui/card_hover_tip.tscn").Instantiate<Control>(PackedScene.GenEditState.Disabled);
		this.AddChildSafely(control);
		NCard node = control.GetNode<NCard>("%Card");
		node.Model = cardTip.Card;
		node.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);
	}

	/// <summary>
	/// Lays out cards vertically, then horizontally, then sets the position and the size of the container according
	/// to the passed global start position and alignment.
	/// </summary>
	/// <param name="globalStartLocation">Where to start positioning nodes.</param>
	/// <param name="alignment">Which side of the global start location the cards should be placed on.</param>
	/// <returns></returns>
	public void LayoutResizeAndReposition(Vector2 globalStartLocation, HoverTipAlignment alignment)
	{
		Vector2 size = NGame.Instance.GetViewportRect().Size;
		Vector2 size2 = Vector2.Zero;
		Vector2 zero = Vector2.Zero;
		float b = 0f;
		foreach (Control tip in Tips)
		{
			tip.Position = zero;
			size2 = new Vector2(Mathf.Max(zero.X + tip.Size.X, size2.X), Mathf.Max(zero.Y + tip.Size.Y, size2.Y));
			zero += Vector2.Down * (tip.Size.Y + 4f);
			b = Mathf.Max(tip.Size.X, b);
		}
		switch (alignment)
		{
		case HoverTipAlignment.Right:
			base.GlobalPosition = globalStartLocation;
			break;
		case HoverTipAlignment.Left:
			base.GlobalPosition = globalStartLocation + Vector2.Left * size2.X;
			break;
		}
		base.Size = size2;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.LayoutResizeAndReposition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "globalStartLocation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "alignment", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.LayoutResizeAndReposition && args.Count == 2)
		{
			LayoutResizeAndReposition(VariantUtils.ConvertTo<Vector2>(in args[0]), VariantUtils.ConvertTo<HoverTipAlignment>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.LayoutResizeAndReposition)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
