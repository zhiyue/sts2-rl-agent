using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NMouseCardPlay.cs")]
public class NMouseCardPlay : NCardPlay
{
	public new class MethodName : NCardPlay.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Input = "_Input";

		public new static readonly StringName Start = "Start";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName DisconnectTargetingSignals = "DisconnectTargetingSignals";

		public new static readonly StringName OnCancelPlayCard = "OnCancelPlayCard";

		public static readonly StringName IsCardInPlayZone = "IsCardInPlayZone";

		public static readonly StringName IsCardInCancelZone = "IsCardInCancelZone";
	}

	public new class PropertyName : NCardPlay.PropertyName
	{
		public static readonly StringName PlayZoneThreshold = "PlayZoneThreshold";

		public static readonly StringName CancelZoneThreshold = "CancelZoneThreshold";

		public static readonly StringName _dragStartYPosition = "_dragStartYPosition";

		public static readonly StringName _isLeftMouseDown = "_isLeftMouseDown";

		public static readonly StringName _onCreatureHoverCallable = "_onCreatureHoverCallable";

		public static readonly StringName _onCreatureUnhoverCallable = "_onCreatureUnhoverCallable";

		public static readonly StringName _signalsConnected = "_signalsConnected";

		public static readonly StringName _cancelShortcut = "_cancelShortcut";

		public static readonly StringName _skipStartCardDrag = "_skipStartCardDrag";
	}

	public new class SignalName : NCardPlay.SignalName
	{
	}

	private const float _fakeLowerEnterPlayZoneDistance = 100f;

	private const float _fakeUpperEnterPlayZoneDistance = 50f;

	private const float _playZoneScreenProportion = 0.75f;

	private const float _cancelZoneScreenProportion = 0.95f;

	private float _dragStartYPosition;

	private Creature? _target;

	private bool _isLeftMouseDown;

	private CancellationTokenSource _cancellationTokenSource;

	private Callable _onCreatureHoverCallable;

	private Callable _onCreatureUnhoverCallable;

	private bool _signalsConnected;

	private StringName _cancelShortcut;

	private bool _skipStartCardDrag;

	private float PlayZoneThreshold
	{
		get
		{
			float num = _viewport.GetVisibleRect().Size.Y * 0.75f;
			if (_skipStartCardDrag)
			{
				return num + 100f;
			}
			if (_dragStartYPosition > num)
			{
				return Mathf.Max(num, _dragStartYPosition - 100f);
			}
			return Mathf.Min(num, _dragStartYPosition - 50f);
		}
	}

	private float CancelZoneThreshold => _viewport.GetVisibleRect().Size.Y * 0.95f;

	public static NMouseCardPlay Create(NHandCardHolder holder, StringName cancelShortcut, bool wasStartedWithShortcut)
	{
		NMouseCardPlay nMouseCardPlay = new NMouseCardPlay();
		nMouseCardPlay.Holder = holder;
		nMouseCardPlay.Player = holder.CardModel.Owner;
		nMouseCardPlay._cancelShortcut = cancelShortcut;
		nMouseCardPlay._skipStartCardDrag = wasStartedWithShortcut;
		return nMouseCardPlay;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventMouseButton { ButtonIndex: var buttonIndex } inputEventMouseButton)
		{
			switch (buttonIndex)
			{
			case MouseButton.Left:
				if (inputEventMouseButton.IsPressed())
				{
					_isLeftMouseDown = true;
				}
				else if (inputEventMouseButton.IsReleased())
				{
					_isLeftMouseDown = false;
				}
				break;
			case MouseButton.Right:
				if (inputEventMouseButton.IsPressed())
				{
					CancelPlayCard();
				}
				break;
			}
		}
		if (inputEvent.IsActionPressed(_cancelShortcut) || inputEvent.IsActionPressed(MegaInput.releaseCard))
		{
			CancelPlayCard();
			GetViewport()?.SetInputAsHandled();
		}
	}

	public override void Start()
	{
		_isLeftMouseDown = !_skipStartCardDrag;
		base.Holder.Hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
		_cancellationTokenSource = new CancellationTokenSource();
		_onCreatureHoverCallable = Callable.From<NCreature>(base.OnCreatureHover);
		_onCreatureUnhoverCallable = Callable.From<NCreature>(base.OnCreatureUnhover);
		TaskHelper.RunSafely(StartAsync());
	}

	public override void _EnterTree()
	{
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Connect(NControllerManager.SignalName.MouseDetected, Callable.From(base.CancelPlayCard));
			NControllerManager.Instance.Connect(NControllerManager.SignalName.ControllerDetected, Callable.From(base.CancelPlayCard));
		}
	}

	public override void _ExitTree()
	{
		if (NControllerManager.Instance != null)
		{
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.MouseDetected, Callable.From(base.CancelPlayCard));
			NControllerManager.Instance.Disconnect(NControllerManager.SignalName.ControllerDetected, Callable.From(base.CancelPlayCard));
		}
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		DisconnectTargetingSignals();
	}

	private async Task StartAsync()
	{
		if (base.Card == null || base.CardNode == null)
		{
			return;
		}
		await StartCardDrag();
		if (_cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		if (!base.Card.CanPlay(out UnplayableReason reason, out AbstractModel preventer))
		{
			CannotPlayThisCardFtueCheck(base.Card);
			CancelPlayCard();
			LocString playerDialogueLine = reason.GetPlayerDialogueLine(preventer);
			if (playerDialogueLine != null)
			{
				NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(NThoughtBubbleVfx.Create(playerDialogueLine.GetFormattedText(), base.Card.Owner.Creature, 1.0));
			}
			return;
		}
		base.CardNode.CardHighlight.AnimFlash();
		TargetMode targetMode = ((!_skipStartCardDrag) ? (_isLeftMouseDown ? TargetMode.ReleaseMouseToTarget : TargetMode.ClickMouseToTarget) : TargetMode.ClickMouseToTarget);
		await TargetSelection(targetMode);
		if (!_cancellationTokenSource.IsCancellationRequested)
		{
			if (!IsCardInPlayZone())
			{
				CancelPlayCard();
			}
			if (!_cancellationTokenSource.IsCancellationRequested)
			{
				TryPlayCard(_target);
			}
		}
	}

	private async Task StartCardDrag()
	{
		NDebugAudioManager.Instance?.Play("card_select.mp3", 0.5f);
		NHoverTipSet.Remove(base.Holder);
		_dragStartYPosition = _viewport.GetMousePosition().Y;
		if (!_skipStartCardDrag)
		{
			do
			{
				await LerpToMouse(base.Holder);
			}
			while (!IsCardInPlayZone() && !_cancellationTokenSource.IsCancellationRequested);
		}
	}

	private async Task TargetSelection(TargetMode targetMode)
	{
		if (base.Card != null)
		{
			TryShowEvokingOrbs();
			base.CardNode?.CardHighlight.AnimFlash();
			TargetType targetType = base.Card.TargetType;
			if ((targetType == TargetType.AnyEnemy || targetType == TargetType.AnyAlly) ? true : false)
			{
				await SingleCreatureTargeting(targetMode, base.Card.TargetType);
			}
			else
			{
				await MultiCreatureTargeting(targetMode);
			}
		}
	}

	private async Task SingleCreatureTargeting(TargetMode targetMode, TargetType targetType)
	{
		if (_cancellationTokenSource.IsCancellationRequested)
		{
			return;
		}
		CenterCard();
		NTargetManager instance = NTargetManager.Instance;
		instance.Connect(NTargetManager.SignalName.CreatureHovered, _onCreatureHoverCallable);
		instance.Connect(NTargetManager.SignalName.CreatureUnhovered, _onCreatureUnhoverCallable);
		_signalsConnected = true;
		instance.StartTargeting(targetType, base.CardNode, targetMode, () => IsCardInCancelZone() || _cancellationTokenSource.IsCancellationRequested, null);
		Node node = await instance.SelectionFinished();
		if (node != null)
		{
			Creature target;
			if (!(node is NCreature nCreature))
			{
				if (!(node is NMultiplayerPlayerState nMultiplayerPlayerState))
				{
					throw new ArgumentOutOfRangeException("target", node, null);
				}
				target = nMultiplayerPlayerState.Player.Creature;
			}
			else
			{
				target = nCreature.Entity;
			}
			_target = target;
		}
		DisconnectTargetingSignals();
	}

	private void DisconnectTargetingSignals()
	{
		if (_signalsConnected)
		{
			_signalsConnected = false;
			NTargetManager instance = NTargetManager.Instance;
			instance.Disconnect(NTargetManager.SignalName.CreatureHovered, _onCreatureHoverCallable);
			instance.Disconnect(NTargetManager.SignalName.CreatureUnhovered, _onCreatureUnhoverCallable);
		}
	}

	private async Task MultiCreatureTargeting(TargetMode targetMode)
	{
		bool isShowingTargetingVisuals = false;
		Func<bool> shouldFinishTargeting = ((targetMode == TargetMode.ReleaseMouseToTarget) ? ((Func<bool>)(() => !_isLeftMouseDown)) : ((Func<bool>)(() => _isLeftMouseDown)));
		do
		{
			if (isShowingTargetingVisuals)
			{
				if (!IsCardInPlayZone())
				{
					HideTargetingVisuals();
					isShowingTargetingVisuals = false;
				}
			}
			else if (IsCardInPlayZone())
			{
				ShowMultiCreatureTargetingVisuals();
				isShowingTargetingVisuals = true;
			}
			await LerpToMouse(base.Holder);
		}
		while (!shouldFinishTargeting() && !_cancellationTokenSource.IsCancellationRequested && !IsCardInCancelZone());
		if (!_cancellationTokenSource.IsCancellationRequested && IsCardInCancelZone())
		{
			CancelPlayCard();
		}
	}

	protected override void OnCancelPlayCard()
	{
		if (GodotObject.IsInstanceValid(this) && IsInsideTree())
		{
			base.Holder.Hitbox.MouseFilter = Control.MouseFilterEnum.Stop;
			_cancellationTokenSource.Cancel();
		}
	}

	private async Task LerpToMouse(NHandCardHolder cardHolder)
	{
		cardHolder.SetTargetPosition(_viewport.GetMousePosition());
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
	}

	private bool IsCardInPlayZone()
	{
		return _viewport.GetMousePosition().Y < PlayZoneThreshold;
	}

	private bool IsCardInCancelZone()
	{
		return _viewport.GetMousePosition().Y > CancelZoneThreshold;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(9);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.StringName, "cancelShortcut", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "wasStartedWithShortcut", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Start, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectTargetingSignals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnCancelPlayCard, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsCardInPlayZone, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsCardInCancelZone, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NMouseCardPlay>(Create(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Start && args.Count == 0)
		{
			Start();
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
		if (method == MethodName.DisconnectTargetingSignals && args.Count == 0)
		{
			DisconnectTargetingSignals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnCancelPlayCard && args.Count == 0)
		{
			OnCancelPlayCard();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsCardInPlayZone && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsCardInPlayZone());
			return true;
		}
		if (method == MethodName.IsCardInCancelZone && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsCardInCancelZone());
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<NMouseCardPlay>(Create(VariantUtils.ConvertTo<NHandCardHolder>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2])));
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.Start)
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
		if (method == MethodName.DisconnectTargetingSignals)
		{
			return true;
		}
		if (method == MethodName.OnCancelPlayCard)
		{
			return true;
		}
		if (method == MethodName.IsCardInPlayZone)
		{
			return true;
		}
		if (method == MethodName.IsCardInCancelZone)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._dragStartYPosition)
		{
			_dragStartYPosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._isLeftMouseDown)
		{
			_isLeftMouseDown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._onCreatureHoverCallable)
		{
			_onCreatureHoverCallable = VariantUtils.ConvertTo<Callable>(in value);
			return true;
		}
		if (name == PropertyName._onCreatureUnhoverCallable)
		{
			_onCreatureUnhoverCallable = VariantUtils.ConvertTo<Callable>(in value);
			return true;
		}
		if (name == PropertyName._signalsConnected)
		{
			_signalsConnected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cancelShortcut)
		{
			_cancelShortcut = VariantUtils.ConvertTo<StringName>(in value);
			return true;
		}
		if (name == PropertyName._skipStartCardDrag)
		{
			_skipStartCardDrag = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		float from;
		if (name == PropertyName.PlayZoneThreshold)
		{
			from = PlayZoneThreshold;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CancelZoneThreshold)
		{
			from = CancelZoneThreshold;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._dragStartYPosition)
		{
			value = VariantUtils.CreateFrom(in _dragStartYPosition);
			return true;
		}
		if (name == PropertyName._isLeftMouseDown)
		{
			value = VariantUtils.CreateFrom(in _isLeftMouseDown);
			return true;
		}
		if (name == PropertyName._onCreatureHoverCallable)
		{
			value = VariantUtils.CreateFrom(in _onCreatureHoverCallable);
			return true;
		}
		if (name == PropertyName._onCreatureUnhoverCallable)
		{
			value = VariantUtils.CreateFrom(in _onCreatureUnhoverCallable);
			return true;
		}
		if (name == PropertyName._signalsConnected)
		{
			value = VariantUtils.CreateFrom(in _signalsConnected);
			return true;
		}
		if (name == PropertyName._cancelShortcut)
		{
			value = VariantUtils.CreateFrom(in _cancelShortcut);
			return true;
		}
		if (name == PropertyName._skipStartCardDrag)
		{
			value = VariantUtils.CreateFrom(in _skipStartCardDrag);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.PlayZoneThreshold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.CancelZoneThreshold, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._dragStartYPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLeftMouseDown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName._onCreatureHoverCallable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Callable, PropertyName._onCreatureUnhoverCallable, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._signalsConnected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.StringName, PropertyName._cancelShortcut, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._skipStartCardDrag, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._dragStartYPosition, Variant.From(in _dragStartYPosition));
		info.AddProperty(PropertyName._isLeftMouseDown, Variant.From(in _isLeftMouseDown));
		info.AddProperty(PropertyName._onCreatureHoverCallable, Variant.From(in _onCreatureHoverCallable));
		info.AddProperty(PropertyName._onCreatureUnhoverCallable, Variant.From(in _onCreatureUnhoverCallable));
		info.AddProperty(PropertyName._signalsConnected, Variant.From(in _signalsConnected));
		info.AddProperty(PropertyName._cancelShortcut, Variant.From(in _cancelShortcut));
		info.AddProperty(PropertyName._skipStartCardDrag, Variant.From(in _skipStartCardDrag));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._dragStartYPosition, out var value))
		{
			_dragStartYPosition = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._isLeftMouseDown, out var value2))
		{
			_isLeftMouseDown = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._onCreatureHoverCallable, out var value3))
		{
			_onCreatureHoverCallable = value3.As<Callable>();
		}
		if (info.TryGetProperty(PropertyName._onCreatureUnhoverCallable, out var value4))
		{
			_onCreatureUnhoverCallable = value4.As<Callable>();
		}
		if (info.TryGetProperty(PropertyName._signalsConnected, out var value5))
		{
			_signalsConnected = value5.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cancelShortcut, out var value6))
		{
			_cancelShortcut = value6.As<StringName>();
		}
		if (info.TryGetProperty(PropertyName._skipStartCardDrag, out var value7))
		{
			_skipStartCardDrag = value7.As<bool>();
		}
	}
}
