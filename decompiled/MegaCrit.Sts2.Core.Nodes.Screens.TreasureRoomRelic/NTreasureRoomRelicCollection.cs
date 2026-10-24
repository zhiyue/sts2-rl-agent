using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

/// <summary>
/// Selection screen for relics at the treasure room screen.
/// In singleplayer, this just shows one relic. In multiplayer, this shows multiple relics and handles animation that
/// occurs after all players select a relic.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/TreasureRoomRelic/NTreasureRoomRelicCollection.cs")]
public class NTreasureRoomRelicCollection : Control, IScreenContext
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
		/// Cached name for the 'InitializeRelics' method.
		/// </summary>
		public static readonly StringName InitializeRelics = "InitializeRelics";

		/// <summary>
		/// Cached name for the 'SpawnEmptyChestVfx' method.
		/// </summary>
		public static readonly StringName SpawnEmptyChestVfx = "SpawnEmptyChestVfx";

		/// <summary>
		/// Cached name for the 'SetSelectionEnabled' method.
		/// </summary>
		public static readonly StringName SetSelectionEnabled = "SetSelectionEnabled";

		/// <summary>
		/// Cached name for the 'AnimIn' method.
		/// </summary>
		public static readonly StringName AnimIn = "AnimIn";

		/// <summary>
		/// Cached name for the 'AnimOut' method.
		/// </summary>
		public static readonly StringName AnimOut = "AnimOut";

		/// <summary>
		/// Cached name for the 'PickRelic' method.
		/// </summary>
		public static readonly StringName PickRelic = "PickRelic";

		/// <summary>
		/// Cached name for the 'RefreshVotes' method.
		/// </summary>
		public static readonly StringName RefreshVotes = "RefreshVotes";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'SingleplayerRelicHolder' property.
		/// </summary>
		public static readonly StringName SingleplayerRelicHolder = "SingleplayerRelicHolder";

		/// <summary>
		/// Cached name for the 'DefaultFocusedControl' property.
		/// </summary>
		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		/// <summary>
		/// Cached name for the '_fightBackstop' field.
		/// </summary>
		public static readonly StringName _fightBackstop = "_fightBackstop";

		/// <summary>
		/// Cached name for the '_fightLabel' field.
		/// </summary>
		public static readonly StringName _fightLabel = "_fightLabel";

		/// <summary>
		/// Cached name for the '_emptyVfxTween' field.
		/// </summary>
		public static readonly StringName _emptyVfxTween = "_emptyVfxTween";

		/// <summary>
		/// Cached name for the '_hands' field.
		/// </summary>
		public static readonly StringName _hands = "_hands";

		/// <summary>
		/// Cached name for the '_openedTicks' field.
		/// </summary>
		public static readonly StringName _openedTicks = "_openedTicks";

		/// <summary>
		/// Cached name for the '_isEmptyChest' field.
		/// </summary>
		public static readonly StringName _isEmptyChest = "_isEmptyChest";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private const ulong _noSelectionTimeMsec = 200uL;

	private Control _fightBackstop;

	private MegaLabel _fightLabel;

	private Tween? _emptyVfxTween;

	private NHandImageCollection _hands;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private readonly List<NTreasureRoomRelicHolder> _multiplayerHolders = new List<NTreasureRoomRelicHolder>();

	private List<NTreasureRoomRelicHolder> _holdersInUse = new List<NTreasureRoomRelicHolder>();

	private readonly TaskCompletionSource _relicPickingBeganTaskCompletionSource = new TaskCompletionSource();

	private readonly TaskCompletionSource _relicPickingCompleteTaskCompletionSource = new TaskCompletionSource();

	private ulong _openedTicks;

	private IRunState _runState;

	private bool _isEmptyChest;

	public NTreasureRoomRelicHolder SingleplayerRelicHolder { get; private set; }

	public Control? DefaultFocusedControl
	{
		get
		{
			if (_holdersInUse.Count <= 0)
			{
				return null;
			}
			return _holdersInUse[_runState.GetPlayerSlotIndex(LocalContext.GetMe(_runState.Players))];
		}
	}

	public override void _Ready()
	{
		_fightBackstop = GetNode<Control>("%FightBackstop");
		_hands = GetNode<NHandImageCollection>("%HandsContainer");
		Control node = GetNode<Control>("Container");
		SingleplayerRelicHolder = node.GetNode<NTreasureRoomRelicHolder>("%SingleplayerRelicHolder");
		foreach (NTreasureRoomRelicHolder item in node.GetChildren().OfType<NTreasureRoomRelicHolder>())
		{
			if (item != SingleplayerRelicHolder)
			{
				_multiplayerHolders.Add(item);
			}
		}
		Control fightBackstop = _fightBackstop;
		Color modulate = _fightBackstop.Modulate;
		modulate.A = 0f;
		fightBackstop.Modulate = modulate;
		_fightBackstop.Visible = false;
		_fightLabel = GetNode<MegaLabel>("%FightLabel");
		_fightLabel.Text = new LocString("gameplay_ui", "TREASURE_FIGHT_TEXT").GetFormattedText();
		RunManager.Instance.TreasureRoomRelicSynchronizer.VotesChanged += RefreshVotes;
		RunManager.Instance.TreasureRoomRelicSynchronizer.RelicsAwarded += OnRelicsAwarded;
	}

	public override void _EnterTree()
	{
		_cts = new CancellationTokenSource();
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		RunManager.Instance.TreasureRoomRelicSynchronizer.VotesChanged -= RefreshVotes;
		RunManager.Instance.TreasureRoomRelicSynchronizer.RelicsAwarded -= OnRelicsAwarded;
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
		_hands.Initialize(runState);
	}

	/// <summary>
	/// Initialize the relic display.
	/// This can't get called in _Ready because RelicPickingSynchronizer.BeginRelicPicking has not been called by then.
	/// </summary>
	public void InitializeRelics()
	{
		IReadOnlyList<RelicModel> currentRelics = RunManager.Instance.TreasureRoomRelicSynchronizer.CurrentRelics;
		if (currentRelics == null || currentRelics.Count == 0)
		{
			_isEmptyChest = true;
			SingleplayerRelicHolder.Visible = false;
			foreach (NTreasureRoomRelicHolder multiplayerHolder in _multiplayerHolders)
			{
				multiplayerHolder.Visible = false;
			}
			SpawnEmptyChestVfx();
			return;
		}
		if (currentRelics.Count == 1)
		{
			SingleplayerRelicHolder.Initialize(currentRelics[0], _runState);
			SingleplayerRelicHolder.Visible = true;
			SingleplayerRelicHolder.Index = 0;
			SingleplayerRelicHolder.Connect(NClickableControl.SignalName.Released, Callable.From<NTreasureRoomRelicHolder>(delegate
			{
				PickRelic(SingleplayerRelicHolder);
			}));
			int num = 1;
			List<NTreasureRoomRelicHolder> list = new List<NTreasureRoomRelicHolder>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<NTreasureRoomRelicHolder> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = SingleplayerRelicHolder;
			_holdersInUse = list;
			{
				foreach (NTreasureRoomRelicHolder multiplayerHolder2 in _multiplayerHolders)
				{
					multiplayerHolder2.Visible = false;
				}
				return;
			}
		}
		SingleplayerRelicHolder.Visible = false;
		for (int num2 = 0; num2 < _multiplayerHolders.Count; num2++)
		{
			NTreasureRoomRelicHolder holder = _multiplayerHolders[num2];
			if (num2 < currentRelics.Count)
			{
				holder.Visible = true;
				holder.Relic.Model = currentRelics[num2];
				holder.Initialize(currentRelics[num2], _runState);
			}
			else
			{
				holder.Visible = false;
			}
			holder.Index = num2;
			holder.Connect(NClickableControl.SignalName.Released, Callable.From<NTreasureRoomRelicHolder>(delegate
			{
				PickRelic(holder);
			}));
			_holdersInUse.Add(holder);
			holder.VoteContainer.RefreshPlayerVotes();
		}
		for (int num3 = 0; num3 < _holdersInUse.Count; num3++)
		{
			_holdersInUse[num3].SetFocusMode(FocusModeEnum.All);
			_holdersInUse[num3].FocusNeighborTop = _holdersInUse[num3].GetPath();
			_holdersInUse[num3].FocusNeighborBottom = _holdersInUse[num3].GetPath();
			NTreasureRoomRelicHolder nTreasureRoomRelicHolder = _holdersInUse[num3];
			NodePath path;
			if (num3 <= 0)
			{
				List<NTreasureRoomRelicHolder> holdersInUse = _holdersInUse;
				path = holdersInUse[holdersInUse.Count - 1].GetPath();
			}
			else
			{
				path = _holdersInUse[num3 - 1].GetPath();
			}
			nTreasureRoomRelicHolder.FocusNeighborLeft = path;
			_holdersInUse[num3].FocusNeighborRight = ((num3 < _holdersInUse.Count - 1) ? _holdersInUse[num3 + 1].GetPath() : _holdersInUse[0].GetPath());
		}
		if (currentRelics.Count == 2)
		{
			_multiplayerHolders[1].Position = _multiplayerHolders[3].Position;
		}
	}

	private void SpawnEmptyChestVfx()
	{
		MegaLabel node = GetNode<MegaLabel>("%EmptyLabel");
		node.Text = new LocString("gameplay_ui", "TREASURE_EMPTY").GetFormattedText();
		node.Visible = true;
		_emptyVfxTween = CreateTween().SetParallel();
		_emptyVfxTween.TweenProperty(node, "self_modulate:a", 1f, 0.25);
		_emptyVfxTween.TweenProperty(node, "position:y", node.Position.Y, 1.0).From(node.Position.Y + 64f).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Spring);
		_emptyVfxTween.Chain().TweenInterval(1.0);
		_emptyVfxTween.Chain();
		_emptyVfxTween.TweenProperty(node, "self_modulate:a", 0f, 1.0);
		GpuParticles2D node2 = GetNode<GpuParticles2D>("%SmokePuffVfx");
		node2.Emitting = true;
	}

	public void SetSelectionEnabled(bool isEnabled)
	{
		if (isEnabled)
		{
			SingleplayerRelicHolder.Enable();
			{
				foreach (NTreasureRoomRelicHolder multiplayerHolder in _multiplayerHolders)
				{
					multiplayerHolder.Enable();
				}
				return;
			}
		}
		SingleplayerRelicHolder.Disable();
		foreach (NTreasureRoomRelicHolder multiplayerHolder2 in _multiplayerHolders)
		{
			multiplayerHolder2.Disable();
		}
	}

	/// <summary>
	/// Await this to know when relic picking begins.
	/// </summary>
	public Task RelicPickingBegan()
	{
		return _relicPickingBeganTaskCompletionSource.Task;
	}

	/// <summary>
	/// Await this to know when to hide the screen.
	/// </summary>
	public Task RelicPickingFinished()
	{
		return _relicPickingCompleteTaskCompletionSource.Task;
	}

	/// <summary>
	/// Animates in the relic collection.
	/// </summary>
	/// <param name="chestVisual">Chest visual to fade out.</param>
	public void AnimIn(Node chestVisual)
	{
		base.Visible = true;
		base.Modulate = Colors.Transparent;
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(this, "modulate", Colors.White, 0.4);
		tween.TweenProperty(chestVisual, "modulate", StsColors.halfTransparentWhite, 0.4);
		if (_isEmptyChest)
		{
			LocalContext.GetMe(_runState)?.Relics.OfType<SilverCrucible>().FirstOrDefault()?.Flash();
			tween.TweenCallback(Callable.From(delegate
			{
				RunManager.Instance.TreasureRoomRelicSynchronizer.CompleteWithNoRelics();
			})).SetDelay(1.0);
			return;
		}
		foreach (NTreasureRoomRelicHolder holder in _holdersInUse)
		{
			holder.MouseFilter = MouseFilterEnum.Ignore;
			float num = ((_holdersInUse.Count == 1) ? 150f : 50f);
			float num2 = 0.2f + 0.2f * Rng.Chaotic.NextFloat();
			holder.Modulate = Colors.Black;
			NTreasureRoomRelicHolder nTreasureRoomRelicHolder = holder;
			Vector2 position = holder.Position;
			position.Y = holder.Position.Y + num;
			nTreasureRoomRelicHolder.Position = position;
			Tween tween2 = CreateTween().SetParallel();
			tween2.TweenProperty(holder, "modulate", Colors.White, 0.2).SetDelay(num2);
			tween2.TweenProperty(holder, "position:y", holder.Position.Y - num, 0.6).SetDelay(num2).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Back);
			tween2.TweenCallback(Callable.From(() => holder.MouseFilter = MouseFilterEnum.Stop)).SetDelay((double)num2 + 0.6);
		}
		NRun.Instance.ScreenStateTracker.SetIsInSharedRelicPickingScreen(isInSharedRelicPicking: true);
		_hands.AnimateHandsIn();
	}

	/// <summary>
	/// Animates out the relic collection, after relic picking is done.
	/// </summary>
	/// <param name="chestVisual">Chest visual to fade back in.</param>
	public void AnimOut(Node chestVisual)
	{
		base.Modulate = Colors.White;
		Tween tween = CreateTween().Parallel();
		tween.TweenProperty(this, "modulate", StsColors.transparentWhite, 0.3);
		tween.TweenProperty(chestVisual, "modulate", Colors.White, 0.3);
		tween.TweenCallback(Callable.From(() => base.Visible = false));
		NRun.Instance.ScreenStateTracker.SetIsInSharedRelicPickingScreen(isInSharedRelicPicking: false);
	}

	private void PickRelic(NTreasureRoomRelicHolder holder)
	{
		if (Time.GetTicksMsec() - _openedTicks > 200)
		{
			RunManager.Instance.TreasureRoomRelicSynchronizer.PickRelicLocally(holder.Index);
		}
	}

	private void OnRelicsAwarded(List<RelicPickingResult> results)
	{
		TaskHelper.RunSafely(AnimateRelicAwards(results));
	}

	private async Task AnimateRelicAwards(List<RelicPickingResult> results)
	{
		foreach (NTreasureRoomRelicHolder item in _holdersInUse)
		{
			item.SetFocusMode(FocusModeEnum.None);
		}
		_relicPickingBeganTaskCompletionSource.SetResult();
		foreach (Player player in _runState.Players)
		{
			TreasureRoomRelicSynchronizer.PlayerVote playerVote = RunManager.Instance.TreasureRoomRelicSynchronizer.GetPlayerVote(player);
			if (playerVote.voteReceived && !playerVote.index.HasValue)
			{
				_hands.GetHand(player.NetId)?.SetSkipped();
			}
		}
		_hands.BeforeRelicsAwarded();
		List<Task> tasksToWait = new List<Task>();
		RelicPickingResultType? relicPickingResultType = null;
		results.Sort((RelicPickingResult r1, RelicPickingResult r2) => r1.type.CompareTo(r2.type));
		foreach (RelicPickingResult result in results)
		{
			NTreasureRoomRelicHolder holder = _holdersInUse.First((NTreasureRoomRelicHolder h) => h.Relic.Model == result.relic);
			holder.AnimateAwayVotes();
			if (relicPickingResultType.HasValue && result.type != relicPickingResultType)
			{
				await Cmd.Wait(0.5f, _cts.Token);
			}
			if (result.type == RelicPickingResultType.FoughtOver)
			{
				holder.ZIndex = 1;
				_fightBackstop.Visible = true;
				Tween tween = CreateTween();
				tween.TweenProperty(holder, "global_position", (_fightBackstop.Size - holder.Size) * 0.5f, 0.25).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
				tween.TweenProperty(_fightBackstop, "modulate:a", 1f, 0.25);
				_hands.BeforeFightStarted(result.fight.playersInvolved);
				await tween.AwaitFinished(_cts.Token);
				await Cmd.Wait(1f, _cts.Token);
				await _hands.DoFight(result, holder);
				tween = CreateTween();
				tween.TweenProperty(_fightBackstop, "modulate:a", 0f, 0.25);
				await tween.AwaitFinished(_cts.Token);
				_fightBackstop.Visible = false;
				holder.ZIndex = 0;
			}
			else if (result.type != RelicPickingResultType.Skipped)
			{
				NHandImage hand = _hands.GetHand(result.player.NetId);
				if (hand != null)
				{
					tasksToWait.Add(TaskHelper.RunSafely(hand.GrabRelic(holder)));
					await Cmd.Wait(0.25f, _cts.Token);
				}
			}
			relicPickingResultType = result.type;
		}
		await Task.WhenAll(tasksToWait);
		if (tasksToWait.Count == 0 && _runState.Players.Count > 1)
		{
			await Cmd.Wait(0.7f, _cts.Token);
		}
		foreach (RelicPickingResult result2 in results)
		{
			NTreasureRoomRelicHolder nTreasureRoomRelicHolder = _holdersInUse.First((NTreasureRoomRelicHolder h) => h.Relic.Model == result2.relic);
			RelicModel relic = result2.relic.ToMutable();
			nTreasureRoomRelicHolder.Disable();
			if (result2.type != RelicPickingResultType.Skipped)
			{
				TaskHelper.RunSafely(RelicCmd.Obtain(relic, result2.player));
				if (LocalContext.IsMe(result2.player))
				{
					NRun.Instance.GlobalUi.RelicInventory.AnimateRelic(relic, nTreasureRoomRelicHolder.GlobalPosition, nTreasureRoomRelicHolder.Scale);
				}
				if (_runState.Players.Count == 1)
				{
					nTreasureRoomRelicHolder.Visible = false;
				}
			}
			foreach (Player player2 in _runState.Players)
			{
				if (player2 != result2.player)
				{
					player2.RelicGrabBag.MoveToFallback(result2.relic);
				}
			}
		}
		_relicPickingCompleteTaskCompletionSource.SetResult();
	}

	private void RefreshVotes()
	{
		foreach (NTreasureRoomRelicHolder item in _holdersInUse)
		{
			item.VoteContainer.RefreshPlayerVotes();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(10);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitializeRelics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SpawnEmptyChestVfx, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetSelectionEnabled, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "isEnabled", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "chestVisual", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimOut, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "chestVisual", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PickRelic, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshVotes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.InitializeRelics && args.Count == 0)
		{
			InitializeRelics();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SpawnEmptyChestVfx && args.Count == 0)
		{
			SpawnEmptyChestVfx();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSelectionEnabled && args.Count == 1)
		{
			SetSelectionEnabled(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimIn && args.Count == 1)
		{
			AnimIn(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimOut && args.Count == 1)
		{
			AnimOut(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PickRelic && args.Count == 1)
		{
			PickRelic(VariantUtils.ConvertTo<NTreasureRoomRelicHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshVotes && args.Count == 0)
		{
			RefreshVotes();
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
		if (method == MethodName.InitializeRelics)
		{
			return true;
		}
		if (method == MethodName.SpawnEmptyChestVfx)
		{
			return true;
		}
		if (method == MethodName.SetSelectionEnabled)
		{
			return true;
		}
		if (method == MethodName.AnimIn)
		{
			return true;
		}
		if (method == MethodName.AnimOut)
		{
			return true;
		}
		if (method == MethodName.PickRelic)
		{
			return true;
		}
		if (method == MethodName.RefreshVotes)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.SingleplayerRelicHolder)
		{
			SingleplayerRelicHolder = VariantUtils.ConvertTo<NTreasureRoomRelicHolder>(in value);
			return true;
		}
		if (name == PropertyName._fightBackstop)
		{
			_fightBackstop = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._fightLabel)
		{
			_fightLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._emptyVfxTween)
		{
			_emptyVfxTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hands)
		{
			_hands = VariantUtils.ConvertTo<NHandImageCollection>(in value);
			return true;
		}
		if (name == PropertyName._openedTicks)
		{
			_openedTicks = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName._isEmptyChest)
		{
			_isEmptyChest = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SingleplayerRelicHolder)
		{
			value = VariantUtils.CreateFrom<NTreasureRoomRelicHolder>(SingleplayerRelicHolder);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._fightBackstop)
		{
			value = VariantUtils.CreateFrom(in _fightBackstop);
			return true;
		}
		if (name == PropertyName._fightLabel)
		{
			value = VariantUtils.CreateFrom(in _fightLabel);
			return true;
		}
		if (name == PropertyName._emptyVfxTween)
		{
			value = VariantUtils.CreateFrom(in _emptyVfxTween);
			return true;
		}
		if (name == PropertyName._hands)
		{
			value = VariantUtils.CreateFrom(in _hands);
			return true;
		}
		if (name == PropertyName._openedTicks)
		{
			value = VariantUtils.CreateFrom(in _openedTicks);
			return true;
		}
		if (name == PropertyName._isEmptyChest)
		{
			value = VariantUtils.CreateFrom(in _isEmptyChest);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fightBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fightLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._emptyVfxTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hands, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._openedTicks, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isEmptyChest, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.SingleplayerRelicHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.SingleplayerRelicHolder, Variant.From<NTreasureRoomRelicHolder>(SingleplayerRelicHolder));
		info.AddProperty(PropertyName._fightBackstop, Variant.From(in _fightBackstop));
		info.AddProperty(PropertyName._fightLabel, Variant.From(in _fightLabel));
		info.AddProperty(PropertyName._emptyVfxTween, Variant.From(in _emptyVfxTween));
		info.AddProperty(PropertyName._hands, Variant.From(in _hands));
		info.AddProperty(PropertyName._openedTicks, Variant.From(in _openedTicks));
		info.AddProperty(PropertyName._isEmptyChest, Variant.From(in _isEmptyChest));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.SingleplayerRelicHolder, out var value))
		{
			SingleplayerRelicHolder = value.As<NTreasureRoomRelicHolder>();
		}
		if (info.TryGetProperty(PropertyName._fightBackstop, out var value2))
		{
			_fightBackstop = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._fightLabel, out var value3))
		{
			_fightLabel = value3.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._emptyVfxTween, out var value4))
		{
			_emptyVfxTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hands, out var value5))
		{
			_hands = value5.As<NHandImageCollection>();
		}
		if (info.TryGetProperty(PropertyName._openedTicks, out var value6))
		{
			_openedTicks = value6.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._isEmptyChest, out var value7))
		{
			_isEmptyChest = value7.As<bool>();
		}
	}
}
