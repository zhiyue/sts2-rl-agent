using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Unlocks;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

[ScriptPath("res://src/Core/Nodes/Screens/CharacterSelect/NCharacterSelectButton.cs")]
public class NCharacterSelectButton : NButton
{
	private enum State
	{
		NotSelected,
		SelectedLocally,
		SelectedRemotely
	}

	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName LockForAnimation = "LockForAnimation";

		public static readonly StringName Reset = "Reset";

		public static readonly StringName OnRemotePlayerSelected = "OnRemotePlayerSelected";

		public static readonly StringName OnRemotePlayerDeselected = "OnRemotePlayerDeselected";

		public static readonly StringName Select = "Select";

		public static readonly StringName Deselect = "Deselect";

		public static readonly StringName RefreshState = "RefreshState";

		public static readonly StringName GetSaturationForCurrentState = "GetSaturationForCurrentState";

		public static readonly StringName GetValueForCurrentState = "GetValueForCurrentState";

		public static readonly StringName AnimateSaturationToCurrentState = "AnimateSaturationToCurrentState";

		public static readonly StringName RefreshOutline = "RefreshOutline";

		public static readonly StringName RefreshPlayerIcons = "RefreshPlayerIcons";

		public static readonly StringName DebugUnlock = "DebugUnlock";

		public static readonly StringName UnlockIfPossible = "UnlockIfPossible";

		public static readonly StringName UpdateShaderH = "UpdateShaderH";

		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public static readonly StringName IsRandom = "IsRandom";

		public static readonly StringName IsLocked = "IsLocked";

		public static readonly StringName _icon = "_icon";

		public static readonly StringName _iconAdd = "_iconAdd";

		public static readonly StringName _lock = "_lock";

		public static readonly StringName _outlineLocal = "_outlineLocal";

		public static readonly StringName _outlineRemote = "_outlineRemote";

		public static readonly StringName _outlineMixed = "_outlineMixed";

		public static readonly StringName _shadow = "_shadow";

		public static readonly StringName _playerIconContainer = "_playerIconContainer";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _isLocked = "_isLocked";

		public static readonly StringName _currentOutline = "_currentOutline";

		public static readonly StringName _isSelected = "_isSelected";

		public static readonly StringName _state = "_state";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _hsvTween = "_hsvTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	private static readonly string _playerIconScenePath = SceneHelper.GetScenePath("screens/char_select/char_select_player_icon");

	private static readonly string _unlockedIconPath = ImageHelper.GetImagePath("packed/character_select/char_select_lock3_unlocked.png");

	private TextureRect _icon;

	private TextureRect _iconAdd;

	private TextureRect _lock;

	private Control _outlineLocal;

	private Control _outlineRemote;

	private Control _outlineMixed;

	private Control _shadow;

	private Control _playerIconContainer;

	private CharacterModel _character;

	private ShaderMaterial _hsv;

	private bool _isLocked;

	private static readonly Vector2 _hoverTipOffset = new Vector2(-90f, -180f);

	private ICharacterSelectButtonDelegate? _delegate;

	private Control? _currentOutline;

	private bool _isSelected;

	private readonly HashSet<ulong> _remoteSelectedPlayers = new HashSet<ulong>();

	private State _state;

	private static readonly Vector2 _hoverScale = Vector2.One * 1.1f;

	private Tween? _hoverTween;

	private Tween? _hsvTween;

	private const float _unhoverDuration = 0.5f;

	private const float _glowSpeed = 1.6f;

	private const float _selectedSaturation = 1f;

	private const float _selectedValue = 1.1f;

	private const float _remotelySelectedSaturation = 0.8f;

	private const float _remotelySelectedValue = 0.4f;

	private const float _notSelectedSaturation = 0.2f;

	private const float _notSelectedValue = 0.4f;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { _playerIconScenePath, _unlockedIconPath });

	public bool IsRandom { get; private set; }

	public IReadOnlyCollection<ulong> RemoteSelectedPlayers => _remoteSelectedPlayers;

	public CharacterModel Character => _character;

	public bool IsLocked => _isLocked;

	public override void _Ready()
	{
		ConnectSignals();
		_icon = GetNode<TextureRect>("%Icon");
		_iconAdd = GetNode<TextureRect>("%IconAdd");
		_lock = GetNode<TextureRect>("%Lock");
		_outlineLocal = GetNode<Control>("%OutlineLocal");
		_outlineRemote = GetNode<Control>("%OutlineRemote");
		_outlineMixed = GetNode<Control>("%OutlineMixed");
		_shadow = GetNode<Control>("%Shadow");
		_playerIconContainer = GetNode<Control>("%PlayerIconContainer");
		_hsv = (ShaderMaterial)_icon.Material;
		_hsv.SetShaderParameter(_s, 0.2f);
		_hsv.SetShaderParameter(_v, 0.4f);
		Connect(Control.SignalName.FocusEntered, Callable.From(Select));
	}

	public void Init(CharacterModel character, ICharacterSelectButtonDelegate del)
	{
		_delegate = del;
		_character = character;
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		if (character is RandomCharacter)
		{
			IsRandom = true;
			_isLocked = ModelDb.AllCharacters.Any((CharacterModel c) => !unlockState.Characters.Contains(c));
		}
		else
		{
			_isLocked = !unlockState.Characters.Contains(_character);
		}
		if (_isLocked)
		{
			_icon.Texture = character.CharacterSelectLockedIcon;
			_lock.Visible = true;
		}
		else
		{
			_icon.Texture = character.CharacterSelectIcon;
		}
	}

	protected override void OnFocus()
	{
		if (!_isSelected)
		{
			_hoverTween?.Kill();
			base.Scale = _hoverScale;
			_hsv.SetShaderParameter(_s, 1f);
			_hsv.SetShaderParameter(_v, 1.1f);
			if (_isLocked)
			{
				HoverTip hoverTip = new HoverTip(new LocString("main_menu_ui", "CHARACTER_SELECT.locked.title"), _character.GetUnlockText());
				NHoverTipSet.CreateAndShow(this, hoverTip).GlobalPosition = base.GlobalPosition + _hoverTipOffset;
			}
			SfxCmd.Play("event:/sfx/ui/clicks/ui_hover");
		}
	}

	protected override void OnPress()
	{
	}

	protected override void OnUnfocus()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		NHoverTipSet.Remove(this);
		AnimateSaturationToCurrentState(_hoverTween);
	}

	public override void _Process(double delta)
	{
		if (_currentOutline != null)
		{
			if (_isSelected)
			{
				float a = Mathf.Lerp(0.35f, 1f, (Mathf.Cos((float)Time.GetTicksMsec() * 0.001f * 1.6f * (float)Math.PI) + 1f) * 0.5f);
				Control? currentOutline = _currentOutline;
				Color modulate = _currentOutline.Modulate;
				modulate.A = a;
				currentOutline.Modulate = modulate;
			}
			else
			{
				Control? currentOutline2 = _currentOutline;
				Color modulate = _currentOutline.Modulate;
				modulate.A = 0.5f;
				currentOutline2.Modulate = modulate;
			}
		}
	}

	public void LockForAnimation()
	{
		_icon.Texture = _character.CharacterSelectLockedIcon;
		_lock.Visible = true;
		base.ZIndex = 1;
		_lock.Modulate = Colors.White;
		Disable();
	}

	public async Task AnimateUnlock()
	{
		GpuParticles2D chargeParticles = GetNode<GpuParticles2D>("%UnlockChargeParticles");
		chargeParticles.Emitting = true;
		float num = 1f;
		Vector2 originalLockPosition = _lock.Position;
		float timer = 0f;
		NDebugAudioManager.Instance.Play("character_unlock_charge.mp3");
		while (timer < 1f)
		{
			Vector2 vector = Vector2.Right.Rotated(Rng.Chaotic.NextFloat((float)Math.PI * 2f)) * num;
			_lock.Position = originalLockPosition + vector;
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			timer += (float)GetProcessDeltaTime();
			num = Mathf.Lerp(1f, 5f, Ease.QuadOut(timer));
		}
		NDebugAudioManager.Instance.Play("character_unlock.mp3");
		_lock.Position = originalLockPosition;
		_lock.Texture = PreloadManager.Cache.GetTexture2D(_unlockedIconPath);
		_icon.Texture = _character.CharacterSelectIcon;
		_iconAdd.Texture = _icon.Texture;
		_iconAdd.Visible = true;
		GpuParticles2D node = GetNode<GpuParticles2D>("%UnlockParticles");
		node.Emitting = true;
		chargeParticles.Emitting = false;
		Tween tween = CreateTween();
		tween.SetParallel();
		tween.TweenProperty(_iconAdd, "scale", Vector2.One * 1.5f, 1.0);
		tween.TweenProperty(_iconAdd, "modulate:a", 0f, 1.0).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Expo);
		tween.TweenProperty(_lock, "modulate:a", 0f, 0.5).SetDelay(0.5);
		base.ZIndex = 0;
		Enable();
	}

	public void Reset()
	{
		foreach (Node child in _playerIconContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		_remoteSelectedPlayers.Clear();
		Deselect();
	}

	public void OnRemotePlayerSelected(ulong playerId)
	{
		_remoteSelectedPlayers.Add(playerId);
		RefreshState();
	}

	public void OnRemotePlayerDeselected(ulong playerId)
	{
		_remoteSelectedPlayers.Remove(playerId);
		RefreshState();
	}

	public void Select()
	{
		if (!_isSelected)
		{
			_hoverTween?.Kill();
			_isSelected = true;
			_delegate.SelectCharacter(this, _character);
			RefreshState();
		}
	}

	public void Deselect()
	{
		_isSelected = false;
		RefreshState();
	}

	private void RefreshState()
	{
		State state = (_isSelected ? State.SelectedLocally : ((_remoteSelectedPlayers.Count > 0) ? State.SelectedRemotely : State.NotSelected));
		State state2 = _state;
		if (state2 != state)
		{
			_state = state;
			if (state2 == State.NotSelected)
			{
				_hsv.SetShaderParameter(_s, GetSaturationForCurrentState());
				_hsv.SetShaderParameter(_v, GetValueForCurrentState());
			}
			else
			{
				_hoverTween?.Kill();
				_hoverTween = CreateTween().SetParallel();
				AnimateSaturationToCurrentState(_hoverTween);
			}
		}
		RefreshOutline();
		RefreshPlayerIcons();
	}

	private float GetSaturationForCurrentState()
	{
		return _state switch
		{
			State.SelectedLocally => 1f, 
			State.SelectedRemotely => 0.8f, 
			State.NotSelected => 0.2f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private float GetValueForCurrentState()
	{
		return _state switch
		{
			State.SelectedLocally => 1.1f, 
			State.SelectedRemotely => 0.4f, 
			State.NotSelected => 0.8f, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void AnimateSaturationToCurrentState(Tween tween)
	{
		tween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), GetSaturationForCurrentState(), 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		tween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), GetValueForCurrentState(), 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	private void RefreshOutline()
	{
		if (_currentOutline != null)
		{
			_currentOutline.Visible = false;
		}
		if (_isSelected && _remoteSelectedPlayers.Count > 0)
		{
			_currentOutline = _outlineMixed;
		}
		else if (_isSelected)
		{
			_currentOutline = _outlineLocal;
		}
		else if (_remoteSelectedPlayers.Count > 0)
		{
			_currentOutline = _outlineRemote;
		}
		else
		{
			_currentOutline = null;
		}
		if (_currentOutline != null)
		{
			_currentOutline.Visible = true;
		}
	}

	private void RefreshPlayerIcons()
	{
		if (_delegate != null && _delegate.Lobby.NetService.Type != NetGameType.Singleplayer)
		{
			int num = _remoteSelectedPlayers.Count + (_isSelected ? 1 : 0);
			for (int i = _playerIconContainer.GetChildCount(); i < num; i++)
			{
				TextureRect child = PreloadManager.Cache.GetScene(_playerIconScenePath).Instantiate<TextureRect>(PackedScene.GenEditState.Disabled);
				_playerIconContainer.AddChildSafely(child);
			}
			while (_playerIconContainer.GetChildCount() > num)
			{
				Control child2 = _playerIconContainer.GetChild<Control>(0);
				_playerIconContainer.RemoveChildSafely(child2);
				child2.QueueFreeSafely();
			}
			for (int j = 0; j < _playerIconContainer.GetChildCount(); j++)
			{
				Control child3 = _playerIconContainer.GetChild<Control>(j);
				child3.Modulate = ((_isSelected && j == 0) ? StsColors.gold : StsColors.blue);
			}
		}
	}

	public void DebugUnlock()
	{
		_icon.Texture = _character.CharacterSelectIcon;
		_isLocked = false;
		_lock.Visible = false;
		Enable();
	}

	public void UnlockIfPossible()
	{
		UnlockState unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress();
		if (unlockState.Characters.Contains(_character))
		{
			_icon.Texture = _character.CharacterSelectIcon;
			_isLocked = false;
			_lock.Visible = false;
			Enable();
		}
	}

	private void UpdateShaderH(float value)
	{
		_hsv.SetShaderParameter(_h, value);
	}

	private void UpdateShaderS(float value)
	{
		_hsv.SetShaderParameter(_s, value);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(22);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.LockForAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reset, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRemotePlayerSelected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnRemotePlayerDeselected, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "playerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Select, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Deselect, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetSaturationForCurrentState, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetValueForCurrentState, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimateSaturationToCurrentState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tween", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Tween"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshOutline, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshPlayerIcons, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugUnlock, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UnlockIfPossible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderH, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderS, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.LockForAnimation && args.Count == 0)
		{
			LockForAnimation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reset && args.Count == 0)
		{
			Reset();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRemotePlayerSelected && args.Count == 1)
		{
			OnRemotePlayerSelected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnRemotePlayerDeselected && args.Count == 1)
		{
			OnRemotePlayerDeselected(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Select && args.Count == 0)
		{
			Select();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Deselect && args.Count == 0)
		{
			Deselect();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshState && args.Count == 0)
		{
			RefreshState();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSaturationForCurrentState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetSaturationForCurrentState());
			return true;
		}
		if (method == MethodName.GetValueForCurrentState && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetValueForCurrentState());
			return true;
		}
		if (method == MethodName.AnimateSaturationToCurrentState && args.Count == 1)
		{
			AnimateSaturationToCurrentState(VariantUtils.ConvertTo<Tween>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshOutline && args.Count == 0)
		{
			RefreshOutline();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshPlayerIcons && args.Count == 0)
		{
			RefreshPlayerIcons();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DebugUnlock && args.Count == 0)
		{
			DebugUnlock();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnlockIfPossible && args.Count == 0)
		{
			UnlockIfPossible();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderH && args.Count == 1)
		{
			UpdateShaderH(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderS && args.Count == 1)
		{
			UpdateShaderS(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateShaderV && args.Count == 1)
		{
			UpdateShaderV(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.LockForAnimation)
		{
			return true;
		}
		if (method == MethodName.Reset)
		{
			return true;
		}
		if (method == MethodName.OnRemotePlayerSelected)
		{
			return true;
		}
		if (method == MethodName.OnRemotePlayerDeselected)
		{
			return true;
		}
		if (method == MethodName.Select)
		{
			return true;
		}
		if (method == MethodName.Deselect)
		{
			return true;
		}
		if (method == MethodName.RefreshState)
		{
			return true;
		}
		if (method == MethodName.GetSaturationForCurrentState)
		{
			return true;
		}
		if (method == MethodName.GetValueForCurrentState)
		{
			return true;
		}
		if (method == MethodName.AnimateSaturationToCurrentState)
		{
			return true;
		}
		if (method == MethodName.RefreshOutline)
		{
			return true;
		}
		if (method == MethodName.RefreshPlayerIcons)
		{
			return true;
		}
		if (method == MethodName.DebugUnlock)
		{
			return true;
		}
		if (method == MethodName.UnlockIfPossible)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderH)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderS)
		{
			return true;
		}
		if (method == MethodName.UpdateShaderV)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsRandom)
		{
			IsRandom = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._icon)
		{
			_icon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._iconAdd)
		{
			_iconAdd = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._lock)
		{
			_lock = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outlineLocal)
		{
			_outlineLocal = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._outlineRemote)
		{
			_outlineRemote = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._outlineMixed)
		{
			_outlineMixed = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			_shadow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			_playerIconContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._isLocked)
		{
			_isLocked = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._currentOutline)
		{
			_currentOutline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			_isSelected = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._state)
		{
			_state = VariantUtils.ConvertTo<State>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hsvTween)
		{
			_hsvTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		bool from;
		if (name == PropertyName.IsRandom)
		{
			from = IsRandom;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.IsLocked)
		{
			from = IsLocked;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._icon)
		{
			value = VariantUtils.CreateFrom(in _icon);
			return true;
		}
		if (name == PropertyName._iconAdd)
		{
			value = VariantUtils.CreateFrom(in _iconAdd);
			return true;
		}
		if (name == PropertyName._lock)
		{
			value = VariantUtils.CreateFrom(in _lock);
			return true;
		}
		if (name == PropertyName._outlineLocal)
		{
			value = VariantUtils.CreateFrom(in _outlineLocal);
			return true;
		}
		if (name == PropertyName._outlineRemote)
		{
			value = VariantUtils.CreateFrom(in _outlineRemote);
			return true;
		}
		if (name == PropertyName._outlineMixed)
		{
			value = VariantUtils.CreateFrom(in _outlineMixed);
			return true;
		}
		if (name == PropertyName._shadow)
		{
			value = VariantUtils.CreateFrom(in _shadow);
			return true;
		}
		if (name == PropertyName._playerIconContainer)
		{
			value = VariantUtils.CreateFrom(in _playerIconContainer);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._isLocked)
		{
			value = VariantUtils.CreateFrom(in _isLocked);
			return true;
		}
		if (name == PropertyName._currentOutline)
		{
			value = VariantUtils.CreateFrom(in _currentOutline);
			return true;
		}
		if (name == PropertyName._isSelected)
		{
			value = VariantUtils.CreateFrom(in _isSelected);
			return true;
		}
		if (name == PropertyName._state)
		{
			value = VariantUtils.CreateFrom(in _state);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._hsvTween)
		{
			value = VariantUtils.CreateFrom(in _hsvTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._icon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._iconAdd, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outlineLocal, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outlineRemote, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outlineMixed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shadow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._playerIconContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isLocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsRandom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isSelected, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._state, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsvTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsLocked, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsRandom, Variant.From<bool>(IsRandom));
		info.AddProperty(PropertyName._icon, Variant.From(in _icon));
		info.AddProperty(PropertyName._iconAdd, Variant.From(in _iconAdd));
		info.AddProperty(PropertyName._lock, Variant.From(in _lock));
		info.AddProperty(PropertyName._outlineLocal, Variant.From(in _outlineLocal));
		info.AddProperty(PropertyName._outlineRemote, Variant.From(in _outlineRemote));
		info.AddProperty(PropertyName._outlineMixed, Variant.From(in _outlineMixed));
		info.AddProperty(PropertyName._shadow, Variant.From(in _shadow));
		info.AddProperty(PropertyName._playerIconContainer, Variant.From(in _playerIconContainer));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._isLocked, Variant.From(in _isLocked));
		info.AddProperty(PropertyName._currentOutline, Variant.From(in _currentOutline));
		info.AddProperty(PropertyName._isSelected, Variant.From(in _isSelected));
		info.AddProperty(PropertyName._state, Variant.From(in _state));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._hsvTween, Variant.From(in _hsvTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsRandom, out var value))
		{
			IsRandom = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._icon, out var value2))
		{
			_icon = value2.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._iconAdd, out var value3))
		{
			_iconAdd = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._lock, out var value4))
		{
			_lock = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outlineLocal, out var value5))
		{
			_outlineLocal = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._outlineRemote, out var value6))
		{
			_outlineRemote = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._outlineMixed, out var value7))
		{
			_outlineMixed = value7.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._shadow, out var value8))
		{
			_shadow = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._playerIconContainer, out var value9))
		{
			_playerIconContainer = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value10))
		{
			_hsv = value10.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._isLocked, out var value11))
		{
			_isLocked = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._currentOutline, out var value12))
		{
			_currentOutline = value12.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._isSelected, out var value13))
		{
			_isSelected = value13.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._state, out var value14))
		{
			_state = value14.As<State>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value15))
		{
			_hoverTween = value15.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hsvTween, out var value16))
		{
			_hsvTween = value16.As<Tween>();
		}
	}
}
