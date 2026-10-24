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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NRunHistoryPlayerIcon.cs")]
public class NRunHistoryPlayerIcon : NButton
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NButton.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Select' method.
		/// </summary>
		public static readonly StringName Select = "Select";

		/// <summary>
		/// Cached name for the 'Deselect' method.
		/// </summary>
		public static readonly StringName Deselect = "Deselect";

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
	public new class PropertyName : NButton.PropertyName
	{
		/// <summary>
		/// Cached name for the '_achievementLock' field.
		/// </summary>
		public static readonly StringName _achievementLock = "_achievementLock";

		/// <summary>
		/// Cached name for the '_ascensionIcon' field.
		/// </summary>
		public static readonly StringName _ascensionIcon = "_ascensionIcon";

		/// <summary>
		/// Cached name for the '_ascensionLabel' field.
		/// </summary>
		public static readonly StringName _ascensionLabel = "_ascensionLabel";

		/// <summary>
		/// Cached name for the '_selectionReticle' field.
		/// </summary>
		public static readonly StringName _selectionReticle = "_selectionReticle";

		/// <summary>
		/// Cached name for the '_hsv' field.
		/// </summary>
		public static readonly StringName _hsv = "_hsv";

		/// <summary>
		/// Cached name for the '_icon' field.
		/// </summary>
		public static readonly StringName _icon = "_icon";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	public static readonly string scenePath = SceneHelper.GetScenePath("screens/run_history_screen/run_history_player_icon");

	private readonly List<IHoverTip> _hoverTips = new List<IHoverTip>();

	private Control _achievementLock;

	private Control _ascensionIcon;

	private MegaLabel _ascensionLabel;

	private NSelectionReticle _selectionReticle;

	private ShaderMaterial _hsv;

	private TextureRect _icon;

	private Tween? _tween;

	private static readonly Vector2 _enabledScale = Vector2.One * 1.1f;

	private static readonly Vector2 _disabledScale = Vector2.One * 0.95f;

	public RunHistoryPlayer Player { get; private set; }

	public override void _Ready()
	{
		ConnectSignals();
		_achievementLock = GetNode<Control>("%AchievementLock");
		_ascensionIcon = GetNode<Control>("%AscensionIcon");
		_ascensionLabel = GetNode<MegaLabel>("%AscensionLabel");
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		_icon = GetNode<TextureRect>("%Icon");
		_hsv = (ShaderMaterial)_icon.GetMaterial();
		Deselect();
	}

	public void LoadRun(RunHistoryPlayer player, RunHistory history)
	{
		Player = player;
		CharacterModel characterModel = SaveUtil.CharacterOrDeprecated(player.Character);
		_icon.Texture = characterModel.IconTexture;
		LocString locString = new LocString("ascension", "PORTRAIT_TITLE");
		locString.Add("character", characterModel.Title);
		locString.Add("ascension", history.Ascension);
		LocString locString2 = new LocString("ascension", "PORTRAIT_DESCRIPTION");
		List<string> list = new List<string>();
		for (int i = 1; i <= history.Ascension; i++)
		{
			list.Add(AscensionHelper.GetTitle(i).GetFormattedText());
		}
		locString2.Add("ascensions", list);
		_achievementLock.Visible = history.GameMode.AreAchievementsAndEpochsLocked();
		_ascensionIcon.Visible = false;
		_ascensionLabel.SetTextAutoSize((history.Ascension > 0) ? history.Ascension.ToString() : string.Empty);
		LocString locString3 = new LocString("run_history", "PLAYER_HOVER");
		if (history.Players.Count > 1)
		{
			locString3.Add("PlayerName", PlatformUtil.GetPlayerName(history.PlatformType, player.Id));
			locString3.Add("CharacterName", characterModel.Title.GetFormattedText());
		}
		else
		{
			locString3.Add("PlayerName", characterModel.Title.GetFormattedText());
			locString3.Add("CharacterName", string.Empty);
		}
		if (history.Ascension > 0 || history.GameMode.AreAchievementsAndEpochsLocked())
		{
			_hoverTips.Add(AscensionHelper.GetHoverTip(characterModel, history.Ascension, history.GameMode.AreAchievementsAndEpochsLocked()));
		}
		else
		{
			_hoverTips.Add(new HoverTip(locString3));
		}
	}

	public void Select()
	{
		_hsv.SetShaderParameter(_s, 1f);
		_hsv.SetShaderParameter(_v, 1f);
		_ascensionIcon.Visible = _ascensionLabel.Text != string.Empty;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", _enabledScale, 0.05);
	}

	public void Deselect()
	{
		_hsv.SetShaderParameter(_s, 0.3f);
		_hsv.SetShaderParameter(_v, 0.55f);
		_ascensionIcon.Visible = false;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_icon, "scale", _disabledScale, 0.05);
	}

	protected override void OnFocus()
	{
		NHoverTipSet.CreateAndShow(this, _hoverTips)?.SetGlobalPosition(base.GlobalPosition + new Vector2(0f, base.Size.Y + 20f));
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
	}

	protected override void OnUnfocus()
	{
		_selectionReticle.OnDeselect();
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
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Select, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._achievementLock)
		{
			_achievementLock = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
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
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._achievementLock)
		{
			value = VariantUtils.CreateFrom(in _achievementLock);
			return true;
		}
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
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._achievementLock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ascensionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._achievementLock, Variant.From(in _achievementLock));
		info.AddProperty(PropertyName._ascensionIcon, Variant.From(in _ascensionIcon));
		info.AddProperty(PropertyName._ascensionLabel, Variant.From(in _ascensionLabel));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._achievementLock, out var value))
		{
			_achievementLock = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ascensionIcon, out var value2))
		{
			_ascensionIcon = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ascensionLabel, out var value3))
		{
			_ascensionLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value4))
		{
			_selectionReticle = value4.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value5))
		{
			_hsv = value5.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value6))
		{
			_icon = value6.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value7))
		{
			_tween = value7.As<Tween>();
		}
	}
}
