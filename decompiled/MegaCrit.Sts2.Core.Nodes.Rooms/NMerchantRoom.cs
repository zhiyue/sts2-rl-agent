using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NMerchantRoom.cs")]
public class NMerchantRoom : Control, IScreenContext, IRoomWithProceedButton
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ToggleMerchantTrack = "ToggleMerchantTrack";

		public static readonly StringName AfterRoomIsLoaded = "AfterRoomIsLoaded";

		public static readonly StringName HideScreen = "HideScreen";

		public static readonly StringName MerchantFtueCheck = "MerchantFtueCheck";

		public static readonly StringName OnMerchantOpened = "OnMerchantOpened";

		public static readonly StringName OpenInventory = "OpenInventory";

		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";

		public static readonly StringName BlockInput = "BlockInput";

		public static readonly StringName UnblockInput = "UnblockInput";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ProceedButton = "ProceedButton";

		public static readonly StringName Inventory = "Inventory";

		public static readonly StringName MerchantButton = "MerchantButton";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _characterContainer = "_characterContainer";

		public static readonly StringName _inputBlocker = "_inputBlocker";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _animVariance = 0.5f;

	private static readonly string _scenePath = SceneHelper.GetScenePath("rooms/merchant_room");

	private readonly List<Player> _players = new List<Player>();

	private MerchantDialogueSet _dialogue;

	private NProceedButton _proceedButton;

	private Control _characterContainer;

	private Control _inputBlocker;

	private readonly List<NMerchantCharacter> _playerVisuals = new List<NMerchantCharacter>();

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static NMerchantRoom? Instance => NRun.Instance?.MerchantRoom;

	public NProceedButton ProceedButton => _proceedButton;

	public MerchantRoom Room { get; private set; }

	public NMerchantInventory Inventory { get; private set; }

	public NMerchantButton MerchantButton { get; private set; }

	public IReadOnlyList<NMerchantCharacter> PlayerVisuals => _playerVisuals;

	public Control? DefaultFocusedControl => null;

	public static NMerchantRoom? Create(MerchantRoom room, IReadOnlyList<Player> players)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NMerchantRoom nMerchantRoom = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NMerchantRoom>(PackedScene.GenEditState.Disabled);
		nMerchantRoom.Room = room;
		nMerchantRoom._players.AddRange(players);
		nMerchantRoom._dialogue = MerchantRoom.Dialogue;
		return nMerchantRoom;
	}

	public override void _Ready()
	{
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(HideScreen));
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		_proceedButton.SetPulseState(isPulsing: false);
		_proceedButton.Enable();
		MerchantButton = GetNode<NMerchantButton>("%MerchantButton");
		MerchantButton.IsLocalPlayerDead = LocalContext.GetMe(_players).Creature.IsDead;
		MerchantButton.PlayerDeadLines = _dialogue.PlayerDeadLines;
		MerchantButton.Connect(NMerchantButton.SignalName.MerchantOpened, Callable.From<NMerchantButton>(OnMerchantOpened));
		Inventory = GetNode<NMerchantInventory>("%Inventory");
		Inventory.MouseFilter = MouseFilterEnum.Ignore;
		Inventory.Initialize(Room.Inventory, _dialogue);
		_characterContainer = GetNode<Control>("%CharacterContainer");
		_inputBlocker = GetNode<Control>("%InputBlocker");
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		NGame.Instance.SetScreenShakeTarget(this);
		AfterRoomIsLoaded();
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
		NMapScreen.Instance.Connect(NMapScreen.SignalName.Opened, Callable.From(ToggleMerchantTrack));
		NMapScreen.Instance.Connect(NMapScreen.SignalName.Closed, Callable.From(ToggleMerchantTrack));
	}

	public override void _ExitTree()
	{
		NGame.Instance.ClearScreenShakeTarget();
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
		NMapScreen.Instance.Disconnect(NMapScreen.SignalName.Opened, Callable.From(ToggleMerchantTrack));
		NMapScreen.Instance.Disconnect(NMapScreen.SignalName.Closed, Callable.From(ToggleMerchantTrack));
	}

	public void FoulPotionThrown(FoulPotion potion)
	{
		SfxCmd.Play("event:/sfx/npcs/merchant/merchant_thank_yous");
		LocString locString = Rng.Chaotic.NextItem(_dialogue.FoulPotionLines);
		if (locString != null)
		{
			NSpeechBubbleVfx nSpeechBubbleVfx = MerchantButton.PlayDialogue(locString);
			if (nSpeechBubbleVfx != null)
			{
				NGame.Instance?.ScreenRumble(ShakeStrength.Medium, ShakeDuration.Short, RumbleStyle.Rumble);
			}
		}
	}

	private void ToggleMerchantTrack()
	{
		NRunMusicController.Instance?.ToggleMerchantTrack();
	}

	private void AfterRoomIsLoaded()
	{
		Player me = LocalContext.GetMe(_players);
		_players.Remove(me);
		_players.Insert(0, me);
		int num = Mathf.CeilToInt(Mathf.Sqrt(_players.Count));
		for (int i = 0; i < num; i++)
		{
			float num2 = -140f * (float)i;
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				if (num3 >= _players.Count)
				{
					break;
				}
				NMerchantCharacter nMerchantCharacter = PreloadManager.Cache.GetScene(_players[num3].Character.MerchantAnimPath).Instantiate<NMerchantCharacter>(PackedScene.GenEditState.Disabled);
				_characterContainer.AddChildSafely(nMerchantCharacter);
				_characterContainer.MoveChild(nMerchantCharacter, 0);
				nMerchantCharacter.Position = new Vector2(num2, -50f * (float)i);
				if (i > 0)
				{
					nMerchantCharacter.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
				num2 -= 275f;
				_playerVisuals.Add(nMerchantCharacter);
			}
		}
	}

	private void HideScreen(NButton _)
	{
		if (!MerchantFtueCheck())
		{
			NMapScreen.Instance.Open();
		}
	}

	private bool MerchantFtueCheck()
	{
		if (!SaveManager.Instance.SeenFtue("merchant_ftue"))
		{
			NModalContainer.Instance.Add(NMerchantFtue.Create(this));
			SaveManager.Instance.MarkFtueAsComplete("merchant_ftue");
			return true;
		}
		return false;
	}

	private void OnMerchantOpened(NMerchantButton _)
	{
		OpenInventory();
	}

	public void OpenInventory()
	{
		if (!Inventory.IsOpen)
		{
			_proceedButton.Disable();
			Inventory.Open();
			MerchantButton.Disable();
			Inventory.Connect(NMerchantInventory.SignalName.InventoryClosed, Callable.From(delegate
			{
				MerchantButton.Enable();
				_proceedButton.Enable();
				_proceedButton.SetPulseState(isPulsing: true);
			}), 4u);
		}
	}

	private void OnActiveScreenUpdated()
	{
		this.UpdateControllerNavEnabled();
		if (ActiveScreenContext.Instance.IsCurrent(this))
		{
			MerchantButton.Enable();
			_proceedButton.Enable();
		}
		else
		{
			MerchantButton.Disable();
			_proceedButton.Disable();
		}
	}

	public void BlockInput()
	{
		_inputBlocker.MouseFilter = MouseFilterEnum.Stop;
		NHotkeyManager.Instance.AddBlockingScreen(_inputBlocker);
	}

	public void UnblockInput()
	{
		_inputBlocker.MouseFilter = MouseFilterEnum.Ignore;
		NHotkeyManager.Instance.RemoveBlockingScreen(_inputBlocker);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(12);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleMerchantTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterRoomIsLoaded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.MerchantFtueCheck, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMerchantOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenInventory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BlockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnblockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.ToggleMerchantTrack && args.Count == 0)
		{
			ToggleMerchantTrack();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterRoomIsLoaded && args.Count == 0)
		{
			AfterRoomIsLoaded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideScreen && args.Count == 1)
		{
			HideScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.MerchantFtueCheck && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(MerchantFtueCheck());
			return true;
		}
		if (method == MethodName.OnMerchantOpened && args.Count == 1)
		{
			OnMerchantOpened(VariantUtils.ConvertTo<NMerchantButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenInventory && args.Count == 0)
		{
			OpenInventory();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated && args.Count == 0)
		{
			OnActiveScreenUpdated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BlockInput && args.Count == 0)
		{
			BlockInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnblockInput && args.Count == 0)
		{
			UnblockInput();
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
		if (method == MethodName.ToggleMerchantTrack)
		{
			return true;
		}
		if (method == MethodName.AfterRoomIsLoaded)
		{
			return true;
		}
		if (method == MethodName.HideScreen)
		{
			return true;
		}
		if (method == MethodName.MerchantFtueCheck)
		{
			return true;
		}
		if (method == MethodName.OnMerchantOpened)
		{
			return true;
		}
		if (method == MethodName.OpenInventory)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated)
		{
			return true;
		}
		if (method == MethodName.BlockInput)
		{
			return true;
		}
		if (method == MethodName.UnblockInput)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Inventory)
		{
			Inventory = VariantUtils.ConvertTo<NMerchantInventory>(in value);
			return true;
		}
		if (name == PropertyName.MerchantButton)
		{
			MerchantButton = VariantUtils.ConvertTo<NMerchantButton>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._characterContainer)
		{
			_characterContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			_inputBlocker = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName.Inventory)
		{
			value = VariantUtils.CreateFrom<NMerchantInventory>(Inventory);
			return true;
		}
		if (name == PropertyName.MerchantButton)
		{
			value = VariantUtils.CreateFrom<NMerchantButton>(MerchantButton);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._characterContainer)
		{
			value = VariantUtils.CreateFrom(in _characterContainer);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			value = VariantUtils.CreateFrom(in _inputBlocker);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ProceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputBlocker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Inventory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MerchantButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Inventory, Variant.From<NMerchantInventory>(Inventory));
		info.AddProperty(PropertyName.MerchantButton, Variant.From<NMerchantButton>(MerchantButton));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._characterContainer, Variant.From(in _characterContainer));
		info.AddProperty(PropertyName._inputBlocker, Variant.From(in _inputBlocker));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Inventory, out var value))
		{
			Inventory = value.As<NMerchantInventory>();
		}
		if (info.TryGetProperty(PropertyName.MerchantButton, out var value2))
		{
			MerchantButton = value2.As<NMerchantButton>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value3))
		{
			_proceedButton = value3.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._characterContainer, out var value4))
		{
			_characterContainer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._inputBlocker, out var value5))
		{
			_inputBlocker = value5.As<Control>();
		}
	}
}
