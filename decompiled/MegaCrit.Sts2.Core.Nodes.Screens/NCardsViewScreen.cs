using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens;

[ScriptPath("res://src/Core/Nodes/Screens/NCardsViewScreen.cs")]
public abstract class NCardsViewScreen : Control, ICapstoneScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignals = "ConnectSignals";

		public static readonly StringName ToggleShowUpgrades = "ToggleShowUpgrades";

		public static readonly StringName OnReturnButtonPressed = "OnReturnButtonPressed";

		public static readonly StringName AfterCapstoneOpened = "AfterCapstoneOpened";

		public static readonly StringName AfterCapstoneClosed = "AfterCapstoneClosed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName _background = "_background";

		public static readonly StringName _grid = "_grid";

		public static readonly StringName _backButton = "_backButton";

		public static readonly StringName _showUpgrades = "_showUpgrades";

		public static readonly StringName _bottomLabel = "_bottomLabel";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private ColorRect _background;

	protected NCardGrid _grid;

	protected NButton _backButton;

	private NTickbox _showUpgrades;

	private RichTextLabel _bottomLabel;

	protected List<CardModel> _cards;

	protected LocString _infoText;

	public abstract NetScreenType ScreenType { get; }

	public Control? DefaultFocusedControl => _grid.DefaultFocusedControl;

	public Control? FocusedControlFromTopBar => _grid.FocusedControlFromTopBar;

	public bool UseSharedBackstop => true;

	public override void _Ready()
	{
		if (GetType() != typeof(NCardsViewScreen))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignals();
	}

	protected virtual void ConnectSignals()
	{
		_bottomLabel = GetNode<RichTextLabel>("%BottomLabel");
		_backButton = GetNode<NButton>("BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnReturnButtonPressed));
		_backButton.Enable();
		_grid = GetNode<NCardGrid>("CardGrid");
		_grid.Connect(NCardGrid.SignalName.HolderPressed, Callable.From(delegate(NCardHolder h)
		{
			ShowCardDetail(h.CardModel);
		}));
		_grid.Connect(NCardGrid.SignalName.HolderAltPressed, Callable.From(delegate(NCardHolder h)
		{
			ShowCardDetail(h.CardModel);
		}));
		_grid.InsetForTopBar();
		_bottomLabel.Text = _infoText.GetFormattedText();
		_showUpgrades = GetNode<NTickbox>("%Upgrades");
		_showUpgrades.Connect(NTickbox.SignalName.Toggled, Callable.From<NTickbox>(ToggleShowUpgrades));
		base.ProcessMode = (ProcessModeEnum)(base.Visible ? 0 : 4);
	}

	private void ShowCardDetail(CardModel cardModel)
	{
		_backButton.Disable();
		List<CardModel> list = _grid.CurrentlyDisplayedCards.ToList();
		NInspectCardScreen inspectCardScreen = NGame.Instance.GetInspectCardScreen();
		inspectCardScreen.Open(list, list.IndexOf(cardModel), _grid.IsShowingUpgrades);
		inspectCardScreen.Connect(CanvasItem.SignalName.VisibilityChanged, Callable.From(delegate
		{
			if (!inspectCardScreen.Visible)
			{
				_backButton.Enable();
			}
		}), 4u);
	}

	private void ToggleShowUpgrades(NTickbox tickbox)
	{
		_grid.IsShowingUpgrades = tickbox.IsTicked;
	}

	protected void OnReturnButtonPressed(NButton _)
	{
		NCapstoneContainer.Instance.Close();
	}

	public virtual void AfterCapstoneOpened()
	{
		_showUpgrades.IsTicked = false;
	}

	public virtual void AfterCapstoneClosed()
	{
		base.Visible = false;
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(6);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleShowUpgrades, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tickbox", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnReturnButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterCapstoneClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignals && args.Count == 0)
		{
			ConnectSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades && args.Count == 1)
		{
			ToggleShowUpgrades(VariantUtils.ConvertTo<NTickbox>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReturnButtonPressed && args.Count == 1)
		{
			OnReturnButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened && args.Count == 0)
		{
			AfterCapstoneOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed && args.Count == 0)
		{
			AfterCapstoneClosed();
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
		if (method == MethodName.ConnectSignals)
		{
			return true;
		}
		if (method == MethodName.ToggleShowUpgrades)
		{
			return true;
		}
		if (method == MethodName.OnReturnButtonPressed)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneOpened)
		{
			return true;
		}
		if (method == MethodName.AfterCapstoneClosed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._background)
		{
			_background = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._grid)
		{
			_grid = VariantUtils.ConvertTo<NCardGrid>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._showUpgrades)
		{
			_showUpgrades = VariantUtils.ConvertTo<NTickbox>(in value);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			_bottomLabel = VariantUtils.ConvertTo<RichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		Control from;
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName._background)
		{
			value = VariantUtils.CreateFrom(in _background);
			return true;
		}
		if (name == PropertyName._grid)
		{
			value = VariantUtils.CreateFrom(in _grid);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._showUpgrades)
		{
			value = VariantUtils.CreateFrom(in _showUpgrades);
			return true;
		}
		if (name == PropertyName._bottomLabel)
		{
			value = VariantUtils.CreateFrom(in _bottomLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._grid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._showUpgrades, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bottomLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._background, Variant.From(in _background));
		info.AddProperty(PropertyName._grid, Variant.From(in _grid));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._showUpgrades, Variant.From(in _showUpgrades));
		info.AddProperty(PropertyName._bottomLabel, Variant.From(in _bottomLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._background, out var value))
		{
			_background = value.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._grid, out var value2))
		{
			_grid = value2.As<NCardGrid>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value3))
		{
			_backButton = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._showUpgrades, out var value4))
		{
			_showUpgrades = value4.As<NTickbox>();
		}
		if (info.TryGetProperty(PropertyName._bottomLabel, out var value5))
		{
			_bottomLabel = value5.As<RichTextLabel>();
		}
	}
}
