using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Nodes.Events.Custom;

[ScriptPath("res://src/Core/Nodes/Events/Custom/NFakeMerchant.cs")]
public class NFakeMerchant : Control, ICustomEventNode, IScreenContext
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
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'ToggleMerchantTrack' method.
		/// </summary>
		public static readonly StringName ToggleMerchantTrack = "ToggleMerchantTrack";

		/// <summary>
		/// Cached name for the 'AfterRoomIsLoaded' method.
		/// </summary>
		public static readonly StringName AfterRoomIsLoaded = "AfterRoomIsLoaded";

		/// <summary>
		/// Cached name for the 'StartCharacterAnimation' method.
		/// </summary>
		public static readonly StringName StartCharacterAnimation = "StartCharacterAnimation";

		/// <summary>
		/// Cached name for the 'HideScreen' method.
		/// </summary>
		public static readonly StringName HideScreen = "HideScreen";

		/// <summary>
		/// Cached name for the 'OnMerchantOpened' method.
		/// </summary>
		public static readonly StringName OnMerchantOpened = "OnMerchantOpened";

		/// <summary>
		/// Cached name for the 'OpenInventory' method.
		/// </summary>
		public static readonly StringName OpenInventory = "OpenInventory";

		/// <summary>
		/// Cached name for the 'ShowProceedButton' method.
		/// </summary>
		public static readonly StringName ShowProceedButton = "ShowProceedButton";

		/// <summary>
		/// Cached name for the 'OnActiveScreenUpdated' method.
		/// </summary>
		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";

		/// <summary>
		/// Cached name for the 'BlockInput' method.
		/// </summary>
		public static readonly StringName BlockInput = "BlockInput";

		/// <summary>
		/// Cached name for the 'UnblockInput' method.
		/// </summary>
		public static readonly StringName UnblockInput = "UnblockInput";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Inventory' property.
		/// </summary>
		public static readonly StringName Inventory = "Inventory";

		/// <summary>
		/// Cached name for the 'MerchantButton' property.
		/// </summary>
		public static readonly StringName MerchantButton = "MerchantButton";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_proceedButton' field.
		/// </summary>
		public static readonly StringName _proceedButton = "_proceedButton";

		/// <summary>
		/// Cached name for the '_characterContainer' field.
		/// </summary>
		public static readonly StringName _characterContainer = "_characterContainer";

		/// <summary>
		/// Cached name for the '_inputBlocker' field.
		/// </summary>
		public static readonly StringName _inputBlocker = "_inputBlocker";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const float _animVariance = 0.5f;

	private readonly List<Player> _players = new List<Player>();

	private FakeMerchant _event;

	private MerchantDialogueSet _dialogue;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private NProceedButton _proceedButton;

	private Control _characterContainer;

	private Control _inputBlocker;

	public NMerchantInventory Inventory { get; private set; }

	public NMerchantButton MerchantButton { get; private set; }

	public IScreenContext CurrentScreenContext
	{
		get
		{
			if (!Inventory.IsOpen)
			{
				return this;
			}
			return Inventory;
		}
	}

	public Control? DefaultFocusedControl => null;

	public void Initialize(EventModel eventModel)
	{
		_event = (FakeMerchant)eventModel;
		_dialogue = FakeMerchant.Dialogue;
		_players.AddRange(_event.Owner.RunState.Players);
	}

	public override void _Ready()
	{
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(HideScreen));
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		_proceedButton.SetPulseState(isPulsing: false);
		_proceedButton.Enable();
		MerchantButton = GetNode<NMerchantButton>("%MerchantButton");
		if (_event.StartedFight)
		{
			MerchantButton.Hide();
			Player me = LocalContext.GetMe(_players);
			if (me.GetRelic<FakeMerchantsRug>() != null)
			{
				MegaSprite megaSprite = new MegaSprite(GetNode<Node2D>("%FakeMerchantBackground"));
				megaSprite.GetSkeleton()?.FindBone("rug")?.Hide();
			}
		}
		else
		{
			MerchantButton.IsLocalPlayerDead = LocalContext.GetMe(_players).Creature.IsDead;
			MerchantButton.PlayerDeadLines = _dialogue.PlayerDeadLines;
			MerchantButton.Connect(NMerchantButton.SignalName.MerchantOpened, Callable.From<NMerchantButton>(OnMerchantOpened));
		}
		Inventory = GetNode<NMerchantInventory>("%Inventory");
		Inventory.MouseFilter = MouseFilterEnum.Ignore;
		Inventory.Initialize(_event.Inventory, _dialogue);
		_characterContainer = GetNode<Control>("%CharacterContainer");
		_inputBlocker = GetNode<Control>("%InputBlocker");
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		NGame.Instance.SetScreenShakeTarget(this);
		AfterRoomIsLoaded();
	}

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
		NMapScreen.Instance.Connect(NMapScreen.SignalName.Opened, Callable.From(ToggleMerchantTrack));
		NMapScreen.Instance.Connect(NMapScreen.SignalName.Closed, Callable.From(ToggleMerchantTrack));
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		NGame.Instance.ClearScreenShakeTarget();
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
		NMapScreen.Instance.Disconnect(NMapScreen.SignalName.Opened, Callable.From(ToggleMerchantTrack));
		NMapScreen.Instance.Disconnect(NMapScreen.SignalName.Closed, Callable.From(ToggleMerchantTrack));
	}

	public async Task FoulPotionThrown()
	{
		LocString locString = Rng.Chaotic.NextItem(_dialogue.FoulPotionLines);
		if (locString != null)
		{
			NSpeechBubbleVfx nSpeechBubbleVfx = MerchantButton.PlayDialogue(locString);
			if (nSpeechBubbleVfx != null)
			{
				await Cmd.Wait((float)nSpeechBubbleVfx.SecondsToDisplay - 1f, _cts.Token);
			}
		}
	}

	private void ToggleMerchantTrack()
	{
	}

	private void AfterRoomIsLoaded()
	{
		Player me = LocalContext.GetMe(_players);
		_players.Remove(me);
		_players.Insert(0, me);
		int num = Mathf.CeilToInt(Mathf.Sqrt(_players.Count));
		for (int i = 0; i < num; i++)
		{
			float num2 = -75f * (float)i;
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				if (num3 >= _players.Count)
				{
					break;
				}
				NCreatureVisuals nCreatureVisuals = _players[num3].Character.CreateVisuals();
				_characterContainer.AddChildSafely(nCreatureVisuals);
				StartCharacterAnimation(nCreatureVisuals);
				_characterContainer.MoveChildSafely(nCreatureVisuals, 0);
				nCreatureVisuals.Position = new Vector2(num2, -50f * (float)i);
				if (i > 0)
				{
					nCreatureVisuals.Modulate = new Color(0.5f, 0.5f, 0.5f);
				}
				num2 -= nCreatureVisuals.Bounds.Size.X * 0.5f + 25f;
			}
		}
		if (!_event.StartedFight)
		{
			TaskHelper.RunSafely(ShowWelcomeDialogue());
		}
	}

	private async Task ShowWelcomeDialogue()
	{
		LocString line = Rng.Chaotic.NextItem(_dialogue.WelcomeLines);
		if (line != null)
		{
			await Cmd.Wait(0.75f, _cts.Token);
			SfxCmd.Play("event:/sfx/npcs/reverse_merchant/reverse_merchant_laugh");
			MerchantButton.PlayDialogue(line, 4.0);
		}
	}

	private void StartCharacterAnimation(NCreatureVisuals visuals)
	{
		MegaTrackEntry megaTrackEntry = visuals.SpineAnimation.SetAnimation("relaxed_loop");
		if (megaTrackEntry != null)
		{
			megaTrackEntry.SetLoop(loop: true);
			megaTrackEntry.SetTimeScale(Rng.Chaotic.NextFloat(0.9f, 1.1f));
			float animationEnd = megaTrackEntry.GetAnimationEnd();
			megaTrackEntry.SetTrackTime((animationEnd + Rng.Chaotic.NextFloat(-0.5f, 0.5f)) % animationEnd);
		}
	}

	private void HideScreen(NButton _)
	{
		NMapScreen.Instance.Open();
	}

	private void OnMerchantOpened(NMerchantButton _)
	{
		OpenInventory();
	}

	private void OpenInventory()
	{
		if (!Inventory.IsOpen)
		{
			_proceedButton.Disable();
			Inventory.Open();
			MerchantButton.Disable();
			Inventory.Connect(NMerchantInventory.SignalName.InventoryClosed, Callable.From(delegate
			{
				MerchantButton.Enable();
				ShowProceedButton();
			}), 4u);
		}
	}

	private void ShowProceedButton()
	{
		_proceedButton.Enable();
		_proceedButton.SetPulseState(isPulsing: true);
	}

	private void OnActiveScreenUpdated()
	{
		this.UpdateControllerNavEnabled();
		if (ActiveScreenContext.Instance.IsCurrent(this))
		{
			MerchantButton.Enable();
			if (!_proceedButton.IsEnabled)
			{
				_proceedButton.Enable();
			}
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

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ToggleMerchantTrack, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterRoomIsLoaded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.StartCharacterAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "visuals", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.HideScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnMerchantOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenInventory, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowProceedButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BlockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnblockInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.StartCharacterAnimation && args.Count == 1)
		{
			StartCharacterAnimation(VariantUtils.ConvertTo<NCreatureVisuals>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideScreen && args.Count == 1)
		{
			HideScreen(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
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
		if (method == MethodName.ShowProceedButton && args.Count == 0)
		{
			ShowProceedButton();
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

	/// <inheritdoc />
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
		if (method == MethodName.StartCharacterAnimation)
		{
			return true;
		}
		if (method == MethodName.HideScreen)
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
		if (method == MethodName.ShowProceedButton)
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._characterContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputBlocker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Inventory, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MerchantButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
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
