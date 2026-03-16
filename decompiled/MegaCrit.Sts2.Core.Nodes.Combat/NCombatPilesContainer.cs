using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NCombatPilesContainer.cs")]
public class NCombatPilesContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName AnimIn = "AnimIn";

		public static readonly StringName AnimOut = "AnimOut";

		public static readonly StringName Enable = "Enable";

		public static readonly StringName Disable = "Disable";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName DrawPile = "DrawPile";

		public static readonly StringName DiscardPile = "DiscardPile";

		public static readonly StringName ExhaustPile = "ExhaustPile";

		public static readonly StringName _drawPile = "_drawPile";

		public static readonly StringName _discardPile = "_discardPile";

		public static readonly StringName _exhaustPile = "_exhaustPile";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("combat/combat_piles_container");

	private NDrawPileButton _drawPile;

	private NDiscardPileButton _discardPile;

	private NExhaustPileButton _exhaustPile;

	public NDrawPileButton DrawPile => _drawPile;

	public NDiscardPileButton DiscardPile => _discardPile;

	public NExhaustPileButton ExhaustPile => _exhaustPile;

	public override void _Ready()
	{
		_drawPile = GetNode<NDrawPileButton>("%DrawPile");
		_discardPile = GetNode<NDiscardPileButton>("%DiscardPile");
		_exhaustPile = GetNode<NExhaustPileButton>("%ExhaustPile");
	}

	public void Initialize(Player player)
	{
		_drawPile.Initialize(player);
		_discardPile.Initialize(player);
		_exhaustPile.Initialize(player);
	}

	public void AnimIn()
	{
		_drawPile.AnimIn();
		_discardPile.AnimIn();
	}

	public void AnimOut()
	{
		_drawPile.AnimOut();
		_discardPile.AnimOut();
		_exhaustPile.AnimOut();
	}

	public void Enable()
	{
		_drawPile.Enable();
		_discardPile.Enable();
		_exhaustPile.Enable();
	}

	public void Disable()
	{
		_drawPile.Disable();
		_discardPile.Disable();
		_exhaustPile.Disable();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Enable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Disable, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 0)
		{
			AnimOut();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Enable && args.Count == 0)
		{
			Enable();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Disable && args.Count == 0)
		{
			Disable();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.Enable)
		{
			return true;
		}
		if (method == MethodName.Disable)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._drawPile)
		{
			_drawPile = VariantUtils.ConvertTo<NDrawPileButton>(in value);
			return true;
		}
		if (name == PropertyName._discardPile)
		{
			_discardPile = VariantUtils.ConvertTo<NDiscardPileButton>(in value);
			return true;
		}
		if (name == PropertyName._exhaustPile)
		{
			_exhaustPile = VariantUtils.ConvertTo<NExhaustPileButton>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DrawPile)
		{
			value = VariantUtils.CreateFrom<NDrawPileButton>(DrawPile);
			return true;
		}
		if (name == PropertyName.DiscardPile)
		{
			value = VariantUtils.CreateFrom<NDiscardPileButton>(DiscardPile);
			return true;
		}
		if (name == PropertyName.ExhaustPile)
		{
			value = VariantUtils.CreateFrom<NExhaustPileButton>(ExhaustPile);
			return true;
		}
		if (name == PropertyName._drawPile)
		{
			value = VariantUtils.CreateFrom(in _drawPile);
			return true;
		}
		if (name == PropertyName._discardPile)
		{
			value = VariantUtils.CreateFrom(in _discardPile);
			return true;
		}
		if (name == PropertyName._exhaustPile)
		{
			value = VariantUtils.CreateFrom(in _exhaustPile);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._drawPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._discardPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._exhaustPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DrawPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DiscardPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ExhaustPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._drawPile, Variant.From(in _drawPile));
		info.AddProperty(PropertyName._discardPile, Variant.From(in _discardPile));
		info.AddProperty(PropertyName._exhaustPile, Variant.From(in _exhaustPile));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._drawPile, out var value))
		{
			_drawPile = value.As<NDrawPileButton>();
		}
		if (info.TryGetProperty(PropertyName._discardPile, out var value2))
		{
			_discardPile = value2.As<NDiscardPileButton>();
		}
		if (info.TryGetProperty(PropertyName._exhaustPile, out var value3))
		{
			_exhaustPile = value3.As<NExhaustPileButton>();
		}
	}
}
