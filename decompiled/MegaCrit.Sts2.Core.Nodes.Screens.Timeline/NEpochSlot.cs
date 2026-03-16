using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Timeline;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Timeline;

[ScriptPath("res://src/Core/Nodes/Screens/Timeline/NEpochSlot.cs")]
public class NEpochSlot : NButton
{
	public new class MethodName : NButton.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnRelease = "OnRelease";

		public static readonly StringName RevealEpoch = "RevealEpoch";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public new static readonly StringName OnPress = "OnPress";

		public new static readonly StringName _Process = "_Process";

		public static readonly StringName DisableHighlight = "DisableHighlight";

		public static readonly StringName EnableHighlight = "EnableHighlight";

		public static readonly StringName SetState = "SetState";

		public static readonly StringName UpdateShaderS = "UpdateShaderS";

		public static readonly StringName UpdateShaderV = "UpdateShaderV";

		public static readonly StringName UpdateBlurLod = "UpdateBlurLod";
	}

	public new class PropertyName : NButton.PropertyName
	{
		public new static readonly StringName ClickedSfx = "ClickedSfx";

		public new static readonly StringName HoveredSfx = "HoveredSfx";

		public static readonly StringName State = "State";

		public static readonly StringName HasSpawned = "HasSpawned";

		public static readonly StringName _slotImage = "_slotImage";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _chains = "_chains";

		public static readonly StringName _hsv = "_hsv";

		public static readonly StringName _blurPortrait = "_blurPortrait";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _blur = "_blur";

		public static readonly StringName _blurShader = "_blurShader";

		public static readonly StringName _selectionReticle = "_selectionReticle";

		public static readonly StringName _offscreenVfx = "_offscreenVfx";

		public static readonly StringName _highlightVfx = "_highlightVfx";

		public static readonly StringName _subViewportContainer = "_subViewportContainer";

		public static readonly StringName _subViewport = "_subViewport";

		public static readonly StringName _isGlowPulsing = "_isGlowPulsing";

		public static readonly StringName _isComplete = "_isComplete";

		public new static readonly StringName _isHovered = "_isHovered";

		public static readonly StringName _era = "_era";

		public static readonly StringName eraPosition = "eraPosition";

		public static readonly StringName _glowTween = "_glowTween";

		public static readonly StringName _spawnTween = "_spawnTween";

		public static readonly StringName _hoverTween = "_hoverTween";
	}

	public new class SignalName : NButton.SignalName
	{
	}

	private static readonly StringName _lod = new StringName("lod");

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private const string _unlockIconPath = "res://images/packed/unlock_icon.png";

	private const string _scenePath = "res://scenes/timeline_screen/epoch_slot.tscn";

	public static readonly IEnumerable<string> assetPaths = new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://scenes/timeline_screen/epoch_slot.tscn", "res://images/packed/unlock_icon.png" });

	private TextureRect _slotImage;

	private TextureRect _portrait;

	private TextureRect _chains;

	private ShaderMaterial _hsv;

	private TextureRect _blurPortrait;

	private TextureRect _outline;

	private Control _blur;

	private ShaderMaterial _blurShader;

	private NSelectionReticle _selectionReticle;

	private NEpochOffscreenVfx? _offscreenVfx;

	private Control? _highlightVfx;

	private SubViewportContainer _subViewportContainer;

	private SubViewport _subViewport;

	private bool _isGlowPulsing;

	public EpochModel model;

	private bool _isComplete;

	private bool _isHovered;

	private EpochEra _era;

	public int eraPosition;

	private Tween? _glowTween;

	private Tween? _spawnTween;

	private Tween? _hoverTween;

	private static readonly Color _highlightSlotColor = StsColors.purple;

	private static readonly Color _defaultSlotOutlineColor = new Color("70a0ff18");

	private IHoverTip? _hoverTip;

	protected override string ClickedSfx => "event:/sfx/ui/timeline/ui_timeline_click";

	protected override string HoveredSfx
	{
		get
		{
			if (State != EpochSlotState.NotObtained)
			{
				return "event:/sfx/ui/timeline/ui_timeline_hover";
			}
			return "event:/sfx/ui/timeline/ui_timeline_hover_locked";
		}
	}

	public EpochSlotState State { get; private set; }

	public bool HasSpawned { get; private set; }

	public override void _Ready()
	{
		ConnectSignals();
		_slotImage = GetNode<TextureRect>("%SlotImage");
		_portrait = GetNode<TextureRect>("%Portrait");
		_chains = GetNode<TextureRect>("%Chains");
		_hsv = (ShaderMaterial)_portrait.GetMaterial();
		_blurPortrait = GetNode<TextureRect>("%BlurPortrait");
		_outline = GetNode<TextureRect>("%Outline");
		_subViewportContainer = GetNode<SubViewportContainer>("%SubViewportContainer");
		_subViewport = GetNode<SubViewport>("%SubViewport");
		_blurShader = (ShaderMaterial)GetNode<Control>("%Blur").GetMaterial();
		_selectionReticle = GetNode<NSelectionReticle>("%SelectionReticle");
		if (!NGame.IsReleaseGame())
		{
			MegaLabel node = GetNode<MegaLabel>("%DebugLabel");
			node.Text = model.GetType().Name;
			node.Visible = true;
		}
		SetState(State);
	}

	public static NEpochSlot Create(EpochSlotData data)
	{
		NEpochSlot nEpochSlot = PreloadManager.Cache.GetScene("res://scenes/timeline_screen/epoch_slot.tscn").Instantiate<NEpochSlot>(PackedScene.GenEditState.Disabled);
		nEpochSlot._era = data.Era;
		nEpochSlot.State = data.State;
		nEpochSlot.eraPosition = data.EraPosition;
		nEpochSlot.model = data.Model;
		return nEpochSlot;
	}

	protected override void OnRelease()
	{
		if (!NGame.IsReleaseGame() && State == EpochSlotState.NotObtained && Input.IsKeyPressed(Key.Ctrl))
		{
			NHoverTipSet.Remove(this);
			State = EpochSlotState.Obtained;
		}
		base.OnRelease();
		if (State == EpochSlotState.Obtained)
		{
			NTimelineScreen.Instance.DisableInput();
			GetViewport().GuiReleaseFocus();
			RevealEpoch();
			SetState(EpochSlotState.Complete);
		}
		else if (State == EpochSlotState.Complete)
		{
			NTimelineScreen.Instance.DisableInput();
			NTimelineScreen.Instance.OpenInspectScreen(this, playAnimation: false);
		}
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_s), 1f, 0.05);
	}

	private void RevealEpoch()
	{
		State = EpochSlotState.Complete;
		_portrait.Visible = true;
		_portrait.Texture = model.Portrait;
		DisableHighlight();
		SaveManager.Instance.RevealEpoch(model.Id);
		_slotImage.Modulate = Colors.White;
		_slotImage.ClipChildren = ClipChildrenMode.AndDraw;
		_chains.Visible = false;
		NTimelineScreen.Instance.OpenInspectScreen(this, playAnimation: true);
	}

	protected override void OnFocus()
	{
		base.OnFocus();
		_isGlowPulsing = false;
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		if (NControllerManager.Instance.IsUsingController)
		{
			_selectionReticle.OnSelect();
		}
		if (State != EpochSlotState.NotObtained)
		{
			_hoverTween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.05);
			if (State == EpochSlotState.Complete)
			{
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 1.1f, 0.05);
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1.1f, 0.05);
				LocString unlockInfo = model.UnlockInfo;
				unlockInfo.Add("IsRevealed", variable: true);
				_hoverTip = new HoverTip(model.Title, unlockInfo, PreloadManager.Cache.GetTexture2D("res://images/packed/unlock_icon.png"));
			}
			else if (State == EpochSlotState.Obtained)
			{
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 0.65f, 0.05);
				LocString unlockInfo2 = model.UnlockInfo;
				unlockInfo2.Add("IsRevealed", variable: false);
				_hoverTip = new HoverTip(model.Title, unlockInfo2);
			}
		}
		else
		{
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 0.25f, 0.05);
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1.2f, 0.05);
			LocString unlockInfo3 = model.UnlockInfo;
			unlockInfo3.Add("IsRevealed", variable: false);
			_hoverTip = new HoverTip(model.Title, unlockInfo3);
		}
		if (_hoverTip != null)
		{
			NHoverTipSet nHoverTipSet = NHoverTipSet.CreateAndShow(this, _hoverTip);
			float num = (base.Size.X * 0.5f + 6f) * GetGlobalTransform().Scale.X;
			float num2 = base.GlobalPosition.X + (base.Size.X * 0.5f + 6f) * GetGlobalTransform().Scale.X;
			if (base.GlobalPosition.X > NGame.Instance.GetViewportRect().Size.X * 0.7f)
			{
				nHoverTipSet.GlobalPosition = new Vector2(num2 - num - 360f, base.GlobalPosition.Y);
			}
			else
			{
				nHoverTipSet.GlobalPosition = new Vector2(num2 + num, base.GlobalPosition.Y);
			}
			nHoverTipSet.SetFollowOwner();
		}
	}

	protected override void OnUnfocus()
	{
		base.OnUnfocus();
		_selectionReticle.OnDeselect();
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(this, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		if (State != EpochSlotState.NotObtained)
		{
			if (State == EpochSlotState.Obtained)
			{
				_isGlowPulsing = true;
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 0.5f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
			else if (State == EpochSlotState.Complete)
			{
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
		}
		else
		{
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 0.25f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1.1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		}
		NHoverTipSet.Remove(this);
	}

	protected override void OnPress()
	{
		if (State != EpochSlotState.NotObtained)
		{
			base.OnPress();
			_hoverTween?.Kill();
			_hoverTween = CreateTween().SetParallel();
			_hoverTween.TweenProperty(this, "scale", Vector2.One * 0.95f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			if (State == EpochSlotState.Complete)
			{
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderS), _hsv.GetShaderParameter(_s), 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 1f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
			else if (State == EpochSlotState.Obtained)
			{
				_hoverTween.TweenMethod(Callable.From<float>(UpdateShaderV), _hsv.GetShaderParameter(_v), 0.5f, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
			}
		}
	}

	public async Task SpawnSlot()
	{
		_spawnTween = CreateTween().SetParallel();
		_spawnTween.Chain();
		_spawnTween.TweenInterval(Rng.Chaotic.NextDouble(0.0, 0.3));
		_spawnTween.Chain();
		_spawnTween.TweenProperty(_slotImage, "position", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(new Vector2(0f, 64f));
		_spawnTween.TweenProperty(_outline, "position", Vector2.Zero, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(new Vector2(0f, 64f));
		_spawnTween.TweenProperty(_slotImage, "self_modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		await ToSignal(_spawnTween, Tween.SignalName.Finished);
		HasSpawned = true;
		if (State == EpochSlotState.Obtained)
		{
			EnableHighlight();
		}
		else
		{
			_outline.Modulate = _defaultSlotOutlineColor;
		}
	}

	public override void _Process(double delta)
	{
		if (_isGlowPulsing)
		{
			_outline.Modulate = new Color(_outline.Modulate.R, _outline.Modulate.G, _outline.Modulate.B, (Mathf.Sin((float)Time.GetTicksMsec() * 0.005f) + 2f) * 0.25f);
		}
	}

	private void DisableHighlight()
	{
		_isGlowPulsing = false;
		_outline.Modulate = Colors.Transparent;
		_highlightVfx?.QueueFreeSafely();
		_offscreenVfx?.QueueFreeSafely();
	}

	private void EnableHighlight()
	{
		_isGlowPulsing = true;
		_outline.Modulate = _highlightSlotColor;
		_outline.SelfModulate = StsColors.transparentWhite;
		_glowTween?.Kill();
		_glowTween = CreateTween();
		_glowTween.TweenProperty(_outline, "self_modulate:a", 1f, 1.0);
		_offscreenVfx = NEpochOffscreenVfx.Create(this);
		_highlightVfx = NEpochHighlightVfx.Create();
		this.AddChildSafely(_highlightVfx);
		NTimelineScreen.Instance.GetReminderVfxHolder().AddChildSafely(_offscreenVfx);
		MoveChild(_highlightVfx, 0);
	}

	public void SetState(EpochSlotState setState)
	{
		State = setState;
		if (State == EpochSlotState.None)
		{
			Log.Error("Slot State is invalid.");
			return;
		}
		_slotImage.Modulate = Colors.White;
		_slotImage.ClipChildren = ClipChildrenMode.AndDraw;
		_portrait.Visible = true;
		if (State == EpochSlotState.Complete)
		{
			DisableHighlight();
			_portrait.Texture = model.Portrait;
			UpdateShaderS(1f);
			UpdateShaderV(1f);
		}
		else if (State == EpochSlotState.Obtained)
		{
			_portrait.Texture = model.Portrait;
			UpdateShaderS(0f);
			UpdateShaderV(0.5f);
			_chains.Visible = true;
		}
		else if (State == EpochSlotState.NotObtained)
		{
			_blurShader.SetShaderParameter(_lod, 2f);
			UpdateShaderS(0.25f);
			UpdateShaderV(1f);
			_blurPortrait.Texture = model.Portrait;
			_subViewportContainer.Visible = true;
			_portrait.Texture = _subViewport.GetTexture();
		}
		base.MouseDefaultCursorShape = (CursorShape)((State == EpochSlotState.Complete) ? 16 : 0);
	}

	private void UpdateShaderS(float value)
	{
		_hsv.SetShaderParameter(_s, value);
	}

	private void UpdateShaderV(float value)
	{
		_hsv.SetShaderParameter(_v, value);
	}

	private void UpdateBlurLod(float value)
	{
		_blurShader.SetShaderParameter(_lod, value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(13);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelease, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RevealEpoch, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnPress, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DisableHighlight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableHighlight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "setState", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderS, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateShaderV, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "value", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateBlurLod, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.OnRelease && args.Count == 0)
		{
			OnRelease();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RevealEpoch && args.Count == 0)
		{
			RevealEpoch();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnPress && args.Count == 0)
		{
			OnPress();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DisableHighlight && args.Count == 0)
		{
			DisableHighlight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableHighlight && args.Count == 0)
		{
			EnableHighlight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetState && args.Count == 1)
		{
			SetState(VariantUtils.ConvertTo<EpochSlotState>(in args[0]));
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
		if (method == MethodName.UpdateBlurLod && args.Count == 1)
		{
			UpdateBlurLod(VariantUtils.ConvertTo<float>(in args[0]));
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
		if (method == MethodName.OnRelease)
		{
			return true;
		}
		if (method == MethodName.RevealEpoch)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.OnPress)
		{
			return true;
		}
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName.DisableHighlight)
		{
			return true;
		}
		if (method == MethodName.EnableHighlight)
		{
			return true;
		}
		if (method == MethodName.SetState)
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
		if (method == MethodName.UpdateBlurLod)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.State)
		{
			State = VariantUtils.ConvertTo<EpochSlotState>(in value);
			return true;
		}
		if (name == PropertyName.HasSpawned)
		{
			HasSpawned = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._slotImage)
		{
			_slotImage = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._chains)
		{
			_chains = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			_hsv = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._blurPortrait)
		{
			_blurPortrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._blur)
		{
			_blur = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._blurShader)
		{
			_blurShader = VariantUtils.ConvertTo<ShaderMaterial>(in value);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			_selectionReticle = VariantUtils.ConvertTo<NSelectionReticle>(in value);
			return true;
		}
		if (name == PropertyName._offscreenVfx)
		{
			_offscreenVfx = VariantUtils.ConvertTo<NEpochOffscreenVfx>(in value);
			return true;
		}
		if (name == PropertyName._highlightVfx)
		{
			_highlightVfx = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._subViewportContainer)
		{
			_subViewportContainer = VariantUtils.ConvertTo<SubViewportContainer>(in value);
			return true;
		}
		if (name == PropertyName._subViewport)
		{
			_subViewport = VariantUtils.ConvertTo<SubViewport>(in value);
			return true;
		}
		if (name == PropertyName._isGlowPulsing)
		{
			_isGlowPulsing = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isComplete)
		{
			_isComplete = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._era)
		{
			_era = VariantUtils.ConvertTo<EpochEra>(in value);
			return true;
		}
		if (name == PropertyName.eraPosition)
		{
			eraPosition = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			_glowTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._spawnTween)
		{
			_spawnTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		string from;
		if (name == PropertyName.ClickedSfx)
		{
			from = ClickedSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HoveredSfx)
		{
			from = HoveredSfx;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.State)
		{
			value = VariantUtils.CreateFrom<EpochSlotState>(State);
			return true;
		}
		if (name == PropertyName.HasSpawned)
		{
			value = VariantUtils.CreateFrom<bool>(HasSpawned);
			return true;
		}
		if (name == PropertyName._slotImage)
		{
			value = VariantUtils.CreateFrom(in _slotImage);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
		if (name == PropertyName._chains)
		{
			value = VariantUtils.CreateFrom(in _chains);
			return true;
		}
		if (name == PropertyName._hsv)
		{
			value = VariantUtils.CreateFrom(in _hsv);
			return true;
		}
		if (name == PropertyName._blurPortrait)
		{
			value = VariantUtils.CreateFrom(in _blurPortrait);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._blur)
		{
			value = VariantUtils.CreateFrom(in _blur);
			return true;
		}
		if (name == PropertyName._blurShader)
		{
			value = VariantUtils.CreateFrom(in _blurShader);
			return true;
		}
		if (name == PropertyName._selectionReticle)
		{
			value = VariantUtils.CreateFrom(in _selectionReticle);
			return true;
		}
		if (name == PropertyName._offscreenVfx)
		{
			value = VariantUtils.CreateFrom(in _offscreenVfx);
			return true;
		}
		if (name == PropertyName._highlightVfx)
		{
			value = VariantUtils.CreateFrom(in _highlightVfx);
			return true;
		}
		if (name == PropertyName._subViewportContainer)
		{
			value = VariantUtils.CreateFrom(in _subViewportContainer);
			return true;
		}
		if (name == PropertyName._subViewport)
		{
			value = VariantUtils.CreateFrom(in _subViewport);
			return true;
		}
		if (name == PropertyName._isGlowPulsing)
		{
			value = VariantUtils.CreateFrom(in _isGlowPulsing);
			return true;
		}
		if (name == PropertyName._isComplete)
		{
			value = VariantUtils.CreateFrom(in _isComplete);
			return true;
		}
		if (name == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom(in _isHovered);
			return true;
		}
		if (name == PropertyName._era)
		{
			value = VariantUtils.CreateFrom(in _era);
			return true;
		}
		if (name == PropertyName.eraPosition)
		{
			value = VariantUtils.CreateFrom(in eraPosition);
			return true;
		}
		if (name == PropertyName._glowTween)
		{
			value = VariantUtils.CreateFrom(in _glowTween);
			return true;
		}
		if (name == PropertyName._spawnTween)
		{
			value = VariantUtils.CreateFrom(in _spawnTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.ClickedSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.String, PropertyName.HoveredSfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._slotImage, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._chains, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hsv, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blurPortrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blur, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blurShader, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._selectionReticle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._offscreenVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._highlightVfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._subViewportContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._subViewport, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isGlowPulsing, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isComplete, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._isHovered, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.State, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._era, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.eraPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._glowTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._spawnTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.HasSpawned, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.State, Variant.From<EpochSlotState>(State));
		info.AddProperty(PropertyName.HasSpawned, Variant.From<bool>(HasSpawned));
		info.AddProperty(PropertyName._slotImage, Variant.From(in _slotImage));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._chains, Variant.From(in _chains));
		info.AddProperty(PropertyName._hsv, Variant.From(in _hsv));
		info.AddProperty(PropertyName._blurPortrait, Variant.From(in _blurPortrait));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._blur, Variant.From(in _blur));
		info.AddProperty(PropertyName._blurShader, Variant.From(in _blurShader));
		info.AddProperty(PropertyName._selectionReticle, Variant.From(in _selectionReticle));
		info.AddProperty(PropertyName._offscreenVfx, Variant.From(in _offscreenVfx));
		info.AddProperty(PropertyName._highlightVfx, Variant.From(in _highlightVfx));
		info.AddProperty(PropertyName._subViewportContainer, Variant.From(in _subViewportContainer));
		info.AddProperty(PropertyName._subViewport, Variant.From(in _subViewport));
		info.AddProperty(PropertyName._isGlowPulsing, Variant.From(in _isGlowPulsing));
		info.AddProperty(PropertyName._isComplete, Variant.From(in _isComplete));
		info.AddProperty(PropertyName._isHovered, Variant.From(in _isHovered));
		info.AddProperty(PropertyName._era, Variant.From(in _era));
		info.AddProperty(PropertyName.eraPosition, Variant.From(in eraPosition));
		info.AddProperty(PropertyName._glowTween, Variant.From(in _glowTween));
		info.AddProperty(PropertyName._spawnTween, Variant.From(in _spawnTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.State, out var value))
		{
			State = value.As<EpochSlotState>();
		}
		if (info.TryGetProperty(PropertyName.HasSpawned, out var value2))
		{
			HasSpawned = value2.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._slotImage, out var value3))
		{
			_slotImage = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value4))
		{
			_portrait = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._chains, out var value5))
		{
			_chains = value5.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._hsv, out var value6))
		{
			_hsv = value6.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._blurPortrait, out var value7))
		{
			_blurPortrait = value7.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value8))
		{
			_outline = value8.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._blur, out var value9))
		{
			_blur = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._blurShader, out var value10))
		{
			_blurShader = value10.As<ShaderMaterial>();
		}
		if (info.TryGetProperty(PropertyName._selectionReticle, out var value11))
		{
			_selectionReticle = value11.As<NSelectionReticle>();
		}
		if (info.TryGetProperty(PropertyName._offscreenVfx, out var value12))
		{
			_offscreenVfx = value12.As<NEpochOffscreenVfx>();
		}
		if (info.TryGetProperty(PropertyName._highlightVfx, out var value13))
		{
			_highlightVfx = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._subViewportContainer, out var value14))
		{
			_subViewportContainer = value14.As<SubViewportContainer>();
		}
		if (info.TryGetProperty(PropertyName._subViewport, out var value15))
		{
			_subViewport = value15.As<SubViewport>();
		}
		if (info.TryGetProperty(PropertyName._isGlowPulsing, out var value16))
		{
			_isGlowPulsing = value16.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isComplete, out var value17))
		{
			_isComplete = value17.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._isHovered, out var value18))
		{
			_isHovered = value18.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._era, out var value19))
		{
			_era = value19.As<EpochEra>();
		}
		if (info.TryGetProperty(PropertyName.eraPosition, out var value20))
		{
			eraPosition = value20.As<int>();
		}
		if (info.TryGetProperty(PropertyName._glowTween, out var value21))
		{
			_glowTween = value21.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._spawnTween, out var value22))
		{
			_spawnTween = value22.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value23))
		{
			_hoverTween = value23.As<Tween>();
		}
	}
}
