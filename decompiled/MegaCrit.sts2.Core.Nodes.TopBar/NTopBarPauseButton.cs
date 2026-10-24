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
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NTopBarButton.MethodName
	{
		/// <summary>
		/// Cached name for the 'OnRelease' method.
		/// </summary>
		public new static readonly StringName OnRelease = "OnRelease";

		/// <summary>
		/// Cached name for the 'IsOpen' method.
		/// </summary>
		public new static readonly StringName IsOpen = "IsOpen";

		/// <summary>
		/// Cached name for the '_Process' method.
		/// </summary>
		public new static readonly StringName _Process = "_Process";

		/// <summary>
		/// Cached name for the 'ToggleAnimState' method.
		/// </summary>
		public static readonly StringName ToggleAnimState = "ToggleAnimState";

		/// <summary>
		/// Cached name for the 'OnFocus' method.
		/// </summary>
		public new static readonly StringName OnFocus = "OnFocus";

		/// <summary>
		/// Cached name for the 'OnUnfocus' method.
		/// </summary>
		public new static readonly StringName OnUnfocus = "OnUnfocus";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NTopBarButton.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Hotkeys' property.
		/// </summary>
		public new static readonly StringName Hotkeys = "Hotkeys";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NTopBarButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

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
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			float targetAngle = startAngle + (float)Math.PI / 4f;
			while (num < 0.25f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, targetAngle, Ease.CubicOut(num / 0.25f));
				_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 0.4f, Ease.CubicOut(num / 0.25f)));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_icon.Rotation = targetAngle;
			_hsv?.SetShaderParameter(_v, 0.4f);
		}
	}

	protected override async Task AnimHover(CancellationTokenSource cancelToken)
	{
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			while (num < 0.5f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, -(float)Math.PI, Ease.BackOut(num / 0.5f));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_icon.Rotation = -(float)Math.PI;
		}
	}

	protected override async Task AnimUnhover(CancellationTokenSource cancelToken)
	{
		if (_icon.IsValid())
		{
			float num = 0f;
			float startAngle = _icon.Rotation;
			while (num < 1f)
			{
				_icon.Rotation = Mathf.LerpAngle(startAngle, 0f, Ease.ElasticOut(num / 1f));
				_hsv?.SetShaderParameter(_v, Mathf.Lerp(1.1f, 0.9f, Ease.ExpoOut(num / 1f)));
				_icon.Scale = NTopBarButton._hoverScale.Lerp(Vector2.One, Ease.ExpoOut(num / 1f));
				float num2 = num;
				num = num2 + await this.AwaitProcessFrame(cancelToken.Token);
			}
			_hsv?.SetShaderParameter(_v, 0.9f);
			_icon.Rotation = 0f;
			_icon.Scale = Vector2.One;
		}
	}

	/// <summary>
	/// Toggles the anim state of this button.
	/// Utilized when an external UI (ie BackButton) closes this screen.
	/// </summary>
	public void ToggleAnimState()
	{
		UpdateScreenOpen();
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		HoverTip hoverTip = new HoverTip(new LocString("static_hover_tips", "SETTINGS.title"), new LocString("static_hover_tips", "SETTINGS.description"));
		NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, hoverTip);
		nHoverTipSet?.SetGlobalPosition(base.GlobalPosition + new Vector2(base.Size.X - nHoverTipSet.Size.X, base.Size.Y + 20f));
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		NHoverTipSet.Remove(this);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
