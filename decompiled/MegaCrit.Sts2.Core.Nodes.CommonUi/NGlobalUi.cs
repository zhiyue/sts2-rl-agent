using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.CommonUi;

/// <summary>
/// Container for run-level global UI elements.
/// Also adjusts the Window size to meet aspect ratio requirements
/// </summary>
[ScriptPath("res://src/Core/Nodes/CommonUi/NGlobalUi.cs")]
public class NGlobalUi : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'OnWindowChange' method.
		/// </summary>
		public static readonly StringName OnWindowChange = "OnWindowChange";

		/// <summary>
		/// Cached name for the 'ReparentCard' method.
		/// </summary>
		public static readonly StringName ReparentCard = "ReparentCard";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'TopBar' property.
		/// </summary>
		public static readonly StringName TopBar = "TopBar";

		/// <summary>
		/// Cached name for the 'Overlays' property.
		/// </summary>
		public static readonly StringName Overlays = "Overlays";

		/// <summary>
		/// Cached name for the 'CapstoneContainer' property.
		/// </summary>
		public static readonly StringName CapstoneContainer = "CapstoneContainer";

		/// <summary>
		/// Cached name for the 'RelicInventory' property.
		/// </summary>
		public static readonly StringName RelicInventory = "RelicInventory";

		/// <summary>
		/// Cached name for the 'EventCardPreviewContainer' property.
		/// </summary>
		public static readonly StringName EventCardPreviewContainer = "EventCardPreviewContainer";

		/// <summary>
		/// Cached name for the 'CardPreviewContainer' property.
		/// </summary>
		public static readonly StringName CardPreviewContainer = "CardPreviewContainer";

		/// <summary>
		/// Cached name for the 'MessyCardPreviewContainer' property.
		/// </summary>
		public static readonly StringName MessyCardPreviewContainer = "MessyCardPreviewContainer";

		/// <summary>
		/// Cached name for the 'GridCardPreviewContainer' property.
		/// </summary>
		public static readonly StringName GridCardPreviewContainer = "GridCardPreviewContainer";

		/// <summary>
		/// Cached name for the 'AboveTopBarVfxContainer' property.
		/// </summary>
		public static readonly StringName AboveTopBarVfxContainer = "AboveTopBarVfxContainer";

		/// <summary>
		/// Cached name for the 'MapScreen' property.
		/// </summary>
		public static readonly StringName MapScreen = "MapScreen";

		/// <summary>
		/// Cached name for the 'MultiplayerPlayerContainer' property.
		/// </summary>
		public static readonly StringName MultiplayerPlayerContainer = "MultiplayerPlayerContainer";

		/// <summary>
		/// Cached name for the 'TimeoutOverlay' property.
		/// </summary>
		public static readonly StringName TimeoutOverlay = "TimeoutOverlay";

		/// <summary>
		/// Cached name for the 'SubmenuStack' property.
		/// </summary>
		public static readonly StringName SubmenuStack = "SubmenuStack";

		/// <summary>
		/// Cached name for the 'TargetManager' property.
		/// </summary>
		public static readonly StringName TargetManager = "TargetManager";

		/// <summary>
		/// Cached name for the '_window' field.
		/// </summary>
		public static readonly StringName _window = "_window";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const float _maxNarrowRatio = 1.3333334f;

	private const float _maxWideRatio = 2.3888888f;

	private Window _window;

	public NTopBar TopBar { get; private set; }

	public NOverlayStack Overlays { get; private set; }

	public NCapstoneContainer CapstoneContainer { get; private set; }

	public NRelicInventory RelicInventory { get; private set; }

	public Control EventCardPreviewContainer { get; private set; }

	public Control CardPreviewContainer { get; private set; }

	public NMessyCardPreviewContainer MessyCardPreviewContainer { get; private set; }

	public NGridCardPreviewContainer GridCardPreviewContainer { get; private set; }

	public Control AboveTopBarVfxContainer { get; private set; }

	public NMapScreen MapScreen { get; private set; }

	public NMultiplayerPlayerStateContainer MultiplayerPlayerContainer { get; private set; }

	public NMultiplayerTimeoutOverlay TimeoutOverlay { get; private set; }

	public NCapstoneSubmenuStack SubmenuStack { get; private set; }

	public NTargetManager TargetManager { get; private set; }

	public override void _Ready()
	{
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(OnWindowChange));
		EventCardPreviewContainer = GetNode<Control>("%EventCardPreviewContainer");
		CardPreviewContainer = GetNode<Control>("%CardPreviewContainer");
		MessyCardPreviewContainer = GetNode<NMessyCardPreviewContainer>("%MessyCardPreviewContainer");
		GridCardPreviewContainer = GetNode<NGridCardPreviewContainer>("%GridCardPreviewContainer");
		TopBar = GetNode<NTopBar>("%TopBar");
		Overlays = GetNode<NOverlayStack>("%OverlayScreensContainer");
		CapstoneContainer = GetNode<NCapstoneContainer>("%CapstoneScreenContainer");
		MapScreen = GetNode<NMapScreen>("%MapScreen");
		SubmenuStack = GetNode<NCapstoneSubmenuStack>("%CapstoneSubmenuStack");
		RelicInventory = GetNode<NRelicInventory>("%RelicInventory");
		MultiplayerPlayerContainer = GetNode<NMultiplayerPlayerStateContainer>("%MultiplayerPlayerContainer");
		TargetManager = GetNode<NTargetManager>("TargetManager");
		TimeoutOverlay = GetNode<NMultiplayerTimeoutOverlay>("%MultiplayerTimeoutOverlay");
		AboveTopBarVfxContainer = GetNode<Control>("%AboveTopBarVfxContainer");
	}

	private void OnWindowChange()
	{
		if (SaveManager.Instance.SettingsSave.AspectRatioSetting == AspectRatioSetting.Auto)
		{
			float num = (float)_window.Size.X / (float)_window.Size.Y;
			if (num > 2.3888888f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepWidth;
				_window.ContentScaleSize = new Vector2I(2580, 1080);
			}
			else if (num < 1.3333334f)
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.KeepHeight;
				_window.ContentScaleSize = new Vector2I(1680, 1260);
			}
			else
			{
				_window.ContentScaleAspect = Window.ContentScaleAspectEnum.Expand;
				_window.ContentScaleSize = new Vector2I(1680, 1080);
			}
		}
	}

	public void ReparentCard(NCard card)
	{
		Vector2 globalPosition = card.GlobalPosition;
		card.GetParent()?.RemoveChildSafely(card);
		TopBar.TrailContainer.AddChildSafely(card);
		card.GlobalPosition = globalPosition;
	}

	public void Initialize(RunState runState)
	{
		TopBar.Initialize(runState);
		MultiplayerPlayerContainer.Initialize(runState);
		RelicInventory.Initialize(runState);
		MapScreen.Initialize(runState);
		TimeoutOverlay.Initialize(RunManager.Instance.NetService, isGameLevel: false);
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnWindowChange, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReparentCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.OnWindowChange && args.Count == 0)
		{
			OnWindowChange();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReparentCard && args.Count == 1)
		{
			ReparentCard(VariantUtils.ConvertTo<NCard>(in args[0]));
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
		if (method == MethodName.OnWindowChange)
		{
			return true;
		}
		if (method == MethodName.ReparentCard)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.TopBar)
		{
			TopBar = VariantUtils.ConvertTo<NTopBar>(in value);
			return true;
		}
		if (name == PropertyName.Overlays)
		{
			Overlays = VariantUtils.ConvertTo<NOverlayStack>(in value);
			return true;
		}
		if (name == PropertyName.CapstoneContainer)
		{
			CapstoneContainer = VariantUtils.ConvertTo<NCapstoneContainer>(in value);
			return true;
		}
		if (name == PropertyName.RelicInventory)
		{
			RelicInventory = VariantUtils.ConvertTo<NRelicInventory>(in value);
			return true;
		}
		if (name == PropertyName.EventCardPreviewContainer)
		{
			EventCardPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			CardPreviewContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			MessyCardPreviewContainer = VariantUtils.ConvertTo<NMessyCardPreviewContainer>(in value);
			return true;
		}
		if (name == PropertyName.GridCardPreviewContainer)
		{
			GridCardPreviewContainer = VariantUtils.ConvertTo<NGridCardPreviewContainer>(in value);
			return true;
		}
		if (name == PropertyName.AboveTopBarVfxContainer)
		{
			AboveTopBarVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.MapScreen)
		{
			MapScreen = VariantUtils.ConvertTo<NMapScreen>(in value);
			return true;
		}
		if (name == PropertyName.MultiplayerPlayerContainer)
		{
			MultiplayerPlayerContainer = VariantUtils.ConvertTo<NMultiplayerPlayerStateContainer>(in value);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			TimeoutOverlay = VariantUtils.ConvertTo<NMultiplayerTimeoutOverlay>(in value);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			SubmenuStack = VariantUtils.ConvertTo<NCapstoneSubmenuStack>(in value);
			return true;
		}
		if (name == PropertyName.TargetManager)
		{
			TargetManager = VariantUtils.ConvertTo<NTargetManager>(in value);
			return true;
		}
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.TopBar)
		{
			value = VariantUtils.CreateFrom<NTopBar>(TopBar);
			return true;
		}
		if (name == PropertyName.Overlays)
		{
			value = VariantUtils.CreateFrom<NOverlayStack>(Overlays);
			return true;
		}
		if (name == PropertyName.CapstoneContainer)
		{
			value = VariantUtils.CreateFrom<NCapstoneContainer>(CapstoneContainer);
			return true;
		}
		if (name == PropertyName.RelicInventory)
		{
			value = VariantUtils.CreateFrom<NRelicInventory>(RelicInventory);
			return true;
		}
		Control from;
		if (name == PropertyName.EventCardPreviewContainer)
		{
			from = EventCardPreviewContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CardPreviewContainer)
		{
			from = CardPreviewContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MessyCardPreviewContainer)
		{
			value = VariantUtils.CreateFrom<NMessyCardPreviewContainer>(MessyCardPreviewContainer);
			return true;
		}
		if (name == PropertyName.GridCardPreviewContainer)
		{
			value = VariantUtils.CreateFrom<NGridCardPreviewContainer>(GridCardPreviewContainer);
			return true;
		}
		if (name == PropertyName.AboveTopBarVfxContainer)
		{
			from = AboveTopBarVfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.MapScreen)
		{
			value = VariantUtils.CreateFrom<NMapScreen>(MapScreen);
			return true;
		}
		if (name == PropertyName.MultiplayerPlayerContainer)
		{
			value = VariantUtils.CreateFrom<NMultiplayerPlayerStateContainer>(MultiplayerPlayerContainer);
			return true;
		}
		if (name == PropertyName.TimeoutOverlay)
		{
			value = VariantUtils.CreateFrom<NMultiplayerTimeoutOverlay>(TimeoutOverlay);
			return true;
		}
		if (name == PropertyName.SubmenuStack)
		{
			value = VariantUtils.CreateFrom<NCapstoneSubmenuStack>(SubmenuStack);
			return true;
		}
		if (name == PropertyName.TargetManager)
		{
			value = VariantUtils.CreateFrom<NTargetManager>(TargetManager);
			return true;
		}
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
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
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Overlays, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CapstoneContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RelicInventory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EventCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MessyCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.GridCardPreviewContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.AboveTopBarVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MapScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MultiplayerPlayerContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TimeoutOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.SubmenuStack, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TargetManager, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.TopBar, Variant.From<NTopBar>(TopBar));
		info.AddProperty(PropertyName.Overlays, Variant.From<NOverlayStack>(Overlays));
		info.AddProperty(PropertyName.CapstoneContainer, Variant.From<NCapstoneContainer>(CapstoneContainer));
		info.AddProperty(PropertyName.RelicInventory, Variant.From<NRelicInventory>(RelicInventory));
		info.AddProperty(PropertyName.EventCardPreviewContainer, Variant.From<Control>(EventCardPreviewContainer));
		info.AddProperty(PropertyName.CardPreviewContainer, Variant.From<Control>(CardPreviewContainer));
		info.AddProperty(PropertyName.MessyCardPreviewContainer, Variant.From<NMessyCardPreviewContainer>(MessyCardPreviewContainer));
		info.AddProperty(PropertyName.GridCardPreviewContainer, Variant.From<NGridCardPreviewContainer>(GridCardPreviewContainer));
		info.AddProperty(PropertyName.AboveTopBarVfxContainer, Variant.From<Control>(AboveTopBarVfxContainer));
		info.AddProperty(PropertyName.MapScreen, Variant.From<NMapScreen>(MapScreen));
		info.AddProperty(PropertyName.MultiplayerPlayerContainer, Variant.From<NMultiplayerPlayerStateContainer>(MultiplayerPlayerContainer));
		info.AddProperty(PropertyName.TimeoutOverlay, Variant.From<NMultiplayerTimeoutOverlay>(TimeoutOverlay));
		info.AddProperty(PropertyName.SubmenuStack, Variant.From<NCapstoneSubmenuStack>(SubmenuStack));
		info.AddProperty(PropertyName.TargetManager, Variant.From<NTargetManager>(TargetManager));
		info.AddProperty(PropertyName._window, Variant.From(in _window));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.TopBar, out var value))
		{
			TopBar = value.As<NTopBar>();
		}
		if (info.TryGetProperty(PropertyName.Overlays, out var value2))
		{
			Overlays = value2.As<NOverlayStack>();
		}
		if (info.TryGetProperty(PropertyName.CapstoneContainer, out var value3))
		{
			CapstoneContainer = value3.As<NCapstoneContainer>();
		}
		if (info.TryGetProperty(PropertyName.RelicInventory, out var value4))
		{
			RelicInventory = value4.As<NRelicInventory>();
		}
		if (info.TryGetProperty(PropertyName.EventCardPreviewContainer, out var value5))
		{
			EventCardPreviewContainer = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.CardPreviewContainer, out var value6))
		{
			CardPreviewContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.MessyCardPreviewContainer, out var value7))
		{
			MessyCardPreviewContainer = value7.As<NMessyCardPreviewContainer>();
		}
		if (info.TryGetProperty(PropertyName.GridCardPreviewContainer, out var value8))
		{
			GridCardPreviewContainer = value8.As<NGridCardPreviewContainer>();
		}
		if (info.TryGetProperty(PropertyName.AboveTopBarVfxContainer, out var value9))
		{
			AboveTopBarVfxContainer = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.MapScreen, out var value10))
		{
			MapScreen = value10.As<NMapScreen>();
		}
		if (info.TryGetProperty(PropertyName.MultiplayerPlayerContainer, out var value11))
		{
			MultiplayerPlayerContainer = value11.As<NMultiplayerPlayerStateContainer>();
		}
		if (info.TryGetProperty(PropertyName.TimeoutOverlay, out var value12))
		{
			TimeoutOverlay = value12.As<NMultiplayerTimeoutOverlay>();
		}
		if (info.TryGetProperty(PropertyName.SubmenuStack, out var value13))
		{
			SubmenuStack = value13.As<NCapstoneSubmenuStack>();
		}
		if (info.TryGetProperty(PropertyName.TargetManager, out var value14))
		{
			TargetManager = value14.As<NTargetManager>();
		}
		if (info.TryGetProperty(PropertyName._window, out var value15))
		{
			_window = value15.As<Window>();
		}
	}
}
