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
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NRestSiteRoom.cs")]
public class NRestSiteRoom : Control, IScreenContext, IRoomWithProceedButton
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName DisableOptions = "DisableOptions";

		public static readonly StringName EnableOptions = "EnableOptions";

		public static readonly StringName AnimateDescriptionDown = "AnimateDescriptionDown";

		public static readonly StringName AnimateDescriptionUp = "AnimateDescriptionUp";

		public static readonly StringName UpdateRestSiteOptions = "UpdateRestSiteOptions";

		public static readonly StringName RestSiteButtonHovered = "RestSiteButtonHovered";

		public static readonly StringName RestSiteButtonUnhovered = "RestSiteButtonUnhovered";

		public static readonly StringName OnPlayerChangedHoveredRestSiteOption = "OnPlayerChangedHoveredRestSiteOption";

		public static readonly StringName ShowProceedButton = "ShowProceedButton";

		public static readonly StringName OnProceedButtonReleased = "OnProceedButtonReleased";

		public static readonly StringName SetText = "SetText";

		public static readonly StringName FadeOutOptionDescription = "FadeOutOptionDescription";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName ExtinguishFireIfAble = "ExtinguishFireIfAble";

		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ProceedButton = "ProceedButton";

		public static readonly StringName Header = "Header";

		public static readonly StringName Description = "Description";

		public static readonly StringName BgContainer = "BgContainer";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _choicesContainer = "_choicesContainer";

		public static readonly StringName _choicesScreen = "_choicesScreen";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _restSiteLighting = "_restSiteLighting";

		public static readonly StringName _descriptionTween = "_descriptionTween";

		public static readonly StringName _descriptionPositionTween = "_descriptionPositionTween";

		public static readonly StringName _choicesTween = "_choicesTween";

		public static readonly StringName _originalDescriptionYPos = "_originalDescriptionYPos";

		public static readonly StringName _lastFocused = "_lastFocused";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public readonly List<NRestSiteCharacter> characterAnims = new List<NRestSiteCharacter>();

	private const float _lowDescriptionYPos = 885f;

	private const string _scenePath = "res://scenes/rooms/rest_site_room.tscn";

	private static bool _isDebugUiVisible;

	private RestSiteRoom _room;

	private IRunState _runState;

	private Control _choicesContainer;

	private Control _choicesScreen;

	private NProceedButton _proceedButton;

	private Control _restSiteLighting;

	private readonly List<Control> _characterContainers = new List<Control>();

	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	private Tween? _descriptionTween;

	private Tween? _descriptionPositionTween;

	private Tween? _choicesTween;

	private float _originalDescriptionYPos;

	private Control? _lastFocused;

	public static NRestSiteRoom? Instance => NRun.Instance?.RestSiteRoom;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/rooms/rest_site_room.tscn");

	public NProceedButton ProceedButton => _proceedButton;

	private MegaLabel Header { get; set; }

	private MegaRichTextLabel Description { get; set; }

	private Control BgContainer { get; set; }

	public IReadOnlyList<RestSiteOption> Options => _room.Options;

	public List<NRestSiteCharacter> Characters { get; } = new List<NRestSiteCharacter>();

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_lastFocused != null)
			{
				return _lastFocused;
			}
			if (_choicesContainer.GetChildCount() <= 0)
			{
				return null;
			}
			return _choicesContainer.GetChild<NRestSiteButton>(0);
		}
	}

	public static NRestSiteRoom? Create(RestSiteRoom room, IRunState runState)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NRestSiteRoom nRestSiteRoom = PreloadManager.Cache.GetScene("res://scenes/rooms/rest_site_room.tscn").Instantiate<NRestSiteRoom>(PackedScene.GenEditState.Disabled);
		nRestSiteRoom._room = room;
		nRestSiteRoom._runState = runState;
		return nRestSiteRoom;
	}

	public override void _Ready()
	{
		Header = GetNode<MegaLabel>("%Header");
		Description = GetNode<MegaRichTextLabel>("%Description");
		Description.Modulate = Colors.Transparent;
		_choicesContainer = GetNode<Control>("%ChoicesContainer");
		_choicesScreen = GetNode<Control>("%ChoicesScreen");
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		BgContainer = GetNode<Control>("BgContainer");
		_characterContainers.Add(GetNode<Control>("BgContainer/Character_1"));
		_characterContainers.Add(GetNode<Control>("BgContainer/Character_2"));
		_characterContainers.Add(GetNode<Control>("BgContainer/Character_3"));
		_characterContainers.Add(GetNode<Control>("BgContainer/Character_4"));
		Control control = _runState.Act.CreateRestSiteBackground();
		BgContainer.AddChildSafely(control);
		BgContainer.MoveChild(control, 0);
		_restSiteLighting = control.GetNode<Control>("%RestSiteLighting");
		Header.SetTextAutoSize(new LocString("rest_site_ui", "PROMPT").GetFormattedText());
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnProceedButtonReleased));
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		if (_isDebugUiVisible)
		{
			_choicesScreen.Modulate = Colors.Transparent;
		}
		NGame.Instance.SetScreenShakeTarget(this);
		_proceedButton.Disable();
		for (int i = 0; i < _runState.Players.Count; i++)
		{
			NRestSiteCharacter nRestSiteCharacter = NRestSiteCharacter.Create(_runState.Players[i], i);
			characterAnims.Add(nRestSiteCharacter);
			_characterContainers[i].AddChildSafely(nRestSiteCharacter);
			nRestSiteCharacter.Position = Vector2.Zero;
			if (i % 2 == 1)
			{
				nRestSiteCharacter.FlipX();
			}
			Characters.Add(nRestSiteCharacter);
		}
		_originalDescriptionYPos = Description.Position.Y;
		UpdateRestSiteOptions();
		TaskHelper.RunSafely(ShowFtueIfNeeded());
		RunManager.Instance.RestSiteSynchronizer.PlayerHoverChanged += OnPlayerChangedHoveredRestSiteOption;
		RunManager.Instance.RestSiteSynchronizer.BeforePlayerOptionChosen += OnBeforePlayerSelectedRestSiteOption;
		RunManager.Instance.RestSiteSynchronizer.AfterPlayerOptionChosen += OnAfterPlayerSelectedRestSiteOption;
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		_cts.Dispose();
		_descriptionTween?.Kill();
		_descriptionPositionTween?.Kill();
		_choicesTween?.Kill();
		RunManager.Instance.RestSiteSynchronizer.PlayerHoverChanged -= OnPlayerChangedHoveredRestSiteOption;
		RunManager.Instance.RestSiteSynchronizer.BeforePlayerOptionChosen -= OnBeforePlayerSelectedRestSiteOption;
		RunManager.Instance.RestSiteSynchronizer.AfterPlayerOptionChosen -= OnAfterPlayerSelectedRestSiteOption;
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
	}

	public void AfterSelectingOption(RestSiteOption option)
	{
		TaskHelper.RunSafely(AfterSelectingOptionAsync(option));
	}

	private async Task ShowFtueIfNeeded()
	{
		if (!SaveManager.Instance.SeenFtue("rest_site_ftue"))
		{
			_choicesContainer.Visible = false;
			Header.Visible = false;
			await Cmd.Wait(0.5f, _cts.Token);
			_choicesContainer.Visible = true;
			Header.Visible = true;
			Control choicesContainer = _choicesContainer;
			Color modulate = _choicesContainer.Modulate;
			modulate.A = 0f;
			choicesContainer.Modulate = modulate;
			MegaLabel header = Header;
			modulate = Header.Modulate;
			modulate.A = 0f;
			header.Modulate = modulate;
			Tween tween = CreateTween();
			tween.TweenProperty(_choicesContainer, "modulate:a", 1f, 0.5);
			tween.TweenProperty(Header, "modulate:a", 1f, 0.5);
			NModalContainer.Instance.Add(NRestSiteFtue.Create(_choicesContainer));
			SaveManager.Instance.MarkFtueAsComplete("rest_site_ftue");
		}
	}

	public void DisableOptions()
	{
		foreach (NRestSiteButton item in _choicesContainer.GetChildren().OfType<NRestSiteButton>())
		{
			item.Disable();
		}
	}

	public void EnableOptions()
	{
		foreach (NRestSiteButton item in _choicesContainer.GetChildren().OfType<NRestSiteButton>())
		{
			item.Enable();
		}
		ActiveScreenContext.Instance.Update();
	}

	public void AnimateDescriptionDown()
	{
		_descriptionPositionTween?.Kill();
		_descriptionPositionTween = CreateTween();
		_descriptionPositionTween.TweenProperty(Description, "position:y", 885f, 0.800000011920929).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
	}

	public void AnimateDescriptionUp()
	{
		_descriptionPositionTween?.Kill();
		_descriptionPositionTween = CreateTween();
		_descriptionPositionTween.TweenProperty(Description, "position:y", _originalDescriptionYPos, 0.800000011920929).SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
	}

	private void UpdateRestSiteOptions()
	{
		if (!this.IsValid() || !IsInsideTree())
		{
			return;
		}
		foreach (Node child in _choicesContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (RestSiteOption option in Options)
		{
			NRestSiteButton nRestSiteButton = NRestSiteButton.Create(option);
			_choicesContainer.AddChildSafely(nRestSiteButton);
			nRestSiteButton.Connect(NClickableControl.SignalName.Focused, Callable.From<NRestSiteButton>(RestSiteButtonHovered));
			nRestSiteButton.Connect(NClickableControl.SignalName.Unfocused, Callable.From<NRestSiteButton>(RestSiteButtonUnhovered));
		}
	}

	private void RestSiteButtonHovered(NRestSiteButton button)
	{
		RunManager.Instance.RestSiteSynchronizer.LocalOptionHovered(button.Option);
		_lastFocused = button;
	}

	private void RestSiteButtonUnhovered(NRestSiteButton button)
	{
		RunManager.Instance.RestSiteSynchronizer.LocalOptionHovered(null);
	}

	private void OnPlayerChangedHoveredRestSiteOption(ulong playerId)
	{
		if (_runState.Players.Count > 1)
		{
			NRestSiteCharacter nRestSiteCharacter = Characters.First((NRestSiteCharacter c) => c.Player.NetId == playerId);
			int? hoveredOptionIndex = RunManager.Instance.RestSiteSynchronizer.GetHoveredOptionIndex(playerId);
			RestSiteOption option = ((!hoveredOptionIndex.HasValue) ? null : RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(playerId)[hoveredOptionIndex.Value]);
			nRestSiteCharacter.ShowHoveredRestSiteOption(option);
		}
	}

	private void OnBeforePlayerSelectedRestSiteOption(RestSiteOption option, ulong playerId)
	{
		if (_runState.Players.Count > 1)
		{
			NRestSiteCharacter nRestSiteCharacter = Characters.First((NRestSiteCharacter c) => c.Player.NetId == playerId);
			nRestSiteCharacter.SetSelectingRestSiteOption(option);
		}
	}

	private void OnAfterPlayerSelectedRestSiteOption(RestSiteOption option, bool success, ulong playerId)
	{
		if (_runState.Players.Count <= 1)
		{
			return;
		}
		NRestSiteCharacter nRestSiteCharacter = Characters.First((NRestSiteCharacter c) => c.Player.NetId == playerId);
		nRestSiteCharacter.SetSelectingRestSiteOption(null);
		if (success)
		{
			nRestSiteCharacter.ShowSelectedRestSiteOption(option);
			if (!LocalContext.IsMe(nRestSiteCharacter.Player))
			{
				TaskHelper.RunSafely(option.DoRemotePostSelectVfx());
			}
		}
	}

	public NRestSiteButton? GetButtonForOption(RestSiteOption option)
	{
		foreach (NRestSiteButton item in _choicesContainer.GetChildren().OfType<NRestSiteButton>())
		{
			if (item.Option == option)
			{
				return item;
			}
		}
		return null;
	}

	public NRestSiteCharacter? GetCharacterForPlayer(Player player)
	{
		foreach (NRestSiteCharacter characterAnim in characterAnims)
		{
			if (characterAnim.Player == player)
			{
				return characterAnim;
			}
		}
		return null;
	}

	private async Task AfterSelectingOptionAsync(RestSiteOption option)
	{
		Task task = HideChoices(_cts.Token);
		Task task2 = option.DoLocalPostSelectVfx(_cts.Token);
		ExtinguishFireIfAble();
		global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = task;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = task2;
		await Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
		UpdateRestSiteOptions();
		ShowProceedButton();
		if (Options.Count > 0)
		{
			await ShowChoices(_cts.Token);
			ActiveScreenContext.Instance.Update();
		}
	}

	private void ShowProceedButton()
	{
		if (!_proceedButton.IsEnabled)
		{
			_proceedButton.Enable();
			NMapScreen.Instance.SetTravelEnabled(enabled: true);
		}
	}

	private void OnProceedButtonReleased(NButton _)
	{
		NMapScreen.Instance.Open();
	}

	public void SetText(string formattedText)
	{
		_descriptionTween?.Kill();
		Description.Modulate = Colors.White;
		Description.Text = formattedText;
	}

	public void FadeOutOptionDescription()
	{
		_descriptionTween?.Kill();
		_descriptionTween = CreateTween();
		_descriptionTween.TweenProperty(Description, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo)
			.From(1f);
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideRestSite))
		{
			_isDebugUiVisible = !_isDebugUiVisible;
			_choicesScreen.Modulate = (_isDebugUiVisible ? Colors.Transparent : Colors.White);
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create(_isDebugUiVisible ? "Hide RestSite UI" : "Show RestSite UI"));
		}
	}

	private void ExtinguishFireIfAble()
	{
		if (RunManager.Instance.RestSiteSynchronizer.GetLocalOptions().Count > 0 || !_restSiteLighting.Visible)
		{
			return;
		}
		foreach (NRestSiteCharacter characterAnim in characterAnims)
		{
			characterAnim.HideFlameGlow();
		}
		foreach (Control characterContainer in _characterContainers)
		{
			characterContainer.Modulate = Colors.DarkGray;
		}
		_restSiteLighting.Visible = false;
		NRunMusicController.Instance?.TriggerCampfireGoingOut();
	}

	private async Task ShowChoices(CancellationToken ct)
	{
		_choicesTween?.Kill();
		_choicesTween = CreateTween();
		_choicesTween.TweenProperty(_choicesScreen, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		await _choicesTween.AwaitFinished(ct);
	}

	private async Task HideChoices(CancellationToken ct)
	{
		foreach (NButton item in _choicesContainer.GetChildren().OfType<NButton>())
		{
			item.Disable();
		}
		_choicesTween?.Kill();
		_choicesTween = CreateTween();
		_choicesTween.TweenProperty(_choicesScreen, "modulate:a", 0f, 0.5);
		_lastFocused = null;
		await _choicesTween.AwaitFinished(ct);
	}

	private void OnActiveScreenUpdated()
	{
		this.UpdateControllerNavEnabled();
		if (ActiveScreenContext.Instance.IsCurrent(this) && Options.Count == 0)
		{
			ShowProceedButton();
		}
		else if (!_proceedButton.IsEnabled)
		{
			_proceedButton.Disable();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateDescriptionDown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateDescriptionUp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateRestSiteOptions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RestSiteButtonHovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RestSiteButtonUnhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPlayerChangedHoveredRestSiteOption, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowProceedButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonReleased, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "formattedText", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FadeOutOptionDescription, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ExtinguishFireIfAble, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.DisableOptions && args.Count == 0)
		{
			DisableOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableOptions && args.Count == 0)
		{
			EnableOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateDescriptionDown && args.Count == 0)
		{
			AnimateDescriptionDown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateDescriptionUp && args.Count == 0)
		{
			AnimateDescriptionUp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateRestSiteOptions && args.Count == 0)
		{
			UpdateRestSiteOptions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestSiteButtonHovered && args.Count == 1)
		{
			RestSiteButtonHovered(VariantUtils.ConvertTo<NRestSiteButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RestSiteButtonUnhovered && args.Count == 1)
		{
			RestSiteButtonUnhovered(VariantUtils.ConvertTo<NRestSiteButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPlayerChangedHoveredRestSiteOption && args.Count == 1)
		{
			OnPlayerChangedHoveredRestSiteOption(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowProceedButton && args.Count == 0)
		{
			ShowProceedButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnProceedButtonReleased && args.Count == 1)
		{
			OnProceedButtonReleased(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetText && args.Count == 1)
		{
			SetText(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FadeOutOptionDescription && args.Count == 0)
		{
			FadeOutOptionDescription();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ExtinguishFireIfAble && args.Count == 0)
		{
			ExtinguishFireIfAble();
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
		if (method == MethodName.DisableOptions)
		{
			return true;
		}
		if (method == MethodName.EnableOptions)
		{
			return true;
		}
		if (method == MethodName.AnimateDescriptionDown)
		{
			return true;
		}
		if (method == MethodName.AnimateDescriptionUp)
		{
			return true;
		}
		if (method == MethodName.UpdateRestSiteOptions)
		{
			return true;
		}
		if (method == MethodName.RestSiteButtonHovered)
		{
			return true;
		}
		if (method == MethodName.RestSiteButtonUnhovered)
		{
			return true;
		}
		if (method == MethodName.OnPlayerChangedHoveredRestSiteOption)
		{
			return true;
		}
		if (method == MethodName.ShowProceedButton)
		{
			return true;
		}
		if (method == MethodName.OnProceedButtonReleased)
		{
			return true;
		}
		if (method == MethodName.SetText)
		{
			return true;
		}
		if (method == MethodName.FadeOutOptionDescription)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ExtinguishFireIfAble)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Header)
		{
			Header = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName.Description)
		{
			Description = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName.BgContainer)
		{
			BgContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._choicesContainer)
		{
			_choicesContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._choicesScreen)
		{
			_choicesScreen = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._restSiteLighting)
		{
			_restSiteLighting = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._descriptionTween)
		{
			_descriptionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._descriptionPositionTween)
		{
			_descriptionPositionTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._choicesTween)
		{
			_choicesTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalDescriptionYPos)
		{
			_originalDescriptionYPos = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._lastFocused)
		{
			_lastFocused = VariantUtils.ConvertTo<Control>(in value);
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
		if (name == PropertyName.Header)
		{
			value = VariantUtils.CreateFrom<MegaLabel>(Header);
			return true;
		}
		if (name == PropertyName.Description)
		{
			value = VariantUtils.CreateFrom<MegaRichTextLabel>(Description);
			return true;
		}
		Control from;
		if (name == PropertyName.BgContainer)
		{
			from = BgContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._choicesContainer)
		{
			value = VariantUtils.CreateFrom(in _choicesContainer);
			return true;
		}
		if (name == PropertyName._choicesScreen)
		{
			value = VariantUtils.CreateFrom(in _choicesScreen);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._restSiteLighting)
		{
			value = VariantUtils.CreateFrom(in _restSiteLighting);
			return true;
		}
		if (name == PropertyName._descriptionTween)
		{
			value = VariantUtils.CreateFrom(in _descriptionTween);
			return true;
		}
		if (name == PropertyName._descriptionPositionTween)
		{
			value = VariantUtils.CreateFrom(in _descriptionPositionTween);
			return true;
		}
		if (name == PropertyName._choicesTween)
		{
			value = VariantUtils.CreateFrom(in _choicesTween);
			return true;
		}
		if (name == PropertyName._originalDescriptionYPos)
		{
			value = VariantUtils.CreateFrom(in _originalDescriptionYPos);
			return true;
		}
		if (name == PropertyName._lastFocused)
		{
			value = VariantUtils.CreateFrom(in _lastFocused);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._choicesContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._choicesScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ProceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._restSiteLighting, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionPositionTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._choicesTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._originalDescriptionYPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lastFocused, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Header, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Description, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Header, Variant.From<MegaLabel>(Header));
		info.AddProperty(PropertyName.Description, Variant.From<MegaRichTextLabel>(Description));
		info.AddProperty(PropertyName.BgContainer, Variant.From<Control>(BgContainer));
		info.AddProperty(PropertyName._choicesContainer, Variant.From(in _choicesContainer));
		info.AddProperty(PropertyName._choicesScreen, Variant.From(in _choicesScreen));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._restSiteLighting, Variant.From(in _restSiteLighting));
		info.AddProperty(PropertyName._descriptionTween, Variant.From(in _descriptionTween));
		info.AddProperty(PropertyName._descriptionPositionTween, Variant.From(in _descriptionPositionTween));
		info.AddProperty(PropertyName._choicesTween, Variant.From(in _choicesTween));
		info.AddProperty(PropertyName._originalDescriptionYPos, Variant.From(in _originalDescriptionYPos));
		info.AddProperty(PropertyName._lastFocused, Variant.From(in _lastFocused));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Header, out var value))
		{
			Header = value.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName.Description, out var value2))
		{
			Description = value2.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName.BgContainer, out var value3))
		{
			BgContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._choicesContainer, out var value4))
		{
			_choicesContainer = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._choicesScreen, out var value5))
		{
			_choicesScreen = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value6))
		{
			_proceedButton = value6.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._restSiteLighting, out var value7))
		{
			_restSiteLighting = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._descriptionTween, out var value8))
		{
			_descriptionTween = value8.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._descriptionPositionTween, out var value9))
		{
			_descriptionPositionTween = value9.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._choicesTween, out var value10))
		{
			_choicesTween = value10.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalDescriptionYPos, out var value11))
		{
			_originalDescriptionYPos = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._lastFocused, out var value12))
		{
			_lastFocused = value12.As<Control>();
		}
	}
}
