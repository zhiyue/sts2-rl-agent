using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.TopBar;

[ScriptPath("res://src/Core/Nodes/TopBar/NTopBarPauseButton.cs")]
public class NTopBarPauseButton : NTopBarButton
{
	public new class MethodName : NTopBarButton.MethodName
	{
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
	}

	public new class SignalName : NTopBarButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly HoverTip _hoverTip = new HoverTip(new LocString("static_hover_tips", "SETTINGS.title"), new LocString("static_hover_tips", "SETTINGS.description"));

	private const float _hoverAngle = -(float)Math.PI;

	private const float _hoverShaderV = 1.1f;

	private const float _defaultV = 0.9f;

	private const float _pressDownV = 0.4f;

	private IRunState _runState;

	protected override string[] Hotkeys => new string[1] { MegaInput.pauseAndBack };

	protected override void OnRelease()
	{
		base.OnRelease();
		if (IsOpen())
		{
			NCapstoneContainer.Instance.Close();
		}
		else
		{
			NPauseMenu nPauseMenu = (NPauseMenu)NRun.Instance.GlobalUi.SubmenuStack.ShowScreen(CapstoneSubmenuType.PauseMenu);
			nPauseMenu.Initialize(_runState);
		}
		UpdateScreenOpen();
		_hsv?.SetShaderParameter(_v, 0.9f);
	}

	protected override bool IsOpen()
	{
		if (NCapstoneContainer.Instance.CurrentCapstoneScreen is NCapstoneSubmenuStack nCapstoneSubmenuStack)
		{
			return nCapstoneSubmenuStack.ScreenType == NetScreenType.PauseMenu;
		}
		return false;
	}

	public override void _Process(double delta)
	{
		if (base.IsScreenOpen)
		{
			_icon.Rotation += (float)delta;
		}
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
	}

	protected override async Task AnimPressDown(CancellationTokenSource cancelToken)
	{
		float timer = 0f;
		float startAngle = _icon.Rotation;
		float targetAngle = startAngle + (float)Math.PI / 4f;
		for (; timer < 0.25f; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			_icon.Rotation = Mathf.LerpAngle(startAngle, targetAngle, Ease.CubicOut(timer / 0.25f));
			_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 0.4f, Ease.CubicOut(timer / 0.25f)));
			if (!this.IsValid() || !IsInsideTree())
			{
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_icon.Rotation = targetAngle;
		_hsv?.SetShaderParameter(_v, 0.4f);
	}

	protected override async Task AnimHover(CancellationTokenSource cancelToken)
	{
		float timer = 0f;
		float startAngle = _icon.Rotation;
		for (; timer < 0.5f; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			_icon.Rotation = Mathf.LerpAngle(startAngle, -(float)Math.PI, Ease.BackOut(timer / 0.5f));
			if (!this.IsValid() || !IsInsideTree())
			{
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_icon.Rotation = -(float)Math.PI;
	}

	protected override async Task AnimUnhover(CancellationTokenSource cancelToken)
	{
		float timer = 0f;
		float startAngle = _icon.Rotation;
		for (; timer < 1f; timer += (float)GetProcessDeltaTime())
		{
			if (cancelToken.IsCancellationRequested)
			{
				return;
			}
			_icon.Rotation = Mathf.LerpAngle(startAngle, 0f, Ease.ElasticOut(timer / 1f));
			_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 0.9f, Ease.ExpoOut(timer / 1f)));
			_icon.Scale = NTopBarButton._hoverScale.Lerp(Vector2.One, Ease.ExpoOut(timer / 1f));
			if (!this.IsValid() || !IsInsideTree())
			{
				return;
			}
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
		_hsv?.SetShaderParameter(_v, 0.9f);
		_icon.Rotation = 0f;
		_icon.Scale = Vector2.One;
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
		List<MethodInfo> list = new List<MethodInfo>(6);
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
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hotkeys)
		{
			value = VariantUtils.CreateFrom<string[]>(Hotkeys);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.PackedStringArray, PropertyName.Hotkeys, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
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
