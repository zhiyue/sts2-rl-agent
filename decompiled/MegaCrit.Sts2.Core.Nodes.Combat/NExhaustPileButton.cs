using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NExhaustPileButton.cs")]
public class NExhaustPileButton : NCombatCardPile
{
	public new class MethodName : NCombatCardPile.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName AddCard = "AddCard";

		public new static readonly StringName SetAnimInOutPositions = "SetAnimInOutPositions";

		public new static readonly StringName AnimIn = "AnimIn";
	}

	public new class PropertyName : NCombatCardPile.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public new static readonly StringName Pile = "Pile";

		public static readonly StringName _viewport = "_viewport";

		public static readonly StringName _posOffset = "_posOffset";
	}

	public new class SignalName : NCombatCardPile.SignalName
	{
	}

	private Viewport _viewport;

	private Vector2 _posOffset;

	private static readonly Vector2 _hideOffset = new Vector2(150f, 0f);

	protected override string[] Hotkeys => new string[1] { MegaInput.viewExhaustPileAndTabRight };

	protected override PileType Pile => PileType.Exhaust;

	public override void _Ready()
	{
		ConnectSignals();
		base.Visible = false;
		_viewport = GetViewport();
		_posOffset = new Vector2(base.OffsetRight + 100f, 0f - base.OffsetBottom + 90f);
		GetTree().Root.Connect(Viewport.SignalName.SizeChanged, Callable.From(SetAnimInOutPositions));
		SetAnimInOutPositions();
		Disable();
	}

	public override void Initialize(Player player)
	{
		base.Initialize(player);
		if (Pile.GetPile(player).Cards.Count > 0)
		{
			base.Visible = true;
			base.Position = _showPosition;
			Enable();
		}
	}

	protected override void AddCard()
	{
		base.AddCard();
		if (!base.Visible)
		{
			AnimIn();
		}
		Enable();
	}

	protected override void SetAnimInOutPositions()
	{
		_showPosition = NGame.Instance.Size - _posOffset;
		_hidePosition = _showPosition + _hideOffset;
	}

	public override void AnimIn()
	{
		base.AnimIn();
		base.Visible = true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AddCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetAnimInOutPositions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.AddCard && args.Count == 0)
		{
			AddCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetAnimInOutPositions && args.Count == 0)
		{
			SetAnimInOutPositions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 0)
		{
			AnimIn();
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
		if (method == MethodName.AddCard)
		{
			return true;
		}
		if (method == MethodName.SetAnimInOutPositions)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._viewport)
		{
			_viewport = VariantUtils.ConvertTo<Viewport>(in value);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			_posOffset = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		if (name == PropertyName.Pile)
		{
			value = VariantUtils.CreateFrom<PileType>(Pile);
			return true;
		}
		if (name == PropertyName._viewport)
		{
			value = VariantUtils.CreateFrom(in _viewport);
			return true;
		}
		if (name == PropertyName._posOffset)
		{
			value = VariantUtils.CreateFrom(in _posOffset);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._viewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._posOffset, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Pile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._posOffset, Variant.From(in _posOffset));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._viewport, out var value))
		{
			_viewport = value.As<Viewport>();
		}
		if (info.TryGetProperty(PropertyName._posOffset, out var value2))
		{
			_posOffset = value2.As<Vector2>();
		}
	}
}
