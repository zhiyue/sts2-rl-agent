using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Potions;

[ScriptPath("res://src/Core/Nodes/Potions/NPotionContainer.cs")]
public class NPotionContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName ConnectPlayerEvents = "ConnectPlayerEvents";

		public static readonly StringName DisconnectPlayerEvents = "DisconnectPlayerEvents";

		public static readonly StringName GrowPotionHolders = "GrowPotionHolders";

		public static readonly StringName UpdateNavigation = "UpdateNavigation";

		public static readonly StringName PotionFtueCheck = "PotionFtueCheck";

		public static readonly StringName PlayAddFailedAnim = "PlayAddFailedAnim";

		public static readonly StringName OnPotionHolderFocused = "OnPotionHolderFocused";

		public static readonly StringName OnPotionHolderUnfocused = "OnPotionHolderUnfocused";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName FirstPotionControl = "FirstPotionControl";

		public static readonly StringName LastPotionControl = "LastPotionControl";

		public static readonly StringName _potionHolders = "_potionHolders";

		public static readonly StringName _potionErrorBg = "_potionErrorBg";

		public static readonly StringName _potionShortcutButton = "_potionShortcutButton";

		public static readonly StringName _potionsFullTween = "_potionsFullTween";

		public static readonly StringName _potionHolderInitPos = "_potionHolderInitPos";

		public static readonly StringName _focusedHolder = "_focusedHolder";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Player? _player;

	private readonly List<NPotionHolder> _holders = new List<NPotionHolder>();

	private Control _potionHolders;

	private Control _potionErrorBg;

	private NButton _potionShortcutButton;

	private Tween? _potionsFullTween;

	private Vector2 _potionHolderInitPos;

	private NPotionHolder? _focusedHolder;

	public Control? FirstPotionControl => _holders.FirstOrDefault();

	public Control? LastPotionControl => _holders.LastOrDefault();

	public override void _Ready()
	{
		Callable.From(UpdateNavigation).CallDeferred();
	}

	public override void _EnterTree()
	{
		_potionHolders = GetNode<Control>("MarginContainer/PotionHolders");
		_potionErrorBg = GetNode<Control>("PotionErrorBg");
		_potionShortcutButton = GetNode<NButton>("PotionShortcutButton");
		_potionErrorBg.Modulate = Colors.Transparent;
		_potionShortcutButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
		{
			_potionHolders.GetChild<Control>(0).TryGrabFocus();
		}));
		CombatManager.Instance.CombatSetUp += OnCombatSetUp;
		ConnectPlayerEvents();
	}

	public override void _ExitTree()
	{
		DisconnectPlayerEvents();
		_player = null;
		CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
	}

	public void Initialize(IRunState runState)
	{
		DisconnectPlayerEvents();
		_player = LocalContext.GetMe(runState);
		ConnectPlayerEvents();
		GrowPotionHolders(_player.MaxPotionCount);
		foreach (PotionModel potion in _player.Potions)
		{
			Add(potion, isInitialization: true);
		}
	}

	private void ConnectPlayerEvents()
	{
		if (_player != null)
		{
			_player.AddPotionFailed += PlayAddFailedAnim;
			_player.PotionProcured += OnPotionProcured;
			_player.UsedPotionRemoved += OnUsedPotionRemoved;
			_player.PotionDiscarded += Discard;
			_player.MaxPotionCountChanged += GrowPotionHolders;
			_player.RelicObtained += OnRelicsUpdated;
			_player.RelicRemoved += OnRelicsUpdated;
		}
	}

	private void DisconnectPlayerEvents()
	{
		if (_player != null)
		{
			_player.AddPotionFailed -= PlayAddFailedAnim;
			_player.PotionProcured -= OnPotionProcured;
			_player.UsedPotionRemoved -= OnUsedPotionRemoved;
			_player.PotionDiscarded -= Discard;
			_player.MaxPotionCountChanged -= GrowPotionHolders;
			_player.RelicObtained -= OnRelicsUpdated;
			_player.RelicRemoved -= OnRelicsUpdated;
		}
	}

	private void GrowPotionHolders(int newMaxPotionSlots)
	{
		for (int i = _holders.Count; i < newMaxPotionSlots; i++)
		{
			NPotionHolder node = NPotionHolder.Create(isUsable: true);
			_holders.Add(node);
			_potionHolders.AddChildSafely(node);
			node.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
			{
				OnPotionHolderFocused(node);
			}));
			node.Connect(Control.SignalName.FocusExited, Callable.From(delegate
			{
				OnPotionHolderUnfocused(node);
			}));
			node.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
			{
				OnPotionHolderFocused(node);
			}));
			node.Connect(Control.SignalName.MouseExited, Callable.From(delegate
			{
				OnPotionHolderUnfocused(node);
			}));
		}
		UpdateNavigation();
	}

	private void OnRelicsUpdated(RelicModel _)
	{
		Callable.From(UpdateNavigation).CallDeferred();
	}

	private void UpdateNavigation()
	{
		Control control = NRun.Instance.GlobalUi.RelicInventory.RelicNodes.FirstOrDefault();
		if (control != null)
		{
			for (int i = 0; i < _holders.Count; i++)
			{
				_holders[i].FocusNeighborLeft = ((i > 0) ? _holders[i - 1].GetPath() : NRun.Instance.GlobalUi.TopBar.Gold.GetPath());
				_holders[i].FocusNeighborRight = ((i < _holders.Count - 1) ? _holders[i + 1].GetPath() : NRun.Instance.GlobalUi.TopBar.RoomIcon.GetPath());
				_holders[i].FocusNeighborBottom = control.GetPath();
				_holders[i].FocusNeighborTop = _holders[i].GetPath();
			}
		}
	}

	private void Add(PotionModel potion, bool isInitialization)
	{
		if (!_holders.All((NPotionHolder h) => h.HasPotion))
		{
			if (!isInitialization)
			{
				PotionFtueCheck();
			}
			NPotion nPotion = NPotion.Create(potion);
			nPotion.Position = new Vector2(-30f, -30f);
			NPotionHolder nPotionHolder = _holders[potion.Owner.PotionSlots.IndexOf<PotionModel>(potion)];
			nPotionHolder.AddPotion(nPotion);
		}
	}

	public void AnimatePotion(PotionModel potion, Vector2? startPosition = null)
	{
		if (LocalContext.IsMine(potion))
		{
			NPotionHolder nPotionHolder = _holders.First((NPotionHolder n) => n.Potion != null && n.Potion.Model == potion);
			TaskHelper.RunSafely(nPotionHolder.Potion.PlayNewlyAcquiredAnimation(startPosition));
		}
	}

	public void OnPotionUseCanceled(PotionModel potion)
	{
		NPotionHolder nPotionHolder = _holders.FirstOrDefault((NPotionHolder n) => n.Potion.Model == potion);
		if (nPotionHolder != null)
		{
			nPotionHolder.CancelPotionUse();
			return;
		}
		Log.Error($"Tried to cancel potion use for potion {potion} but a holder for it does not exist in the player's belt!");
	}

	private void PotionFtueCheck()
	{
		if (!SaveManager.Instance.SeenFtue("obtain_potion_ftue"))
		{
			NModalContainer.Instance.Add(NObtainPotionFtue.Create());
			SaveManager.Instance.MarkFtueAsComplete("obtain_potion_ftue");
		}
	}

	private void PlayAddFailedAnim()
	{
		if (_potionsFullTween != null && _potionsFullTween.IsRunning())
		{
			_potionsFullTween?.Kill();
			_potionHolders.Position = _potionHolderInitPos;
		}
		_potionsFullTween = CreateTween().SetParallel();
		_potionHolderInitPos = _potionHolders.Position;
		_potionsFullTween.TweenMethod(Callable.From(delegate(float t)
		{
			_potionHolders.Position = _potionHolderInitPos + Vector2.Right * 3f * Mathf.Sin(t * 5f) * Mathf.Sin(t * 0.5f);
		}), 0f, (float)Math.PI * 2f, 0.5);
		_potionsFullTween.TweenProperty(_potionErrorBg, "modulate", Colors.White, 0.15);
		_potionsFullTween.TweenProperty(_potionErrorBg, "modulate", Colors.Transparent, 0.5).SetDelay(0.35);
	}

	private void Discard(PotionModel potion)
	{
		NPotionHolder nPotionHolder = _holders.First((NPotionHolder n) => n.Potion != null && n.Potion.Model == potion);
		OnPotionHolderUnfocused(nPotionHolder);
		nPotionHolder.DiscardPotion();
	}

	private void RemoveUsed(PotionModel potion)
	{
		NPotionHolder nPotionHolder = _holders.First((NPotionHolder n) => n.Potion != null && n.Potion.Model == potion);
		OnPotionHolderUnfocused(nPotionHolder);
		nPotionHolder.RemoveUsedPotion();
	}

	private void OnPotionProcured(PotionModel potion)
	{
		Add(potion, isInitialization: false);
	}

	private void OnUsedPotionRemoved(PotionModel potion)
	{
		RemoveUsed(potion);
	}

	private void OnPotionHolderFocused(NPotionHolder holder)
	{
		if (_focusedHolder != holder && holder.Potion != null)
		{
			RunManager.Instance.HoveredModelTracker.OnLocalPotionHovered(holder.Potion.Model);
			_focusedHolder = holder;
		}
	}

	private void OnPotionHolderUnfocused(NPotionHolder holder)
	{
		if (_focusedHolder == holder)
		{
			RunManager.Instance.HoveredModelTracker.OnLocalPotionUnhovered();
			_focusedHolder = null;
		}
	}

	private void OnCombatSetUp(CombatState _)
	{
		TaskHelper.RunSafely(ShinePotions());
	}

	private async Task ShinePotions()
	{
		await Cmd.Wait(1f);
		foreach (NPotionHolder holder in _holders)
		{
			await TaskHelper.RunSafely(holder.ShineOnStartOfCombat());
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(11);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ConnectPlayerEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DisconnectPlayerEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GrowPotionHolders, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "newMaxPotionSlots", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PotionFtueCheck, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlayAddFailedAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPotionHolderFocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnPotionHolderUnfocused, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.ConnectPlayerEvents && args.Count == 0)
		{
			ConnectPlayerEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisconnectPlayerEvents && args.Count == 0)
		{
			DisconnectPlayerEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GrowPotionHolders && args.Count == 1)
		{
			GrowPotionHolders(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PotionFtueCheck && args.Count == 0)
		{
			PotionFtueCheck();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.PlayAddFailedAnim && args.Count == 0)
		{
			PlayAddFailedAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPotionHolderFocused && args.Count == 1)
		{
			OnPotionHolderFocused(VariantUtils.ConvertTo<NPotionHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPotionHolderUnfocused && args.Count == 1)
		{
			OnPotionHolderUnfocused(VariantUtils.ConvertTo<NPotionHolder>(in args[0]));
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
		if (method == MethodName.ConnectPlayerEvents)
		{
			return true;
		}
		if (method == MethodName.DisconnectPlayerEvents)
		{
			return true;
		}
		if (method == MethodName.GrowPotionHolders)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		if (method == MethodName.PotionFtueCheck)
		{
			return true;
		}
		if (method == MethodName.PlayAddFailedAnim)
		{
			return true;
		}
		if (method == MethodName.OnPotionHolderFocused)
		{
			return true;
		}
		if (method == MethodName.OnPotionHolderUnfocused)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._potionHolders)
		{
			_potionHolders = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._potionErrorBg)
		{
			_potionErrorBg = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._potionShortcutButton)
		{
			_potionShortcutButton = VariantUtils.ConvertTo<NButton>(in value);
			return true;
		}
		if (name == PropertyName._potionsFullTween)
		{
			_potionsFullTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._potionHolderInitPos)
		{
			_potionHolderInitPos = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._focusedHolder)
		{
			_focusedHolder = VariantUtils.ConvertTo<NPotionHolder>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		Control from;
		if (name == PropertyName.FirstPotionControl)
		{
			from = FirstPotionControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.LastPotionControl)
		{
			from = LastPotionControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._potionHolders)
		{
			value = VariantUtils.CreateFrom(in _potionHolders);
			return true;
		}
		if (name == PropertyName._potionErrorBg)
		{
			value = VariantUtils.CreateFrom(in _potionErrorBg);
			return true;
		}
		if (name == PropertyName._potionShortcutButton)
		{
			value = VariantUtils.CreateFrom(in _potionShortcutButton);
			return true;
		}
		if (name == PropertyName._potionsFullTween)
		{
			value = VariantUtils.CreateFrom(in _potionsFullTween);
			return true;
		}
		if (name == PropertyName._potionHolderInitPos)
		{
			value = VariantUtils.CreateFrom(in _potionHolderInitPos);
			return true;
		}
		if (name == PropertyName._focusedHolder)
		{
			value = VariantUtils.CreateFrom(in _focusedHolder);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionHolders, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionErrorBg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionShortcutButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._potionsFullTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._potionHolderInitPos, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._focusedHolder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FirstPotionControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.LastPotionControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._potionHolders, Variant.From(in _potionHolders));
		info.AddProperty(PropertyName._potionErrorBg, Variant.From(in _potionErrorBg));
		info.AddProperty(PropertyName._potionShortcutButton, Variant.From(in _potionShortcutButton));
		info.AddProperty(PropertyName._potionsFullTween, Variant.From(in _potionsFullTween));
		info.AddProperty(PropertyName._potionHolderInitPos, Variant.From(in _potionHolderInitPos));
		info.AddProperty(PropertyName._focusedHolder, Variant.From(in _focusedHolder));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._potionHolders, out var value))
		{
			_potionHolders = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._potionErrorBg, out var value2))
		{
			_potionErrorBg = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._potionShortcutButton, out var value3))
		{
			_potionShortcutButton = value3.As<NButton>();
		}
		if (info.TryGetProperty(PropertyName._potionsFullTween, out var value4))
		{
			_potionsFullTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._potionHolderInitPos, out var value5))
		{
			_potionHolderInitPos = value5.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._focusedHolder, out var value6))
		{
			_focusedHolder = value6.As<NPotionHolder>();
		}
	}
}
