using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Helpers.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Pooling;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

[ScriptPath("res://src/Core/Nodes/Cards/NCard.cs")]
public class NCard : Control, IPoolable
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName OnInstantiated = "OnInstantiated";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName InitPool = "InitPool";

		public static readonly StringName GetCurrentSize = "GetCurrentSize";

		public static readonly StringName UpdateVisuals = "UpdateVisuals";

		public static readonly StringName ShowUpgradePreview = "ShowUpgradePreview";

		public static readonly StringName UpdateEnchantmentVisuals = "UpdateEnchantmentVisuals";

		public static readonly StringName OnEnchantmentStatusChanged = "OnEnchantmentStatusChanged";

		public static readonly StringName SetEnchantmentStatus = "SetEnchantmentStatus";

		public static readonly StringName UpdateEnergyCostVisuals = "UpdateEnergyCostVisuals";

		public static readonly StringName SetPretendCardCanBePlayed = "SetPretendCardCanBePlayed";

		public static readonly StringName SetForceUnpoweredPreview = "SetForceUnpoweredPreview";

		public static readonly StringName UpdateEnergyCostColor = "UpdateEnergyCostColor";

		public static readonly StringName UpdateStarCostVisuals = "UpdateStarCostVisuals";

		public static readonly StringName UpdateStarCostText = "UpdateStarCostText";

		public static readonly StringName UpdateStarCostColor = "UpdateStarCostColor";

		public static readonly StringName GetCostTextColorInHand = "GetCostTextColorInHand";

		public static readonly StringName GetCostOutlineColorInHand = "GetCostOutlineColorInHand";

		public static readonly StringName PlayRandomizeCostAnim = "PlayRandomizeCostAnim";

		public static readonly StringName Reload = "Reload";

		public static readonly StringName UpdateTypePlaque = "UpdateTypePlaque";

		public static readonly StringName UpdateTypePlaqueSizeAndPosition = "UpdateTypePlaqueSizeAndPosition";

		public static readonly StringName UpdateTitleLabel = "UpdateTitleLabel";

		public static readonly StringName GetTitleLabelOutlineColor = "GetTitleLabelOutlineColor";

		public static readonly StringName ReloadOverlay = "ReloadOverlay";

		public static readonly StringName OnAfflictionChanged = "OnAfflictionChanged";

		public static readonly StringName OnEnchantmentChanged = "OnEnchantmentChanged";

		public static readonly StringName GetTitleText = "GetTitleText";

		public static readonly StringName ActivateRewardScreenGlow = "ActivateRewardScreenGlow";

		public static readonly StringName KillRarityGlow = "KillRarityGlow";

		public static readonly StringName AnimCardToPlayPile = "AnimCardToPlayPile";

		public static readonly StringName OnReturnedFromPool = "OnReturnedFromPool";

		public static readonly StringName OnFreedToPool = "OnFreedToPool";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CardHighlight = "CardHighlight";

		public static readonly StringName Body = "Body";

		public static readonly StringName Visibility = "Visibility";

		public static readonly StringName PlayPileTween = "PlayPileTween";

		public static readonly StringName RandomizeCostTween = "RandomizeCostTween";

		public static readonly StringName DisplayingPile = "DisplayingPile";

		public static readonly StringName EnchantmentTab = "EnchantmentTab";

		public static readonly StringName EnchantmentVfxOverride = "EnchantmentVfxOverride";

		public static readonly StringName _titleLabel = "_titleLabel";

		public static readonly StringName _descriptionLabel = "_descriptionLabel";

		public static readonly StringName _ancientPortrait = "_ancientPortrait";

		public static readonly StringName _portrait = "_portrait";

		public static readonly StringName _frame = "_frame";

		public static readonly StringName _ancientBorder = "_ancientBorder";

		public static readonly StringName _ancientBanner = "_ancientBanner";

		public static readonly StringName _ancientTextBg = "_ancientTextBg";

		public static readonly StringName _ancientHighlight = "_ancientHighlight";

		public static readonly StringName _portraitBorder = "_portraitBorder";

		public static readonly StringName _banner = "_banner";

		public static readonly StringName _lock = "_lock";

		public static readonly StringName _typePlaque = "_typePlaque";

		public static readonly StringName _typeLabel = "_typeLabel";

		public static readonly StringName _portraitCanvasGroup = "_portraitCanvasGroup";

		public static readonly StringName _rareGlow = "_rareGlow";

		public static readonly StringName _uncommonGlow = "_uncommonGlow";

		public static readonly StringName _sparkles = "_sparkles";

		public static readonly StringName _energyIcon = "_energyIcon";

		public static readonly StringName _energyLabel = "_energyLabel";

		public static readonly StringName _unplayableEnergyIcon = "_unplayableEnergyIcon";

		public static readonly StringName _starIcon = "_starIcon";

		public static readonly StringName _starLabel = "_starLabel";

		public static readonly StringName _unplayableStarIcon = "_unplayableStarIcon";

		public static readonly StringName _overlayContainer = "_overlayContainer";

		public static readonly StringName _cardOverlay = "_cardOverlay";

		public static readonly StringName _enchantmentTab = "_enchantmentTab";

		public static readonly StringName _enchantmentVfxOverride = "_enchantmentVfxOverride";

		public static readonly StringName _enchantmentIcon = "_enchantmentIcon";

		public static readonly StringName _enchantmentLabel = "_enchantmentLabel";

		public static readonly StringName _defaultEnchantmentPosition = "_defaultEnchantmentPosition";

		public static readonly StringName _pretendCardCanBePlayed = "_pretendCardCanBePlayed";

		public static readonly StringName _forceUnpoweredPreview = "_forceUnpoweredPreview";

		public static readonly StringName _portraitBlurMaterial = "_portraitBlurMaterial";

		public static readonly StringName _canvasGroupMaskBlurMaterial = "_canvasGroupMaskBlurMaterial";

		public static readonly StringName _canvasGroupBlurMaterial = "_canvasGroupBlurMaterial";

		public static readonly StringName _canvasGroupMaskMaterial = "_canvasGroupMaskMaterial";

		public static readonly StringName _visibility = "_visibility";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/cards/card.tscn";

	private static readonly string _portraitBlurMaterialPath = "res://scenes/cards/card_portrait_blur_material.tres";

	private static readonly string _canvasGroupMaskMaterialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";

	private static readonly string _canvasGroupBlurMaterialPath = "res://scenes/cards/card_canvas_group_blur_material.tres";

	private static readonly string _canvasGroupMaskBlurMaterialPath = "res://scenes/cards/card_canvas_group_mask_blur_material.tres";

	private static readonly float _typePlaqueXMargin = 17f;

	private static readonly float _typePlaqueMinXSize = 61f;

	private static readonly StringName _v = new StringName("v");

	private static readonly StringName _s = new StringName("s");

	private static readonly StringName _h = new StringName("h");

	public static readonly Vector2 defaultSize = new Vector2(300f, 422f);

	private CardModel? _model;

	private MegaLabel _titleLabel;

	private MegaRichTextLabel _descriptionLabel;

	private TextureRect _ancientPortrait;

	private TextureRect _portrait;

	private TextureRect _frame;

	private TextureRect _ancientBorder;

	private Control _ancientBanner;

	private TextureRect _ancientTextBg;

	private TextureRect _ancientHighlight;

	private TextureRect _portraitBorder;

	private TextureRect _banner;

	private TextureRect _lock;

	private NinePatchRect _typePlaque;

	private MegaLabel _typeLabel;

	private CanvasGroup _portraitCanvasGroup;

	private NCardRareGlow? _rareGlow;

	private NCardUncommonGlow? _uncommonGlow;

	private GpuParticles2D _sparkles;

	private readonly List<NRelicFlashVfx> _flashVfx = new List<NRelicFlashVfx>();

	private TextureRect _energyIcon;

	private MegaLabel _energyLabel;

	private TextureRect _unplayableEnergyIcon;

	private TextureRect _starIcon;

	private MegaLabel _starLabel;

	private TextureRect _unplayableStarIcon;

	private Node _overlayContainer;

	private Control? _cardOverlay;

	private Creature? _previewTarget;

	private Control _enchantmentTab;

	private TextureRect _enchantmentVfxOverride;

	private TextureRect _enchantmentIcon;

	private MegaLabel _enchantmentLabel;

	private Vector2 _defaultEnchantmentPosition;

	private const int _enchantmentTabStarLabelOffset = 45;

	private EnchantmentModel? _subscribedEnchantment;

	private bool _pretendCardCanBePlayed;

	private bool _forceUnpoweredPreview;

	private Material? _portraitBlurMaterial;

	private Material? _canvasGroupMaskBlurMaterial;

	private Material? _canvasGroupBlurMaterial;

	private Material? _canvasGroupMaskMaterial;

	private ModelVisibility _visibility = ModelVisibility.Visible;

	private readonly LocString _unknownDescription = new LocString("card_library", "UNKNOWN.description");

	private readonly LocString _unknownTitle = new LocString("card_library", "UNKNOWN.title");

	private readonly LocString _lockedDescription = new LocString("card_library", "LOCKED.description");

	private readonly LocString _lockedTitle = new LocString("card_library", "LOCKED.title");

	public NCardHighlight CardHighlight { get; private set; }

	public Control Body { get; private set; }

	public ModelVisibility Visibility
	{
		get
		{
			return _visibility;
		}
		set
		{
			if (_visibility != value)
			{
				_visibility = value;
				Reload();
			}
		}
	}

	public Tween? PlayPileTween { get; set; }

	private Tween? RandomizeCostTween { get; set; }

	public PileType DisplayingPile { get; private set; }

	public Control EnchantmentTab => _enchantmentTab;

	public TextureRect EnchantmentVfxOverride => _enchantmentVfxOverride;

	public CardModel? Model
	{
		get
		{
			return _model;
		}
		set
		{
			if (_model != value)
			{
				CardModel model = _model;
				UnsubscribeFromModel(model);
				_model = value;
				Reload();
				SubscribeToModel(_model);
				this.ModelChanged?.Invoke(model);
				if (_model != null && (_model.RunState != null || _model.CombatState != null) && LocalContext.IsMine(_model))
				{
					SaveManager.Instance.MarkCardAsSeen(_model);
				}
			}
		}
	}

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "res://scenes/cards/card.tscn", _portraitBlurMaterialPath, _canvasGroupBlurMaterialPath, _canvasGroupMaskBlurMaterialPath, _canvasGroupMaskMaterialPath });

	public event Action<CardModel?>? ModelChanged;

	public void OnInstantiated()
	{
	}

	public override void _Ready()
	{
		_titleLabel = GetNode<MegaLabel>("%TitleLabel");
		_descriptionLabel = GetNode<MegaRichTextLabel>("%DescriptionLabel");
		_frame = GetNode<TextureRect>("%Frame");
		_ancientBorder = GetNode<TextureRect>("%AncientBorder");
		_ancientTextBg = GetNode<TextureRect>("%AncientTextBg");
		_ancientBanner = GetNode<Control>("%AncientBanner");
		_ancientHighlight = GetNode<TextureRect>("%AncientHighlight");
		_portrait = GetNode<TextureRect>("%Portrait");
		_ancientPortrait = GetNode<TextureRect>("%AncientPortrait");
		_typeLabel = GetNode<MegaLabel>("%TypeLabel");
		_portraitBorder = GetNode<TextureRect>("%PortraitBorder");
		_portraitCanvasGroup = GetNode<CanvasGroup>("%PortraitCanvasGroup");
		_energyLabel = GetNode<MegaLabel>("%EnergyLabel");
		_energyIcon = GetNode<TextureRect>("%EnergyIcon");
		_starLabel = GetNode<MegaLabel>("%StarLabel");
		_starIcon = GetNode<TextureRect>("%StarIcon");
		_banner = GetNode<TextureRect>("%TitleBanner");
		_lock = GetNode<TextureRect>("%Lock");
		_typePlaque = GetNode<NinePatchRect>("%TypePlaque");
		_unplayableEnergyIcon = GetNode<TextureRect>("%UnplayableEnergyIcon");
		_unplayableStarIcon = GetNode<TextureRect>("%UnplayableStarIcon");
		_enchantmentIcon = GetNode<TextureRect>("%Enchantment/Icon");
		_enchantmentLabel = GetNode<MegaLabel>("%Enchantment/Label");
		_sparkles = GetNode<GpuParticles2D>("CardContainer/CardSparkles");
		_enchantmentTab = GetNode<Control>("%Enchantment");
		_enchantmentTab.Visible = false;
		_enchantmentVfxOverride = GetNode<TextureRect>("%EnchantmentVfxOverride");
		CardHighlight = GetNode<NCardHighlight>("%Highlight");
		Body = GetNode<Control>("%CardContainer");
		_overlayContainer = GetNode("%OverlayContainer");
		_defaultEnchantmentPosition = _enchantmentTab.Position;
		Reload();
	}

	public override void _EnterTree()
	{
		SubscribeToModel(Model);
	}

	public override void _ExitTree()
	{
		UnsubscribeFromModel(Model);
	}

	private void SubscribeToModel(CardModel? model)
	{
		if (model != null && IsInsideTree())
		{
			model.AfflictionChanged += OnAfflictionChanged;
			model.EnchantmentChanged += OnEnchantmentChanged;
			SubscribeToEnchantment(model.Enchantment);
		}
	}

	private void UnsubscribeFromModel(CardModel? model)
	{
		if (model != null)
		{
			model.AfflictionChanged -= OnAfflictionChanged;
			model.EnchantmentChanged -= OnEnchantmentChanged;
			UnsubscribeFromEnchantment(model.Enchantment);
		}
	}

	private void SubscribeToEnchantment(EnchantmentModel? model)
	{
		if (model != null && IsInsideTree())
		{
			if (_subscribedEnchantment != null)
			{
				throw new InvalidOperationException($"Attempted to subscribe to enchantment {model}, but {this} is already subscribed to {_subscribedEnchantment}!");
			}
			_subscribedEnchantment = model;
			_subscribedEnchantment.StatusChanged += OnEnchantmentStatusChanged;
		}
	}

	private void UnsubscribeFromEnchantment(EnchantmentModel? model)
	{
		if (model != null && model == _subscribedEnchantment)
		{
			_subscribedEnchantment.StatusChanged -= OnEnchantmentStatusChanged;
			_subscribedEnchantment = null;
		}
	}

	public static void InitPool()
	{
		NodePool.Init<NCard>("res://scenes/cards/card.tscn", 30);
	}

	public static NCard? Create(CardModel card, ModelVisibility visibility = ModelVisibility.Visible)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCard nCard = NodePool.Get<NCard>();
		nCard.Model = card;
		nCard.Visibility = visibility;
		return nCard;
	}

	public static NCard? FindOnTable(CardModel card, PileType? overridePile = null)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (!CombatManager.Instance.IsInProgress)
		{
			return null;
		}
		NCombatUi nCombatUi = NCombatRoom.Instance?.Ui;
		if (nCombatUi == null)
		{
			return null;
		}
		CardPile? pile = card.Pile;
		return ((pile != null) ? new PileType?(pile.Type) : overridePile) switch
		{
			PileType.None => null, 
			PileType.Draw => null, 
			PileType.Hand => nCombatUi.Hand.GetCard(card) ?? nCombatUi.PlayQueue.GetCardNode(card) ?? nCombatUi.GetCardFromPlayContainer(card), 
			PileType.Discard => null, 
			PileType.Exhaust => null, 
			PileType.Play => nCombatUi.GetCardFromPlayContainer(card), 
			PileType.Deck => null, 
			null => null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public Vector2 GetCurrentSize()
	{
		return defaultSize * base.Scale;
	}

	public void SetPreviewTarget(Creature? creature)
	{
		if (_previewTarget != creature)
		{
			_previewTarget = creature;
			UpdateVisuals(DisplayingPile, CardPreviewMode.Normal);
		}
	}

	public void UpdateVisuals(PileType pileType, CardPreviewMode previewMode)
	{
		if (!IsNodeReady())
		{
			return;
		}
		if (Model == null)
		{
			throw new InvalidOperationException("Cannot update text with no model.");
		}
		DisplayingPile = pileType;
		Creature target = _previewTarget ?? Model.CurrentTarget;
		UpdateTitleLabel();
		UpdateEnergyCostVisuals(pileType);
		UpdateStarCostVisuals(pileType);
		UpdateEnchantmentVisuals();
		Model.DynamicVars.ClearPreview();
		if (!_forceUnpoweredPreview)
		{
			Model.UpdateDynamicVarPreview(previewMode, target, Model.DynamicVars);
			if (Model.Enchantment != null)
			{
				Model.Enchantment.DynamicVars.ClearPreview();
				Model.UpdateDynamicVarPreview(previewMode, target, Model.Enchantment.DynamicVars);
			}
		}
		string text = ((previewMode != CardPreviewMode.Upgrade) ? Model.GetDescriptionForPile(pileType, target) : Model.GetDescriptionForUpgradePreview());
		switch (Visibility)
		{
		case ModelVisibility.Visible:
			_descriptionLabel.SetTextAutoSize("[center]" + text + "[/center]");
			break;
		case ModelVisibility.NotSeen:
			_descriptionLabel.SetTextAutoSize("[center][font_size=40]" + _unknownDescription.GetFormattedText() + "[/font_size][/center]");
			break;
		case ModelVisibility.Locked:
			_descriptionLabel.SetTextAutoSize("[center][font_size=40]" + _lockedDescription.GetFormattedText() + "[/font_size][/center]");
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	public void ShowUpgradePreview()
	{
		UpdateVisuals(DisplayingPile, CardPreviewMode.Upgrade);
	}

	private void UpdateEnchantmentVisuals()
	{
		if (Model == null)
		{
			throw new InvalidOperationException("Cannot show enchantment with no model.");
		}
		EnchantmentModel enchantment = Model.Enchantment;
		if (enchantment != null)
		{
			_enchantmentTab.Visible = true;
			_enchantmentIcon.Texture = enchantment.Icon;
			_enchantmentLabel.SetTextAutoSize(enchantment.DisplayAmount.ToString());
			_enchantmentLabel.Visible = enchantment.ShowAmount;
			SetEnchantmentStatus(enchantment.Status);
		}
		else
		{
			_enchantmentTab.Visible = false;
		}
		if (Model.HasStarCostX || Model.CurrentStarCost >= 0)
		{
			_enchantmentTab.Position = _defaultEnchantmentPosition;
		}
		else
		{
			_enchantmentTab.Position = _defaultEnchantmentPosition + Vector2.Up * 45f;
		}
	}

	private void OnEnchantmentStatusChanged()
	{
		SetEnchantmentStatus(Model?.Enchantment?.Status ?? EnchantmentStatus.Disabled);
	}

	private void SetEnchantmentStatus(EnchantmentStatus status)
	{
		if (status == EnchantmentStatus.Disabled)
		{
			_enchantmentTab.Modulate = new Color(1f, 1f, 1f, 0.9f);
			ShaderMaterial shaderMaterial = (ShaderMaterial)_enchantmentTab.Material;
			shaderMaterial.SetShaderParameter(_h, 0.25);
			shaderMaterial.SetShaderParameter(_s, 0.1);
			shaderMaterial.SetShaderParameter(_v, 0.6);
			_enchantmentIcon.UseParentMaterial = true;
			_enchantmentLabel.SelfModulate = StsColors.gray;
		}
		else
		{
			_enchantmentTab.Modulate = Colors.White;
			ShaderMaterial shaderMaterial2 = (ShaderMaterial)_enchantmentTab.Material;
			shaderMaterial2.SetShaderParameter(_h, 0.25);
			shaderMaterial2.SetShaderParameter(_s, 0.4);
			shaderMaterial2.SetShaderParameter(_v, 0.6);
			_enchantmentIcon.UseParentMaterial = false;
			_enchantmentLabel.SelfModulate = Colors.White;
		}
	}

	private void UpdateEnergyCostVisuals(PileType pileType)
	{
		if (Visibility != ModelVisibility.Visible)
		{
			_energyLabel.SetTextAutoSize("?");
			_energyIcon.Visible = true;
			_energyLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, StsColors.cream);
			_energyLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, Model.Pool.EnergyOutlineColor);
			return;
		}
		if (Model.EnergyCost.CostsX)
		{
			_energyLabel.SetTextAutoSize("X");
			_energyIcon.Visible = true;
		}
		else
		{
			int withModifiers = Model.EnergyCost.GetWithModifiers(CostModifiers.All);
			_energyLabel.SetTextAutoSize(withModifiers.ToString());
			_energyIcon.Visible = withModifiers >= 0;
		}
		UpdateEnergyCostColor(pileType);
		if (pileType == PileType.Hand && !Model.CanPlay(out UnplayableReason reason, out AbstractModel _))
		{
			_unplayableEnergyIcon.Visible = !reason.HasResourceCostReason();
		}
		else
		{
			_unplayableEnergyIcon.Visible = false;
		}
	}

	public void SetPretendCardCanBePlayed(bool pretendCardCanBePlayed)
	{
		_pretendCardCanBePlayed = pretendCardCanBePlayed;
		UpdateEnergyCostVisuals(DisplayingPile);
		UpdateStarCostVisuals(DisplayingPile);
	}

	public void SetForceUnpoweredPreview(bool forceUnpoweredPreview)
	{
		_forceUnpoweredPreview = forceUnpoweredPreview;
	}

	private void UpdateEnergyCostColor(PileType pileType)
	{
		Color color = StsColors.cream;
		Color color2 = Model.Pool.EnergyOutlineColor;
		CardEnergyCost energyCost = Model.EnergyCost;
		if (energyCost != null && !energyCost.CostsX && energyCost.WasJustUpgraded)
		{
			color = StsColors.green;
			color2 = StsColors.energyGreenOutline;
		}
		else if (pileType == PileType.Hand)
		{
			CardCostColor energyCostColor = CardCostHelper.GetEnergyCostColor(Model, Model.CombatState);
			color = GetCostTextColorInHand(energyCostColor, _pretendCardCanBePlayed, color);
			color2 = GetCostOutlineColorInHand(energyCostColor, _pretendCardCanBePlayed, color2);
		}
		_energyLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, color);
		_energyLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, color2);
	}

	private void UpdateStarCostVisuals(PileType pileType)
	{
		if (Visibility != ModelVisibility.Visible)
		{
			_starLabel.SetTextAutoSize(string.Empty);
			_starIcon.Visible = false;
			_starLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, StsColors.cream);
			_starLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, Model.Pool.EnergyOutlineColor);
			return;
		}
		if (Model.HasStarCostX)
		{
			_starLabel.SetTextAutoSize("X");
			_starIcon.Visible = true;
		}
		else
		{
			_starLabel.SetTextAutoSize(Model.GetStarCostWithModifiers().ToString());
			_starIcon.Visible = Model.GetStarCostWithModifiers() >= 0;
		}
		UpdateStarCostColor(pileType);
		if (pileType == PileType.Hand && !Model.CanPlay(out UnplayableReason reason, out AbstractModel _))
		{
			_unplayableStarIcon.Visible = !reason.HasResourceCostReason();
		}
		else
		{
			_unplayableStarIcon.Visible = false;
		}
	}

	private void UpdateStarCostText(int cost)
	{
		if (Model.HasStarCostX)
		{
			_starLabel.SetTextAutoSize("X");
			_starIcon.Visible = true;
		}
		else if (cost >= 0)
		{
			_starLabel.SetTextAutoSize(cost.ToString());
			_starIcon.Visible = true;
		}
		else
		{
			_starIcon.Visible = false;
		}
	}

	private void UpdateStarCostColor(PileType pileType)
	{
		Color color = StsColors.cream;
		Color color2 = StsColors.defaultStarCostOutline;
		if (!Model.HasStarCostX && Model.WasStarCostJustUpgraded)
		{
			color = StsColors.green;
			color2 = StsColors.energyGreenOutline;
		}
		else if (pileType == PileType.Hand)
		{
			CardCostColor starCostColor = CardCostHelper.GetStarCostColor(Model, Model.CombatState);
			color = GetCostTextColorInHand(starCostColor, _pretendCardCanBePlayed, color);
			color2 = GetCostOutlineColorInHand(starCostColor, _pretendCardCanBePlayed, color2);
		}
		_starLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, color);
		_starLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, color2);
	}

	private static Color GetCostTextColorInHand(CardCostColor costColor, bool pretendCardCanBePlayed, Color defaultColor)
	{
		return costColor switch
		{
			CardCostColor.Unmodified => defaultColor, 
			CardCostColor.Increased => StsColors.energyBlue, 
			CardCostColor.Decreased => StsColors.green, 
			CardCostColor.InsufficientResources => pretendCardCanBePlayed ? defaultColor : StsColors.red, 
			_ => throw new ArgumentOutOfRangeException("costColor", costColor, null), 
		};
	}

	private static Color GetCostOutlineColorInHand(CardCostColor costColor, bool pretendCardCanBePlayed, Color defaultColor)
	{
		return costColor switch
		{
			CardCostColor.Unmodified => defaultColor, 
			CardCostColor.Increased => StsColors.energyBlueOutline, 
			CardCostColor.Decreased => StsColors.energyGreenOutline, 
			CardCostColor.InsufficientResources => pretendCardCanBePlayed ? defaultColor : StsColors.unplayableEnergyCostOutline, 
			_ => throw new ArgumentOutOfRangeException("costColor", costColor, null), 
		};
	}

	public void PlayRandomizeCostAnim()
	{
		RandomizeCostTween?.Kill();
		RandomizeCostTween = CreateTween();
		float offset = Rng.Chaotic.NextFloat(10f);
		RandomizeCostTween.TweenMethod(Callable.From(delegate(float t)
		{
			int num = (int)(offset + t) % 8;
			if (num > 3)
			{
				_energyLabel.SetTextAutoSize("?");
			}
			else
			{
				_energyLabel.SetTextAutoSize((t % 4f).ToString());
			}
		}), 0, 50, Rng.Chaotic.NextFloat(0.4f, 0.6f)).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		RandomizeCostTween.Connect(Tween.SignalName.Finished, Callable.From(delegate
		{
			UpdateEnergyCostVisuals(Model.Pile.Type);
		}), 4u);
	}

	private void Reload()
	{
		if (!IsNodeReady() || Model == null)
		{
			return;
		}
		if (OS.HasFeature("editor"))
		{
			base.Name = $"{typeof(NCard)}-{Model.Id}";
		}
		_energyIcon.Texture = Model.EnergyIcon;
		UpdateTypePlaque();
		bool flag = Model.Rarity == CardRarity.Ancient;
		_portraitBorder.Visible = !flag;
		_portrait.Visible = !flag;
		_frame.Visible = !flag;
		_ancientPortrait.Visible = flag;
		_ancientBorder.Visible = flag;
		_ancientTextBg.Visible = flag;
		_ancientBanner.Visible = flag;
		_banner.Visible = !flag;
		_lock.Visible = Visibility == ModelVisibility.Locked;
		Texture2D portrait = Model.Portrait;
		if (Visibility != ModelVisibility.Visible)
		{
			if (_portraitBlurMaterial == null)
			{
				_portraitBlurMaterial = PreloadManager.Cache.GetMaterial(_portraitBlurMaterialPath);
			}
			if (flag)
			{
				if (_canvasGroupMaskBlurMaterial == null)
				{
					_canvasGroupMaskBlurMaterial = PreloadManager.Cache.GetMaterial(_canvasGroupMaskBlurMaterialPath);
				}
				_portraitCanvasGroup.Material = _canvasGroupMaskBlurMaterial;
			}
			else
			{
				if (_canvasGroupBlurMaterial == null)
				{
					_canvasGroupBlurMaterial = PreloadManager.Cache.GetMaterial(_canvasGroupBlurMaterialPath);
				}
				_portraitCanvasGroup.Material = _canvasGroupBlurMaterial;
			}
			_portrait.Material = _portraitBlurMaterial;
			_ancientPortrait.Material = _portraitBlurMaterial;
		}
		else
		{
			if (flag)
			{
				if (_canvasGroupMaskMaterial == null)
				{
					_canvasGroupMaskMaterial = PreloadManager.Cache.GetMaterial(_canvasGroupMaskMaterialPath);
				}
				_portraitCanvasGroup.Material = _canvasGroupMaskMaterial;
			}
			else
			{
				_portraitCanvasGroup.Material = null;
			}
			_portrait.Material = null;
			_ancientPortrait.Material = null;
		}
		if (Model.Rarity != CardRarity.Ancient)
		{
			_portrait.Texture = portrait;
			_portraitBorder.Texture = Model.PortraitBorder;
			_portraitBorder.Material = Model.BannerMaterial;
			_frame.Texture = Model.Frame;
			_banner.Material = Model.BannerMaterial;
			_banner.Texture = Model.BannerTexture;
		}
		else
		{
			_ancientTextBg.Texture = Model.AncientTextBg;
			_ancientPortrait.Texture = portrait;
			_banner.Material = null;
		}
		_frame.Material = Model.FrameMaterial;
		ReloadOverlay();
	}

	private void UpdateTypePlaque()
	{
		_typeLabel.SetTextAutoSize(Model.Type.ToLocString().GetFormattedText());
		Material bannerMaterial = Model.BannerMaterial;
		if (_typePlaque.Material != bannerMaterial)
		{
			_typePlaque.Material = Model.BannerMaterial;
		}
		Callable.From(UpdateTypePlaqueSizeAndPosition).CallDeferred();
	}

	private void UpdateTypePlaqueSizeAndPosition()
	{
		float num = _typePlaque.Position.X + _typePlaque.Size.X * 0.5f;
		NinePatchRect typePlaque = _typePlaque;
		Vector2 size = _typePlaque.Size;
		size.X = Mathf.Max(_typeLabel.Size.X + _typePlaqueXMargin, _typePlaqueMinXSize);
		typePlaque.Size = size;
		NinePatchRect typePlaque2 = _typePlaque;
		size = _typePlaque.Position;
		size.X = num - _typePlaque.Size.X * 0.5f;
		typePlaque2.Position = size;
	}

	private void UpdateTitleLabel()
	{
		string textAutoSize;
		Color color;
		Color color2;
		if (Visibility == ModelVisibility.NotSeen)
		{
			textAutoSize = _unknownTitle.GetFormattedText();
			color = StsColors.cream;
			color2 = GetTitleLabelOutlineColor();
		}
		else if (Visibility == ModelVisibility.Locked)
		{
			textAutoSize = _lockedTitle.GetFormattedText();
			color = StsColors.cream;
			color2 = GetTitleLabelOutlineColor();
		}
		else if (Model.CurrentUpgradeLevel == 0)
		{
			textAutoSize = Model.Title;
			color = StsColors.cream;
			color2 = GetTitleLabelOutlineColor();
		}
		else
		{
			textAutoSize = Model.Title;
			color = StsColors.green;
			color2 = StsColors.cardTitleOutlineSpecial;
		}
		_titleLabel.SetTextAutoSize(textAutoSize);
		_titleLabel.AddThemeColorOverride(ThemeConstants.Label.fontColor, color);
		_titleLabel.AddThemeColorOverride(ThemeConstants.Label.fontOutlineColor, color2);
	}

	private Color GetTitleLabelOutlineColor()
	{
		switch (_model.Rarity)
		{
		case CardRarity.None:
		case CardRarity.Basic:
		case CardRarity.Common:
		case CardRarity.Token:
			return StsColors.cardTitleOutlineCommon;
		case CardRarity.Uncommon:
			return StsColors.cardTitleOutlineUncommon;
		case CardRarity.Rare:
			return StsColors.cardTitleOutlineRare;
		case CardRarity.Curse:
			return StsColors.cardTitleOutlineCurse;
		case CardRarity.Quest:
			return StsColors.cardTitleOutlineQuest;
		case CardRarity.Status:
			return StsColors.cardTitleOutlineStatus;
		case CardRarity.Event:
			return StsColors.cardTitleOutlineSpecial;
		case CardRarity.Ancient:
			return StsColors.cardTitleOutlineCommon;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void ReloadOverlay()
	{
		if (_cardOverlay != null)
		{
			_overlayContainer.RemoveChildSafely(_cardOverlay);
			_cardOverlay.QueueFreeSafely();
			_cardOverlay = null;
		}
		if (Model != null)
		{
			if (Model.Rarity == CardRarity.Ancient)
			{
				_frame.Visible = false;
				_ancientBorder.Visible = true;
				_ancientHighlight.Visible = true;
			}
			AfflictionModel affliction = Model.Affliction;
			if (affliction != null && affliction.HasOverlay)
			{
				_cardOverlay = Model.Affliction.CreateOverlay();
			}
			else if (Model.HasBuiltInOverlay)
			{
				_cardOverlay = Model.CreateOverlay();
			}
			if (_cardOverlay != null)
			{
				_overlayContainer.AddChildSafely(_cardOverlay);
			}
		}
	}

	private void OnAfflictionChanged()
	{
		ReloadOverlay();
	}

	private void OnEnchantmentChanged()
	{
		UnsubscribeFromEnchantment(_subscribedEnchantment);
		SubscribeToEnchantment(Model?.Enchantment);
		UpdateEnchantmentVisuals();
	}

	private string GetTitleText()
	{
		return _titleLabel.Text;
	}

	public void ActivateRewardScreenGlow()
	{
		if (_model.Rarity == CardRarity.Rare)
		{
			_sparkles.Visible = true;
			_rareGlow = NCardRareGlow.Create();
			if (_rareGlow != null)
			{
				Body.AddChildSafely(_rareGlow);
				Body.MoveChild(_rareGlow, 1);
			}
			CardHighlight.Modulate = NCardHighlight.gold;
		}
		else if (_model.Rarity == CardRarity.Uncommon)
		{
			_uncommonGlow = NCardUncommonGlow.Create();
			if (_uncommonGlow != null)
			{
				Body.AddChildSafely(_uncommonGlow);
				Body.MoveChild(_uncommonGlow, 1);
			}
			CardHighlight.Modulate = NCardHighlight.playableColor;
		}
	}

	public void FlashRelicOnCard(RelicModel relic)
	{
		NRelicFlashVfx nRelicFlashVfx = NRelicFlashVfx.Create(relic);
		Body.AddChildSafely(nRelicFlashVfx);
		nRelicFlashVfx.Scale = Vector2.One * 2f;
		nRelicFlashVfx.Position = base.Size * 0.5f;
		_flashVfx.Add(nRelicFlashVfx);
	}

	public void KillRarityGlow()
	{
		_rareGlow?.Kill();
		_uncommonGlow?.Kill();
	}

	public async Task AnimMultiCardPlay()
	{
		if (GodotObject.IsInstanceValid(this))
		{
			Vector2 scale = base.Scale;
			float y = base.Position.Y;
			PlayPileTween?.FastForwardToCompletion();
			PlayPileTween = CreateTween().SetParallel();
			PlayPileTween.TweenProperty(this, "modulate", StsColors.transparentBlack, 0.2);
			PlayPileTween.Chain();
			PlayPileTween.TweenInterval((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.1 : 0.2);
			PlayPileTween.Chain();
			PlayPileTween.TweenProperty(this, "modulate", Colors.White, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
			PlayPileTween.TweenProperty(this, "scale", scale, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic)
				.From(scale * 0.5f);
			PlayPileTween.TweenProperty(this, "position:y", y, 0.25).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
				.From(y + 250f);
			await Cmd.CustomScaledWait(0.4f, 0.5f);
		}
	}

	public void AnimCardToPlayPile()
	{
		Vector2 targetPosition = PileType.Play.GetTargetPosition(this);
		PlayPileTween?.FastForwardToCompletion();
		PlayPileTween = CreateTween().SetParallel();
		PlayPileTween.TweenProperty(this, "position", targetPosition, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		PlayPileTween.TweenProperty(this, "scale", Vector2.One * 0.8f, 0.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
	}

	public void OnReturnedFromPool()
	{
		if (IsNodeReady())
		{
			base.Position = Vector2.Zero;
			base.Rotation = 0f;
			base.Scale = Vector2.One;
			base.Modulate = Colors.White;
			base.Visible = true;
			Body.Visible = true;
			Body.Modulate = Colors.White;
			Body.Scale = Vector2.One;
			_visibility = ModelVisibility.Visible;
			CardHighlight.Modulate = NCardHighlight.playableColor;
			CardHighlight.AnimHideInstantly();
			_sparkles.Visible = false;
			_enchantmentTab.Visible = false;
			_enchantmentVfxOverride.Visible = false;
			_model = null;
			_previewTarget = null;
			_pretendCardCanBePlayed = false;
			_forceUnpoweredPreview = false;
			_portrait.Material = null;
			_ancientPortrait.Material = null;
			_portraitCanvasGroup.Material = null;
			DisplayingPile = PileType.None;
			this.ModelChanged = null;
		}
	}

	public void OnFreedToPool()
	{
		_rareGlow?.QueueFreeSafely();
		_rareGlow = null;
		_uncommonGlow?.QueueFreeSafely();
		_uncommonGlow = null;
		_cardOverlay?.QueueFreeSafely();
		_cardOverlay = null;
		_portrait.Texture = null;
		_enchantmentVfxOverride.Texture = null;
		foreach (NRelicFlashVfx item in _flashVfx)
		{
			if (item.IsValid())
			{
				item.QueueFreeSafely();
			}
		}
		_flashVfx.Clear();
		PlayPileTween?.Kill();
		PlayPileTween = null;
		RandomizeCostTween?.Kill();
		RandomizeCostTween = null;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(35);
		list.Add(new MethodInfo(MethodName.OnInstantiated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.InitPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.GetCurrentSize, new PropertyInfo(Variant.Type.Vector2, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pileType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "previewMode", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.ShowUpgradePreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateEnchantmentVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnchantmentStatusChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetEnchantmentStatus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "status", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateEnergyCostVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pileType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetPretendCardCanBePlayed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "pretendCardCanBePlayed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetForceUnpoweredPreview, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "forceUnpoweredPreview", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateEnergyCostColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pileType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateStarCostVisuals, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pileType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateStarCostText, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "cost", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateStarCostColor, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "pileType", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCostTextColorInHand, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "costColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "pretendCardCanBePlayed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "defaultColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.GetCostOutlineColorInHand, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "costColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Bool, "pretendCardCanBePlayed", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Color, "defaultColor", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.PlayRandomizeCostAnim, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Reload, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTypePlaque, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTypePlaqueSizeAndPosition, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.UpdateTitleLabel, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetTitleLabelOutlineColor, new PropertyInfo(Variant.Type.Color, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ReloadOverlay, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnAfflictionChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnEnchantmentChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetTitleText, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.ActivateRewardScreenGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.KillRarityGlow, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AnimCardToPlayPile, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnReturnedFromPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFreedToPool, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.OnInstantiated && args.Count == 0)
		{
			OnInstantiated();
			ret = default(godot_variant);
			return true;
		}
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
		if (method == MethodName.InitPool && args.Count == 0)
		{
			InitPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCurrentSize && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Vector2>(GetCurrentSize());
			return true;
		}
		if (method == MethodName.UpdateVisuals && args.Count == 2)
		{
			UpdateVisuals(VariantUtils.ConvertTo<PileType>(in args[0]), VariantUtils.ConvertTo<CardPreviewMode>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ShowUpgradePreview && args.Count == 0)
		{
			ShowUpgradePreview();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateEnchantmentVisuals && args.Count == 0)
		{
			UpdateEnchantmentVisuals();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnchantmentStatusChanged && args.Count == 0)
		{
			OnEnchantmentStatusChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetEnchantmentStatus && args.Count == 1)
		{
			SetEnchantmentStatus(VariantUtils.ConvertTo<EnchantmentStatus>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateEnergyCostVisuals && args.Count == 1)
		{
			UpdateEnergyCostVisuals(VariantUtils.ConvertTo<PileType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetPretendCardCanBePlayed && args.Count == 1)
		{
			SetPretendCardCanBePlayed(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetForceUnpoweredPreview && args.Count == 1)
		{
			SetForceUnpoweredPreview(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateEnergyCostColor && args.Count == 1)
		{
			UpdateEnergyCostColor(VariantUtils.ConvertTo<PileType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateStarCostVisuals && args.Count == 1)
		{
			UpdateStarCostVisuals(VariantUtils.ConvertTo<PileType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateStarCostText && args.Count == 1)
		{
			UpdateStarCostText(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateStarCostColor && args.Count == 1)
		{
			UpdateStarCostColor(VariantUtils.ConvertTo<PileType>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCostTextColorInHand && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Color>(GetCostTextColorInHand(VariantUtils.ConvertTo<CardCostColor>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		if (method == MethodName.GetCostOutlineColorInHand && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Color>(GetCostOutlineColorInHand(VariantUtils.ConvertTo<CardCostColor>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		if (method == MethodName.PlayRandomizeCostAnim && args.Count == 0)
		{
			PlayRandomizeCostAnim();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Reload && args.Count == 0)
		{
			Reload();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTypePlaque && args.Count == 0)
		{
			UpdateTypePlaque();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTypePlaqueSizeAndPosition && args.Count == 0)
		{
			UpdateTypePlaqueSizeAndPosition();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateTitleLabel && args.Count == 0)
		{
			UpdateTitleLabel();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetTitleLabelOutlineColor && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Color>(GetTitleLabelOutlineColor());
			return true;
		}
		if (method == MethodName.ReloadOverlay && args.Count == 0)
		{
			ReloadOverlay();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnAfflictionChanged && args.Count == 0)
		{
			OnAfflictionChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnEnchantmentChanged && args.Count == 0)
		{
			OnEnchantmentChanged();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetTitleText && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<string>(GetTitleText());
			return true;
		}
		if (method == MethodName.ActivateRewardScreenGlow && args.Count == 0)
		{
			ActivateRewardScreenGlow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.KillRarityGlow && args.Count == 0)
		{
			KillRarityGlow();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AnimCardToPlayPile && args.Count == 0)
		{
			AnimCardToPlayPile();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnReturnedFromPool && args.Count == 0)
		{
			OnReturnedFromPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFreedToPool && args.Count == 0)
		{
			OnFreedToPool();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.InitPool && args.Count == 0)
		{
			InitPool();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetCostTextColorInHand && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Color>(GetCostTextColorInHand(VariantUtils.ConvertTo<CardCostColor>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		if (method == MethodName.GetCostOutlineColorInHand && args.Count == 3)
		{
			ret = VariantUtils.CreateFrom<Color>(GetCostOutlineColorInHand(VariantUtils.ConvertTo<CardCostColor>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]), VariantUtils.ConvertTo<Color>(in args[2])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.OnInstantiated)
		{
			return true;
		}
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
		if (method == MethodName.InitPool)
		{
			return true;
		}
		if (method == MethodName.GetCurrentSize)
		{
			return true;
		}
		if (method == MethodName.UpdateVisuals)
		{
			return true;
		}
		if (method == MethodName.ShowUpgradePreview)
		{
			return true;
		}
		if (method == MethodName.UpdateEnchantmentVisuals)
		{
			return true;
		}
		if (method == MethodName.OnEnchantmentStatusChanged)
		{
			return true;
		}
		if (method == MethodName.SetEnchantmentStatus)
		{
			return true;
		}
		if (method == MethodName.UpdateEnergyCostVisuals)
		{
			return true;
		}
		if (method == MethodName.SetPretendCardCanBePlayed)
		{
			return true;
		}
		if (method == MethodName.SetForceUnpoweredPreview)
		{
			return true;
		}
		if (method == MethodName.UpdateEnergyCostColor)
		{
			return true;
		}
		if (method == MethodName.UpdateStarCostVisuals)
		{
			return true;
		}
		if (method == MethodName.UpdateStarCostText)
		{
			return true;
		}
		if (method == MethodName.UpdateStarCostColor)
		{
			return true;
		}
		if (method == MethodName.GetCostTextColorInHand)
		{
			return true;
		}
		if (method == MethodName.GetCostOutlineColorInHand)
		{
			return true;
		}
		if (method == MethodName.PlayRandomizeCostAnim)
		{
			return true;
		}
		if (method == MethodName.Reload)
		{
			return true;
		}
		if (method == MethodName.UpdateTypePlaque)
		{
			return true;
		}
		if (method == MethodName.UpdateTypePlaqueSizeAndPosition)
		{
			return true;
		}
		if (method == MethodName.UpdateTitleLabel)
		{
			return true;
		}
		if (method == MethodName.GetTitleLabelOutlineColor)
		{
			return true;
		}
		if (method == MethodName.ReloadOverlay)
		{
			return true;
		}
		if (method == MethodName.OnAfflictionChanged)
		{
			return true;
		}
		if (method == MethodName.OnEnchantmentChanged)
		{
			return true;
		}
		if (method == MethodName.GetTitleText)
		{
			return true;
		}
		if (method == MethodName.ActivateRewardScreenGlow)
		{
			return true;
		}
		if (method == MethodName.KillRarityGlow)
		{
			return true;
		}
		if (method == MethodName.AnimCardToPlayPile)
		{
			return true;
		}
		if (method == MethodName.OnReturnedFromPool)
		{
			return true;
		}
		if (method == MethodName.OnFreedToPool)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CardHighlight)
		{
			CardHighlight = VariantUtils.ConvertTo<NCardHighlight>(in value);
			return true;
		}
		if (name == PropertyName.Body)
		{
			Body = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.Visibility)
		{
			Visibility = VariantUtils.ConvertTo<ModelVisibility>(in value);
			return true;
		}
		if (name == PropertyName.PlayPileTween)
		{
			PlayPileTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName.RandomizeCostTween)
		{
			RandomizeCostTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName.DisplayingPile)
		{
			DisplayingPile = VariantUtils.ConvertTo<PileType>(in value);
			return true;
		}
		if (name == PropertyName._titleLabel)
		{
			_titleLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			_descriptionLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._ancientPortrait)
		{
			_ancientPortrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			_portrait = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._frame)
		{
			_frame = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._ancientBorder)
		{
			_ancientBorder = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._ancientBanner)
		{
			_ancientBanner = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._ancientTextBg)
		{
			_ancientTextBg = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._ancientHighlight)
		{
			_ancientHighlight = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._portraitBorder)
		{
			_portraitBorder = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._banner)
		{
			_banner = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._lock)
		{
			_lock = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._typePlaque)
		{
			_typePlaque = VariantUtils.ConvertTo<NinePatchRect>(in value);
			return true;
		}
		if (name == PropertyName._typeLabel)
		{
			_typeLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._portraitCanvasGroup)
		{
			_portraitCanvasGroup = VariantUtils.ConvertTo<CanvasGroup>(in value);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			_rareGlow = VariantUtils.ConvertTo<NCardRareGlow>(in value);
			return true;
		}
		if (name == PropertyName._uncommonGlow)
		{
			_uncommonGlow = VariantUtils.ConvertTo<NCardUncommonGlow>(in value);
			return true;
		}
		if (name == PropertyName._sparkles)
		{
			_sparkles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
			return true;
		}
		if (name == PropertyName._energyIcon)
		{
			_energyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._energyLabel)
		{
			_energyLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._unplayableEnergyIcon)
		{
			_unplayableEnergyIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._starIcon)
		{
			_starIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._starLabel)
		{
			_starLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._unplayableStarIcon)
		{
			_unplayableStarIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._overlayContainer)
		{
			_overlayContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName._cardOverlay)
		{
			_cardOverlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentTab)
		{
			_enchantmentTab = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentVfxOverride)
		{
			_enchantmentVfxOverride = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentIcon)
		{
			_enchantmentIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentLabel)
		{
			_enchantmentLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		if (name == PropertyName._defaultEnchantmentPosition)
		{
			_defaultEnchantmentPosition = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._pretendCardCanBePlayed)
		{
			_pretendCardCanBePlayed = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._forceUnpoweredPreview)
		{
			_forceUnpoweredPreview = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._portraitBlurMaterial)
		{
			_portraitBlurMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._canvasGroupMaskBlurMaterial)
		{
			_canvasGroupMaskBlurMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._canvasGroupBlurMaterial)
		{
			_canvasGroupBlurMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._canvasGroupMaskMaterial)
		{
			_canvasGroupMaskMaterial = VariantUtils.ConvertTo<Material>(in value);
			return true;
		}
		if (name == PropertyName._visibility)
		{
			_visibility = VariantUtils.ConvertTo<ModelVisibility>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CardHighlight)
		{
			value = VariantUtils.CreateFrom<NCardHighlight>(CardHighlight);
			return true;
		}
		Control from;
		if (name == PropertyName.Body)
		{
			from = Body;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Visibility)
		{
			value = VariantUtils.CreateFrom<ModelVisibility>(Visibility);
			return true;
		}
		Tween from2;
		if (name == PropertyName.PlayPileTween)
		{
			from2 = PlayPileTween;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.RandomizeCostTween)
		{
			from2 = RandomizeCostTween;
			value = VariantUtils.CreateFrom(in from2);
			return true;
		}
		if (name == PropertyName.DisplayingPile)
		{
			value = VariantUtils.CreateFrom<PileType>(DisplayingPile);
			return true;
		}
		if (name == PropertyName.EnchantmentTab)
		{
			from = EnchantmentTab;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.EnchantmentVfxOverride)
		{
			value = VariantUtils.CreateFrom<TextureRect>(EnchantmentVfxOverride);
			return true;
		}
		if (name == PropertyName._titleLabel)
		{
			value = VariantUtils.CreateFrom(in _titleLabel);
			return true;
		}
		if (name == PropertyName._descriptionLabel)
		{
			value = VariantUtils.CreateFrom(in _descriptionLabel);
			return true;
		}
		if (name == PropertyName._ancientPortrait)
		{
			value = VariantUtils.CreateFrom(in _ancientPortrait);
			return true;
		}
		if (name == PropertyName._portrait)
		{
			value = VariantUtils.CreateFrom(in _portrait);
			return true;
		}
		if (name == PropertyName._frame)
		{
			value = VariantUtils.CreateFrom(in _frame);
			return true;
		}
		if (name == PropertyName._ancientBorder)
		{
			value = VariantUtils.CreateFrom(in _ancientBorder);
			return true;
		}
		if (name == PropertyName._ancientBanner)
		{
			value = VariantUtils.CreateFrom(in _ancientBanner);
			return true;
		}
		if (name == PropertyName._ancientTextBg)
		{
			value = VariantUtils.CreateFrom(in _ancientTextBg);
			return true;
		}
		if (name == PropertyName._ancientHighlight)
		{
			value = VariantUtils.CreateFrom(in _ancientHighlight);
			return true;
		}
		if (name == PropertyName._portraitBorder)
		{
			value = VariantUtils.CreateFrom(in _portraitBorder);
			return true;
		}
		if (name == PropertyName._banner)
		{
			value = VariantUtils.CreateFrom(in _banner);
			return true;
		}
		if (name == PropertyName._lock)
		{
			value = VariantUtils.CreateFrom(in _lock);
			return true;
		}
		if (name == PropertyName._typePlaque)
		{
			value = VariantUtils.CreateFrom(in _typePlaque);
			return true;
		}
		if (name == PropertyName._typeLabel)
		{
			value = VariantUtils.CreateFrom(in _typeLabel);
			return true;
		}
		if (name == PropertyName._portraitCanvasGroup)
		{
			value = VariantUtils.CreateFrom(in _portraitCanvasGroup);
			return true;
		}
		if (name == PropertyName._rareGlow)
		{
			value = VariantUtils.CreateFrom(in _rareGlow);
			return true;
		}
		if (name == PropertyName._uncommonGlow)
		{
			value = VariantUtils.CreateFrom(in _uncommonGlow);
			return true;
		}
		if (name == PropertyName._sparkles)
		{
			value = VariantUtils.CreateFrom(in _sparkles);
			return true;
		}
		if (name == PropertyName._energyIcon)
		{
			value = VariantUtils.CreateFrom(in _energyIcon);
			return true;
		}
		if (name == PropertyName._energyLabel)
		{
			value = VariantUtils.CreateFrom(in _energyLabel);
			return true;
		}
		if (name == PropertyName._unplayableEnergyIcon)
		{
			value = VariantUtils.CreateFrom(in _unplayableEnergyIcon);
			return true;
		}
		if (name == PropertyName._starIcon)
		{
			value = VariantUtils.CreateFrom(in _starIcon);
			return true;
		}
		if (name == PropertyName._starLabel)
		{
			value = VariantUtils.CreateFrom(in _starLabel);
			return true;
		}
		if (name == PropertyName._unplayableStarIcon)
		{
			value = VariantUtils.CreateFrom(in _unplayableStarIcon);
			return true;
		}
		if (name == PropertyName._overlayContainer)
		{
			value = VariantUtils.CreateFrom(in _overlayContainer);
			return true;
		}
		if (name == PropertyName._cardOverlay)
		{
			value = VariantUtils.CreateFrom(in _cardOverlay);
			return true;
		}
		if (name == PropertyName._enchantmentTab)
		{
			value = VariantUtils.CreateFrom(in _enchantmentTab);
			return true;
		}
		if (name == PropertyName._enchantmentVfxOverride)
		{
			value = VariantUtils.CreateFrom(in _enchantmentVfxOverride);
			return true;
		}
		if (name == PropertyName._enchantmentIcon)
		{
			value = VariantUtils.CreateFrom(in _enchantmentIcon);
			return true;
		}
		if (name == PropertyName._enchantmentLabel)
		{
			value = VariantUtils.CreateFrom(in _enchantmentLabel);
			return true;
		}
		if (name == PropertyName._defaultEnchantmentPosition)
		{
			value = VariantUtils.CreateFrom(in _defaultEnchantmentPosition);
			return true;
		}
		if (name == PropertyName._pretendCardCanBePlayed)
		{
			value = VariantUtils.CreateFrom(in _pretendCardCanBePlayed);
			return true;
		}
		if (name == PropertyName._forceUnpoweredPreview)
		{
			value = VariantUtils.CreateFrom(in _forceUnpoweredPreview);
			return true;
		}
		if (name == PropertyName._portraitBlurMaterial)
		{
			value = VariantUtils.CreateFrom(in _portraitBlurMaterial);
			return true;
		}
		if (name == PropertyName._canvasGroupMaskBlurMaterial)
		{
			value = VariantUtils.CreateFrom(in _canvasGroupMaskBlurMaterial);
			return true;
		}
		if (name == PropertyName._canvasGroupBlurMaterial)
		{
			value = VariantUtils.CreateFrom(in _canvasGroupBlurMaterial);
			return true;
		}
		if (name == PropertyName._canvasGroupMaskMaterial)
		{
			value = VariantUtils.CreateFrom(in _canvasGroupMaskMaterial);
			return true;
		}
		if (name == PropertyName._visibility)
		{
			value = VariantUtils.CreateFrom(in _visibility);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._titleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._descriptionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientPortrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portrait, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._frame, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientBorder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientBanner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientTextBg, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._ancientHighlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portraitBorder, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._banner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._lock, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._typePlaque, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._typeLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portraitCanvasGroup, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._rareGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._uncommonGlow, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._sparkles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._energyLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unplayableEnergyIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._starLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._unplayableStarIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._overlayContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentVfxOverride, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._defaultEnchantmentPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CardHighlight, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Body, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._pretendCardCanBePlayed, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._forceUnpoweredPreview, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._portraitBlurMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._canvasGroupMaskBlurMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._canvasGroupBlurMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._canvasGroupMaskMaterial, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._visibility, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Visibility, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.PlayPileTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RandomizeCostTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.DisplayingPile, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EnchantmentTab, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EnchantmentVfxOverride, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CardHighlight, Variant.From<NCardHighlight>(CardHighlight));
		info.AddProperty(PropertyName.Body, Variant.From<Control>(Body));
		info.AddProperty(PropertyName.Visibility, Variant.From<ModelVisibility>(Visibility));
		info.AddProperty(PropertyName.PlayPileTween, Variant.From<Tween>(PlayPileTween));
		info.AddProperty(PropertyName.RandomizeCostTween, Variant.From<Tween>(RandomizeCostTween));
		info.AddProperty(PropertyName.DisplayingPile, Variant.From<PileType>(DisplayingPile));
		info.AddProperty(PropertyName._titleLabel, Variant.From(in _titleLabel));
		info.AddProperty(PropertyName._descriptionLabel, Variant.From(in _descriptionLabel));
		info.AddProperty(PropertyName._ancientPortrait, Variant.From(in _ancientPortrait));
		info.AddProperty(PropertyName._portrait, Variant.From(in _portrait));
		info.AddProperty(PropertyName._frame, Variant.From(in _frame));
		info.AddProperty(PropertyName._ancientBorder, Variant.From(in _ancientBorder));
		info.AddProperty(PropertyName._ancientBanner, Variant.From(in _ancientBanner));
		info.AddProperty(PropertyName._ancientTextBg, Variant.From(in _ancientTextBg));
		info.AddProperty(PropertyName._ancientHighlight, Variant.From(in _ancientHighlight));
		info.AddProperty(PropertyName._portraitBorder, Variant.From(in _portraitBorder));
		info.AddProperty(PropertyName._banner, Variant.From(in _banner));
		info.AddProperty(PropertyName._lock, Variant.From(in _lock));
		info.AddProperty(PropertyName._typePlaque, Variant.From(in _typePlaque));
		info.AddProperty(PropertyName._typeLabel, Variant.From(in _typeLabel));
		info.AddProperty(PropertyName._portraitCanvasGroup, Variant.From(in _portraitCanvasGroup));
		info.AddProperty(PropertyName._rareGlow, Variant.From(in _rareGlow));
		info.AddProperty(PropertyName._uncommonGlow, Variant.From(in _uncommonGlow));
		info.AddProperty(PropertyName._sparkles, Variant.From(in _sparkles));
		info.AddProperty(PropertyName._energyIcon, Variant.From(in _energyIcon));
		info.AddProperty(PropertyName._energyLabel, Variant.From(in _energyLabel));
		info.AddProperty(PropertyName._unplayableEnergyIcon, Variant.From(in _unplayableEnergyIcon));
		info.AddProperty(PropertyName._starIcon, Variant.From(in _starIcon));
		info.AddProperty(PropertyName._starLabel, Variant.From(in _starLabel));
		info.AddProperty(PropertyName._unplayableStarIcon, Variant.From(in _unplayableStarIcon));
		info.AddProperty(PropertyName._overlayContainer, Variant.From(in _overlayContainer));
		info.AddProperty(PropertyName._cardOverlay, Variant.From(in _cardOverlay));
		info.AddProperty(PropertyName._enchantmentTab, Variant.From(in _enchantmentTab));
		info.AddProperty(PropertyName._enchantmentVfxOverride, Variant.From(in _enchantmentVfxOverride));
		info.AddProperty(PropertyName._enchantmentIcon, Variant.From(in _enchantmentIcon));
		info.AddProperty(PropertyName._enchantmentLabel, Variant.From(in _enchantmentLabel));
		info.AddProperty(PropertyName._defaultEnchantmentPosition, Variant.From(in _defaultEnchantmentPosition));
		info.AddProperty(PropertyName._pretendCardCanBePlayed, Variant.From(in _pretendCardCanBePlayed));
		info.AddProperty(PropertyName._forceUnpoweredPreview, Variant.From(in _forceUnpoweredPreview));
		info.AddProperty(PropertyName._portraitBlurMaterial, Variant.From(in _portraitBlurMaterial));
		info.AddProperty(PropertyName._canvasGroupMaskBlurMaterial, Variant.From(in _canvasGroupMaskBlurMaterial));
		info.AddProperty(PropertyName._canvasGroupBlurMaterial, Variant.From(in _canvasGroupBlurMaterial));
		info.AddProperty(PropertyName._canvasGroupMaskMaterial, Variant.From(in _canvasGroupMaskMaterial));
		info.AddProperty(PropertyName._visibility, Variant.From(in _visibility));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CardHighlight, out var value))
		{
			CardHighlight = value.As<NCardHighlight>();
		}
		if (info.TryGetProperty(PropertyName.Body, out var value2))
		{
			Body = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.Visibility, out var value3))
		{
			Visibility = value3.As<ModelVisibility>();
		}
		if (info.TryGetProperty(PropertyName.PlayPileTween, out var value4))
		{
			PlayPileTween = value4.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName.RandomizeCostTween, out var value5))
		{
			RandomizeCostTween = value5.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName.DisplayingPile, out var value6))
		{
			DisplayingPile = value6.As<PileType>();
		}
		if (info.TryGetProperty(PropertyName._titleLabel, out var value7))
		{
			_titleLabel = value7.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._descriptionLabel, out var value8))
		{
			_descriptionLabel = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._ancientPortrait, out var value9))
		{
			_ancientPortrait = value9.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._portrait, out var value10))
		{
			_portrait = value10.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._frame, out var value11))
		{
			_frame = value11.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._ancientBorder, out var value12))
		{
			_ancientBorder = value12.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._ancientBanner, out var value13))
		{
			_ancientBanner = value13.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._ancientTextBg, out var value14))
		{
			_ancientTextBg = value14.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._ancientHighlight, out var value15))
		{
			_ancientHighlight = value15.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._portraitBorder, out var value16))
		{
			_portraitBorder = value16.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._banner, out var value17))
		{
			_banner = value17.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._lock, out var value18))
		{
			_lock = value18.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._typePlaque, out var value19))
		{
			_typePlaque = value19.As<NinePatchRect>();
		}
		if (info.TryGetProperty(PropertyName._typeLabel, out var value20))
		{
			_typeLabel = value20.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._portraitCanvasGroup, out var value21))
		{
			_portraitCanvasGroup = value21.As<CanvasGroup>();
		}
		if (info.TryGetProperty(PropertyName._rareGlow, out var value22))
		{
			_rareGlow = value22.As<NCardRareGlow>();
		}
		if (info.TryGetProperty(PropertyName._uncommonGlow, out var value23))
		{
			_uncommonGlow = value23.As<NCardUncommonGlow>();
		}
		if (info.TryGetProperty(PropertyName._sparkles, out var value24))
		{
			_sparkles = value24.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._energyIcon, out var value25))
		{
			_energyIcon = value25.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._energyLabel, out var value26))
		{
			_energyLabel = value26.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._unplayableEnergyIcon, out var value27))
		{
			_unplayableEnergyIcon = value27.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._starIcon, out var value28))
		{
			_starIcon = value28.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._starLabel, out var value29))
		{
			_starLabel = value29.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._unplayableStarIcon, out var value30))
		{
			_unplayableStarIcon = value30.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._overlayContainer, out var value31))
		{
			_overlayContainer = value31.As<Node>();
		}
		if (info.TryGetProperty(PropertyName._cardOverlay, out var value32))
		{
			_cardOverlay = value32.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentTab, out var value33))
		{
			_enchantmentTab = value33.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentVfxOverride, out var value34))
		{
			_enchantmentVfxOverride = value34.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentIcon, out var value35))
		{
			_enchantmentIcon = value35.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentLabel, out var value36))
		{
			_enchantmentLabel = value36.As<MegaLabel>();
		}
		if (info.TryGetProperty(PropertyName._defaultEnchantmentPosition, out var value37))
		{
			_defaultEnchantmentPosition = value37.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._pretendCardCanBePlayed, out var value38))
		{
			_pretendCardCanBePlayed = value38.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._forceUnpoweredPreview, out var value39))
		{
			_forceUnpoweredPreview = value39.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._portraitBlurMaterial, out var value40))
		{
			_portraitBlurMaterial = value40.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._canvasGroupMaskBlurMaterial, out var value41))
		{
			_canvasGroupMaskBlurMaterial = value41.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._canvasGroupBlurMaterial, out var value42))
		{
			_canvasGroupBlurMaterial = value42.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._canvasGroupMaskMaterial, out var value43))
		{
			_canvasGroupMaskMaterial = value43.As<Material>();
		}
		if (info.TryGetProperty(PropertyName._visibility, out var value44))
		{
			_visibility = value44.As<ModelVisibility>();
		}
	}
}
