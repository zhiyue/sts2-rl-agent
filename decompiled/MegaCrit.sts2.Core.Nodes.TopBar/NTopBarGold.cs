using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarGold.cs")]
public class NTopBarGold : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName UpdateGold = "UpdateGold";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _goldLabel = "_goldLabel";

		public static readonly StringName _goldPopupLabel = "_goldPopupLabel";

		public static readonly StringName _currentGold = "_currentGold";

		public static readonly StringName _additionalGold = "_additionalGold";

		public static readonly StringName _alreadyRunning = "_alreadyRunning";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly HoverTip _hoverTip = new HoverTip(new LocString("static_hover_tips", "MONEY_POUCH.title"), new LocString("static_hover_tips", "MONEY_POUCH.description"));

	private Player? _player;

	private MegaLabel _goldLabel;

	private MegaLabel _goldPopupLabel;

	private int _currentGold;

	private int _additionalGold;

	private bool _alreadyRunning;

	public override void _Ready()
	{
		_goldLabel = GetNode<MegaLabel>("%GoldLabel");
		_goldPopupLabel = GetNode<MegaLabel>("%GoldPopup");
		_goldPopupLabel.Modulate = Colors.Transparent;
		ConnectSignals();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (_player != null)
		{
			_player.GoldChanged -= UpdateGold;
		}
	}

	public void Initialize(Player player)
	{
		_player = player;
		_currentGold = _player.Gold;
		_goldLabel.SetTextAutoSize($"{_currentGold}");
		_player.GoldChanged += UpdateGold;
	}

	private void UpdateGold()
	{
		TaskHelper.RunSafely(UpdateGoldAnim());
	}

	private async Task UpdateGoldAnim()
	{
		if (_player == null)
		{
			return;
		}
		int currentGold = _player.Gold - _currentGold;
		_additionalGold = (_currentGold = currentGold);
		_currentGold = _player.Gold;
		_goldPopupLabel.SetTextAutoSize(((_additionalGold > 0) ? "+" : "") + _additionalGold);
		if (_alreadyRunning)
		{
			return;
		}
		_alreadyRunning = true;
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_goldPopupLabel, "modulate:a", 1f, 0.15000000596046448);
		tween.TweenProperty(_goldPopupLabel, "position:y", _goldPopupLabel.Position.Y + 30f, 0.25);
		await ToSignal(tween, Tween.SignalName.Finished);
		await Task.Delay(150);
		while (_additionalGold != 0)
		{
			int num = 1;
			if (Mathf.Abs(_additionalGold) > 100)
			{
				num = 75;
			}
			else if (Mathf.Abs(_additionalGold) > 50)
			{
				num = 10;
			}
			_additionalGold = ((_additionalGold > 0) ? (_additionalGold - num) : (_additionalGold + num));
			_goldPopupLabel.SetTextAutoSize(((_additionalGold >= 0) ? "+" : "") + _additionalGold);
			_goldLabel.SetTextAutoSize($"{_player.Gold - _additionalGold}");
			await Task.Delay((int)Mathf.Lerp(10f, 20f, Mathf.Max(0, 10 - Mathf.Abs(_additionalGold))));
		}
		await Task.Delay(250);
		Tween tween2 = CreateTween().SetParallel();
		tween2.TweenProperty(_goldPopupLabel, "modulate:a", 0f, 0.10000000149011612);
		tween2.TweenProperty(_goldPopupLabel, "position:y", _goldPopupLabel.Position.Y - 30f, 0.25).FromCurrent();
		_goldLabel.SetTextAutoSize($"{_player.Gold}");
		_alreadyRunning = false;
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

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateGold, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.UpdateGold && args.Count == 0)
		{
			UpdateGold();
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
		if (method == MethodName.UpdateGold)
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
		if (name == PropertyName._goldLabel)
		{
			_goldLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._goldPopupLabel)
		{
			_goldPopupLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._currentGold)
		{
			_currentGold = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._additionalGold)
		{
			_additionalGold = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._alreadyRunning)
		{
			_alreadyRunning = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._goldLabel)
		{
			value = VariantUtils.CreateFrom(in _goldLabel);
			return true;
		}
		if (name == PropertyName._goldPopupLabel)
		{
			value = VariantUtils.CreateFrom(in _goldPopupLabel);
			return true;
		}
		if (name == PropertyName._currentGold)
		{
			value = VariantUtils.CreateFrom(in _currentGold);
			return true;
		}
		if (name == PropertyName._additionalGold)
		{
			value = VariantUtils.CreateFrom(in _additionalGold);
			return true;
		}
		if (name == PropertyName._alreadyRunning)
		{
			value = VariantUtils.CreateFrom(in _alreadyRunning);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._goldLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._goldPopupLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentGold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._additionalGold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._alreadyRunning, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._goldLabel, Variant.From(in _goldLabel));
		info.AddProperty(PropertyName._goldPopupLabel, Variant.From(in _goldPopupLabel));
		info.AddProperty(PropertyName._currentGold, Variant.From(in _currentGold));
		info.AddProperty(PropertyName._additionalGold, Variant.From(in _additionalGold));
		info.AddProperty(PropertyName._alreadyRunning, Variant.From(in _alreadyRunning));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._goldLabel, out var value))
		{
			_goldLabel = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._goldPopupLabel, out var value2))
		{
			_goldPopupLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._currentGold, out var value3))
		{
			_currentGold = value3.As<int>();
		}
		if (info.TryGetProperty(PropertyName._additionalGold, out var value4))
		{
			_additionalGold = value4.As<int>();
		}
		if (info.TryGetProperty(PropertyName._alreadyRunning, out var value5))
		{
			_alreadyRunning = value5.As<bool>();
		}
	}
}
