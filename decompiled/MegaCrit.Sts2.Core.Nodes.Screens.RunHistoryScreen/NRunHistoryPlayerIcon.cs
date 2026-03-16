using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NRunHistoryPlayerIcon.cs")]
public class NRunHistoryPlayerIcon : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Select = "Select";

		public static readonly StringName Deselect = "Deselect";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _ascensionIcon = "_ascensionIcon";

		public static readonly StringName _ascensionLabel = "_ascensionLabel";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _currentIcon = "_currentIcon";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("screens/run_history_screen/run_history_player_icon");

	private readonly List<IHoverTip> _hoverTips = new List<IHoverTip>();

	private Control _ascensionIcon;

	private MegaLabel _ascensionLabel;

	private NSelectionReticle _selectionReticle;

	private Control? _currentIcon;

	public RunHistoryPlayer Player { get; private set; }

	public override void _Ready()
	{
		_ascensionIcon = GetNode<Control>("%AscensionIcon");
		_ascensionLabel = GetNode<MegaLabel>("%AscensionLabel");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		ConnectSignals();
	}

	public void LoadRun(RunHistoryPlayer player, RunHistory history)
	{
		Player = player;
		CharacterModel byId = ModelDb.GetById<CharacterModel>(player.Character);
		_currentIcon?.QueueFreeSafely();
		_currentIcon = byId.Icon;
		this.AddChildSafely(_currentIcon);
		MoveChild(_currentIcon, 0);
		LocString locString = new LocString("ascension", "PORTRAIT_TITLE");
		locString.Add("character", byId.Title);
		locString.Add("ascension", history.Ascension);
		LocString locString2 = new LocString("ascension", "PORTRAIT_DESCRIPTION");
		List<string> list = new List<string>();
		for (int i = 1; i <= history.Ascension; i++)
		{
			list.Add(AscensionHelper.GetTitle(i).GetFormattedText());
		}
		locString2.Add("ascensions", list);
		_selectionReticle.Visible = history.Players.Count > 1;
		_ascensionIcon.Visible = false;
		_ascensionLabel.SetTextAutoSize((history.Ascension > 0) ? history.Ascension.ToString() : string.Empty);
		LocString locString3 = new LocString("run_history", "PLAYER_HOVER");
		if (history.Players.Count > 1)
		{
			locString3.Add("PlayerName", PlatformUtil.GetPlayerName(history.PlatformType, player.Id));
			locString3.Add("CharacterName", byId.Title.GetFormattedText());
		}
		else
		{
			locString3.Add("PlayerName", byId.Title.GetFormattedText());
			locString3.Add("CharacterName", string.Empty);
		}
		_hoverTips.Add(new HoverTip(locString3));
		if (history.Ascension > 0)
		{
			_hoverTips.Add(AscensionHelper.GetHoverTip(byId, history.Ascension));
		}
	}

	public void Select()
	{
		_ascensionIcon.Visible = _ascensionLabel.Text != string.Empty;
		_selectionReticle.OnSelect();
	}

	public void Deselect()
	{
		_ascensionIcon.Visible = false;
		_selectionReticle.OnDeselect();
	}

	protected override void OnFocus()
	{
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTips);
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
		list.Add(new MethodInfo(MethodName.Select, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Select && args.Count == 0)
		{
			Select();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Deselect && args.Count == 0)
		{
			Deselect();
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
		if (method == MethodName.Select)
		{
			return true;
		}
		if (method == MethodName.Deselect)
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
		if (name == PropertyName._ascensionIcon)
		{
			_ascensionIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			_ascensionLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._currentIcon)
		{
			_currentIcon = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._ascensionIcon)
		{
			value = VariantUtils.CreateFrom(in _ascensionIcon);
			return true;
		}
		if (name == PropertyName._ascensionLabel)
		{
			value = VariantUtils.CreateFrom(in _ascensionLabel);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._currentIcon)
		{
			value = VariantUtils.CreateFrom(in _currentIcon);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._ascensionIcon, Variant.From(in _ascensionIcon));
		info.AddProperty(PropertyName._ascensionLabel, Variant.From(in _ascensionLabel));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._currentIcon, Variant.From(in _currentIcon));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._ascensionIcon, out var value))
		{
			_ascensionIcon = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLabel, out var value2))
		{
			_ascensionLabel = value2.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value3))
		{
			_selectionReticle = value3.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._currentIcon, out var value4))
		{
			_currentIcon = value4.As<Control>();
		}
	}
}
