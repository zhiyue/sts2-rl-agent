using System;
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

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarRoomIcon.cs")]
public class NTopBarRoomIcon : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName DebugSetMapPointTypeOverride = "DebugSetMapPointTypeOverride";

		public static readonly StringName DebugClearMapPointTypeOverride = "DebugClearMapPointTypeOverride";

		public new static readonly StringName OnFocus = "OnFocus";

		public static readonly StringName GetHoverTipPrefixForRoomType = "GetHoverTipPrefixForRoomType";

		public static readonly StringName GetHoverTipPrefixForUnknownRoomType = "GetHoverTipPrefixForUnknownRoomType";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName UpdateIcon = "UpdateIcon";

		public static readonly StringName GetCurrentMapPointType = "GetCurrentMapPointType";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName _roomIcon = "_roomIcon";

		public static readonly StringName _roomIconOutline = "_roomIconOutline";

		public static readonly StringName _debugMapPointTypeOverride = "_debugMapPointTypeOverride";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private IRunState _runState;

	private TextureRect _roomIcon;

	private TextureRect _roomIconOutline;

	private MapPointType _debugMapPointTypeOverride;

	public override void _Ready()
	{
		_roomIcon = GetNode<TextureRect>("Icon");
		_roomIconOutline = GetNode<TextureRect>("Icon/Outline");
		ConnectSignals();
	}

	public override void _EnterTree()
	{
		RunManager.Instance.RoomEntered += UpdateIcon;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.RoomEntered -= UpdateIcon;
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
		UpdateIcon();
	}

	public void DebugSetMapPointTypeOverride(MapPointType mapPointType)
	{
		if (mapPointType != MapPointType.Unassigned)
		{
			_debugMapPointTypeOverride = mapPointType;
		}
	}

	public void DebugClearMapPointTypeOverride()
	{
		_debugMapPointTypeOverride = MapPointType.Unassigned;
	}

	protected override void OnFocus()
	{
		string hoverTipPrefixForRoomType = GetHoverTipPrefixForRoomType();
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(hoverTip: new HoverTip(new LocString("static_hover_tips", hoverTipPrefixForRoomType + ".title"), new LocString("static_hover_tips", hoverTipPrefixForRoomType + ".description")), owner: _roomIcon);
		nHoverTipSet.GlobalPosition = _roomIcon.GlobalPosition + new Vector2(0f, base.Size.Y + 20f);
	}

	private string GetHoverTipPrefixForRoomType()
	{
		return GetCurrentMapPointType() switch
		{
			MapPointType.Unassigned => "ROOM_MAP", 
			MapPointType.Unknown => GetHoverTipPrefixForUnknownRoomType(), 
			MapPointType.Shop => "ROOM_MERCHANT", 
			MapPointType.Treasure => "ROOM_TREASURE", 
			MapPointType.RestSite => "ROOM_REST", 
			MapPointType.Monster => "ROOM_ENEMY", 
			MapPointType.Elite => "ROOM_ELITE", 
			MapPointType.Boss => "ROOM_BOSS", 
			MapPointType.Ancient => "ROOM_ANCIENT", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private string GetHoverTipPrefixForUnknownRoomType()
	{
		AbstractRoom baseRoom = _runState.BaseRoom;
		return baseRoom.RoomType switch
		{
			RoomType.Monster => "ROOM_UNKNOWN_ENEMY", 
			RoomType.Treasure => "ROOM_UNKNOWN_TREASURE", 
			RoomType.Shop => "ROOM_UNKNOWN_MERCHANT", 
			RoomType.Event => "ROOM_UNKNOWN_EVENT", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	protected override void OnUnfocus()
	{
		NHoverTipSet.Remove(_roomIcon);
	}

	private void UpdateIcon()
	{
		if (_runState.CurrentRoom != null)
		{
			AbstractRoom baseRoom = _runState.BaseRoom;
			ActModel act = _runState.Act;
			MapPointType currentMapPointType = GetCurrentMapPointType();
			ModelId modelId = null;
			switch (currentMapPointType)
			{
			case MapPointType.Boss:
				modelId = ((_runState.CurrentMapPoint == _runState.Map.SecondBossMapPoint) ? act.SecondBossEncounter.Id : act.BossEncounter.Id);
				break;
			case MapPointType.Ancient:
				modelId = act.Ancient.Id;
				break;
			}
			string roomIconPath = ImageHelper.GetRoomIconPath(currentMapPointType, baseRoom.RoomType, modelId);
			if (roomIconPath != null)
			{
				_roomIcon.Visible = true;
				_roomIcon.Texture = PreloadManager.Cache.GetCompressedTexture2D(roomIconPath);
			}
			else
			{
				_roomIcon.Visible = false;
			}
			string roomIconOutlinePath = ImageHelper.GetRoomIconOutlinePath(currentMapPointType, baseRoom.RoomType, modelId);
			if (roomIconOutlinePath != null)
			{
				_roomIconOutline.Visible = true;
				_roomIconOutline.Texture = PreloadManager.Cache.GetCompressedTexture2D(roomIconOutlinePath);
			}
			else
			{
				_roomIconOutline.Visible = false;
			}
			if (baseRoom.IsVictoryRoom)
			{
				_roomIcon.Visible = false;
				_roomIconOutline.Visible = false;
				base.FocusMode = FocusModeEnum.None;
				base.MouseFilter = MouseFilterEnum.Ignore;
			}
		}
	}

	private MapPointType GetCurrentMapPointType()
	{
		if (_debugMapPointTypeOverride == MapPointType.Unassigned)
		{
			return _runState.CurrentMapPoint?.PointType ?? MapPointType.Unassigned;
		}
		return _debugMapPointTypeOverride;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugSetMapPointTypeOverride, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "mapPointType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DebugClearMapPointTypeOverride, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetHoverTipPrefixForRoomType, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetHoverTipPrefixForUnknownRoomType, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateIcon, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetCurrentMapPointType, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DebugSetMapPointTypeOverride && args.Count == 1)
		{
			DebugSetMapPointTypeOverride(VariantUtils.ConvertTo<MapPointType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugClearMapPointTypeOverride && args.Count == 0)
		{
			DebugClearMapPointTypeOverride();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHoverTipPrefixForRoomType && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetHoverTipPrefixForRoomType());
			return true;
		}
		if (method == MethodName.GetHoverTipPrefixForUnknownRoomType && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetHoverTipPrefixForUnknownRoomType());
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateIcon && args.Count == 0)
		{
			UpdateIcon();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCurrentMapPointType && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<MapPointType>(GetCurrentMapPointType());
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
		if (method == MethodName.DebugSetMapPointTypeOverride)
		{
			return true;
		}
		if (method == MethodName.DebugClearMapPointTypeOverride)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.GetHoverTipPrefixForRoomType)
		{
			return true;
		}
		if (method == MethodName.GetHoverTipPrefixForUnknownRoomType)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.UpdateIcon)
		{
			return true;
		}
		if (method == MethodName.GetCurrentMapPointType)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._roomIcon)
		{
			_roomIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._roomIconOutline)
		{
			_roomIconOutline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._debugMapPointTypeOverride)
		{
			_debugMapPointTypeOverride = VariantUtils.ConvertTo<MapPointType>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._roomIcon)
		{
			value = VariantUtils.CreateFrom(in _roomIcon);
			return true;
		}
		if (name == PropertyName._roomIconOutline)
		{
			value = VariantUtils.CreateFrom(in _roomIconOutline);
			return true;
		}
		if (name == PropertyName._debugMapPointTypeOverride)
		{
			value = VariantUtils.CreateFrom(in _debugMapPointTypeOverride);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._roomIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._roomIconOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._debugMapPointTypeOverride, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._roomIcon, Variant.From(in _roomIcon));
		info.AddProperty(PropertyName._roomIconOutline, Variant.From(in _roomIconOutline));
		info.AddProperty(PropertyName._debugMapPointTypeOverride, Variant.From(in _debugMapPointTypeOverride));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._roomIcon, out var value))
		{
			_roomIcon = value.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._roomIconOutline, out var value2))
		{
			_roomIconOutline = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._debugMapPointTypeOverride, out var value3))
		{
			_debugMapPointTypeOverride = value3.As<MapPointType>();
		}
	}
}
