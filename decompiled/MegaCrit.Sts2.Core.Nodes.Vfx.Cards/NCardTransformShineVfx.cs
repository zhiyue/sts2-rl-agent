using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

[ScriptPath("res://src/Core/Nodes/Vfx/Cards/NCardTransformShineVfx.cs")]
public class NCardTransformShineVfx : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'GetEffectiveDuration' method.
		/// </summary>
		public static readonly StringName GetEffectiveDuration = "GetEffectiveDuration";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the '_overlay' field.
		/// </summary>
		public static readonly StringName _overlay = "_overlay";

		/// <summary>
		/// Cached name for the '_borderGlow' field.
		/// </summary>
		public static readonly StringName _borderGlow = "_borderGlow";

		/// <summary>
		/// Cached name for the '_revealParticles' field.
		/// </summary>
		public static readonly StringName _revealParticles = "_revealParticles";

		/// <summary>
		/// Cached name for the '_shineParticles' field.
		/// </summary>
		public static readonly StringName _shineParticles = "_shineParticles";

		/// <summary>
		/// Cached name for the '_endParticles' field.
		/// </summary>
		public static readonly StringName _endParticles = "_endParticles";

		/// <summary>
		/// Cached name for the '_anticipationScaleCurve' field.
		/// </summary>
		public static readonly StringName _anticipationScaleCurve = "_anticipationScaleCurve";

		/// <summary>
		/// Cached name for the '_revealScaleCurve' field.
		/// </summary>
		public static readonly StringName _revealScaleCurve = "_revealScaleCurve";

		/// <summary>
		/// Cached name for the '_overlayShowDuration' field.
		/// </summary>
		public static readonly StringName _overlayShowDuration = "_overlayShowDuration";

		/// <summary>
		/// Cached name for the '_overlayShowShortDuration' field.
		/// </summary>
		public static readonly StringName _overlayShowShortDuration = "_overlayShowShortDuration";

		/// <summary>
		/// Cached name for the '_overlayIdleDuration' field.
		/// </summary>
		public static readonly StringName _overlayIdleDuration = "_overlayIdleDuration";

		/// <summary>
		/// Cached name for the '_overlayIdleShortDuration' field.
		/// </summary>
		public static readonly StringName _overlayIdleShortDuration = "_overlayIdleShortDuration";

		/// <summary>
		/// Cached name for the '_overlayHideDuration' field.
		/// </summary>
		public static readonly StringName _overlayHideDuration = "_overlayHideDuration";

		/// <summary>
		/// Cached name for the '_glowFadeDuration' field.
		/// </summary>
		public static readonly StringName _glowFadeDuration = "_glowFadeDuration";

		/// <summary>
		/// Cached name for the '_glowTopScale' field.
		/// </summary>
		public static readonly StringName _glowTopScale = "_glowTopScale";

		/// <summary>
		/// Cached name for the '_shineDelay' field.
		/// </summary>
		public static readonly StringName _shineDelay = "_shineDelay";

		/// <summary>
		/// Cached name for the '_endParticlesDelay' field.
		/// </summary>
		public static readonly StringName _endParticlesDelay = "_endParticlesDelay";

		/// <summary>
		/// Cached name for the '_cardNode' field.
		/// </summary>
		public static readonly StringName _cardNode = "_cardNode";

		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";

		/// <summary>
		/// Cached name for the '_whiteOpaque' field.
		/// </summary>
		public static readonly StringName _whiteOpaque = "_whiteOpaque";

		/// <summary>
		/// Cached name for the '_whiteClear' field.
		/// </summary>
		public static readonly StringName _whiteClear = "_whiteClear";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	public static readonly string scenePath = SceneHelper.GetScenePath("vfx/ui/card/vfx_card_transform");

	[Export(PropertyHint.None, "")]
	private Control _overlay;

	[Export(PropertyHint.None, "")]
	private Control _borderGlow;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _revealParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _shineParticles;

	[Export(PropertyHint.None, "")]
	private NParticlesContainer _endParticles;

	[Export(PropertyHint.None, "")]
	private CurveXyzTexture _anticipationScaleCurve;

	[Export(PropertyHint.None, "")]
	private CurveXyzTexture _revealScaleCurve;

	[Export(PropertyHint.None, "")]
	private float _overlayShowDuration = 0.75f;

	[Export(PropertyHint.None, "")]
	private float _overlayShowShortDuration = 0.75f;

	[Export(PropertyHint.None, "")]
	private float _overlayIdleDuration = 0.125f;

	[Export(PropertyHint.None, "")]
	private float _overlayIdleShortDuration = 0.75f;

	[Export(PropertyHint.None, "")]
	private float _overlayHideDuration = 0.25f;

	[Export(PropertyHint.None, "")]
	private float _glowFadeDuration = 0.25f;

	[Export(PropertyHint.None, "")]
	private float _glowTopScale = 1.5f;

	[Export(PropertyHint.None, "")]
	private float _shineDelay = 0.125f;

	[Export(PropertyHint.None, "")]
	private float _endParticlesDelay = 0.4f;

	private NCard _cardNode;

	private CardModel _endCard;

	private IEnumerable<RelicModel>? _relicsToFlash;

	private Tween? _tween;

	private Color _whiteOpaque = new Color(1f, 1f, 1f);

	private Color _whiteClear = new Color(1f, 1f, 1f, 0f);

	private static Vector2 _originalCardScale = new Vector2(1f, 1f);

	public static NCardTransformShineVfx? Create(NCard cardNode, CardModel endCard, IEnumerable<RelicModel>? relicsToFlash)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardTransformShineVfx nCardTransformShineVfx = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCardTransformShineVfx>(PackedScene.GenEditState.Disabled);
		nCardTransformShineVfx._cardNode = cardNode;
		nCardTransformShineVfx._endCard = endCard;
		nCardTransformShineVfx._relicsToFlash = relicsToFlash;
		cardNode.CardVfxContainer.AddChildSafely(nCardTransformShineVfx);
		return nCardTransformShineVfx;
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task<bool> WaitAndInterruptIfNecessary(float seconds, NCard cardNode)
	{
		float num = 0f;
		while (num <= seconds)
		{
			if (!cardNode.IsInsideTree() || _endCard.Pile == null)
			{
				return false;
			}
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		return true;
	}

	private static void UpdateCard(NCard cardNode, CardModel endCard)
	{
		if (endCard.Pile != null)
		{
			NPlayerHand.Instance?.TryCancelCardPlay(cardNode.Model);
			cardNode.Model = endCard;
			cardNode.UpdateVisuals(endCard.Pile.Type, CardPreviewMode.Normal);
			if (NCombatRoom.Instance?.Ui.Hand.GetCardHolder(endCard) is NHandCardHolder nHandCardHolder)
			{
				nHandCardHolder.UpdateCard();
			}
		}
	}

	public float GetEffectiveDuration(bool shortVersion = false)
	{
		float num = (shortVersion ? _overlayShowShortDuration : _overlayShowDuration);
		float num2 = (shortVersion ? _overlayIdleShortDuration : _overlayIdleDuration);
		return num + num2;
	}

	public async Task PlayAnimation(bool shortVersion = false)
	{
		_overlay.SelfModulate = _whiteClear;
		_borderGlow.SelfModulate = _whiteClear;
		float num = (shortVersion ? _overlayShowShortDuration : _overlayShowDuration);
		float num2 = (shortVersion ? _overlayIdleShortDuration : _overlayIdleDuration);
		TaskHelper.RunSafely(AnimatingCardScale(_anticipationScaleCurve, num + num2));
		_tween = CreateTween();
		_tween.TweenProperty(_overlay, "self_modulate", _whiteOpaque, num).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quart);
		if (!(await WaitAndInterruptIfNecessary(num + num2, _cardNode)))
		{
			_cardNode.Scale = _originalCardScale;
			this.QueueFreeSafely();
			return;
		}
		UpdateCard(_cardNode, _endCard);
		TaskHelper.RunSafely(AnimatingCardScale(_revealScaleCurve, _glowFadeDuration));
		_borderGlow.SelfModulate = _whiteOpaque;
		_borderGlow.Scale = Vector2.One;
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_overlay, "self_modulate", _whiteClear, _overlayHideDuration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart);
		_tween.TweenProperty(_borderGlow, "scale", Vector2.One * _glowTopScale, _glowFadeDuration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);
		_tween.TweenProperty(_borderGlow, "self_modulate", _whiteClear, _glowFadeDuration).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);
		if (_relicsToFlash != null)
		{
			foreach (RelicModel item in _relicsToFlash)
			{
				item.Flash();
				_cardNode.FlashRelicOnCard(item);
			}
		}
		_revealParticles.Restart();
		if (!(await WaitAndInterruptIfNecessary(_shineDelay, _cardNode)))
		{
			_cardNode.Scale = _originalCardScale;
			this.QueueFreeSafely();
			return;
		}
		_shineParticles.Restart();
		if (!(await WaitAndInterruptIfNecessary(_endParticlesDelay, _cardNode)))
		{
			_cardNode.Scale = _originalCardScale;
			this.QueueFreeSafely();
		}
		else
		{
			_endParticles.Restart();
			TaskHelper.RunSafely(DelayedFree());
		}
	}

	private async Task DelayedFree()
	{
		await Cmd.Wait(2f);
		this.QueueFreeSafely();
	}

	private async Task AnimatingCardScale(CurveXyzTexture curve, float duration)
	{
		float num = 0f;
		Vector2 one = Vector2.One;
		while (num < duration)
		{
			float offset = num / duration;
			float x = curve.CurveX.Sample(offset);
			float y = curve.CurveY.Sample(offset);
			one.X = x;
			one.Y = y;
			_cardNode.Scale = _originalCardScale * one;
			float num2 = num;
			num = num2 + await this.AwaitProcessFrame();
		}
		one.X = curve.CurveX.Sample(1f);
		one.Y = curve.CurveY.Sample(1f);
		_cardNode.Scale = _originalCardScale * one;
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetEffectiveDuration, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "shortVersion", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetEffectiveDuration && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(GetEffectiveDuration(VariantUtils.ConvertTo<bool>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.GetEffectiveDuration)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._overlay)
		{
			_overlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._borderGlow)
		{
			_borderGlow = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._revealParticles)
		{
			_revealParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._shineParticles)
		{
			_shineParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			_endParticles = VariantUtils.ConvertTo<NParticlesContainer>(in value);
			return true;
		}
		if (name == PropertyName._anticipationScaleCurve)
		{
			_anticipationScaleCurve = VariantUtils.ConvertTo<CurveXyzTexture>(in value);
			return true;
		}
		if (name == PropertyName._revealScaleCurve)
		{
			_revealScaleCurve = VariantUtils.ConvertTo<CurveXyzTexture>(in value);
			return true;
		}
		if (name == PropertyName._overlayShowDuration)
		{
			_overlayShowDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._overlayShowShortDuration)
		{
			_overlayShowShortDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._overlayIdleDuration)
		{
			_overlayIdleDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._overlayIdleShortDuration)
		{
			_overlayIdleShortDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._overlayHideDuration)
		{
			_overlayHideDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._glowFadeDuration)
		{
			_glowFadeDuration = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._glowTopScale)
		{
			_glowTopScale = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._shineDelay)
		{
			_shineDelay = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._endParticlesDelay)
		{
			_endParticlesDelay = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._whiteOpaque)
		{
			_whiteOpaque = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		if (name == PropertyName._whiteClear)
		{
			_whiteClear = VariantUtils.ConvertTo<Color>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._overlay)
		{
			value = VariantUtils.CreateFrom(in _overlay);
			return true;
		}
		if (name == PropertyName._borderGlow)
		{
			value = VariantUtils.CreateFrom(in _borderGlow);
			return true;
		}
		if (name == PropertyName._revealParticles)
		{
			value = VariantUtils.CreateFrom(in _revealParticles);
			return true;
		}
		if (name == PropertyName._shineParticles)
		{
			value = VariantUtils.CreateFrom(in _shineParticles);
			return true;
		}
		if (name == PropertyName._endParticles)
		{
			value = VariantUtils.CreateFrom(in _endParticles);
			return true;
		}
		if (name == PropertyName._anticipationScaleCurve)
		{
			value = VariantUtils.CreateFrom(in _anticipationScaleCurve);
			return true;
		}
		if (name == PropertyName._revealScaleCurve)
		{
			value = VariantUtils.CreateFrom(in _revealScaleCurve);
			return true;
		}
		if (name == PropertyName._overlayShowDuration)
		{
			value = VariantUtils.CreateFrom(in _overlayShowDuration);
			return true;
		}
		if (name == PropertyName._overlayShowShortDuration)
		{
			value = VariantUtils.CreateFrom(in _overlayShowShortDuration);
			return true;
		}
		if (name == PropertyName._overlayIdleDuration)
		{
			value = VariantUtils.CreateFrom(in _overlayIdleDuration);
			return true;
		}
		if (name == PropertyName._overlayIdleShortDuration)
		{
			value = VariantUtils.CreateFrom(in _overlayIdleShortDuration);
			return true;
		}
		if (name == PropertyName._overlayHideDuration)
		{
			value = VariantUtils.CreateFrom(in _overlayHideDuration);
			return true;
		}
		if (name == PropertyName._glowFadeDuration)
		{
			value = VariantUtils.CreateFrom(in _glowFadeDuration);
			return true;
		}
		if (name == PropertyName._glowTopScale)
		{
			value = VariantUtils.CreateFrom(in _glowTopScale);
			return true;
		}
		if (name == PropertyName._shineDelay)
		{
			value = VariantUtils.CreateFrom(in _shineDelay);
			return true;
		}
		if (name == PropertyName._endParticlesDelay)
		{
			value = VariantUtils.CreateFrom(in _endParticlesDelay);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._whiteOpaque)
		{
			value = VariantUtils.CreateFrom(in _whiteOpaque);
			return true;
		}
		if (name == PropertyName._whiteClear)
		{
			value = VariantUtils.CreateFrom(in _whiteClear);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overlay, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._borderGlow, PropertyHint.NodeType, "Control", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._revealParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._shineParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._endParticles, PropertyHint.NodeType, "Node2D", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._anticipationScaleCurve, PropertyHint.ResourceType, "CurveXYZTexture", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._revealScaleCurve, PropertyHint.ResourceType, "CurveXYZTexture", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._overlayShowDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._overlayShowShortDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._overlayIdleDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._overlayIdleShortDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._overlayHideDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._glowFadeDuration, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._glowTopScale, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._shineDelay, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._endParticlesDelay, PropertyHint.None, "", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._whiteOpaque, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Color, PropertyName._whiteClear, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._overlay, Variant.From(in _overlay));
		info.AddProperty(PropertyName._borderGlow, Variant.From(in _borderGlow));
		info.AddProperty(PropertyName._revealParticles, Variant.From(in _revealParticles));
		info.AddProperty(PropertyName._shineParticles, Variant.From(in _shineParticles));
		info.AddProperty(PropertyName._endParticles, Variant.From(in _endParticles));
		info.AddProperty(PropertyName._anticipationScaleCurve, Variant.From(in _anticipationScaleCurve));
		info.AddProperty(PropertyName._revealScaleCurve, Variant.From(in _revealScaleCurve));
		info.AddProperty(PropertyName._overlayShowDuration, Variant.From(in _overlayShowDuration));
		info.AddProperty(PropertyName._overlayShowShortDuration, Variant.From(in _overlayShowShortDuration));
		info.AddProperty(PropertyName._overlayIdleDuration, Variant.From(in _overlayIdleDuration));
		info.AddProperty(PropertyName._overlayIdleShortDuration, Variant.From(in _overlayIdleShortDuration));
		info.AddProperty(PropertyName._overlayHideDuration, Variant.From(in _overlayHideDuration));
		info.AddProperty(PropertyName._glowFadeDuration, Variant.From(in _glowFadeDuration));
		info.AddProperty(PropertyName._glowTopScale, Variant.From(in _glowTopScale));
		info.AddProperty(PropertyName._shineDelay, Variant.From(in _shineDelay));
		info.AddProperty(PropertyName._endParticlesDelay, Variant.From(in _endParticlesDelay));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._whiteOpaque, Variant.From(in _whiteOpaque));
		info.AddProperty(PropertyName._whiteClear, Variant.From(in _whiteClear));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._overlay, out var value))
		{
			_overlay = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._borderGlow, out var value2))
		{
			_borderGlow = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._revealParticles, out var value3))
		{
			_revealParticles = value3.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._shineParticles, out var value4))
		{
			_shineParticles = value4.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._endParticles, out var value5))
		{
			_endParticles = value5.As<NParticlesContainer>();
		}
		if (info.TryGetProperty(PropertyName._anticipationScaleCurve, out var value6))
		{
			_anticipationScaleCurve = value6.As<CurveXyzTexture>();
		}
		if (info.TryGetProperty(PropertyName._revealScaleCurve, out var value7))
		{
			_revealScaleCurve = value7.As<CurveXyzTexture>();
		}
		if (info.TryGetProperty(PropertyName._overlayShowDuration, out var value8))
		{
			_overlayShowDuration = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._overlayShowShortDuration, out var value9))
		{
			_overlayShowShortDuration = value9.As<float>();
		}
		if (info.TryGetProperty(PropertyName._overlayIdleDuration, out var value10))
		{
			_overlayIdleDuration = value10.As<float>();
		}
		if (info.TryGetProperty(PropertyName._overlayIdleShortDuration, out var value11))
		{
			_overlayIdleShortDuration = value11.As<float>();
		}
		if (info.TryGetProperty(PropertyName._overlayHideDuration, out var value12))
		{
			_overlayHideDuration = value12.As<float>();
		}
		if (info.TryGetProperty(PropertyName._glowFadeDuration, out var value13))
		{
			_glowFadeDuration = value13.As<float>();
		}
		if (info.TryGetProperty(PropertyName._glowTopScale, out var value14))
		{
			_glowTopScale = value14.As<float>();
		}
		if (info.TryGetProperty(PropertyName._shineDelay, out var value15))
		{
			_shineDelay = value15.As<float>();
		}
		if (info.TryGetProperty(PropertyName._endParticlesDelay, out var value16))
		{
			_endParticlesDelay = value16.As<float>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value17))
		{
			_cardNode = value17.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value18))
		{
			_tween = value18.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._whiteOpaque, out var value19))
		{
			_whiteOpaque = value19.As<Color>();
		}
		if (info.TryGetProperty(PropertyName._whiteClear, out var value20))
		{
			_whiteClear = value20.As<Color>();
		}
	}
}
