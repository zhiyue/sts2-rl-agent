using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

[ScriptPath("res://src/Core/Nodes/Screens/CardSelection/NCardGridSelectionScreen.cs")]
public abstract class NCardGridSelectionScreen : Control, IOverlayScreen, IScreenContext, ICardSelector
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName ConnectSignalsAndInitGrid = "ConnectSignalsAndInitGrid";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName SetPeekButtonTargets = "SetPeekButtonTargets";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _grid = "_grid";

		public static readonly StringName _peekButton = "_peekButton";
	}

	public new class SignalName : Control.SignalName
	{
	}

	protected NCardGrid _grid;

	protected NPeekButton _peekButton;

	protected IReadOnlyList<CardModel> _cards;

	protected readonly TaskCompletionSource<IEnumerable<CardModel>> _completionSource = new TaskCompletionSource<IEnumerable<CardModel>>();

	public NetScreenType ScreenType => NetScreenType.CardSelection;

	protected abstract IEnumerable<Control> PeekButtonTargets { get; }

	public bool UseSharedBackstop => true;

	public virtual Control? DefaultFocusedControl
	{
		get
		{
			if (_peekButton.IsPeeking)
			{
				return NCombatRoom.Instance.DefaultFocusedControl;
			}
			return _grid.DefaultFocusedControl;
		}
	}

	public virtual Control? FocusedControlFromTopBar
	{
		get
		{
			if (_peekButton.IsPeeking)
			{
				return NCombatRoom.Instance.FocusedControlFromTopBar;
			}
			return _grid.FocusedControlFromTopBar;
		}
	}

	public override void _Ready()
	{
		if (GetType() != typeof(NCardGridSelectionScreen))
		{
			Log.Error($"{GetType()}");
			throw new InvalidOperationException("Don't call base._Ready()! Call ConnectSignals() instead.");
		}
		ConnectSignalsAndInitGrid();
	}

	protected virtual void ConnectSignalsAndInitGrid()
	{
		_grid = GetNode<NCardGrid>("%CardGrid");
		NCardGrid grid = _grid;
		IReadOnlyList<CardModel> cards = _cards;
		int num = 1;
		List<SortingOrders> list = new List<SortingOrders>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<SortingOrders> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = SortingOrders.Ascending;
		grid.SetCards(cards, PileType.None, list);
		_grid.Connect(NCardGrid.SignalName.HolderPressed, Callable.From(delegate(NCardHolder h)
		{
			OnCardClicked(h.CardModel);
		}));
		_grid.Connect(NCardGrid.SignalName.HolderAltPressed, Callable.From(delegate(NCardHolder h)
		{
			ShowCardDetail(h.CardModel);
		}));
		_grid.InsetForTopBar();
		_peekButton = GetNode<NPeekButton>("%PeekButton");
		_peekButton.Connect(NPeekButton.SignalName.Toggled, Callable.From<NPeekButton>(delegate
		{
			if (_peekButton.IsPeeking)
			{
				base.MouseFilter = MouseFilterEnum.Ignore;
			}
			else
			{
				base.MouseFilter = MouseFilterEnum.Stop;
				ActiveScreenContext.Instance.Update();
			}
		}));
		Callable.From(SetPeekButtonTargets).CallDeferred();
	}

	protected abstract void OnCardClicked(CardModel card);

	public async Task<IEnumerable<CardModel>> CardsSelected()
	{
		return await _completionSource.Task;
	}

	public override void _ExitTree()
	{
		if (!_completionSource.Task.IsCompleted)
		{
			_completionSource.SetCanceled();
		}
	}

	private void SetPeekButtonTargets()
	{
		HashSet<Control> hashSet = new HashSet<Control> { _grid };
		hashSet.UnionWith(PeekButtonTargets);
		_peekButton.AddTargets(hashSet.ToArray());
	}

	public virtual void AfterOverlayOpened()
	{
	}

	public virtual void AfterOverlayClosed()
	{
		_peekButton.SetPeeking(isPeeking: false);
		this.QueueFreeSafely();
	}

	public virtual void AfterOverlayShown()
	{
		base.Visible = true;
		if (CombatManager.Instance.IsInProgress)
		{
			_peekButton.Enable();
		}
	}

	public virtual void AfterOverlayHidden()
	{
		base.Visible = false;
		_peekButton.Disable();
	}

	private void ShowCardDetail(CardModel card)
	{
		if (!NControllerManager.Instance.IsUsingController)
		{
			NGame.Instance.GetInspectCardScreen().Open(_cards.ToList(), _cards.IndexOf(card), _grid.IsShowingUpgrades);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectSignalsAndInitGrid, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetPeekButtonTargets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ConnectSignalsAndInitGrid && args.Count == 0)
		{
			ConnectSignalsAndInitGrid();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPeekButtonTargets && args.Count == 0)
		{
			SetPeekButtonTargets();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName.ConnectSignalsAndInitGrid)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SetPeekButtonTargets)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._grid)
		{
			_grid = VariantUtils.ConvertTo<NCardGrid>(in value);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			_peekButton = VariantUtils.ConvertTo<NPeekButton>(in value);
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
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
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
		if (name == PropertyName._grid)
		{
			value = VariantUtils.CreateFrom(in _grid);
			return true;
		}
		if (name == PropertyName._peekButton)
		{
			value = VariantUtils.CreateFrom(in _peekButton);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._grid, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._peekButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._grid, Variant.From(in _grid));
		info.AddProperty(PropertyName._peekButton, Variant.From(in _peekButton));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._grid, out var value))
		{
			_grid = value.As<NCardGrid>();
		}
		if (info.TryGetProperty(PropertyName._peekButton, out var value2))
		{
			_peekButton = value2.As<NPeekButton>();
		}
	}
}
