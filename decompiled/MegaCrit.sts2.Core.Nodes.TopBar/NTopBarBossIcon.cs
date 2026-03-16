using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarBossIcon.cs")]
public class NTopBarBossIcon : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnRoomEntered = "OnRoomEntered";

		public static readonly StringName OnActEntered = "OnActEntered";

		public static readonly StringName RefreshBossIcon = "RefreshBossIcon";

		public static readonly StringName RefreshSecondBossIconColor = "RefreshSecondBossIconColor";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName ShouldOnlyShowSecondBossIcon = "ShouldOnlyShowSecondBossIcon";

		public static readonly StringName _bossIcon = "_bossIcon";

		public static readonly StringName _bossIconOutline = "_bossIconOutline";

		public static readonly StringName _secondBossIcon = "_secondBossIcon";

		public static readonly StringName _secondBossIconOutline = "_secondBossIconOutline";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private static readonly LocString _bossHoverTipTitle = new LocString("static_hover_tips", "BOSS.title");

	private static readonly LocString _bossHoverTipDescription = new LocString("static_hover_tips", "BOSS.description");

	private static readonly LocString _doubleBossHoverTipTitle = new LocString("static_hover_tips", "DOUBLE_BOSS.title");

	private static readonly LocString _doubleBossHoverTipDescription = new LocString("static_hover_tips", "DOUBLE_BOSS.description");

	private TextureRect _bossIcon;

	private TextureRect _bossIconOutline;

	private TextureRect? _secondBossIcon;

	private TextureRect? _secondBossIconOutline;

	private static readonly StringName _tintColor = new StringName("tint_color");

	private const string _secondBossIconScenePath = "res://scenes/ui/top_bar/second_boss_icon.tscn";

	private IRunState _runState;

	private bool ShouldOnlyShowSecondBossIcon
	{
		get
		{
			if (_runState.Map.SecondBossMapPoint != null)
			{
				return _runState.CurrentMapPoint == _runState.Map.BossMapPoint;
			}
			return false;
		}
	}

	public override void _Ready()
	{
		_bossIcon = GetNode<TextureRect>("Icon");
		_bossIconOutline = GetNode<TextureRect>("Icon/Outline");
		ConnectSignals();
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
		OnActEntered();
	}

	public override void _EnterTree()
	{
		RunManager.Instance.ActEntered += OnActEntered;
		RunManager.Instance.RoomEntered += OnRoomEntered;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.ActEntered -= OnActEntered;
		RunManager.Instance.RoomEntered -= OnRoomEntered;
	}

	private void OnRoomEntered()
	{
		if (_runState.CurrentRoom != null)
		{
			AbstractRoom baseRoom = _runState.BaseRoom;
			base.Visible = baseRoom.RoomType != RoomType.Boss || ShouldOnlyShowSecondBossIcon;
			base.FocusMode = (FocusModeEnum)((baseRoom.RoomType != RoomType.Boss || ShouldOnlyShowSecondBossIcon) ? 2 : 0);
			if (ShouldOnlyShowSecondBossIcon)
			{
				RefreshBossIcon();
			}
			RefreshSecondBossIconColor();
			if (_runState.CurrentRoom.IsVictoryRoom)
			{
				_bossIcon.SetVisible(visible: false);
				_bossIconOutline.SetVisible(visible: false);
				_secondBossIcon?.SetVisible(visible: false);
				_secondBossIconOutline?.SetVisible(visible: false);
				base.FocusMode = FocusModeEnum.None;
				base.MouseFilter = MouseFilterEnum.Ignore;
			}
		}
	}

	private void OnActEntered()
	{
		RefreshBossIcon();
	}

	public void RefreshBossIcon()
	{
		EncounterModel encounterModel = (ShouldOnlyShowSecondBossIcon ? _runState.Act.SecondBossEncounter : _runState.Act.BossEncounter);
		string roomIconPath = ImageHelper.GetRoomIconPath(MapPointType.Boss, RoomType.Boss, encounterModel.Id);
		string roomIconOutlinePath = ImageHelper.GetRoomIconOutlinePath(MapPointType.Boss, RoomType.Boss, encounterModel.Id);
		_bossIcon.Texture = PreloadManager.Cache.GetTexture2D(roomIconPath);
		_bossIconOutline.Texture = PreloadManager.Cache.GetTexture2D(roomIconOutlinePath);
		EncounterModel secondBossEncounter = _runState.Act.SecondBossEncounter;
		if (secondBossEncounter != null && !ShouldOnlyShowSecondBossIcon)
		{
			if (_secondBossIcon == null)
			{
				PackedScene packedScene = GD.Load<PackedScene>("res://scenes/ui/top_bar/second_boss_icon.tscn");
				_secondBossIcon = packedScene.Instantiate<TextureRect>(PackedScene.GenEditState.Disabled);
				_secondBossIconOutline = _secondBossIcon.GetNode<TextureRect>("%Outline");
				_secondBossIcon.MouseFilter = MouseFilterEnum.Pass;
				_secondBossIconOutline.MouseFilter = MouseFilterEnum.Pass;
				_bossIcon.AddChildSafely(_secondBossIcon);
				_secondBossIcon.Position = new Vector2(30f, 22f);
			}
			string roomIconPath2 = ImageHelper.GetRoomIconPath(MapPointType.Boss, RoomType.Boss, secondBossEncounter.Id);
			string roomIconOutlinePath2 = ImageHelper.GetRoomIconOutlinePath(MapPointType.Boss, RoomType.Boss, secondBossEncounter.Id);
			_secondBossIcon.Texture = PreloadManager.Cache.GetTexture2D(roomIconPath2);
			_secondBossIconOutline.Texture = PreloadManager.Cache.GetTexture2D(roomIconOutlinePath2);
			_secondBossIcon.Visible = true;
			RefreshSecondBossIconColor();
		}
		else
		{
			_secondBossIcon?.SetVisible(visible: false);
			_secondBossIconOutline?.SetVisible(visible: false);
		}
	}

	private void RefreshSecondBossIconColor()
	{
		if (_secondBossIcon?.Material is ShaderMaterial shaderMaterial && _secondBossIconOutline?.Material is ShaderMaterial shaderMaterial2)
		{
			ActModel act = _runState.Act;
			MapPoint currentMapPoint = _runState.CurrentMapPoint;
			MapPoint bossMapPoint = _runState.Map.BossMapPoint;
			MapPoint secondBossMapPoint = _runState.Map.SecondBossMapPoint;
			Color color = ((currentMapPoint == bossMapPoint || currentMapPoint == secondBossMapPoint) ? act.MapTraveledColor : act.MapUntraveledColor);
			shaderMaterial.SetShaderParameter(_tintColor, new Vector3(color.R, color.G, color.B));
			shaderMaterial2.SetShaderParameter(_tintColor, new Vector3(color.R, color.G, color.B));
		}
	}

	protected override void OnFocus()
	{
		EncounterModel bossEncounter = _runState.Act.BossEncounter;
		EncounterModel secondBossEncounter = _runState.Act.SecondBossEncounter;
		HoverTip hoverTip;
		if (secondBossEncounter != null && !ShouldOnlyShowSecondBossIcon)
		{
			_doubleBossHoverTipTitle.Add("BossName1", bossEncounter.Title);
			_doubleBossHoverTipTitle.Add("BossName2", secondBossEncounter.Title);
			_doubleBossHoverTipDescription.Add("BossName1", bossEncounter.Title);
			_doubleBossHoverTipDescription.Add("BossName2", secondBossEncounter.Title);
			hoverTip = new HoverTip(_doubleBossHoverTipTitle, _doubleBossHoverTipDescription);
		}
		else
		{
			_bossHoverTipTitle.Add("BossName", ShouldOnlyShowSecondBossIcon ? secondBossEncounter.Title : bossEncounter.Title);
			_bossHoverTipDescription.Add("BossName", ShouldOnlyShowSecondBossIcon ? secondBossEncounter.Title : bossEncounter.Title);
			hoverTip = new HoverTip(_bossHoverTipTitle, _bossHoverTipDescription);
		}
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTip);
		nHoverTipSet.GlobalPosition = _bossIcon.GlobalPosition + new Vector2(0f, base.Size.Y + 20f);
	}

	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRoomEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshBossIcon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshSecondBossIconColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRoomEntered && args.Count == 0)
		{
			OnRoomEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActEntered && args.Count == 0)
		{
			OnActEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshBossIcon && args.Count == 0)
		{
			RefreshBossIcon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshSecondBossIconColor && args.Count == 0)
		{
			RefreshSecondBossIconColor();
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnRoomEntered)
		{
			return true;
		}
		if (method == MethodName.OnActEntered)
		{
			return true;
		}
		if (method == MethodName.RefreshBossIcon)
		{
			return true;
		}
		if (method == MethodName.RefreshSecondBossIconColor)
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
		if (name == PropertyName._bossIcon)
		{
			_bossIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._bossIconOutline)
		{
			_bossIconOutline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._secondBossIcon)
		{
			_secondBossIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._secondBossIconOutline)
		{
			_secondBossIconOutline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ShouldOnlyShowSecondBossIcon)
		{
			value = VariantUtils.CreateFrom<bool>(ShouldOnlyShowSecondBossIcon);
			return true;
		}
		if (name == PropertyName._bossIcon)
		{
			value = VariantUtils.CreateFrom(in _bossIcon);
			return true;
		}
		if (name == PropertyName._bossIconOutline)
		{
			value = VariantUtils.CreateFrom(in _bossIconOutline);
			return true;
		}
		if (name == PropertyName._secondBossIcon)
		{
			value = VariantUtils.CreateFrom(in _secondBossIcon);
			return true;
		}
		if (name == PropertyName._secondBossIconOutline)
		{
			value = VariantUtils.CreateFrom(in _secondBossIconOutline);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bossIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bossIconOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._secondBossIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._secondBossIconOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.ShouldOnlyShowSecondBossIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._bossIcon, Variant.From(in _bossIcon));
		info.AddProperty(PropertyName._bossIconOutline, Variant.From(in _bossIconOutline));
		info.AddProperty(PropertyName._secondBossIcon, Variant.From(in _secondBossIcon));
		info.AddProperty(PropertyName._secondBossIconOutline, Variant.From(in _secondBossIconOutline));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._bossIcon, out var value))
		{
			_bossIcon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._bossIconOutline, out var value2))
		{
			_bossIconOutline = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._secondBossIcon, out var value3))
		{
			_secondBossIcon = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._secondBossIconOutline, out var value4))
		{
			_secondBossIconOutline = value4.As<TextureRect>();
		}
	}
}
