using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NHealthBar.cs")]
public class NHealthBar : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName DebugToggleVisibility = "DebugToggleVisibility";

		public static readonly StringName UpdateLayoutForCreatureBounds = "UpdateLayoutForCreatureBounds";

		public static readonly StringName UpdateWidthRelativeToReferenceValue = "UpdateWidthRelativeToReferenceValue";

		public static readonly StringName SetHpBarContainerSizeWithOffsets = "SetHpBarContainerSizeWithOffsets";

		public static readonly StringName SetHpBarContainerSizeWithOffsetsImmediately = "SetHpBarContainerSizeWithOffsetsImmediately";

		public static readonly StringName RefreshValues = "RefreshValues";

		public static readonly StringName RefreshMiddleground = "RefreshMiddleground";

		public static readonly StringName RefreshForeground = "RefreshForeground";

		public static readonly StringName RefreshBlockUi = "RefreshBlockUi";

		public static readonly StringName RefreshText = "RefreshText";

		public static readonly StringName IsPoisonLethal = "IsPoisonLethal";

		public static readonly StringName IsDoomLethal = "IsDoomLethal";

		public static readonly StringName GetFgWidth = "GetFgWidth";

		public static readonly StringName FadeOutHpLabel = "FadeOutHpLabel";

		public static readonly StringName FadeInHpLabel = "FadeInHpLabel";

		public static readonly StringName AnimateInBlock = "AnimateInBlock";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName MaxFgWidth = "MaxFgWidth";

		public static readonly StringName HpBarContainer = "HpBarContainer";

		public static readonly StringName _hpForegroundContainer = "_hpForegroundContainer";

		public static readonly StringName _hpForeground = "_hpForeground";

		public static readonly StringName _poisonForeground = "_poisonForeground";

		public static readonly StringName _doomForeground = "_doomForeground";

		public static readonly StringName _hpMiddleground = "_hpMiddleground";

		public static readonly StringName _hpLabel = "_hpLabel";

		public static readonly StringName _blockContainer = "_blockContainer";

		public static readonly StringName _blockLabel = "_blockLabel";

		public static readonly StringName _blockOutline = "_blockOutline";

		public static readonly StringName _infinityTex = "_infinityTex";

		public static readonly StringName _blockTween = "_blockTween";

		public static readonly StringName _hpLabelFadeTween = "_hpLabelFadeTween";

		public static readonly StringName _middlegroundTween = "_middlegroundTween";

		public static readonly StringName _originalBlockPosition = "_originalBlockPosition";

		public static readonly StringName _currentHpOnLastRefresh = "_currentHpOnLastRefresh";

		public static readonly StringName _maxHpOnLastRefresh = "_maxHpOnLastRefresh";

		public static readonly StringName _expectedMaxFgWidth = "_expectedMaxFgWidth";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control _hpForegroundContainer;

	private Control _hpForeground;

	private Control _poisonForeground;

	private Control _doomForeground;

	private Control _hpMiddleground;

	private MegaLabel _hpLabel;

	private Control _blockContainer;

	private MegaLabel _blockLabel;

	private Control _blockOutline;

	private Creature _creature;

	private Creature? _blockTrackingCreature;

	private readonly LocString _healthBarDead = new LocString("gameplay_ui", "HEALTH_BAR.DEAD");

	private TextureRect _infinityTex;

	private Tween? _blockTween;

	private Tween? _hpLabelFadeTween;

	private Tween? _middlegroundTween;

	private Vector2 _originalBlockPosition;

	private int _currentHpOnLastRefresh = -1;

	private int _maxHpOnLastRefresh = -1;

	private float _expectedMaxFgWidth = -1f;

	private const float _minSize = 12f;

	private static readonly Vector2 _blockAnimOffset = new Vector2(0f, 20f);

	private static readonly Color _defaultFontColor = StsColors.cream;

	private static readonly Color _defaultFontOutlineColor = new Color("900000");

	private static readonly Color _blockOutlineColor = new Color("1B3045");

	private static readonly Color _redForegroundColor = new Color("F1373E");

	private static readonly Color _blockHpForegroundColor = new Color("3B6FA3");

	private static readonly Color _invincibleForegroundColor = new Color("C5BBED");

	private const float _foregroundContainerInset = 10f;

	private float MaxFgWidth
	{
		get
		{
			if (!(_expectedMaxFgWidth > 0f))
			{
				return _hpForegroundContainer.Size.X;
			}
			return _expectedMaxFgWidth;
		}
	}

	public Control HpBarContainer { get; private set; }

	public void SetCreature(Creature creature)
	{
		if (_creature != null)
		{
			throw new InvalidOperationException("Creature was already set.");
		}
		_creature = creature;
		_hpForeground.OffsetRight = GetFgWidth(_creature.CurrentHp) - MaxFgWidth;
		_hpMiddleground.OffsetRight = _hpForeground.OffsetRight - 2f;
	}

	public override void _Ready()
	{
		HpBarContainer = GetNode<Control>("%HpBarContainer");
		_hpForegroundContainer = GetNode<Control>("%HpForegroundContainer");
		_hpMiddleground = GetNode<Control>("%HpMiddleground");
		_hpForeground = GetNode<Control>("%HpForeground");
		_poisonForeground = GetNode<Control>("%PoisonForeground");
		_doomForeground = GetNode<Control>("%DoomForeground");
		_hpLabel = GetNode<MegaLabel>("%HpLabel");
		_blockContainer = GetNode<Control>("%BlockContainer");
		_blockLabel = GetNode<MegaLabel>("%BlockLabel");
		_blockOutline = GetNode<Control>("%BlockOutline");
		_infinityTex = GetNode<TextureRect>("%InfinityTex");
		_originalBlockPosition = _blockContainer.Position;
	}

	private void DebugToggleVisibility()
	{
		base.Visible = !NCombatUi.IsDebugHidingHpBar;
	}

	public void UpdateLayoutForCreatureBounds(Control bounds)
	{
		float valueOrDefault = (24f - _creature.Monster?.HpBarSizeReduction).GetValueOrDefault();
		HpBarContainer.GlobalPosition = new Vector2(bounds.GlobalPosition.X - valueOrDefault * 0.5f, HpBarContainer.GlobalPosition.Y);
		float x = bounds.Size.X + valueOrDefault;
		SetHpBarContainerSizeWithOffsets(new Vector2(x, HpBarContainer.Size.Y));
		float num = _blockContainer.Size.X * 0.5f;
		_blockContainer.GlobalPosition = new Vector2(bounds.GlobalPosition.X - num, _blockContainer.GlobalPosition.Y);
		_originalBlockPosition = _blockContainer.Position;
	}

	public void UpdateWidthRelativeToReferenceValue(float refMaxHp, float refWidth)
	{
		Vector2 size = HpBarContainer.Size;
		size.X = (float)_creature.MaxHp / refMaxHp * refWidth;
		SetHpBarContainerSizeWithOffsetsImmediately(size);
	}

	private void SetHpBarContainerSizeWithOffsets(Vector2 size)
	{
		Callable.From(delegate
		{
			SetHpBarContainerSizeWithOffsetsImmediately(size);
		}).CallDeferred();
	}

	private void SetHpBarContainerSizeWithOffsetsImmediately(Vector2 size)
	{
		if (!HpBarContainer.Size.IsEqualApprox(size))
		{
			_middlegroundTween?.Kill();
			HpBarContainer.Size = size;
			_expectedMaxFgWidth = size.X - 10f;
			_hpForeground.OffsetRight = GetFgWidth(_creature.CurrentHp, _expectedMaxFgWidth) - _expectedMaxFgWidth;
			_hpMiddleground.OffsetRight = _hpForeground.OffsetRight - 2f;
		}
	}

	public void RefreshValues()
	{
		RefreshBlockUi();
		RefreshForeground();
		RefreshMiddleground();
		RefreshText();
	}

	private void RefreshMiddleground()
	{
		if (_creature.CurrentHp <= 0)
		{
			_hpMiddleground.Visible = false;
			return;
		}
		_hpMiddleground.Visible = true;
		Control hpMiddleground = _hpMiddleground;
		Vector2 position = _hpMiddleground.Position;
		position.X = 1f;
		hpMiddleground.Position = position;
		int currentHp = _creature.CurrentHp;
		int maxHp = _creature.MaxHp;
		if (currentHp != _currentHpOnLastRefresh || maxHp != _maxHpOnLastRefresh)
		{
			_currentHpOnLastRefresh = currentHp;
			_maxHpOnLastRefresh = maxHp;
			float num = (_creature.HasPower<PoisonPower>() ? _poisonForeground.OffsetRight : _hpForeground.OffsetRight);
			bool flag = num >= _hpMiddleground.OffsetRight;
			_hpMiddleground.OffsetRight += 1f;
			_middlegroundTween?.Kill();
			_middlegroundTween = CreateTween();
			_middlegroundTween.TweenProperty(_hpMiddleground, "offset_right", num - 2f, 1.0).SetDelay(flag ? 0.0 : 1.0).SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Expo);
		}
	}

	private void RefreshForeground()
	{
		if (_creature.CurrentHp <= 0)
		{
			_poisonForeground.Visible = false;
			_doomForeground.Visible = false;
			_hpForeground.Visible = false;
			return;
		}
		_hpForeground.Visible = true;
		float offsetRight = GetFgWidth(_creature.CurrentHp) - MaxFgWidth;
		_hpForeground.OffsetRight = offsetRight;
		if (_creature.ShowsInfiniteHp)
		{
			_hpForeground.SelfModulate = _invincibleForegroundColor;
			return;
		}
		int powerAmount = _creature.GetPowerAmount<DoomPower>();
		int num = _creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0;
		if (_creature.HasPower<PoisonPower>())
		{
			if (num > 0)
			{
				_poisonForeground.Visible = true;
				if (IsPoisonLethal(num))
				{
					_poisonForeground.OffsetLeft = 0f;
					_poisonForeground.OffsetRight = offsetRight;
					_hpForeground.Visible = false;
				}
				else
				{
					float fgWidth = GetFgWidth(_creature.CurrentHp - num);
					_hpForeground.OffsetRight = fgWidth - MaxFgWidth;
					_hpForeground.Visible = true;
					int patchMarginLeft = ((NinePatchRect)_poisonForeground).PatchMarginLeft;
					_poisonForeground.OffsetLeft = Math.Max(0f, fgWidth - (float)patchMarginLeft);
					_poisonForeground.OffsetRight = offsetRight;
				}
			}
			else
			{
				_poisonForeground.Visible = false;
			}
		}
		else
		{
			_poisonForeground.Visible = false;
			_poisonForeground.OffsetLeft = 0f;
		}
		if (_creature.HasPower<DoomPower>())
		{
			if (powerAmount > 0)
			{
				_doomForeground.Visible = true;
				float num2 = GetFgWidth(powerAmount) - MaxFgWidth;
				if (IsDoomLethal(powerAmount, num))
				{
					if (!IsPoisonLethal(num))
					{
						_doomForeground.OffsetRight = _hpForeground.OffsetRight;
						_hpForeground.Visible = false;
					}
					else
					{
						_hpForeground.Visible = false;
						_doomForeground.Visible = false;
					}
				}
				else
				{
					int patchMarginRight = ((NinePatchRect)_doomForeground).PatchMarginRight;
					_doomForeground.OffsetRight = Math.Min(0f, num2 + (float)patchMarginRight);
					_hpForeground.Visible = true;
				}
			}
			else
			{
				_doomForeground.Visible = false;
			}
		}
		else
		{
			_doomForeground.Visible = false;
		}
	}

	private void RefreshBlockUi()
	{
		if (_creature.Block <= 0)
		{
			Creature blockTrackingCreature = _blockTrackingCreature;
			if (blockTrackingCreature == null || blockTrackingCreature.Block <= 0)
			{
				if (_blockContainer.Visible)
				{
					NBlockBrokenVfx nBlockBrokenVfx = NBlockBrokenVfx.Create();
					if (nBlockBrokenVfx != null)
					{
						this.AddChildSafely(nBlockBrokenVfx);
						nBlockBrokenVfx.GlobalPosition = _blockContainer.GlobalPosition + _blockContainer.Size * 0.5f;
					}
				}
				_blockContainer.Visible = false;
				_blockOutline.Visible = false;
				_hpForeground.SelfModulate = _redForegroundColor;
				return;
			}
		}
		_blockOutline.Visible = true;
		_hpForeground.SelfModulate = _blockHpForegroundColor;
		if (_creature.Block > 0)
		{
			_blockContainer.Visible = true;
			_blockLabel.SetTextAutoSize(_creature.Block.ToString());
		}
	}

	private void RefreshText()
	{
		if (_creature.CurrentHp <= 0)
		{
			_hpLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, _defaultFontColor);
			_hpLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, _defaultFontOutlineColor);
			_hpLabel.SetTextAutoSize(_healthBarDead.GetRawText());
			return;
		}
		if (_creature.ShowsInfiniteHp)
		{
			_infinityTex.Visible = _creature.IsAlive;
			_doomForeground.Modulate = Colors.Transparent;
			_hpLabel.Visible = !_infinityTex.Visible;
			return;
		}
		_hpLabel.Visible = true;
		int poisonDamage = _creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0;
		int powerAmount = _creature.GetPowerAmount<DoomPower>();
		Color color;
		Color color2;
		if (IsPoisonLethal(poisonDamage))
		{
			color = new Color("76FF40");
			color2 = new Color("074700");
		}
		else if (IsDoomLethal(powerAmount, poisonDamage))
		{
			color = new Color("FB8DFF");
			color2 = new Color("2D1263");
		}
		else
		{
			if (_creature.Block <= 0)
			{
				Creature blockTrackingCreature = _blockTrackingCreature;
				if (blockTrackingCreature == null || blockTrackingCreature.Block <= 0)
				{
					color = _defaultFontColor;
					color2 = _defaultFontOutlineColor;
					goto IL_0151;
				}
			}
			color = _defaultFontColor;
			color2 = _blockOutlineColor;
		}
		goto IL_0151;
		IL_0151:
		_hpLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, color);
		_hpLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, color2);
		_hpLabel.SetTextAutoSize($"{_creature.CurrentHp}/{_creature.MaxHp}");
	}

	private bool IsPoisonLethal(int poisonDamage)
	{
		if (poisonDamage <= 0 || !_creature.HasPower<PoisonPower>())
		{
			return false;
		}
		return poisonDamage >= _creature.CurrentHp;
	}

	private bool IsDoomLethal(int doomAmount, int poisonDamage)
	{
		if (doomAmount <= 0 || !_creature.HasPower<DoomPower>())
		{
			return false;
		}
		return doomAmount >= _creature.CurrentHp - poisonDamage;
	}

	private float GetFgWidth(int amount)
	{
		return GetFgWidth(amount, MaxFgWidth);
	}

	private float GetFgWidth(int amount, float maxFgWidth)
	{
		if (_creature.MaxHp <= 0)
		{
			return 0f;
		}
		float val = (float)amount / (float)_creature.MaxHp * maxFgWidth;
		return Math.Max(val, (_creature.CurrentHp > 0) ? 12f : 0f);
	}

	public void FadeOutHpLabel(float duration, float finalAlpha)
	{
		_hpLabelFadeTween?.Kill();
		_hpLabelFadeTween = CreateTween();
		_hpLabelFadeTween.TweenProperty(_hpLabel, "modulate:a", finalAlpha, duration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void FadeInHpLabel(float duration)
	{
		_hpLabelFadeTween?.Kill();
		_hpLabelFadeTween = CreateTween();
		_hpLabelFadeTween.TweenProperty(_hpLabel, "modulate:a", 1f, duration);
	}

	public void AnimateInBlock(int oldBlock, int blockGain)
	{
		if (oldBlock == 0 && blockGain != 0)
		{
			_blockContainer.Visible = true;
			if (SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
			{
				_blockContainer.Modulate = StsColors.transparentWhite;
				_blockContainer.Position = _originalBlockPosition - _blockAnimOffset;
				_blockTween?.Kill();
				_blockTween = CreateTween().SetParallel();
				_blockTween.TweenProperty(_blockContainer, "modulate:a", 1f, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
				_blockTween.TweenProperty(_blockContainer, "position", _originalBlockPosition, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
			}
		}
	}

	public void TrackBlockStatus(Creature creature)
	{
		_blockTrackingCreature = creature;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(18);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DebugToggleVisibility, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateLayoutForCreatureBounds, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "bounds", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateWidthRelativeToReferenceValue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "refMaxHp", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "refWidth", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetHpBarContainerSizeWithOffsets, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetHpBarContainerSizeWithOffsetsImmediately, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Vector2, "size", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshValues, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshMiddleground, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshForeground, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshBlockUi, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RefreshText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.IsPoisonLethal, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "poisonDamage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.IsDoomLethal, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "doomAmount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "poisonDamage", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetFgWidth, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetFgWidth, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "amount", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "maxFgWidth", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FadeOutHpLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Float, "finalAlpha", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.FadeInHpLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AnimateInBlock, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "oldBlock", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "blockGain", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.DebugToggleVisibility && args.Count == 0)
		{
			DebugToggleVisibility();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateLayoutForCreatureBounds && args.Count == 1)
		{
			UpdateLayoutForCreatureBounds(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateWidthRelativeToReferenceValue && args.Count == 2)
		{
			UpdateWidthRelativeToReferenceValue(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetHpBarContainerSizeWithOffsets && args.Count == 1)
		{
			SetHpBarContainerSizeWithOffsets(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetHpBarContainerSizeWithOffsetsImmediately && args.Count == 1)
		{
			SetHpBarContainerSizeWithOffsetsImmediately(VariantUtils.ConvertTo<Vector2>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshValues && args.Count == 0)
		{
			RefreshValues();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshMiddleground && args.Count == 0)
		{
			RefreshMiddleground();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshForeground && args.Count == 0)
		{
			RefreshForeground();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshBlockUi && args.Count == 0)
		{
			RefreshBlockUi();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshText && args.Count == 0)
		{
			RefreshText();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsPoisonLethal && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<bool>(IsPoisonLethal(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.IsDoomLethal && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(IsDoomLethal(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.GetFgWidth && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetFgWidth(VariantUtils.ConvertTo<int>(in args[0])));
			return true;
		}
		if (method == MethodName.GetFgWidth && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<float>(GetFgWidth(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<float>(in args[1])));
			return true;
		}
		if (method == MethodName.FadeOutHpLabel && args.Count == 2)
		{
			FadeOutHpLabel(VariantUtils.ConvertTo<float>(in args[0]), VariantUtils.ConvertTo<float>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.FadeInHpLabel && args.Count == 1)
		{
			FadeInHpLabel(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimateInBlock && args.Count == 2)
		{
			AnimateInBlock(VariantUtils.ConvertTo<int>(in args[0]), VariantUtils.ConvertTo<int>(in args[1]));
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
		if (method == MethodName.DebugToggleVisibility)
		{
			return true;
		}
		if (method == MethodName.UpdateLayoutForCreatureBounds)
		{
			return true;
		}
		if (method == MethodName.UpdateWidthRelativeToReferenceValue)
		{
			return true;
		}
		if (method == MethodName.SetHpBarContainerSizeWithOffsets)
		{
			return true;
		}
		if (method == MethodName.SetHpBarContainerSizeWithOffsetsImmediately)
		{
			return true;
		}
		if (method == MethodName.RefreshValues)
		{
			return true;
		}
		if (method == MethodName.RefreshMiddleground)
		{
			return true;
		}
		if (method == MethodName.RefreshForeground)
		{
			return true;
		}
		if (method == MethodName.RefreshBlockUi)
		{
			return true;
		}
		if (method == MethodName.RefreshText)
		{
			return true;
		}
		if (method == MethodName.IsPoisonLethal)
		{
			return true;
		}
		if (method == MethodName.IsDoomLethal)
		{
			return true;
		}
		if (method == MethodName.GetFgWidth)
		{
			return true;
		}
		if (method == MethodName.FadeOutHpLabel)
		{
			return true;
		}
		if (method == MethodName.FadeInHpLabel)
		{
			return true;
		}
		if (method == MethodName.AnimateInBlock)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.HpBarContainer)
		{
			HpBarContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hpForegroundContainer)
		{
			_hpForegroundContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hpForeground)
		{
			_hpForeground = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._poisonForeground)
		{
			_poisonForeground = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._doomForeground)
		{
			_doomForeground = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hpMiddleground)
		{
			_hpMiddleground = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._hpLabel)
		{
			_hpLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._blockContainer)
		{
			_blockContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._blockLabel)
		{
			_blockLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._blockOutline)
		{
			_blockOutline = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._infinityTex)
		{
			_infinityTex = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._blockTween)
		{
			_blockTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hpLabelFadeTween)
		{
			_hpLabelFadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._middlegroundTween)
		{
			_middlegroundTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._originalBlockPosition)
		{
			_originalBlockPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._currentHpOnLastRefresh)
		{
			_currentHpOnLastRefresh = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._maxHpOnLastRefresh)
		{
			_maxHpOnLastRefresh = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._expectedMaxFgWidth)
		{
			_expectedMaxFgWidth = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.MaxFgWidth)
		{
			value = VariantUtils.CreateFrom<float>(MaxFgWidth);
			return true;
		}
		if (name == PropertyName.HpBarContainer)
		{
			value = VariantUtils.CreateFrom<Control>(HpBarContainer);
			return true;
		}
		if (name == PropertyName._hpForegroundContainer)
		{
			value = VariantUtils.CreateFrom(in _hpForegroundContainer);
			return true;
		}
		if (name == PropertyName._hpForeground)
		{
			value = VariantUtils.CreateFrom(in _hpForeground);
			return true;
		}
		if (name == PropertyName._poisonForeground)
		{
			value = VariantUtils.CreateFrom(in _poisonForeground);
			return true;
		}
		if (name == PropertyName._doomForeground)
		{
			value = VariantUtils.CreateFrom(in _doomForeground);
			return true;
		}
		if (name == PropertyName._hpMiddleground)
		{
			value = VariantUtils.CreateFrom(in _hpMiddleground);
			return true;
		}
		if (name == PropertyName._hpLabel)
		{
			value = VariantUtils.CreateFrom(in _hpLabel);
			return true;
		}
		if (name == PropertyName._blockContainer)
		{
			value = VariantUtils.CreateFrom(in _blockContainer);
			return true;
		}
		if (name == PropertyName._blockLabel)
		{
			value = VariantUtils.CreateFrom(in _blockLabel);
			return true;
		}
		if (name == PropertyName._blockOutline)
		{
			value = VariantUtils.CreateFrom(in _blockOutline);
			return true;
		}
		if (name == PropertyName._infinityTex)
		{
			value = VariantUtils.CreateFrom(in _infinityTex);
			return true;
		}
		if (name == PropertyName._blockTween)
		{
			value = VariantUtils.CreateFrom(in _blockTween);
			return true;
		}
		if (name == PropertyName._hpLabelFadeTween)
		{
			value = VariantUtils.CreateFrom(in _hpLabelFadeTween);
			return true;
		}
		if (name == PropertyName._middlegroundTween)
		{
			value = VariantUtils.CreateFrom(in _middlegroundTween);
			return true;
		}
		if (name == PropertyName._originalBlockPosition)
		{
			value = VariantUtils.CreateFrom(in _originalBlockPosition);
			return true;
		}
		if (name == PropertyName._currentHpOnLastRefresh)
		{
			value = VariantUtils.CreateFrom(in _currentHpOnLastRefresh);
			return true;
		}
		if (name == PropertyName._maxHpOnLastRefresh)
		{
			value = VariantUtils.CreateFrom(in _maxHpOnLastRefresh);
			return true;
		}
		if (name == PropertyName._expectedMaxFgWidth)
		{
			value = VariantUtils.CreateFrom(in _expectedMaxFgWidth);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpForegroundContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpForeground, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._poisonForeground, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._doomForeground, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpMiddleground, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blockContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blockLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blockOutline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._infinityTex, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._blockTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hpLabelFadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._middlegroundTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._originalBlockPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._currentHpOnLastRefresh, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._maxHpOnLastRefresh, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._expectedMaxFgWidth, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.MaxFgWidth, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.HpBarContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.HpBarContainer, Variant.From<Control>(HpBarContainer));
		info.AddProperty(PropertyName._hpForegroundContainer, Variant.From(in _hpForegroundContainer));
		info.AddProperty(PropertyName._hpForeground, Variant.From(in _hpForeground));
		info.AddProperty(PropertyName._poisonForeground, Variant.From(in _poisonForeground));
		info.AddProperty(PropertyName._doomForeground, Variant.From(in _doomForeground));
		info.AddProperty(PropertyName._hpMiddleground, Variant.From(in _hpMiddleground));
		info.AddProperty(PropertyName._hpLabel, Variant.From(in _hpLabel));
		info.AddProperty(PropertyName._blockContainer, Variant.From(in _blockContainer));
		info.AddProperty(PropertyName._blockLabel, Variant.From(in _blockLabel));
		info.AddProperty(PropertyName._blockOutline, Variant.From(in _blockOutline));
		info.AddProperty(PropertyName._infinityTex, Variant.From(in _infinityTex));
		info.AddProperty(PropertyName._blockTween, Variant.From(in _blockTween));
		info.AddProperty(PropertyName._hpLabelFadeTween, Variant.From(in _hpLabelFadeTween));
		info.AddProperty(PropertyName._middlegroundTween, Variant.From(in _middlegroundTween));
		info.AddProperty(PropertyName._originalBlockPosition, Variant.From(in _originalBlockPosition));
		info.AddProperty(PropertyName._currentHpOnLastRefresh, Variant.From(in _currentHpOnLastRefresh));
		info.AddProperty(PropertyName._maxHpOnLastRefresh, Variant.From(in _maxHpOnLastRefresh));
		info.AddProperty(PropertyName._expectedMaxFgWidth, Variant.From(in _expectedMaxFgWidth));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.HpBarContainer, out var value))
		{
			HpBarContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hpForegroundContainer, out var value2))
		{
			_hpForegroundContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hpForeground, out var value3))
		{
			_hpForeground = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._poisonForeground, out var value4))
		{
			_poisonForeground = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._doomForeground, out var value5))
		{
			_doomForeground = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hpMiddleground, out var value6))
		{
			_hpMiddleground = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._hpLabel, out var value7))
		{
			_hpLabel = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._blockContainer, out var value8))
		{
			_blockContainer = value8.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._blockLabel, out var value9))
		{
			_blockLabel = value9.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._blockOutline, out var value10))
		{
			_blockOutline = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._infinityTex, out var value11))
		{
			_infinityTex = value11.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._blockTween, out var value12))
		{
			_blockTween = value12.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hpLabelFadeTween, out var value13))
		{
			_hpLabelFadeTween = value13.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._middlegroundTween, out var value14))
		{
			_middlegroundTween = value14.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._originalBlockPosition, out var value15))
		{
			_originalBlockPosition = value15.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._currentHpOnLastRefresh, out var value16))
		{
			_currentHpOnLastRefresh = value16.As<int>();
		}
		if (info.TryGetProperty(PropertyName._maxHpOnLastRefresh, out var value17))
		{
			_maxHpOnLastRefresh = value17.As<int>();
		}
		if (info.TryGetProperty(PropertyName._expectedMaxFgWidth, out var value18))
		{
			_expectedMaxFgWidth = value18.As<float>();
		}
	}
}
