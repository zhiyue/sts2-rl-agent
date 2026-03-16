using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NTreasureRoom.cs")]
public class NTreasureRoom : Control, IScreenContext, IRoomWithProceedButton
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnProceedButtonPressed = "OnProceedButtonPressed";

		public static readonly StringName OnProceedButtonReleased = "OnProceedButtonReleased";

		public static readonly StringName OnChestButtonReleased = "OnChestButtonReleased";

		public static readonly StringName OnMouseEntered = "OnMouseEntered";

		public static readonly StringName OnMouseExited = "OnMouseExited";

		public static readonly StringName UpdateChestSkin = "UpdateChestSkin";

		public static readonly StringName OnActiveScreenChanged = "OnActiveScreenChanged";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ProceedButton = "ProceedButton";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _chestButton = "_chestButton";

		public static readonly StringName _chestNode = "_chestNode";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _goldParticles = "_goldParticles";

		public static readonly StringName _relicCollection = "_relicCollection";

		public static readonly StringName _isRelicCollectionOpen = "_isRelicCollectionOpen";

		public static readonly StringName _hasRelicBeenClaimed = "_hasRelicBeenClaimed";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private TreasureRoom _room;

	private IRunState _runState;

	private NCommonBanner _banner;

	private NButton _chestButton;

	private Node2D _chestNode;

	private MegaSprite _chestAnimController;

	private NProceedButton _proceedButton;

	private MegaSkin _regularChestSkin;

	private MegaSkin _outlineChestSkin;

	private GpuParticles2D _goldParticles;

	private NTreasureRoomRelicCollection _relicCollection;

	private static readonly string _scenePath = SceneHelper.GetScenePath("rooms/treasure_room");

	private bool _isRelicCollectionOpen;

	private bool _hasRelicBeenClaimed;

	public NProceedButton ProceedButton => _proceedButton;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public Control? DefaultFocusedControl
	{
		get
		{
			if (!_isRelicCollectionOpen)
			{
				return null;
			}
			return _relicCollection.DefaultFocusedControl;
		}
	}

	public static NTreasureRoom? Create(TreasureRoom room, IRunState runState)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NTreasureRoom nTreasureRoom = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NTreasureRoom>(PackedScene.GenEditState.Disabled);
		nTreasureRoom._room = room;
		nTreasureRoom._runState = runState;
		return nTreasureRoom;
	}

	public override void _Ready()
	{
		_banner = GetNode<NCommonBanner>("%Banner");
		if (_runState.Players.Count == 1)
		{
			_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "TREASURE_BANNER").GetFormattedText());
		}
		else
		{
			_banner.label.SetTextAutoSize(new LocString("gameplay_ui", "CHOOSE_SHARED_RELIC_HEADER").GetFormattedText());
		}
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		_chestNode = GetNode<Node2D>("%ChestVisual");
		_chestAnimController = new MegaSprite(_chestNode);
		_goldParticles = GetNode<GpuParticles2D>("%GoldExplosion");
		_relicCollection = GetNode<NTreasureRoomRelicCollection>("%RelicCollection");
		_relicCollection.Initialize(_runState);
		_relicCollection.Visible = false;
		_chestAnimController.SetSkeletonDataRes(_runState.Act.ChestSpineResource);
		MegaSkeleton skeleton = _chestAnimController.GetSkeleton();
		MegaSkeletonDataResource data = skeleton.GetData();
		_regularChestSkin = data.FindSkin(_runState.Act.ChestSpineSkinNameNormal);
		_outlineChestSkin = data.FindSkin(_runState.Act.ChestSpineSkinNameStroke);
		skeleton.SetSlotsToSetupPose();
		_chestAnimController.GetAnimationState().Apply(skeleton);
		MegaAnimationState animationState = _chestAnimController.GetAnimationState();
		animationState.SetAnimation("animation", loop: false);
		_chestAnimController.GetAnimationState().AddAnimation("shine_fade", 0f, loop: false);
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnProceedButtonPressed));
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		animationState.SetTimeScale(0f);
		UpdateChestSkin(showOutline: false);
		_chestButton = GetNode<NButton>("%Chest");
		_chestButton.Connect(Control.SignalName.MouseEntered, Callable.From(OnMouseEntered));
		_chestButton.Connect(Control.SignalName.MouseExited, Callable.From(OnMouseExited));
		_chestButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnChestButtonReleased));
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenChanged;
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= OnActiveScreenChanged;
	}

	private void OnProceedButtonPressed(NButton _)
	{
		TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen());
	}

	private void OnProceedButtonReleased(NButton _)
	{
		NMapScreen.Instance.Open();
	}

	private void OnChestButtonReleased(NButton _)
	{
		TaskHelper.RunSafely(OpenChest());
		_chestButton.Disable();
	}

	private void OnMouseEntered()
	{
		UpdateChestSkin(showOutline: true);
	}

	private void OnMouseExited()
	{
		UpdateChestSkin(showOutline: false);
	}

	private async Task OpenChest()
	{
		_banner.AnimateIn();
		_proceedButton.Disable();
		UpdateChestSkin(showOutline: false);
		SfxCmd.Play(_runState.Act.ChestOpenSfx);
		_chestAnimController.GetAnimationState().SetTimeScale(1f);
		_chestButton.MouseFilter = MouseFilterEnum.Ignore;
		int num = await _room.DoNormalRewards();
		if (num > 0)
		{
			_goldParticles.Amount = num;
			_goldParticles.Emitting = true;
		}
		await _room.DoExtraRewardsIfNeeded();
		_relicCollection.InitializeRelics();
		_relicCollection.AnimIn(_chestNode);
		_isRelicCollectionOpen = true;
		ActiveScreenContext.Instance.Update();
		TaskHelper.RunSafely(RelicFtueCheck());
		await _relicCollection.RelicPickingFinished();
		_isRelicCollectionOpen = false;
		_banner.AnimateOut();
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		_proceedButton.Enable();
		_hasRelicBeenClaimed = true;
		_relicCollection.AnimOut(_chestNode);
	}

	private async Task RelicFtueCheck()
	{
		if (!SaveManager.Instance.SeenFtue("obtain_relic_ftue"))
		{
			_relicCollection.SetSelectionEnabled(isEnabled: false);
			await Cmd.Wait(1f);
			Control relicReward = ((!_relicCollection.SingleplayerRelicHolder.Visible) ? ((Control)_relicCollection) : ((Control)_relicCollection.SingleplayerRelicHolder));
			_relicCollection.SetSelectionEnabled(isEnabled: true);
			NModalContainer.Instance.Add(NRelicRewardFtue.Create(relicReward));
			SaveManager.Instance.MarkFtueAsComplete("obtain_relic_ftue");
		}
	}

	private void UpdateChestSkin(bool showOutline)
	{
		MegaSkeleton skeleton = _chestAnimController.GetSkeleton();
		skeleton.SetSkin(showOutline ? _outlineChestSkin : _regularChestSkin);
		skeleton.SetSlotsToSetupPose();
		_chestAnimController.GetAnimationState().Apply(skeleton);
	}

	private void OnActiveScreenChanged()
	{
		this.UpdateControllerNavEnabled();
		if (ActiveScreenContext.Instance.IsCurrent(this) && _hasRelicBeenClaimed)
		{
			_proceedButton.Enable();
		}
		else
		{
			_proceedButton.Disable();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnChestButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMouseEntered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMouseExited, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateChestSkin, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "showOutline", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnProceedButtonPressed && args.Count == 1)
		{
			OnProceedButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnProceedButtonReleased && args.Count == 1)
		{
			OnProceedButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChestButtonReleased && args.Count == 1)
		{
			OnChestButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseEntered && args.Count == 0)
		{
			OnMouseEntered();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMouseExited && args.Count == 0)
		{
			OnMouseExited();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateChestSkin && args.Count == 1)
		{
			UpdateChestSkin(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActiveScreenChanged && args.Count == 0)
		{
			OnActiveScreenChanged();
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
		if (method == MethodName.OnProceedButtonPressed)
		{
			return true;
		}
		if (method == MethodName.OnProceedButtonReleased)
		{
			return true;
		}
		if (method == MethodName.OnChestButtonReleased)
		{
			return true;
		}
		if (method == MethodName.OnMouseEntered)
		{
			return true;
		}
		if (method == MethodName.OnMouseExited)
		{
			return true;
		}
		if (method == MethodName.UpdateChestSkin)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenChanged)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<NCommonBanner>(in value);
			return true;
		}
		if (name == PropertyName._chestButton)
		{
			_chestButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._chestNode)
		{
			_chestNode = VariantUtils.ConvertTo<Node2D>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._goldParticles)
		{
			_goldParticles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._relicCollection)
		{
			_relicCollection = VariantUtils.ConvertTo<NTreasureRoomRelicCollection>(in value);
			return true;
		}
		if (name == PropertyName._isRelicCollectionOpen)
		{
			_isRelicCollectionOpen = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._hasRelicBeenClaimed)
		{
			_hasRelicBeenClaimed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ProceedButton)
		{
			value = VariantUtils.CreateFrom<NProceedButton>(ProceedButton);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._chestButton)
		{
			value = VariantUtils.CreateFrom(in _chestButton);
			return true;
		}
		if (name == PropertyName._chestNode)
		{
			value = VariantUtils.CreateFrom(in _chestNode);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._goldParticles)
		{
			value = VariantUtils.CreateFrom(in _goldParticles);
			return true;
		}
		if (name == PropertyName._relicCollection)
		{
			value = VariantUtils.CreateFrom(in _relicCollection);
			return true;
		}
		if (name == PropertyName._isRelicCollectionOpen)
		{
			value = VariantUtils.CreateFrom(in _isRelicCollectionOpen);
			return true;
		}
		if (name == PropertyName._hasRelicBeenClaimed)
		{
			value = VariantUtils.CreateFrom(in _hasRelicBeenClaimed);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chestButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chestNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ProceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._goldParticles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicCollection, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isRelicCollectionOpen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hasRelicBeenClaimed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._chestButton, Variant.From(in _chestButton));
		info.AddProperty(PropertyName._chestNode, Variant.From(in _chestNode));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._goldParticles, Variant.From(in _goldParticles));
		info.AddProperty(PropertyName._relicCollection, Variant.From(in _relicCollection));
		info.AddProperty(PropertyName._isRelicCollectionOpen, Variant.From(in _isRelicCollectionOpen));
		info.AddProperty(PropertyName._hasRelicBeenClaimed, Variant.From(in _hasRelicBeenClaimed));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._banner, out var value))
		{
			_banner = value.As<NCommonBanner>();
		}
		if (info.TryGetProperty(PropertyName._chestButton, out var value2))
		{
			_chestButton = value2.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._chestNode, out var value3))
		{
			_chestNode = value3.As<Node2D>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value4))
		{
			_proceedButton = value4.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._goldParticles, out var value5))
		{
			_goldParticles = value5.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._relicCollection, out var value6))
		{
			_relicCollection = value6.As<NTreasureRoomRelicCollection>();
		}
		if (info.TryGetProperty(PropertyName._isRelicCollectionOpen, out var value7))
		{
			_isRelicCollectionOpen = value7.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._hasRelicBeenClaimed, out var value8))
		{
			_hasRelicBeenClaimed = value8.As<bool>();
		}
	}
}
