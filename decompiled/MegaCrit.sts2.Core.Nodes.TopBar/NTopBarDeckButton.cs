using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarDeckButton.cs")]
public class NTopBarDeckButton : NTopBarButton
{
	public new class MethodName : NTopBarButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnPileContentsChanged = "OnPileContentsChanged";

		public new static readonly StringName OnRelease = "OnRelease";

		public new static readonly StringName IsOpen = "IsOpen";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName ToggleAnimState = "ToggleAnimState";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NTopBarButton.PropertyName
	{
		public new static readonly StringName Hotkeys = "Hotkeys";

		public static readonly StringName _elapsedTime = "_elapsedTime";

		public static readonly StringName _rockBaseRotation = "_rockBaseRotation";

		public static readonly StringName _countLabel = "_countLabel";

		public static readonly StringName _count = "_count";

		public static readonly StringName _bumpTween = "_bumpTween";
	}

	public new class SignalName : NTopBarButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly HoverTip _hoverTip = new HoverTip(new LocString("static_hover_tips", "DECK.title"), new LocString("static_hover_tips", "DECK.description"));

	private float _elapsedTime;

	private const float _rockSpeed = 4f;

	private const float _rockDist = 0.12f;

	private float _rockBaseRotation;

	private const float _defaultV = 0.9f;

	private Player _player;

	private CardPile _pile;

	private MegaLabel _countLabel;

	private float _count;

	private Tween? _bumpTween;

	protected override string[] Hotkeys => new string[1] { MegaInput.viewDeckAndTabLeft };

	public override void _Ready()
	{
		InitTopBarButton();
		_countLabel = GetNode<MegaLabel>("DeckCardCount");
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		_pile.CardAddFinished -= OnPileContentsChanged;
		_pile.CardRemoveFinished -= OnPileContentsChanged;
	}

	public void Initialize(Player player)
	{
		_player = player;
		_pile = PileType.Deck.GetPile(player);
		_pile.CardAddFinished += OnPileContentsChanged;
		_pile.CardRemoveFinished += OnPileContentsChanged;
		OnPileContentsChanged();
	}

	private void OnPileContentsChanged()
	{
		int count = _pile.Cards.Count;
		if ((float)count > _count)
		{
			_bumpTween?.Kill();
			_bumpTween = CreateTween();
			_bumpTween.TweenProperty(_countLabel, "scale", Vector2.One, 0.5).From(Vector2.One * 1.5f).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
			_countLabel.PivotOffset = _countLabel.Size * 0.5f;
			_count = count;
		}
		_countLabel.SetTextAutoSize(count.ToString());
	}

	protected override void OnRelease()
	{
		base.OnRelease();
		if (IsOpen())
		{
			NCapstoneContainer.Instance.Close();
		}
		else
		{
			NDeckViewScreen.ShowScreen(_player);
		}
		UpdateScreenOpen();
		_hsv?.SetShaderParameter(_v, 0.9f);
	}

	protected override bool IsOpen()
	{
		return NCapstoneContainer.Instance.CurrentCapstoneScreen is NDeckViewScreen;
	}

	public override void _Process(double delta)
	{
		if (base.IsScreenOpen)
		{
			_elapsedTime += (float)delta * 4f;
			_icon.Rotation = _rockBaseRotation + 0.12f * Mathf.Sin(_elapsedTime);
			_rockBaseRotation = (float)Mathf.Lerp(_rockBaseRotation, 0.0, delta);
		}
	}

	public void ToggleAnimState()
	{
		UpdateScreenOpen();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(base.Size.X - nHoverTipSet.Size.X, base.Size.Y + 20f);
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		NHoverTipSet.Remove(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPileContentsChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsOpen, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ToggleAnimState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPileContentsChanged && args.Count == 0)
		{
			OnPileContentsChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsOpen && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsOpen());
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleAnimState && args.Count == 0)
		{
			ToggleAnimState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnPileContentsChanged)
		{
			return true;
		}
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.IsOpen)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.ToggleAnimState)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._elapsedTime)
		{
			_elapsedTime = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._rockBaseRotation)
		{
			_rockBaseRotation = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._countLabel)
		{
			_countLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._count)
		{
			_count = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._bumpTween)
		{
			_bumpTween = VariantUtils.ConvertTo<Tween>(in value);
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
		if (name == PropertyName._elapsedTime)
		{
			value = VariantUtils.CreateFrom(in _elapsedTime);
			return true;
		}
		if (name == PropertyName._rockBaseRotation)
		{
			value = VariantUtils.CreateFrom(in _rockBaseRotation);
			return true;
		}
		if (name == PropertyName._countLabel)
		{
			value = VariantUtils.CreateFrom(in _countLabel);
			return true;
		}
		if (name == PropertyName._count)
		{
			value = VariantUtils.CreateFrom(in _count);
			return true;
		}
		if (name == PropertyName._bumpTween)
		{
			value = VariantUtils.CreateFrom(in _bumpTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._elapsedTime, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._rockBaseRotation, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._countLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._count, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bumpTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._elapsedTime, Variant.From(in _elapsedTime));
		info.AddProperty(PropertyName._rockBaseRotation, Variant.From(in _rockBaseRotation));
		info.AddProperty(PropertyName._countLabel, Variant.From(in _countLabel));
		info.AddProperty(PropertyName._count, Variant.From(in _count));
		info.AddProperty(PropertyName._bumpTween, Variant.From(in _bumpTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._elapsedTime, out var value))
		{
			_elapsedTime = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._rockBaseRotation, out var value2))
		{
			_rockBaseRotation = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._countLabel, out var value3))
		{
			_countLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._count, out var value4))
		{
			_count = value4.As<float>();
		}
		if (info.TryGetProperty(PropertyName._bumpTween, out var value5))
		{
			_bumpTween = value5.As<Tween>();
		}
	}
}
