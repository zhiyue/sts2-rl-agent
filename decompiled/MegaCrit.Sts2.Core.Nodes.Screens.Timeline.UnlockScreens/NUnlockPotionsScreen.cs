using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;

/// <summary>
/// Unlock screen which reveals which potions you unlocked after slotting an Epoch.
/// We want to let players know that these potions show up on future runs and
/// allow them to inspect each potions.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/Timeline/UnlockScreens/NUnlockPotionsScreen.cs")]
public class NUnlockPotionsScreen : NUnlockScreen
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NUnlockScreen.MethodName
	{
		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Open' method.
		/// </summary>
		public new static readonly StringName Open = "Open";

		/// <summary>
		/// Cached name for the 'OnScreenClose' method.
		/// </summary>
		public new static readonly StringName OnScreenClose = "OnScreenClose";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NUnlockScreen.PropertyName
	{
		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public new static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_potionRow' field.
		/// </summary>
		public static readonly StringName _potionRow = "_potionRow";

		/// <summary>
		/// Cached name for the '_banner' field.
		/// </summary>
		public static readonly StringName _banner = "_banner";

		/// <summary>
		/// Cached name for the '_potionTween' field.
		/// </summary>
		public static readonly StringName _potionTween = "_potionTween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NUnlockScreen.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("timeline_screen/unlock_potions_screen");

	private Control _potionRow;

	private NCommonBanner _banner;

	private IReadOnlyList<PotionModel> _potions;

	private Tween? _potionTween;

	private const float _potionXOffset = 350f;

	private static readonly Vector2 _potionScale = Vector2.One * 3f;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public override Control? DefaultFocusedControl
	{
		get
		{
			if (_potionRow.GetChildCount() != 0)
			{
				return _potionRow.GetChild<Control>(_potionRow.GetChildCount() / 2);
			}
			return null;
		}
	}

	public static NUnlockPotionsScreen Create()
	{
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NUnlockPotionsScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		ConnectSignals();
		_banner = GetNode<NCommonBanner>("%Banner");
		_banner.label.SetTextAutoSize(new LocString("timeline", "UNLOCK_POTIONS_BANNER").GetRawText());
		_banner.AnimateIn();
		_potionRow = GetNode<Control>("%PotionRow");
		LocString locString = new LocString("timeline", "UNLOCK_POTIONS");
		GetNode<MegaRichTextLabel>("%ExplanationText").Text = "[center]" + locString.GetFormattedText() + "[/center]";
	}

	public override void Open()
	{
		base.Open();
		SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_unlock");
		_potionTween = CreateTween().SetParallel();
		int num = -1;
		foreach (PotionModel potion in _potions)
		{
			NPotionHolder nPotionHolder = NPotionHolder.Create(isUsable: false);
			_potionRow.AddChildSafely(nPotionHolder);
			nPotionHolder.Position = base.Position;
			nPotionHolder.Modulate = StsColors.transparentBlack;
			nPotionHolder.Scale = _potionScale;
			NPotion nPotion = NPotion.Create(potion.ToMutable());
			nPotionHolder.AddPotion(nPotion);
			nPotion.Position = Vector2.Zero;
			_potionTween.TweenProperty(nPotionHolder, "position", base.Position + Vector2.Right * 350f * num, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_potionTween.TweenProperty(nPotionHolder, "modulate", Colors.White, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			num++;
		}
		ActiveScreenContext.Instance.FocusOnDefaultControl();
	}

	public void SetPotions(IReadOnlyList<PotionModel> potions)
	{
		_potions = potions;
	}

	protected override void OnScreenClose()
	{
		NTimelineScreen.Instance.EnableInput();
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
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Open, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnScreenClose, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NUnlockPotionsScreen>(Create());
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Open && args.Count == 0)
		{
			Open();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnScreenClose && args.Count == 0)
		{
			OnScreenClose();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NUnlockPotionsScreen>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Open)
		{
			return true;
		}
		if (method == MethodName.OnScreenClose)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._potionRow)
		{
			_potionRow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._potionTween)
		{
			_potionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._potionRow)
		{
			value = VariantUtils.CreateFrom(in _potionRow);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._potionTween)
		{
			value = VariantUtils.CreateFrom(in _potionTween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionRow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._potionRow, Variant.From(in _potionRow));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._potionTween, Variant.From(in _potionTween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._potionRow, out var value))
		{
			_potionRow = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value2))
		{
			_banner = value2.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._potionTween, out var value3))
		{
			_potionTween = value3.As<Tween>();
		}
	}
}
