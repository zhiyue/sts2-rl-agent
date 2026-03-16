using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events;

[ScriptPath("res://src/Core/Nodes/Events/NEventLayout.cs")]
public class NEventLayout : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName InitializeVisuals = "InitializeVisuals";

		public static readonly StringName SetPortrait = "SetPortrait";

		public static readonly StringName AddVfxAnchoredToPortrait = "AddVfxAnchoredToPortrait";

		public static readonly StringName RemoveNodesOnPortrait = "RemoveNodesOnPortrait";

		public static readonly StringName SetTitle = "SetTitle";

		public static readonly StringName SetDescription = "SetDescription";

		public static readonly StringName AnimateIn = "AnimateIn";

		public static readonly StringName ClearOptions = "ClearOptions";

		public static readonly StringName OnSetupComplete = "OnSetupComplete";

		public static readonly StringName AnimateButtonsIn = "AnimateButtonsIn";

		public static readonly StringName DisableEventOptions = "DisableEventOptions";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ApplyDebugUiVisibility = "ApplyDebugUiVisibility";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName VfxContainer = "VfxContainer";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _descriptionTween = "_descriptionTween";

		public static readonly StringName _optionsContainer = "_optionsContainer";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _title = "_title";

		public static readonly StringName _sharedEventLabel = "_sharedEventLabel";

		public static readonly StringName _description = "_description";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public const string defaultScenePath = "res://scenes/events/default_event_layout.tscn";

	protected Tween? _descriptionTween;

	protected VBoxContainer _optionsContainer;

	private TextureRect? _portrait;

	private MegaLabel? _title;

	protected EventModel _event;

	protected MegaLabel? _sharedEventLabel;

	private static readonly LocString _sharedEventLoc = new LocString("events", "SHARED_EVENT_INFO");

	protected MegaRichTextLabel? _description;

	private static bool _isDebugUiVisible;

	public Control? VfxContainer { get; private set; }

	public IEnumerable<NEventOptionButton> OptionButtons => _optionsContainer.GetChildren().OfType<NEventOptionButton>();

	public virtual Control? DefaultFocusedControl => OptionButtons.FirstOrDefault();

	public override void _Ready()
	{
		_portrait = GetNodeOrNull<TextureRect>("%Portrait");
		_title = GetNodeOrNull<MegaLabel>("%Title");
		_description = GetNodeOrNull<MegaRichTextLabel>("%EventDescription");
		VfxContainer = GetNodeOrNull<Control>("%VfxContainer");
		_sharedEventLabel = GetNodeOrNull<MegaLabel>("%SharedEventLabel");
		_sharedEventLabel?.SetTextAutoSize(_sharedEventLoc.GetFormattedText());
		_optionsContainer = GetNode<VBoxContainer>("%OptionsContainer");
		_description?.SetText(string.Empty);
		ApplyDebugUiVisibility();
	}

	public override void _EnterTree()
	{
		RunManager.Instance.EventSynchronizer.PlayerVoteChanged += OnPlayerVoteChanged;
	}

	public override void _ExitTree()
	{
		RunManager.Instance.EventSynchronizer.PlayerVoteChanged -= OnPlayerVoteChanged;
	}

	public virtual void SetEvent(EventModel eventModel)
	{
		_event = eventModel;
		InitializeVisuals();
		_event.OnRoomEnter();
	}

	protected virtual void InitializeVisuals()
	{
		SetPortrait(_event.CreateInitialPortrait());
		if (_event.HasVfx)
		{
			Node2D node2D = _event.CreateVfx();
			NEventRoom.Instance.Layout.AddVfxAnchoredToPortrait(node2D);
			node2D.Position = EventModel.VfxOffset;
		}
	}

	public void SetPortrait(Texture2D portrait)
	{
		if (_portrait == null)
		{
			throw new InvalidOperationException("Trying to set a portrait in an event layout that doesn't have one.");
		}
		_portrait.Texture = portrait;
	}

	public void AddVfxAnchoredToPortrait(Node? vfx)
	{
		_portrait.AddChildSafely(vfx);
	}

	public void RemoveNodesOnPortrait()
	{
		foreach (Node child in _portrait.GetChildren())
		{
			_portrait.RemoveChildSafely(child);
		}
	}

	public void SetTitle(string title)
	{
		if (_title != null)
		{
			_title.Text = title;
		}
	}

	public void SetDescription(string description)
	{
		if (_description != null)
		{
			_description.SetTextAutoSize(description);
			AnimateIn();
		}
	}

	protected virtual void AnimateIn()
	{
		if (_sharedEventLabel != null)
		{
			_sharedEventLabel.Modulate = StsColors.transparentWhite;
		}
		if (_description != null)
		{
			_description.Modulate = StsColors.transparentWhite;
			bool flag = SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast;
			_descriptionTween?.Kill();
			_descriptionTween = CreateTween().SetParallel();
			_descriptionTween.TweenInterval(flag ? 0.2 : 0.5);
			_descriptionTween.Chain();
			if (_title != null)
			{
				_descriptionTween.TweenProperty(_title, "modulate", Colors.White, flag ? 0.25 : 0.5);
			}
			_descriptionTween.TweenProperty(_description, "modulate", Colors.White, flag ? 0.5 : 1.0).SetDelay(0.25);
			_descriptionTween.TweenProperty(_description, "visible_ratio", 1f, flag ? 0.5 : 1.0).SetDelay(0.25).From(0f)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Sine);
			if (_sharedEventLabel != null)
			{
				_descriptionTween.TweenProperty(_sharedEventLabel, "modulate", Colors.White, flag ? 0.25 : 0.5).SetDelay(0.25);
			}
		}
	}

	public void ClearOptions()
	{
		foreach (Node item in _optionsContainer.GetChildren().ToList())
		{
			_optionsContainer.RemoveChildSafely(item);
			item.QueueFreeSafely();
		}
	}

	public void AddOptions(IEnumerable<EventOption> options)
	{
		if (_sharedEventLabel != null)
		{
			MegaLabel? sharedEventLabel = _sharedEventLabel;
			EventModel eventModel = _event;
			sharedEventLabel.Visible = eventModel != null && eventModel.IsShared && !eventModel.IsFinished && _event.Owner.RunState.Players.Count > 1;
		}
		foreach (EventOption option in options)
		{
			NEventOptionButton nEventOptionButton = NEventOptionButton.Create(_event, option, _optionsContainer.GetChildCount());
			_optionsContainer.AddChildSafely(nEventOptionButton);
			nEventOptionButton.RefreshVotes();
		}
		int childCount = _optionsContainer.GetChildCount();
		if (childCount != 0)
		{
			NodePath path = _optionsContainer.GetChild<Control>(0).GetPath();
			NodePath path2 = _optionsContainer.GetChild<Control>(childCount - 1).GetPath();
			for (int i = 0; i < childCount; i++)
			{
				Control child = _optionsContainer.GetChild<Control>(i);
				NodePath focusNeighborRight = (child.FocusNeighborLeft = child.GetPath());
				child.FocusNeighborRight = focusNeighborRight;
				child.FocusNeighborTop = ((i > 0) ? _optionsContainer.GetChild<Control>(i - 1).GetPath() : path2);
				child.FocusNeighborBottom = ((i < childCount - 1) ? _optionsContainer.GetChild<Control>(i + 1).GetPath() : path);
			}
			AnimateButtonsIn();
		}
	}

	public virtual void OnSetupComplete()
	{
	}

	protected virtual void AnimateButtonsIn()
	{
		foreach (NEventOptionButton button in OptionButtons)
		{
			Callable.From(delegate
			{
				button.AnimateIn();
			}).CallDeferred();
		}
	}

	public async Task BeforeSharedOptionChosen(EventOption option)
	{
		NEventOptionButton chosenButton = null;
		foreach (NEventOptionButton optionButton in OptionButtons)
		{
			optionButton.Disable();
			if (optionButton.Option == option)
			{
				chosenButton = optionButton;
			}
		}
		if (chosenButton == null)
		{
			return;
		}
		EventSplitVoteAnimation eventSplitVoteAnimation = new EventSplitVoteAnimation(this, _event.Owner.RunState);
		await eventSplitVoteAnimation.TryPlay(chosenButton);
		foreach (NEventOptionButton optionButton2 in OptionButtons)
		{
			if (optionButton2.Option != option)
			{
				optionButton2.GrayOut();
			}
		}
		await chosenButton.FlashConfirmation();
	}

	private void OnPlayerVoteChanged(Player player)
	{
		foreach (NEventOptionButton optionButton in OptionButtons)
		{
			optionButton.RefreshVotes();
		}
	}

	public void DisableEventOptions()
	{
		foreach (NEventOptionButton optionButton in OptionButtons)
		{
			optionButton.Disable();
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideEventUi))
		{
			_isDebugUiVisible = !_isDebugUiVisible;
			ApplyDebugUiVisibility();
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugUiVisible ? "Hide Event UI" : "Show Event UI"));
		}
	}

	private void ApplyDebugUiVisibility()
	{
		if (_isDebugUiVisible)
		{
			_optionsContainer.Visible = false;
			if (_title != null)
			{
				_title.Modulate = Colors.Transparent;
			}
			if (_description != null)
			{
				_description.Visible = false;
			}
		}
		else
		{
			_optionsContainer.Visible = true;
			if (_description != null)
			{
				_description.Visible = true;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(16);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetPortrait, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "portrait", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddVfxAnchoredToPortrait, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "vfx", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveNodesOnPortrait, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetTitle, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "title", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetDescription, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "description", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ClearOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSetupComplete, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateButtonsIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableEventOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ApplyDebugUiVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.InitializeVisuals && args.Count == 0)
		{
			InitializeVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPortrait && args.Count == 1)
		{
			SetPortrait(VariantUtils.ConvertTo<Texture2D>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddVfxAnchoredToPortrait && args.Count == 1)
		{
			AddVfxAnchoredToPortrait(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveNodesOnPortrait && args.Count == 0)
		{
			RemoveNodesOnPortrait();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTitle && args.Count == 1)
		{
			SetTitle(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetDescription && args.Count == 1)
		{
			SetDescription(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateIn && args.Count == 0)
		{
			AnimateIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearOptions && args.Count == 0)
		{
			ClearOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSetupComplete && args.Count == 0)
		{
			OnSetupComplete();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateButtonsIn && args.Count == 0)
		{
			AnimateButtonsIn();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableEventOptions && args.Count == 0)
		{
			DisableEventOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyDebugUiVisibility && args.Count == 0)
		{
			ApplyDebugUiVisibility();
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
		if (method == MethodName.InitializeVisuals)
		{
			return true;
		}
		if (method == MethodName.SetPortrait)
		{
			return true;
		}
		if (method == MethodName.AddVfxAnchoredToPortrait)
		{
			return true;
		}
		if (method == MethodName.RemoveNodesOnPortrait)
		{
			return true;
		}
		if (method == MethodName.SetTitle)
		{
			return true;
		}
		if (method == MethodName.SetDescription)
		{
			return true;
		}
		if (method == MethodName.AnimateIn)
		{
			return true;
		}
		if (method == MethodName.ClearOptions)
		{
			return true;
		}
		if (method == MethodName.OnSetupComplete)
		{
			return true;
		}
		if (method == MethodName.AnimateButtonsIn)
		{
			return true;
		}
		if (method == MethodName.DisableEventOptions)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ApplyDebugUiVisibility)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.VfxContainer)
		{
			VfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._descriptionTween)
		{
			_descriptionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._optionsContainer)
		{
			_optionsContainer = VariantUtils.ConvertTo<VBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._title)
		{
			_title = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._sharedEventLabel)
		{
			_sharedEventLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._description)
		{
			_description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
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
		if (name == PropertyName._descriptionTween)
		{
			value = VariantUtils.CreateFrom(in _descriptionTween);
			return true;
		}
		if (name == PropertyName._optionsContainer)
		{
			value = VariantUtils.CreateFrom(in _optionsContainer);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
		if (name == PropertyName._title)
		{
			value = VariantUtils.CreateFrom(in _title);
			return true;
		}
		if (name == PropertyName._sharedEventLabel)
		{
			value = VariantUtils.CreateFrom(in _sharedEventLabel);
			return true;
		}
		if (name == PropertyName._description)
		{
			value = VariantUtils.CreateFrom(in _description);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.VfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._optionsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._title, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sharedEventLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.VfxContainer, Variant.From<Control>(VfxContainer));
		info.AddProperty(PropertyName._descriptionTween, Variant.From(in _descriptionTween));
		info.AddProperty(PropertyName._optionsContainer, Variant.From(in _optionsContainer));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._title, Variant.From(in _title));
		info.AddProperty(PropertyName._sharedEventLabel, Variant.From(in _sharedEventLabel));
		info.AddProperty(PropertyName._description, Variant.From(in _description));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.VfxContainer, out var value))
		{
			VfxContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._descriptionTween, out var value2))
		{
			_descriptionTween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._optionsContainer, out var value3))
		{
			_optionsContainer = value3.As<VBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value4))
		{
			_portrait = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._title, out var value5))
		{
			_title = value5.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._sharedEventLabel, out var value6))
		{
			_sharedEventLabel = value6.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._description, out var value7))
		{
			_description = value7.As<MegaRichTextLabel>();
		}
	}
}
