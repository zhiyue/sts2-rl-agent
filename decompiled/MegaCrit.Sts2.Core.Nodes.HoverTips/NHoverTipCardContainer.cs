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
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName LayoutResizeAndReposition = "LayoutResizeAndReposition";
	}

	public new class PropertyName : Control.PropertyName
	{
	}

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

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.LayoutResizeAndReposition)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
