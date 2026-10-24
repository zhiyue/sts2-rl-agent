using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

/// <summary>
/// Spawns hand images at the treasure room relic picking screen.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Screens/TreasureRoomRelic/NHandImageCollection.cs")]
public class NHandImageCollection : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_EnterTree' method.
		/// </summary>
		public new static readonly StringName _EnterTree = "_EnterTree";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'OnInputStateAdded' method.
		/// </summary>
		public static readonly StringName OnInputStateAdded = "OnInputStateAdded";

		/// <summary>
		/// Cached name for the 'OnInputStateRemoved' method.
		/// </summary>
		public static readonly StringName OnInputStateRemoved = "OnInputStateRemoved";

		/// <summary>
		/// Cached name for the 'AddHand' method.
		/// </summary>
		public static readonly StringName AddHand = "AddHand";

		/// <summary>
		/// Cached name for the 'OnInputStateChanged' method.
		/// </summary>
		public static readonly StringName OnInputStateChanged = "OnInputStateChanged";

		/// <summary>
		/// Cached name for the '_Input' method.
		/// </summary>
		public new static readonly StringName _Input = "_Input";

		/// <summary>
		/// Cached name for the 'ProcessGuiFocus' method.
		/// </summary>
		public static readonly StringName ProcessGuiFocus = "ProcessGuiFocus";

		/// <summary>
		/// Cached name for the 'GetHand' method.
		/// </summary>
		public static readonly StringName GetHand = "GetHand";

		/// <summary>
		/// Cached name for the 'RemoveHand' method.
		/// </summary>
		public static readonly StringName RemoveHand = "RemoveHand";

		/// <summary>
		/// Cached name for the 'UpdateHandVisibility' method.
		/// </summary>
		public static readonly StringName UpdateHandVisibility = "UpdateHandVisibility";

		/// <summary>
		/// Cached name for the 'BeforeRelicsAwarded' method.
		/// </summary>
		public static readonly StringName BeforeRelicsAwarded = "BeforeRelicsAwarded";

		/// <summary>
		/// Cached name for the 'AnimateHandsIn' method.
		/// </summary>
		public static readonly StringName AnimateHandsIn = "AnimateHandsIn";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_handAnimateInProgress' field.
		/// </summary>
		public static readonly StringName _handAnimateInProgress = "_handAnimateInProgress";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private PeerInputSynchronizer? _synchronizer;

	private readonly List<NHandImage> _hands = new List<NHandImage>();

	private IRunState _runState = NullRunState.Instance;

	private float _handAnimateInProgress;

	public override void _EnterTree()
	{
		base._EnterTree();
		_cts = new CancellationTokenSource();
		GetViewport().Connect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public override void _ExitTree()
	{
		_cts.Cancel();
		if (_synchronizer != null)
		{
			_synchronizer.StateAdded -= OnInputStateAdded;
			_synchronizer.StateChanged -= OnInputStateChanged;
			_synchronizer.StateRemoved -= OnInputStateRemoved;
		}
		NGame.Instance.CursorManager.SetCursorShown(show: true);
		GetViewport().Disconnect(Viewport.SignalName.GuiFocusChanged, Callable.From<Control>(ProcessGuiFocus));
	}

	public void Initialize(IRunState runState)
	{
		_runState = runState;
		if (_runState.Players.Count <= 1)
		{
			return;
		}
		_synchronizer = RunManager.Instance.InputSynchronizer;
		_synchronizer.StateAdded += OnInputStateAdded;
		_synchronizer.StateChanged += OnInputStateChanged;
		_synchronizer.StateRemoved += OnInputStateRemoved;
		foreach (Player player in _runState.Players)
		{
			AddHand(player.NetId);
		}
		UpdateHandVisibility();
	}

	private void OnInputStateAdded(ulong playerId)
	{
		AddHand(playerId);
	}

	private void OnInputStateRemoved(ulong playerId)
	{
		RemoveHand(playerId);
	}

	private void AddHand(ulong playerId)
	{
		if (_hands.Any((NHandImage c) => c.Player.NetId == playerId))
		{
			Log.Error($"Tried to add hand for player {playerId} twice!");
		}
		else
		{
			Player player = _runState.GetPlayer(playerId);
			NHandImage nHandImage = NHandImage.Create(player, _runState.GetPlayerSlotIndex(player));
			_hands.Add(nHandImage);
			this.AddChildSafely(nHandImage);
		}
	}

	private void OnInputStateChanged(ulong playerId)
	{
		Vector2 controlSpaceFocusPosition = _synchronizer.GetControlSpaceFocusPosition(playerId, this);
		NHandImage hand = GetHand(playerId);
		hand.SetIsDown(_synchronizer.GetMouseDown(playerId));
		hand.SetPointingPosition(controlSpaceFocusPosition);
		UpdateHandVisibility();
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (_runState.Players.Count == 1)
		{
			return;
		}
		if (inputEvent is InputEventMouseMotion)
		{
			NHandImage hand = GetHand(LocalContext.NetId.Value);
			hand.SetPointingPosition(GetGlobalMousePosition());
		}
		else if (inputEvent is InputEventMouseButton inputEventMouseButton && inputEventMouseButton.ButtonIndex == MouseButton.Left)
		{
			NHandImage hand2 = GetHand(LocalContext.NetId.Value);
			if (inputEventMouseButton.IsPressed() && !hand2.IsDown)
			{
				hand2.SetIsDown(isDown: true);
			}
			else if (inputEventMouseButton.IsReleased() && hand2.IsDown)
			{
				hand2.SetIsDown(isDown: false);
			}
		}
	}

	private void ProcessGuiFocus(Control focusedControl)
	{
		if (IsVisibleInTree() && NControllerManager.Instance.IsUsingController && _runState.Players.Count != 1)
		{
			if (focusedControl is NTreasureRoomRelicHolder)
			{
				NHandImage hand = GetHand(LocalContext.NetId.Value);
				Vector2 vector = Vector2.Down.Rotated(hand.Rotation);
				Vector2 pointingPosition = focusedControl.GlobalPosition + focusedControl.Size * 0.5f + vector * 100f;
				hand.SetPointingPosition(pointingPosition);
			}
			else
			{
				NHandImage hand2 = GetHand(LocalContext.NetId.Value);
				hand2.AnimateAway();
			}
		}
	}

	public NHandImage? GetHand(ulong playerId)
	{
		return _hands.FirstOrDefault((NHandImage c) => c.Player.NetId == playerId);
	}

	private void RemoveHand(ulong playerId)
	{
		NHandImage hand = GetHand(playerId);
		if (hand != null)
		{
			hand.QueueFreeSafely();
			_hands.Remove(hand);
		}
	}

	private void UpdateHandVisibility()
	{
		NetScreenType screenType = _synchronizer.GetScreenType(LocalContext.NetId.Value);
		foreach (NHandImage hand in _hands)
		{
			NetScreenType netScreenType = ((!RunManager.Instance.IsSingleplayerOrFakeMultiplayer) ? _synchronizer.GetScreenType(hand.Player.NetId) : NetScreenType.SharedRelicPicking);
			bool flag = netScreenType == NetScreenType.SharedRelicPicking && screenType == NetScreenType.SharedRelicPicking;
			if (!hand.IsShown && flag)
			{
				hand.AnimateIn();
			}
			else if (hand.IsShown && !flag)
			{
				hand.AnimateAway();
			}
		}
		NGame.Instance.CursorManager.SetCursorShown(screenType != NetScreenType.SharedRelicPicking);
	}

	/// <summary>
	/// Animates hands to a neutral space near the edges of the screen.
	/// </summary>
	public void BeforeRelicsAwarded()
	{
		foreach (NHandImage hand in _hands)
		{
			hand.SetFrozenForRelicAwards(frozenForRelicAwards: true);
		}
	}

	/// <summary>
	/// Z-indexes hands to 1 so that they appear above the fight backdrop, modulates them to white regardless of whether
	/// they belong to the local player, and adjusts their pivot in preparation for throwing RPS moves.
	/// Note that should happen before DoFight is called, because the hands should already be z-index up as the fight
	/// backdrop starts fading in.
	/// </summary>
	public void BeforeFightStarted(List<Player> playersInvolved)
	{
		foreach (Player item in playersInvolved)
		{
			NHandImage hand = GetHand(item.NetId);
			hand.SetIsInFight(inFight: true);
		}
	}

	/// <summary>
	/// Animates the hands in just before the relic picking finishes.
	/// </summary>
	public void AnimateHandsIn()
	{
		foreach (NHandImage hand in _hands)
		{
			if (hand.Visible)
			{
				hand.AnimateIn();
			}
		}
	}

	/// <summary>
	/// Animates the results of a relic picking fight.
	/// result.type must be FoughtOver, and result.fight must be non-null.
	/// </summary>
	/// <param name="result">The relic fight to animate.</param>
	/// <param name="holder">The relic holder that is displaying result.relic.</param>
	public async Task DoFight(RelicPickingResult result, NTreasureRoomRelicHolder holder)
	{
		RelicPickingFight fight = result.fight;
		List<Tween> tweens = new List<Tween>();
		List<Task> tasks = new List<Task>();
		for (int i = 0; i < fight.rounds.Count; i++)
		{
			float durationMultiplier = 1.5f / ((float)i + 1.5f);
			RelicPickingFightRound round = fight.rounds[i];
			tweens.Clear();
			for (int j = 0; j < fight.playersInvolved.Count; j++)
			{
				RelicPickingFightMove? relicPickingFightMove = round.moves[j];
				if (relicPickingFightMove.HasValue)
				{
					Player player = fight.playersInvolved[j];
					NHandImage hand = GetHand(player.NetId);
					tweens.Add(hand.DoFightMove(relicPickingFightMove.Value, 1.5f * durationMultiplier));
				}
			}
			await Task.WhenAll(tweens.Select((Tween t) => t.AwaitFinished(_cts.Token)));
			List<Player> list = new List<Player>();
			for (int num = 0; num < fight.playersInvolved.Count; num++)
			{
				Player player2 = fight.playersInvolved[num];
				if (i < fight.rounds.Count - 1)
				{
					if (round.moves[num].HasValue && !fight.rounds[i + 1].moves[num].HasValue)
					{
						list.Add(player2);
					}
				}
				else if (round.moves[num].HasValue && result.player != player2)
				{
					list.Add(player2);
				}
			}
			tasks.Clear();
			foreach (Player item in list)
			{
				tasks.Add(DoLoseShake(item, Mathf.Max(1f * durationMultiplier, 0.5f)));
			}
			if (i == fight.rounds.Count - 1)
			{
				await Cmd.Wait(0.5f, _cts.Token);
				NHandImage hand2 = GetHand(result.player.NetId);
				tasks.Add(hand2.GrabRelic(holder));
			}
			if (tasks.Count == 0)
			{
				await Cmd.Wait(1f * durationMultiplier, _cts.Token);
			}
			else
			{
				await Task.WhenAll(tasks);
			}
		}
		foreach (Player item2 in fight.playersInvolved)
		{
			NHandImage hand3 = GetHand(item2.NetId);
			hand3.SetIsInFight(inFight: false);
		}
	}

	private async Task DoLoseShake(Player player, float duration)
	{
		NHandImage hand = GetHand(player.NetId);
		await hand.DoLoseShake(duration);
		hand.SetIsInFight(inFight: false);
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
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnInputStateAdded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnInputStateRemoved, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AddHand, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnInputStateChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ProcessGuiFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "focusedControl", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetHand, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RemoveHand, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateHandVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.BeforeRelicsAwarded, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateHandsIn, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
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
		if (method == MethodName.OnInputStateAdded && args.Count == 1)
		{
			OnInputStateAdded(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInputStateRemoved && args.Count == 1)
		{
			OnInputStateRemoved(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AddHand && args.Count == 1)
		{
			AddHand(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnInputStateChanged && args.Count == 1)
		{
			OnInputStateChanged(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ProcessGuiFocus && args.Count == 1)
		{
			ProcessGuiFocus(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetHand && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NHandImage>(GetHand(VariantUtils.ConvertTo<ulong>(in args[0])));
			return true;
		}
		if (method == MethodName.RemoveHand && args.Count == 1)
		{
			RemoveHand(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateHandVisibility && args.Count == 0)
		{
			UpdateHandVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BeforeRelicsAwarded && args.Count == 0)
		{
			BeforeRelicsAwarded();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateHandsIn && args.Count == 0)
		{
			AnimateHandsIn();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnInputStateAdded)
		{
			return true;
		}
		if (method == MethodName.OnInputStateRemoved)
		{
			return true;
		}
		if (method == MethodName.AddHand)
		{
			return true;
		}
		if (method == MethodName.OnInputStateChanged)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.ProcessGuiFocus)
		{
			return true;
		}
		if (method == MethodName.GetHand)
		{
			return true;
		}
		if (method == MethodName.RemoveHand)
		{
			return true;
		}
		if (method == MethodName.UpdateHandVisibility)
		{
			return true;
		}
		if (method == MethodName.BeforeRelicsAwarded)
		{
			return true;
		}
		if (method == MethodName.AnimateHandsIn)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._handAnimateInProgress)
		{
			_handAnimateInProgress = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._handAnimateInProgress)
		{
			value = VariantUtils.CreateFrom(in _handAnimateInProgress);
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
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._handAnimateInProgress, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._handAnimateInProgress, Variant.From(in _handAnimateInProgress));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._handAnimateInProgress, out var value))
		{
			_handAnimateInProgress = value.As<float>();
		}
	}
}
