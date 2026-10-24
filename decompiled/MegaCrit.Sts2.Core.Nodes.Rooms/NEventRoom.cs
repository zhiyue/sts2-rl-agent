using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NEventRoom.cs")]
public class NEventRoom : Control, IScreenContext
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
		/// Cached name for the 'SetPortrait' method.
		/// </summary>
		public static readonly StringName SetPortrait = "SetPortrait";

		/// <summary>
		/// Cached name for the 'DisableOptionButtons' method.
		/// </summary>
		public static readonly StringName DisableOptionButtons = "DisableOptionButtons";

		/// <summary>
		/// Cached name for the 'OnEnteringEventCombat' method.
		/// </summary>
		public static readonly StringName OnEnteringEventCombat = "OnEnteringEventCombat";

		/// <summary>
		/// Cached name for the 'OnActiveScreenUpdated' method.
		/// </summary>
		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'Layout' property.
		/// </summary>
		public static readonly StringName Layout = "Layout";

		/// <summary>
		/// Cached name for the 'EmbeddedCombatRoom' property.
		/// </summary>
		public static readonly StringName EmbeddedCombatRoom = "EmbeddedCombatRoom";

		/// <summary>
		/// Cached name for the 'VfxContainer' property.
		/// </summary>
		public static readonly StringName VfxContainer = "VfxContainer";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_isPreFinished' field.
		/// </summary>
		public static readonly StringName _isPreFinished = "_isPreFinished";

		/// <summary>
		/// Cached name for the '_eventContainer' field.
		/// </summary>
		public static readonly StringName _eventContainer = "_eventContainer";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	private EventModel _event;

	private IRunState _runState = NullRunState.Instance;

	private bool _isPreFinished;

	private NSceneContainer _eventContainer;

	private const string _scenePath = "res://scenes/rooms/event_room.tscn";

	private readonly List<EventOption> _connectedOptions = new List<EventOption>();

	public static NEventRoom? Instance => NRun.Instance?.EventRoom;

	/// <summary>
	/// The event layout node.
	/// Null in custom-layout events like <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" />.
	/// </summary>
	public NEventLayout? Layout => _eventContainer.CurrentScene as NEventLayout;

	/// <summary>
	/// The Custom Event node.
	/// </summary>
	public ICustomEventNode? CustomEventNode => _eventContainer.CurrentScene as ICustomEventNode;

	public NCombatRoom? EmbeddedCombatRoom => (Layout as NCombatEventLayout)?.EmbeddedCombatRoom;

	/// <summary>
	/// Container node for event VFX.
	/// Null in custom-layout events like <see cref="T:MegaCrit.Sts2.Core.Models.Events.FakeMerchant" />.
	/// </summary>
	public Control? VfxContainer { get; private set; }

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/rooms/event_room.tscn");

	public Control? DefaultFocusedControl
	{
		get
		{
			IScreenContext customEventNode = CustomEventNode;
			if (customEventNode != null)
			{
				return customEventNode.DefaultFocusedControl;
			}
			return Layout?.DefaultFocusedControl;
		}
	}

	/// <summary>
	/// Creates an event room node for the given event.
	/// </summary>
	/// <param name="eventModel">The event to create the room node for.</param>
	/// <param name="runState">The state of the run that the event is in.</param>
	/// <param name="isPreFinished">Whether or not the event is pre-finished (i.e. loaded from a save after being finished).</param>
	public static NEventRoom? Create(EventModel eventModel, IRunState? runState, bool isPreFinished)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NEventRoom nEventRoom = PreloadManager.Cache.GetScene("res://scenes/rooms/event_room.tscn").Instantiate<NEventRoom>(PackedScene.GenEditState.Disabled);
		nEventRoom._event = eventModel;
		nEventRoom._isPreFinished = isPreFinished;
		if (runState != null)
		{
			nEventRoom._runState = runState;
		}
		return nEventRoom;
	}

	public override void _Ready()
	{
		if (_event.Node != null)
		{
			throw new InvalidOperationException("Tried to create event room, but event already has a node!");
		}
		_eventContainer = GetNode<NSceneContainer>("%EventContainer");
		NGame.Instance.SetScreenShakeTarget(_eventContainer);
		Control control = _event.CreateScene().Instantiate<Control>(PackedScene.GenEditState.Disabled);
		_event.SetNode(control);
		_eventContainer.SetCurrentScene(control);
		VfxContainer = Layout?.VfxContainer;
		TaskHelper.RunSafely(SetupLayout());
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		NGame.Instance.ClearScreenShakeTarget();
		_event.StateChanged -= RefreshEventState;
		_event.EnteringEventCombat -= OnEnteringEventCombat;
		foreach (EventOption connectedOption in _connectedOptions)
		{
			connectedOption.BeforeChosen -= BeforeOptionChosen;
		}
		_connectedOptions.Clear();
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
	}

	private async Task SetupLayout()
	{
		if (_event.Owner == null)
		{
			throw new InvalidOperationException("Event must be started before passed to NEventRoom!");
		}
		if (Layout == null)
		{
			return;
		}
		Layout.SetEvent(_event);
		SetTitle(_event.Title);
		_event.StateChanged += RefreshEventState;
		_event.EnteringEventCombat += OnEnteringEventCombat;
		await Cmd.Wait(0.2f, _cts.Token);
		SetDescription(GetDescriptionOrFallback());
		if (_event is AncientEventModel ancientEventModel && !_isPreFinished)
		{
			ModelId id = _event.Owner.Character.Id;
			AncientStats statsForAncient = SaveManager.Instance.Progress.GetStatsForAncient(ancientEventModel.Id);
			int charVisits = statsForAncient?.GetVisitsAs(id) ?? 0;
			int totalVisits = statsForAncient?.TotalVisits ?? 0;
			IEnumerable<AncientDialogue> validDialogues = ancientEventModel.DialogueSet.GetValidDialogues(id, charVisits, totalVisits, !ancientEventModel.AnyCharacterDialogueBlacklist.Contains(_event.Owner.Character));
			AncientDialogue ancientDialogue = Rng.Chaotic.NextItem(validDialogues);
			foreach (AncientDialogueLine line in ancientDialogue.Lines)
			{
				line.LineText?.Add("Act1Name", _runState.Acts[0].Title);
			}
			((NAncientEventLayout)Layout).SetDialogue(ancientDialogue.Lines);
		}
		SetOptions(_event);
		Layout.OnSetupComplete();
	}

	public void SetPortrait(Texture2D portrait)
	{
		Layout.SetPortrait(portrait);
	}

	private void SetTitle(LocString title)
	{
		Layout.SetTitle(title.GetFormattedText());
	}

	private void SetDescription(LocString description)
	{
		if (description.Exists())
		{
			CharacterModel character = _event.Owner.Character;
			character.AddDetailsTo(description);
			description.Add("IsMultiplayer", _event.Owner.RunState.Players.Count > 1);
			_event.DynamicVars.AddTo(description);
			Layout.SetDescription(description.GetFormattedText());
		}
	}

	private void SetOptions(EventModel eventModel)
	{
		Layout.ClearOptions();
		IReadOnlyList<EventOption> readOnlyList = eventModel.CurrentOptions;
		if (eventModel.IsFinished)
		{
			readOnlyList = new global::_003C_003Ez__ReadOnlySingleElementList<EventOption>(new EventOption(eventModel, Proceed, "PROCEED", false, true));
		}
		foreach (EventOption item in readOnlyList)
		{
			item.BeforeChosen += BeforeOptionChosen;
			_connectedOptions.Add(item);
		}
		Layout.AddOptions(readOnlyList);
		DefaultFocusedControl?.TryGrabFocus();
	}

	/// <summary>
	/// Occurs when the local player clicks an event option.
	/// </summary>
	public void OptionButtonClicked(EventOption option, int index)
	{
		if (option.IsLocked)
		{
			return;
		}
		if (option.IsProceed)
		{
			TaskHelper.RunSafely(option.Chosen());
			return;
		}
		if (!_event.IsShared)
		{
			Layout.ClearOptions();
		}
		RunManager.Instance.EventSynchronizer.ChooseLocalOption(index);
	}

	/// <summary>
	/// Occurs after an option is executed. This may not occur at the same time as the local player clicking the button
	/// if we are in a shared multiplayer event.
	/// </summary>
	private async Task BeforeOptionChosen(EventOption option)
	{
		if (_event.Owner.RunState.Players.Count > 1 && RunManager.Instance.EventSynchronizer.IsShared && !option.IsProceed)
		{
			await Layout.BeforeSharedOptionChosen(option);
		}
		else if (!option.IsProceed)
		{
			DisableOptionButtons();
		}
	}

	/// <summary>
	/// Called whenever an event's state changes - i.e. when an option is chosen.
	/// Note that, in multiplayer, this does not necessarily occur at the same time the local player chooses an option.
	/// For shared events, this is called after all players vote on an option.
	/// </summary>
	private void RefreshEventState(EventModel eventModel)
	{
		SetDescription(GetDescriptionOrFallback());
		if (eventModel is AncientEventModel)
		{
			((NAncientEventLayout)Layout).ClearDialogue();
		}
		SetOptions(_event);
	}

	private void DisableOptionButtons()
	{
		Layout?.DisableEventOptions();
	}

	private void OnEnteringEventCombat()
	{
		DisableOptionButtons();
		if (Layout is NCombatEventLayout nCombatEventLayout)
		{
			nCombatEventLayout.HideEventVisuals();
		}
	}

	public static Task Proceed()
	{
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
		NMapScreen.Instance.Open();
		return Task.CompletedTask;
	}

	private LocString GetDescriptionOrFallback()
	{
		return _event.Description ?? new LocString("events", "ERROR.description");
	}

	private void OnActiveScreenUpdated()
	{
		this.UpdateControllerNavEnabled();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(7);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetPortrait, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "portrait", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisableOptionButtons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnteringEventCombat, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetPortrait && args.Count == 1)
		{
			SetPortrait(VariantUtils.ConvertTo<Texture2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableOptionButtons && args.Count == 0)
		{
			DisableOptionButtons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnteringEventCombat && args.Count == 0)
		{
			OnEnteringEventCombat();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated && args.Count == 0)
		{
			OnActiveScreenUpdated();
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
		if (method == MethodName.SetPortrait)
		{
			return true;
		}
		if (method == MethodName.DisableOptionButtons)
		{
			return true;
		}
		if (method == MethodName.OnEnteringEventCombat)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VfxContainer)
		{
			VfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isPreFinished)
		{
			_isPreFinished = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._eventContainer)
		{
			_eventContainer = VariantUtils.ConvertTo<NSceneContainer>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Layout)
		{
			value = VariantUtils.CreateFrom<NEventLayout>(Layout);
			return true;
		}
		if (name == PropertyName.EmbeddedCombatRoom)
		{
			value = VariantUtils.CreateFrom<NCombatRoom>(EmbeddedCombatRoom);
			return true;
		}
		Control from;
		if (name == PropertyName.VfxContainer)
		{
			from = VfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._isPreFinished)
		{
			value = VariantUtils.CreateFrom(in _isPreFinished);
			return true;
		}
		if (name == PropertyName._eventContainer)
		{
			value = VariantUtils.CreateFrom(in _eventContainer);
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
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isPreFinished, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._eventContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Layout, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EmbeddedCombatRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.VfxContainer, Variant.From<Control>(VfxContainer));
		info.AddProperty(PropertyName._isPreFinished, Variant.From(in _isPreFinished));
		info.AddProperty(PropertyName._eventContainer, Variant.From(in _eventContainer));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VfxContainer, out var value))
		{
			VfxContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isPreFinished, out var value2))
		{
			_isPreFinished = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._eventContainer, out var value3))
		{
			_eventContainer = value3.As<NSceneContainer>();
		}
	}
}
