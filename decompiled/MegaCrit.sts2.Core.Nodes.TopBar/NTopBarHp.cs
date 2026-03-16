using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarHp.cs")]
public class NTopBarHp : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateHealth = "UpdateHealth";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateHpTween = "UpdateHpTween";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _hpLabel = "_hpLabel";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly HoverTip _hoverTip = new HoverTip(new LocString("static_hover_tips", "HIT_POINTS.title"), new LocString("static_hover_tips", "HIT_POINTS.description"));

	private Player? _player;

	private MegaLabel _hpLabel;

	public override void _Ready()
	{
		_hpLabel = GetNode<MegaLabel>("%HpLabel");
		ConnectSignals();
	}

	public override void _ExitTree()
	{
		if (_player != null)
		{
			_player.Creature.CurrentHpChanged -= UpdateHealth;
			_player.Creature.MaxHpChanged -= UpdateHealth;
		}
	}

	public void Initialize(Player player)
	{
		_player = player;
		_player.Creature.CurrentHpChanged += UpdateHealth;
		_player.Creature.MaxHpChanged += UpdateHealth;
		UpdateHealth(0, 0);
	}

	private void UpdateHealth(int _, int __)
	{
		if (_player != null)
		{
			Creature creature = _player.Creature;
			int currentHp = creature.CurrentHp;
			int maxHp = creature.MaxHp;
			_hpLabel.SetTextAutoSize($"{currentHp}/{maxHp}");
		}
	}

	protected override void OnFocus()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
		nHoverTipSet.GlobalPosition = base.GlobalPosition + new Vector2(0f, base.Size.Y + 20f);
	}

	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(this);
	}

	public async Task LerpAtNeow()
	{
		if (_player != null)
		{
			_hpLabel.SetTextAutoSize($"0/{_player.Creature.MaxHp}");
			await Cmd.Wait(0.5f);
			Tween tween = CreateTween();
			tween.TweenMethod(Callable.From<float>(UpdateHpTween), 0f, 1f, 1.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
		}
	}

	private void UpdateHpTween(float tweenAmount)
	{
		if (_player != null)
		{
			Creature creature = _player.Creature;
			int value = (int)Math.Round((float)creature.CurrentHp * tweenAmount);
			int maxHp = creature.MaxHp;
			_hpLabel.SetTextAutoSize($"{value}/{maxHp}");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHealth, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "_", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "__", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateHpTween, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "tweenAmount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
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
		if (method == MethodName.UpdateHealth && args.Count == 2)
		{
			UpdateHealth(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
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
		if (method == MethodName.UpdateHpTween && args.Count == 1)
		{
			UpdateHpTween(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.UpdateHealth)
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
		if (method == MethodName.UpdateHpTween)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._hpLabel)
		{
			_hpLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._hpLabel)
		{
			value = VariantUtils.CreateFrom(in _hpLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._hpLabel, Variant.From(in _hpLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._hpLabel, out var value))
		{
			_hpLabel = value.As<MegaLabel>();
		}
	}
}
