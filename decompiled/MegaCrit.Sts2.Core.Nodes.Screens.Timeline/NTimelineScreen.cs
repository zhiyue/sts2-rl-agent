using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Achievements;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Timeline.UnlockScreens;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NTimelineScreen.cs")]
public class NTimelineScreen : NSubmenu
{
	public new class MethodName : NSubmenu.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName OnSubmenuOpened = "OnSubmenuOpened";

		public new static readonly StringName OnSubmenuClosed = "OnSubmenuClosed";

		public new static readonly StringName OnSubmenuShown = "OnSubmenuShown";

		public new static readonly StringName OnSubmenuHidden = "OnSubmenuHidden";

		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnBackButtonPressed = "OnBackButtonPressed";

		public static readonly StringName GetEraTexturePath = "GetEraTexturePath";

		public static readonly StringName GetSlot = "GetSlot";

		public static readonly StringName OpenInspectScreen = "OpenInspectScreen";

		public static readonly StringName QueueMiscUnlock = "QueueMiscUnlock";

		public static readonly StringName SetScreenDraggability = "SetScreenDraggability";

		public static readonly StringName ShowBackstopAndHideUi = "ShowBackstopAndHideUi";

		public static readonly StringName OpenQueuedScreen = "OpenQueuedScreen";

		public static readonly StringName IsScreenQueued = "IsScreenQueued";

		public static readonly StringName IsInspectScreenQueued = "IsInspectScreenQueued";

		public static readonly StringName ShowHeaderAndActionsUi = "ShowHeaderAndActionsUi";

		public static readonly StringName DisableInput = "DisableInput";

		public static readonly StringName EnableInput = "EnableInput";

		public static readonly StringName RefreshBackButton = "RefreshBackButton";

		public static readonly StringName ResetScreen = "ResetScreen";

		public static readonly StringName GetReminderVfxHolder = "GetReminderVfxHolder";
	}

	public new class PropertyName : NSubmenu.PropertyName
	{
		public new static readonly StringName InitialFocusedControl = "InitialFocusedControl";

		public static readonly StringName _inspectScreen = "_inspectScreen";

		public static readonly StringName _reminderText = "_reminderText";

		public static readonly StringName _reminderVfxHolder = "_reminderVfxHolder";

		public static readonly StringName _backstop = "_backstop";

		public static readonly StringName _inputBlocker = "_inputBlocker";

		public static readonly StringName _lineContainer = "_lineContainer";

		public static readonly StringName _line = "_line";

		public static readonly StringName _epochSlotContainer = "_epochSlotContainer";

		public static readonly StringName _slotsContainer = "_slotsContainer";

		public new static readonly StringName _backButton = "_backButton";

		public static readonly StringName _isUiVisible = "_isUiVisible";

		public static readonly StringName _queuedInspectScreen = "_queuedInspectScreen";

		public static readonly StringName _lineGrowTween = "_lineGrowTween";

		public static readonly StringName _backstopTween = "_backstopTween";
	}

	public new class SignalName : NSubmenu.SignalName
	{
	}

	private static readonly string _scenePath = SceneHelper.GetScenePath("timeline_screen/timeline_screen");

	private const string _placeEpochSparksPath = "res://scenes/timeline_screen/place_epoch_sparks.tscn";

	private NEpochInspectScreen _inspectScreen;

	private NEpochReminderText _reminderText;

	private Control _reminderVfxHolder;

	private ColorRect _backstop;

	private Control _inputBlocker;

	private Control _lineContainer;

	private Control _line;

	private HBoxContainer _epochSlotContainer;

	private NSlotsContainer _slotsContainer;

	private NBackButton _backButton;

	private ProgressState _save;

	private bool _isUiVisible;

	private Dictionary<EpochEra, NEraColumn> _uniqueEpochEras = new Dictionary<EpochEra, NEraColumn>();

	private NEpochSlot? _queuedInspectScreen;

	private Queue<NUnlockScreen> _unlockScreens = new Queue<NUnlockScreen>();

	private Tween? _lineGrowTween;

	private Tween? _backstopTween;

	public static string[] AssetPaths
	{
		get
		{
			List<string> list = new List<string>();
			list.Add(_scenePath);
			list.Add("res://scenes/timeline_screen/place_epoch_sparks.tscn");
			list.Add(NEpochHighlightVfx.scenePath);
			list.Add(NEpochOffscreenVfx.scenePath);
			list.Add(NEpochInspectScreen.lockedImagePath);
			list.AddRange(NUnlockTimelineScreen.AssetPaths);
			list.AddRange(NUnlockPotionsScreen.AssetPaths);
			list.AddRange(NUnlockRelicsScreen.AssetPaths);
			list.AddRange(NUnlockCardsScreen.AssetPaths);
			list.AddRange(NUnlockMiscScreen.AssetPaths);
			list.AddRange(NUnlockCharacterScreen.AssetPaths);
			list.AddRange(NEraColumn.assetPaths);
			list.AddRange(GetAllEraTexturePaths());
			return list.ToArray();
		}
	}

	public static NTimelineScreen Instance => NGame.Instance.MainMenu.SubmenuStack.GetSubmenuType<NTimelineScreen>();

	protected override Control? InitialFocusedControl => _epochSlotContainer.GetChildren().SelectMany((Node c) => c.GetChildren().OfType<NEpochSlot>()).FirstOrDefault((NEpochSlot s) => s.model is NeowEpoch);

	public static NTimelineScreen? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NTimelineScreen>(PackedScene.GenEditState.Disabled);
	}

	public override void OnSubmenuOpened()
	{
		ResetScreen();
		DisableInput();
		SerializableEpoch serializableEpoch = SaveManager.Instance.Progress.Epochs.FirstOrDefault((SerializableEpoch e) => e.Id == EpochModel.GetId<NeowEpoch>());
		bool flag = serializableEpoch == null;
		bool flag2 = flag;
		if (!flag2)
		{
			EpochState state = serializableEpoch.State;
			bool flag3 = (uint)(state - 1) <= 1u;
			flag2 = flag3;
		}
		if (flag2)
		{
			SaveManager.Instance.Progress.ObtainEpoch(EpochModel.GetId<NeowEpoch>());
		}
		if (SaveManager.Instance.IsNeowDiscovered())
		{
			TaskHelper.RunSafely(FirstTimeLogic());
		}
		else
		{
			SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_open");
			TaskHelper.RunSafely(InitScreen());
		}
		SetScreenDraggability();
		AchievementsHelper.CheckTimelineComplete();
	}

	public override void OnSubmenuClosed()
	{
		base.OnSubmenuClosed();
		ResetScreen();
	}

	protected override void OnSubmenuShown()
	{
		base.ProcessMode = ProcessModeEnum.Inherit;
		RefreshBackButton();
	}

	protected override void OnSubmenuHidden()
	{
		base.ProcessMode = ProcessModeEnum.Disabled;
		NGame.Instance.MainMenu?.RefreshButtons();
	}

	public override void _Ready()
	{
		ConnectSignals();
		_epochSlotContainer = GetNode<HBoxContainer>("%EpochSlots");
		_reminderText = GetNode<NEpochReminderText>("%EpochReminderText");
		_reminderVfxHolder = GetNode<Control>("%ReminderVfxHolder");
		_inputBlocker = GetNode<Control>("%InputBlocker");
		_backstop = GetNode<ColorRect>("%SharedBackstop");
		_inspectScreen = GetNode<NEpochInspectScreen>("%EpochInspectScreen");
		_line = GetNode<Control>("%Line");
		_lineContainer = GetNode<Control>("%LineContainer");
		_slotsContainer = GetNode<NSlotsContainer>("%SlotsContainer");
		_backButton = GetNode<NBackButton>("BackButton");
		_backButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnBackButtonPressed));
		_save = SaveManager.Instance.Progress;
		Tween tween = CreateTween();
		tween.TweenProperty(_slotsContainer, "modulate:a", 1f, 1.0);
	}

	private static void OnBackButtonPressed(NButton obj)
	{
		SfxCmd.Play("event:/sfx/ui/map/map_close");
	}

	private async Task FirstTimeLogic()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		NTimelineTutorial nTimelineTutorial = SceneHelper.Instantiate<NTimelineTutorial>("timeline_screen/timeline_tutorial");
		this.AddChildSafely(nTimelineTutorial);
		nTimelineTutorial.Init(this);
	}

	public async Task SpawnFirstTimeTimeline()
	{
		SfxCmd.Play("event:/sfx/ui/timeline/ui_timeline_open");
		Log.Info("Running first time logic");
		List<EpochSlotData> slotsToAdd = new List<EpochSlotData>(1)
		{
			new EpochSlotData(EpochModel.GetId<NeowEpoch>(), EpochSlotState.Obtained)
		};
		await AddEpochSlots(slotsToAdd, isAnimated: true);
		SaveManager.Instance.UnlockSlot(EpochModel.GetId<NeowEpoch>());
		EnableInput();
	}

	private async Task InitScreen()
	{
		Log.Info("Initializing Timeline:");
		List<EpochSlotData> list = new List<EpochSlotData>();
		_lineGrowTween?.Kill();
		_lineGrowTween = CreateTween();
		_lineGrowTween.TweenProperty(_lineContainer, "modulate:a", 1f, 0.5);
		foreach (SerializableEpoch epoch in _save.Epochs)
		{
			if (epoch.State != EpochState.ObtainedNoSlot)
			{
				list.Add(new EpochSlotData(epoch.Id, EpochSlotState.NotObtained));
			}
		}
		list = list.OrderBy((EpochSlotData a) => a.EraPosition).ToList();
		await AddEpochSlots(list, isAnimated: false);
		int num = 0;
		foreach (SerializableEpoch epoch2 in _save.Epochs)
		{
			EpochModel epochModel = EpochModel.Get(epoch2.Id);
			if (epoch2.State <= EpochState.ObtainedNoSlot)
			{
				continue;
			}
			foreach (Node child in _uniqueEpochEras[epochModel.Era].GetChildren())
			{
				if (child is NEpochSlot nEpochSlot && nEpochSlot.eraPosition == epochModel.EraPosition)
				{
					num++;
					nEpochSlot.SetState((epoch2.State >= EpochState.Revealed) ? EpochSlotState.Complete : EpochSlotState.Obtained);
					TaskHelper.RunSafely(child.GetParent<NEraColumn>().SpawnNameAndYear());
				}
			}
		}
		Log.Info($"{num} Epochs are complete");
		TaskHelper.RunSafely(NavigateToRevealableSlot());
	}

	private async Task NavigateToRevealableSlot()
	{
		if (SaveManager.Instance.GetDiscoveredEpochCount() == 0)
		{
			EnableInput();
			return;
		}
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		float getInitX = _slotsContainer.GetInitX;
		float slotPositionX = 0f;
		float num = float.MaxValue;
		foreach (NEraColumn value in _uniqueEpochEras.Values)
		{
			foreach (NEpochSlot item in value.GetChildrenRecursive<NEpochSlot>())
			{
				if (item.State == EpochSlotState.Obtained)
				{
					float num2 = Math.Abs(getInitX - item.GlobalPosition.X);
					if (num2 < num)
					{
						slotPositionX = item.GlobalPosition.X;
						num = num2;
					}
				}
			}
		}
		await TaskHelper.RunSafely(_slotsContainer.LerpToSlot(slotPositionX));
		EnableInput();
	}

	public async Task AddEpochSlots(List<EpochSlotData> slotsToAdd, bool isAnimated)
	{
		List<NEraColumn> list = new List<NEraColumn>();
		if (isAnimated)
		{
			foreach (NEraColumn value2 in _uniqueEpochEras.Values)
			{
				TaskHelper.RunSafely(value2.SaveBeforeAnimationPosition());
			}
		}
		foreach (EpochSlotData item in slotsToAdd)
		{
			if (!_uniqueEpochEras.TryGetValue(item.Era, out NEraColumn value))
			{
				NEraColumn nEraColumn = NEraColumn.Create(item);
				_epochSlotContainer.AddChildSafely(nEraColumn);
				int toIndex = 0;
				foreach (Node child in _epochSlotContainer.GetChildren())
				{
					if (child is NEraColumn nEraColumn2 && nEraColumn.era > nEraColumn2.era)
					{
						toIndex = nEraColumn2.GetIndex() + 1;
					}
				}
				_epochSlotContainer.MoveChild(nEraColumn, toIndex);
				list.Add(nEraColumn);
				_uniqueEpochEras.Add(item.Era, nEraColumn);
			}
			else
			{
				value.AddSlot(item);
			}
		}
		Log.Info($" Created {slotsToAdd.Count} Epoch slots");
		Log.Info($" Created {list.Count} Era columns");
		if (isAnimated)
		{
			List<Vector2> list2 = PredictHBoxLayout(_epochSlotContainer);
			foreach (NEraColumn value3 in _uniqueEpochEras.Values)
			{
				value3.SetPredictedPosition(list2[value3.GetIndex()]);
			}
			await GrowTimelineAndAddEraIcons(list);
		}
		else
		{
			InitLineAndIcons(list);
		}
	}

	private List<Vector2> PredictHBoxLayout(HBoxContainer hbox)
	{
		float num = 0f;
		float num2 = hbox.GetThemeConstant(ThemeConstants.BoxContainer.separation, "HBoxContainer");
		List<Control> list = (from c in hbox.GetChildren().OfType<Control>()
			where c.Visible
			select c).ToList();
		int num3 = 0;
		foreach (Control item in list)
		{
			num += item.CustomMinimumSize.X;
			if ((item.SizeFlagsHorizontal & SizeFlags.Expand) != SizeFlags.ShrinkBegin)
			{
				num3++;
			}
		}
		num += num2 * (float)Math.Max(list.Count - 1, 0);
		float num4 = hbox.Size.X - num;
		float num5 = ((num3 > 0) ? (num4 / (float)num3) : 0f);
		float num6 = 0f;
		List<Vector2> list2 = new List<Vector2>();
		foreach (Control item2 in list)
		{
			float num7 = item2.CustomMinimumSize.X;
			if ((item2.SizeFlagsHorizontal & SizeFlags.Expand) != SizeFlags.ShrinkBegin)
			{
				num7 += num5;
			}
			list2.Add(new Vector2(num6, 0f));
			num6 += num7 + num2;
		}
		return list2;
	}

	private async Task GrowTimelineAndAddEraIcons(List<NEraColumn> newlyCreatedColumns)
	{
		if (newlyCreatedColumns.Count > 0)
		{
			_lineGrowTween?.Kill();
			_lineGrowTween = CreateTween().SetParallel();
			_lineGrowTween.TweenProperty(_lineContainer, "modulate:a", 1f, 0.5);
			_lineGrowTween.TweenProperty(_line, "custom_minimum_size:x", (float)_uniqueEpochEras.Count * 226f, 2.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Cubic);
			await ToSignal(_lineGrowTween, Tween.SignalName.Finished);
			Log.Info("Spawning slots...");
			foreach (NEraColumn newlyCreatedColumn in newlyCreatedColumns)
			{
				newlyCreatedColumn.SpawnIcon();
			}
			newlyCreatedColumns.Clear();
		}
		foreach (NEraColumn value in _uniqueEpochEras.Values)
		{
			TaskHelper.RunSafely(value.SpawnSlots(isAnimated: true));
		}
	}

	private void InitLineAndIcons(List<NEraColumn> newlyCreatedColumns)
	{
		if (newlyCreatedColumns.Count == 0)
		{
			return;
		}
		_line.CustomMinimumSize = new Vector2((float)_uniqueEpochEras.Count * 226f, _line.CustomMinimumSize.Y);
		foreach (NEraColumn newlyCreatedColumn in newlyCreatedColumns)
		{
			newlyCreatedColumn.SpawnIcon();
		}
		foreach (NEraColumn value in _uniqueEpochEras.Values)
		{
			TaskHelper.RunSafely(value.SpawnSlots(isAnimated: false));
		}
		newlyCreatedColumns.Clear();
	}

	public static (Texture2D Texture, string Name) GetEraIcon(EpochEra era)
	{
		return (Texture: PreloadManager.Cache.GetTexture2D(GetEraTexturePath(era)), Name: StringHelper.Slugify(era.ToString()));
	}

	private static string GetEraTexturePath(EpochEra era)
	{
		if (era >= EpochEra.Seeds0)
		{
			return $"res://images/atlases/era_atlas.sprites/era_{era}.tres";
		}
		return $"res://images/atlases/era_atlas.sprites/era_minus_{Math.Abs((int)era)}.tres";
	}

	private static IEnumerable<string> GetAllEraTexturePaths()
	{
		EpochEra[] values = Enum.GetValues<EpochEra>();
		foreach (EpochEra era in values)
		{
			yield return GetEraTexturePath(era);
		}
	}

	private NEpochSlot? GetSlot(EpochEra era, int position)
	{
		foreach (KeyValuePair<EpochEra, NEraColumn> uniqueEpochEra in _uniqueEpochEras)
		{
			if (uniqueEpochEra.Key == era)
			{
				return (NEpochSlot)uniqueEpochEra.Value.GetChild(uniqueEpochEra.Value.GetChildCount() - position - 2);
			}
		}
		Log.Error($"Could not find Epoch slot: {era}, {position}");
		return null;
	}

	public void OpenInspectScreen(NEpochSlot slot, bool playAnimation)
	{
		if (playAnimation)
		{
			TaskHelper.RunSafely(slot.GetParent<NEraColumn>().SpawnNameAndYear());
		}
		_lastFocusedControl = slot;
		TaskHelper.RunSafely(_inspectScreen.Open(slot, slot.model, playAnimation));
	}

	public void QueueMiscUnlock(string text)
	{
		NUnlockMiscScreen nUnlockMiscScreen = NUnlockMiscScreen.Create();
		nUnlockMiscScreen.SetUnlocks(text);
		_unlockScreens.Enqueue(nUnlockMiscScreen);
	}

	public void QueueCharacterUnlock<T>(EpochModel epoch) where T : CharacterModel
	{
		_unlockScreens.Enqueue(NUnlockCharacterScreen.Create(epoch, ModelDb.Character<T>()));
	}

	public void QueueCardUnlock(IReadOnlyList<CardModel> cards)
	{
		NUnlockCardsScreen nUnlockCardsScreen = NUnlockCardsScreen.Create();
		nUnlockCardsScreen.SetCards(cards);
		_unlockScreens.Enqueue(nUnlockCardsScreen);
	}

	public void QueueRelicUnlock(List<RelicModel> relics)
	{
		NUnlockRelicsScreen nUnlockRelicsScreen = NUnlockRelicsScreen.Create();
		nUnlockRelicsScreen.SetRelics(relics);
		_unlockScreens.Enqueue(nUnlockRelicsScreen);
	}

	public void QueuePotionUnlock(List<PotionModel> potions)
	{
		NUnlockPotionsScreen nUnlockPotionsScreen = NUnlockPotionsScreen.Create();
		nUnlockPotionsScreen.SetPotions(potions);
		_unlockScreens.Enqueue(nUnlockPotionsScreen);
	}

	public void QueueTimelineExpansion(List<EpochSlotData> eraData)
	{
		NUnlockTimelineScreen nUnlockTimelineScreen = NUnlockTimelineScreen.Create();
		nUnlockTimelineScreen.SetUnlocks(eraData);
		_unlockScreens.Enqueue(nUnlockTimelineScreen);
	}

	public void SetScreenDraggability()
	{
		_slotsContainer.MouseFilter = (MouseFilterEnum)((_save.Epochs.Count <= 4) ? 2 : 0);
	}

	public void ShowBackstopAndHideUi()
	{
		_backstop.Visible = true;
		_backstopTween?.Kill();
		_backstopTween = CreateTween().SetParallel();
		_backstopTween.TweenProperty(_slotsContainer, "modulate:a", 0.1f, 0.4);
		_backstopTween.TweenProperty(_backstop, "modulate:a", 0.5f, 0.4);
		_backButton.Disable();
		_reminderText.AnimateOut();
	}

	public async Task HideBackstopAndShowUi(bool showBackButton)
	{
		_backstopTween?.FastForwardToCompletion();
		_backstopTween = CreateTween().SetParallel();
		_backstopTween.TweenProperty(_slotsContainer, "modulate:a", 1f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		_backstopTween.TweenProperty(_backstop, "modulate:a", 0f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		if (showBackButton)
		{
			RefreshBackButton();
		}
		await ToSignal(_backstopTween, Tween.SignalName.Finished);
		_backstop.Visible = false;
	}

	public void OpenQueuedScreen()
	{
		NUnlockScreen nUnlockScreen = _unlockScreens.Dequeue();
		this.AddChildSafely(nUnlockScreen);
		nUnlockScreen.Open();
	}

	public bool IsScreenQueued()
	{
		return _unlockScreens.Count > 0;
	}

	private bool IsInspectScreenQueued()
	{
		return _queuedInspectScreen != null;
	}

	private async Task SpawnEraLabel(EpochEra era)
	{
		await _uniqueEpochEras[era].SpawnNameAndYear();
	}

	public void ShowHeaderAndActionsUi()
	{
		if (!_isUiVisible)
		{
			_isUiVisible = true;
		}
	}

	public void DisableInput()
	{
		_inputBlocker.Visible = true;
		_inputBlocker.MouseFilter = MouseFilterEnum.Stop;
		_slotsContainer.SetEnabled(enabled: false);
		GetViewport().GuiReleaseFocus();
	}

	public void EnableInput()
	{
		if (_queuedInspectScreen == null && _unlockScreens.Count == 0)
		{
			RefreshBackButton();
			_inputBlocker.Visible = false;
			_inputBlocker.MouseFilter = MouseFilterEnum.Ignore;
			_slotsContainer.SetEnabled(enabled: true);
			ActiveScreenContext.Instance.Update();
		}
	}

	private void RefreshBackButton()
	{
		if (SaveManager.Instance.GetDiscoveredEpochCount() > 0)
		{
			if (_epochSlotContainer.GetChildCount() > 1)
			{
				_reminderText.AnimateIn();
			}
			_backButton.Disable();
		}
		else
		{
			_backButton.Enable();
		}
	}

	private void ResetScreen()
	{
		Control lineContainer = _lineContainer;
		Color modulate = _lineContainer.Modulate;
		modulate.A = 0f;
		lineContainer.Modulate = modulate;
		Log.Info("Cleaning up Timeline screen...");
		_uniqueEpochEras = new Dictionary<EpochEra, NEraColumn>();
		_queuedInspectScreen = null;
		_unlockScreens = new Queue<NUnlockScreen>();
		_epochSlotContainer.FreeChildren();
		_slotsContainer.Reset();
	}

	public Control GetReminderVfxHolder()
	{
		return _reminderVfxHolder;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(22);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnSubmenuHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnBackButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetEraTexturePath, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "era", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetSlot, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "era", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OpenInspectScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "slot", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "playAnimation", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.QueueMiscUnlock, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.String, "text", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetScreenDraggability, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowBackstopAndHideUi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OpenQueuedScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsScreenQueued, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsInspectScreenQueued, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowHeaderAndActionsUi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisableInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshBackButton, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ResetScreen, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetReminderVfxHolder, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NTimelineScreen>(Create());
			return true;
		}
		if (method == MethodName.OnSubmenuOpened && args.Count == 0)
		{
			OnSubmenuOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuClosed && args.Count == 0)
		{
			OnSubmenuClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuShown && args.Count == 0)
		{
			OnSubmenuShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnSubmenuHidden && args.Count == 0)
		{
			OnSubmenuHidden();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnBackButtonPressed && args.Count == 1)
		{
			OnBackButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetEraTexturePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetEraTexturePath(VariantUtils.ConvertTo<EpochEra>(in args[0])));
			return true;
		}
		if (method == MethodName.GetSlot && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NEpochSlot>(GetSlot(VariantUtils.ConvertTo<EpochEra>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.OpenInspectScreen && args.Count == 2)
		{
			OpenInspectScreen(VariantUtils.ConvertTo<NEpochSlot>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.QueueMiscUnlock && args.Count == 1)
		{
			QueueMiscUnlock(VariantUtils.ConvertTo<string>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetScreenDraggability && args.Count == 0)
		{
			SetScreenDraggability();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowBackstopAndHideUi && args.Count == 0)
		{
			ShowBackstopAndHideUi();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OpenQueuedScreen && args.Count == 0)
		{
			OpenQueuedScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsScreenQueued && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsScreenQueued());
			return true;
		}
		if (method == MethodName.IsInspectScreenQueued && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsInspectScreenQueued());
			return true;
		}
		if (method == MethodName.ShowHeaderAndActionsUi && args.Count == 0)
		{
			ShowHeaderAndActionsUi();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableInput && args.Count == 0)
		{
			DisableInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableInput && args.Count == 0)
		{
			EnableInput();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshBackButton && args.Count == 0)
		{
			RefreshBackButton();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ResetScreen && args.Count == 0)
		{
			ResetScreen();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetReminderVfxHolder && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Control>(GetReminderVfxHolder());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NTimelineScreen>(Create());
			return true;
		}
		if (method == MethodName.OnBackButtonPressed && args.Count == 1)
		{
			OnBackButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetEraTexturePath && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetEraTexturePath(VariantUtils.ConvertTo<EpochEra>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuOpened)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuClosed)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuShown)
		{
			return true;
		}
		if (method == MethodName.OnSubmenuHidden)
		{
			return true;
		}
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.OnBackButtonPressed)
		{
			return true;
		}
		if (method == MethodName.GetEraTexturePath)
		{
			return true;
		}
		if (method == MethodName.GetSlot)
		{
			return true;
		}
		if (method == MethodName.OpenInspectScreen)
		{
			return true;
		}
		if (method == MethodName.QueueMiscUnlock)
		{
			return true;
		}
		if (method == MethodName.SetScreenDraggability)
		{
			return true;
		}
		if (method == MethodName.ShowBackstopAndHideUi)
		{
			return true;
		}
		if (method == MethodName.OpenQueuedScreen)
		{
			return true;
		}
		if (method == MethodName.IsScreenQueued)
		{
			return true;
		}
		if (method == MethodName.IsInspectScreenQueued)
		{
			return true;
		}
		if (method == MethodName.ShowHeaderAndActionsUi)
		{
			return true;
		}
		if (method == MethodName.DisableInput)
		{
			return true;
		}
		if (method == MethodName.EnableInput)
		{
			return true;
		}
		if (method == MethodName.RefreshBackButton)
		{
			return true;
		}
		if (method == MethodName.ResetScreen)
		{
			return true;
		}
		if (method == MethodName.GetReminderVfxHolder)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._inspectScreen)
		{
			_inspectScreen = VariantUtils.ConvertTo<NEpochInspectScreen>(in value);
			return true;
		}
		if (name == PropertyName._reminderText)
		{
			_reminderText = VariantUtils.ConvertTo<NEpochReminderText>(in value);
			return true;
		}
		if (name == PropertyName._reminderVfxHolder)
		{
			_reminderVfxHolder = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(in value);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			_inputBlocker = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._lineContainer)
		{
			_lineContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._line)
		{
			_line = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._epochSlotContainer)
		{
			_epochSlotContainer = VariantUtils.ConvertTo<HBoxContainer>(in value);
			return true;
		}
		if (name == PropertyName._slotsContainer)
		{
			_slotsContainer = VariantUtils.ConvertTo<NSlotsContainer>(in value);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			_backButton = VariantUtils.ConvertTo<NBackButton>(in value);
			return true;
		}
		if (name == PropertyName._isUiVisible)
		{
			_isUiVisible = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._queuedInspectScreen)
		{
			_queuedInspectScreen = VariantUtils.ConvertTo<NEpochSlot>(in value);
			return true;
		}
		if (name == PropertyName._lineGrowTween)
		{
			_lineGrowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			_backstopTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InitialFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(InitialFocusedControl);
			return true;
		}
		if (name == PropertyName._inspectScreen)
		{
			value = VariantUtils.CreateFrom(in _inspectScreen);
			return true;
		}
		if (name == PropertyName._reminderText)
		{
			value = VariantUtils.CreateFrom(in _reminderText);
			return true;
		}
		if (name == PropertyName._reminderVfxHolder)
		{
			value = VariantUtils.CreateFrom(in _reminderVfxHolder);
			return true;
		}
		if (name == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom(in _backstop);
			return true;
		}
		if (name == PropertyName._inputBlocker)
		{
			value = VariantUtils.CreateFrom(in _inputBlocker);
			return true;
		}
		if (name == PropertyName._lineContainer)
		{
			value = VariantUtils.CreateFrom(in _lineContainer);
			return true;
		}
		if (name == PropertyName._line)
		{
			value = VariantUtils.CreateFrom(in _line);
			return true;
		}
		if (name == PropertyName._epochSlotContainer)
		{
			value = VariantUtils.CreateFrom(in _epochSlotContainer);
			return true;
		}
		if (name == PropertyName._slotsContainer)
		{
			value = VariantUtils.CreateFrom(in _slotsContainer);
			return true;
		}
		if (name == PropertyName._backButton)
		{
			value = VariantUtils.CreateFrom(in _backButton);
			return true;
		}
		if (name == PropertyName._isUiVisible)
		{
			value = VariantUtils.CreateFrom(in _isUiVisible);
			return true;
		}
		if (name == PropertyName._queuedInspectScreen)
		{
			value = VariantUtils.CreateFrom(in _queuedInspectScreen);
			return true;
		}
		if (name == PropertyName._lineGrowTween)
		{
			value = VariantUtils.CreateFrom(in _lineGrowTween);
			return true;
		}
		if (name == PropertyName._backstopTween)
		{
			value = VariantUtils.CreateFrom(in _backstopTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inspectScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reminderText, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._reminderVfxHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._inputBlocker, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lineContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._line, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._epochSlotContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slotsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isUiVisible, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._queuedInspectScreen, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lineGrowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._backstopTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.InitialFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._inspectScreen, Variant.From(in _inspectScreen));
		info.AddProperty(PropertyName._reminderText, Variant.From(in _reminderText));
		info.AddProperty(PropertyName._reminderVfxHolder, Variant.From(in _reminderVfxHolder));
		info.AddProperty(PropertyName._backstop, Variant.From(in _backstop));
		info.AddProperty(PropertyName._inputBlocker, Variant.From(in _inputBlocker));
		info.AddProperty(PropertyName._lineContainer, Variant.From(in _lineContainer));
		info.AddProperty(PropertyName._line, Variant.From(in _line));
		info.AddProperty(PropertyName._epochSlotContainer, Variant.From(in _epochSlotContainer));
		info.AddProperty(PropertyName._slotsContainer, Variant.From(in _slotsContainer));
		info.AddProperty(PropertyName._backButton, Variant.From(in _backButton));
		info.AddProperty(PropertyName._isUiVisible, Variant.From(in _isUiVisible));
		info.AddProperty(PropertyName._queuedInspectScreen, Variant.From(in _queuedInspectScreen));
		info.AddProperty(PropertyName._lineGrowTween, Variant.From(in _lineGrowTween));
		info.AddProperty(PropertyName._backstopTween, Variant.From(in _backstopTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._inspectScreen, out var value))
		{
			_inspectScreen = value.As<NEpochInspectScreen>();
		}
		if (info.TryGetProperty(PropertyName._reminderText, out var value2))
		{
			_reminderText = value2.As<NEpochReminderText>();
		}
		if (info.TryGetProperty(PropertyName._reminderVfxHolder, out var value3))
		{
			_reminderVfxHolder = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._backstop, out var value4))
		{
			_backstop = value4.As<ColorRect>();
		}
		if (info.TryGetProperty(PropertyName._inputBlocker, out var value5))
		{
			_inputBlocker = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._lineContainer, out var value6))
		{
			_lineContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._line, out var value7))
		{
			_line = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._epochSlotContainer, out var value8))
		{
			_epochSlotContainer = value8.As<HBoxContainer>();
		}
		if (info.TryGetProperty(PropertyName._slotsContainer, out var value9))
		{
			_slotsContainer = value9.As<NSlotsContainer>();
		}
		if (info.TryGetProperty(PropertyName._backButton, out var value10))
		{
			_backButton = value10.As<NBackButton>();
		}
		if (info.TryGetProperty(PropertyName._isUiVisible, out var value11))
		{
			_isUiVisible = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._queuedInspectScreen, out var value12))
		{
			_queuedInspectScreen = value12.As<NEpochSlot>();
		}
		if (info.TryGetProperty(PropertyName._lineGrowTween, out var value13))
		{
			_lineGrowTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._backstopTween, out var value14))
		{
			_backstopTween = value14.As<Tween>();
		}
	}
}
