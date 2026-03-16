using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerPlayerStateContainer.cs")]
public class NMultiplayerPlayerStateContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Input = "_Input";

		public static readonly StringName UpdateNavigation = "UpdateNavigation";

		public static readonly StringName LockNavigation = "LockNavigation";

		public static readonly StringName UnlockNavigation = "UnlockNavigation";

		public static readonly StringName UpdatePositionAfterOneFrame = "UpdatePositionAfterOneFrame";

		public static readonly StringName UpdatePosition = "UpdatePosition";

		public static readonly StringName AnimHide = "AnimHide";

		public static readonly StringName AnimShow = "AnimShow";

		public static readonly StringName ShowImmediately = "ShowImmediately";

		public static readonly StringName HideImmediately = "HideImmediately";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName FirstPlayerState = "FirstPlayerState";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _originalPosition = "_originalPosition";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private IRunState _runState;

	private readonly List<NMultiplayerPlayerState> _nodes = new List<NMultiplayerPlayerState>();

	private Tween? _tween;

	private Vector2 _originalPosition;

	public NMultiplayerPlayerState? FirstPlayerState => GetChild<NMultiplayerPlayerState>(0);

	public override void _Ready()
	{
		_originalPosition = base.Position;
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += UpdateNavigation;
	}

	public override void _ExitTree()
	{
		ActiveScreenContext.Instance.Updated -= UpdateNavigation;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent.IsActionReleased(DebugHotkey.hideMpHealthBars))
		{
			base.Visible = !base.Visible;
			NGame.Instance.AddChildSafely(NFullscreenTextVfx.Create((!base.Visible) ? "Hide MP Health bars" : "Show MP Health bars"));
		}
	}

	public void Initialize(RunState runState)
	{
		_runState = runState;
		if (_runState.Players.Count <= 1)
		{
			return;
		}
		Player me = LocalContext.GetMe(_runState);
		NMultiplayerPlayerState nMultiplayerPlayerState = NMultiplayerPlayerState.Create(me);
		this.AddChildSafely(nMultiplayerPlayerState);
		_nodes.Add(nMultiplayerPlayerState);
		foreach (Player item in _runState.Players.Except(new global::_003C_003Ez__ReadOnlySingleElementList<Player>(me)))
		{
			NMultiplayerPlayerState nMultiplayerPlayerState2 = NMultiplayerPlayerState.Create(item);
			this.AddChildSafely(nMultiplayerPlayerState2);
			_nodes.Add(nMultiplayerPlayerState2);
		}
		UpdatePosition();
		NRun.Instance.GlobalUi.RelicInventory.Connect(NRelicInventory.SignalName.RelicsChanged, Callable.From(UpdatePositionAfterOneFrame));
		GetViewport().Connect(Viewport.SignalName.SizeChanged, Callable.From(UpdatePositionAfterOneFrame));
		for (int i = 0; i < GetChildCount(); i++)
		{
			Control hitbox = GetChild<NMultiplayerPlayerState>(i).Hitbox;
			hitbox.FocusNeighborLeft = hitbox.GetPath();
			hitbox.FocusNeighborTop = ((i > 0) ? GetChild<NMultiplayerPlayerState>(i - 1).Hitbox.GetPath() : null);
			hitbox.FocusNeighborBottom = ((i < GetChildCount() - 1) ? GetChild<NMultiplayerPlayerState>(i + 1).Hitbox.GetPath() : null);
		}
	}

	private void UpdateNavigation()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			Control hitbox = GetChild<NMultiplayerPlayerState>(i).Hitbox;
			hitbox.FocusNeighborTop = ((i > 0) ? GetChild<NMultiplayerPlayerState>(i - 1).Hitbox.GetPath() : null);
			hitbox.FocusNeighborBottom = ((i < GetChildCount() - 1) ? GetChild<NMultiplayerPlayerState>(i + 1).Hitbox.GetPath() : ActiveScreenContext.Instance.GetCurrentScreen()?.FocusedControlFromTopBar?.GetPath());
		}
	}

	public void LockNavigation()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			Control hitbox = GetChild<NMultiplayerPlayerState>(i).Hitbox;
			hitbox.FocusNeighborTop = ((i > 0) ? GetChild<NMultiplayerPlayerState>(i - 1).Hitbox.GetPath() : hitbox.GetPath());
			hitbox.FocusNeighborBottom = ((i < GetChildCount() - 1) ? GetChild<NMultiplayerPlayerState>(i + 1).Hitbox.GetPath() : hitbox.GetPath());
			hitbox.FocusNeighborLeft = hitbox.GetPath();
			hitbox.FocusNeighborRight = hitbox.GetPath();
		}
	}

	public void UnlockNavigation()
	{
		UpdateNavigation();
	}

	private void UpdatePositionAfterOneFrame()
	{
		TaskHelper.RunSafely(UpdatePositionAfterOneFrameAsync());
	}

	private async Task UpdatePositionAfterOneFrameAsync()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		NRelicInventory relicInventory = NRun.Instance.GlobalUi.RelicInventory;
		int lineCount = relicInventory.GetLineCount();
		if (lineCount == 0 || relicInventory.GetChildCount() == 0)
		{
			base.Position = relicInventory.Position;
			return;
		}
		float y = relicInventory.GetChild<Control>(0).Size.Y;
		float num = relicInventory.GetThemeConstant(ThemeConstants.FlowContainer.vSeparation, "FlowContainer");
		base.Position = relicInventory.Position + (float)lineCount * (y + num) * Vector2.Down;
	}

	public void HighlightPlayer(Player player)
	{
		_nodes.FirstOrDefault((NMultiplayerPlayerState n) => n.Player == player)?.OnCreatureHovered();
	}

	public void UnhighlightPlayer(Player player)
	{
		_nodes.FirstOrDefault((NMultiplayerPlayerState n) => n.Player == player)?.OnCreatureUnhovered();
	}

	public void FlashPlayerReady(Player player)
	{
		_nodes.FirstOrDefault((NMultiplayerPlayerState n) => n.Player == player)?.FlashPlayerReady();
	}

	public void AnimHide()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "position:x", 0f - base.Size.X, 0.20000000298023224).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
		_tween.TweenProperty(this, "modulate:a", 0f, 0.20000000298023224).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.InOut);
	}

	public void AnimShow()
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(this, "position:x", _originalPosition.X, 0.25).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(this, "modulate:a", 1f, 0.15000000596046448).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
	}

	public void ShowImmediately()
	{
		_tween?.Kill();
		Vector2 position = base.Position;
		position.X = _originalPosition.X;
		base.Position = position;
		Color modulate = base.Modulate;
		modulate.A = 1f;
		base.Modulate = modulate;
	}

	public void HideImmediately()
	{
		_tween?.Kill();
		Vector2 position = base.Position;
		position.X = 0f - base.Size.X;
		base.Position = position;
		Color modulate = base.Modulate;
		modulate.A = 0f;
		base.Modulate = modulate;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.LockNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnlockNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdatePositionAfterOneFrame, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdatePosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimHide, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimShow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ShowImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HideImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateNavigation && args.Count == 0)
		{
			UpdateNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LockNavigation && args.Count == 0)
		{
			LockNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnlockNavigation && args.Count == 0)
		{
			UnlockNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePositionAfterOneFrame && args.Count == 0)
		{
			UpdatePositionAfterOneFrame();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdatePosition && args.Count == 0)
		{
			UpdatePosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimHide && args.Count == 0)
		{
			AnimHide();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimShow && args.Count == 0)
		{
			AnimShow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowImmediately && args.Count == 0)
		{
			ShowImmediately();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.HideImmediately && args.Count == 0)
		{
			HideImmediately();
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
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.UpdateNavigation)
		{
			return true;
		}
		if (method == MethodName.LockNavigation)
		{
			return true;
		}
		if (method == MethodName.UnlockNavigation)
		{
			return true;
		}
		if (method == MethodName.UpdatePositionAfterOneFrame)
		{
			return true;
		}
		if (method == MethodName.UpdatePosition)
		{
			return true;
		}
		if (method == MethodName.AnimHide)
		{
			return true;
		}
		if (method == MethodName.AnimShow)
		{
			return true;
		}
		if (method == MethodName.ShowImmediately)
		{
			return true;
		}
		if (method == MethodName.HideImmediately)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			_originalPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.FirstPlayerState)
		{
			value = VariantUtils.CreateFrom<NMultiplayerPlayerState>(FirstPlayerState);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._originalPosition)
		{
			value = VariantUtils.CreateFrom(in _originalPosition);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FirstPlayerState, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._originalPosition, Variant.From(in _originalPosition));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalPosition, out var value2))
		{
			_originalPosition = value2.As<Vector2>();
		}
	}
}
