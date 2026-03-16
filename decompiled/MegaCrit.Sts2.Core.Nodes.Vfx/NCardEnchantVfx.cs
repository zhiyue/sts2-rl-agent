using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardEnchantVfx.cs")]
public class NCardEnchantVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName EmbossCurve = "EmbossCurve";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _cardNode = "_cardNode";

		public static readonly StringName _enchantmentSparkles = "_enchantmentSparkles";

		public static readonly StringName _enchantmentIcon = "_enchantmentIcon";

		public static readonly StringName _enchantmentLabel = "_enchantmentLabel";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private static readonly StringName _progress = new StringName("progress");

	private Tween? _tween;

	private CancellationTokenSource? _cts;

	private CardModel _cardModel;

	private NCard _cardNode;

	private GpuParticles2D _enchantmentSparkles;

	private TextureRect _enchantmentIcon;

	private MegaLabel _enchantmentLabel;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_card_enchant");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	[Export(PropertyHint.None, "")]
	public Curve? EmbossCurve { get; set; }

	public static NCardEnchantVfx? Create(CardModel card)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (!LocalContext.IsMine(card))
		{
			return null;
		}
		NCardEnchantVfx nCardEnchantVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardEnchantVfx>(PackedScene.GenEditState.Disabled);
		nCardEnchantVfx._cardModel = card;
		return nCardEnchantVfx;
	}

	public override void _Ready()
	{
		_enchantmentSparkles = GetNode<GpuParticles2D>("%EnchantmentAppearSparkles");
		_enchantmentIcon = GetNode<TextureRect>("%EnchantmentInViewport/Icon");
		_enchantmentLabel = GetNode<MegaLabel>("%EnchantmentInViewport/Label");
		_enchantmentIcon.Texture = _cardModel.Enchantment.Icon;
		_enchantmentLabel.SetTextAutoSize(_cardModel.Enchantment.DisplayAmount.ToString());
		_enchantmentLabel.Visible = _cardModel.Enchantment.ShowAmount;
		_cardNode = NCard.Create(_cardModel);
		this.AddChildSafely(_cardNode);
		MoveChild(_cardNode, 0);
		_cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		_cardNode.EnchantmentTab.Visible = false;
		_cardNode.EnchantmentVfxOverride.Visible = true;
		Viewport node = GetNode<Viewport>("%EnchantmentViewport");
		_cardNode.EnchantmentVfxOverride.Texture = node.GetTexture();
		TaskHelper.RunSafely(PlayAnimation());
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
		_cts?.Cancel();
		_cts?.Dispose();
		if (_cardNode.IsValid() && IsAncestorOf(_cardNode))
		{
			_cardNode.QueueFreeSafely();
		}
	}

	private async Task PlayAnimation()
	{
		_cts = new CancellationTokenSource();
		((ShaderMaterial)_cardNode.EnchantmentVfxOverride.Material).SetShaderParameter(_progress, 0f);
		_tween = CreateTween();
		SfxCmd.Play("event:/sfx/ui/enchant_shimmer");
		_tween.TweenProperty(_cardNode.EnchantmentVfxOverride, "material:shader_parameter/progress", 1f, 1.0).SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Quad);
		_tween.Parallel().TweenCallback(Callable.From(() => _enchantmentSparkles.Emitting = true)).SetDelay(0.20000000298023224);
		_tween.Parallel().TweenProperty(_enchantmentSparkles, "position:x", _enchantmentSparkles.Position.X + 72f, 0.4000000059604645).SetDelay(0.20000000298023224);
		await ToSignal(_tween, Tween.SignalName.Finished);
		await Cmd.Wait(1f, _cts.Token);
		CardModel model = _cardNode.Model;
		if (_cardNode.IsInsideTree() && model.Pile == null)
		{
			_tween = CreateTween();
			_tween.TweenProperty(this, "scale", Vector2.Zero, 0.15000000596046448);
			await ToSignal(_tween, Tween.SignalName.Finished);
		}
		else if (_cardNode.IsInsideTree())
		{
			Vector2 targetPosition = model.Pile.Type.GetTargetPosition(_cardNode);
			NCardFlyVfx nCardFlyVfx = NCardFlyVfx.Create(_cardNode, targetPosition, isAddingToPile: false, model.Owner.Character.TrailPath);
			NRun.Instance?.GlobalUi.TopBar.TrailContainer.AddChildSafely(nCardFlyVfx);
			if (nCardFlyVfx.SwooshAwayCompletion != null)
			{
				await nCardFlyVfx.SwooshAwayCompletion.Task;
			}
		}
		this.QueueFreeSafely();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
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
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.EmbossCurve)
		{
			EmbossCurve = VariantUtils.ConvertTo<Curve>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._enchantmentSparkles)
		{
			_enchantmentSparkles = VariantUtils.ConvertTo<GpuParticles2D>(in value);
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
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.EmbossCurve)
		{
			value = VariantUtils.CreateFrom<Curve>(EmbossCurve);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
			return true;
		}
		if (name == PropertyName._enchantmentSparkles)
		{
			value = VariantUtils.CreateFrom(in _enchantmentSparkles);
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
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EmbossCurve, PropertyHint.ResourceType, "Curve", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentSparkles, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enchantmentLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.EmbossCurve, Variant.From<Curve>(EmbossCurve));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
		info.AddProperty(PropertyName._enchantmentSparkles, Variant.From(in _enchantmentSparkles));
		info.AddProperty(PropertyName._enchantmentIcon, Variant.From(in _enchantmentIcon));
		info.AddProperty(PropertyName._enchantmentLabel, Variant.From(in _enchantmentLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.EmbossCurve, out var value))
		{
			EmbossCurve = value.As<Curve>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value3))
		{
			_cardNode = value3.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentSparkles, out var value4))
		{
			_enchantmentSparkles = value4.As<GpuParticles2D>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentIcon, out var value5))
		{
			_enchantmentIcon = value5.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._enchantmentLabel, out var value6))
		{
			_enchantmentLabel = value6.As<MegaLabel>();
		}
	}
}
