using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

/// <summary>
/// Node script for the Exhaust Pile.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Combat/NExhaustPileButton.cs")]
public class NExhaustPileButton : NCombatCardPile
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NCombatCardPile.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'AddCard' method.
		/// </summary>
		public new static readonly StringName AddCard = "AddCard";

		/// <summary>
		/// Cached name for the 'SetAnimInOutPositions' method.
		/// </summary>
		public new static readonly StringName SetAnimInOutPositions = "SetAnimInOutPositions";

		/// <summary>
		/// Cached name for the 'AnimIn' method.
		/// </summary>
		public new static readonly StringName AnimIn = "AnimIn";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NCombatCardPile.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";

		/// <summary>
		/// Cached name for the 'Pile' property.
		/// </summary>
		public new static readonly StringName Pile = "Pile";

		/// <summary>
		/// Cached name for the '_viewport' field.
		/// </summary>
		public static readonly StringName _viewport = "_viewport";

		/// <summary>
		/// Cached name for the '_posOffset' field.
		/// </summary>
		public static readonly StringName _posOffset = "_posOffset";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
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

	/// <summary>
	/// The text "bump" animation for pile UI.
	/// </summary>
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._viewport, Variant.From(in _viewport));
		info.AddProperty(PropertyName._posOffset, Variant.From(in _posOffset));
	}

	/// <inheritdoc />
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
