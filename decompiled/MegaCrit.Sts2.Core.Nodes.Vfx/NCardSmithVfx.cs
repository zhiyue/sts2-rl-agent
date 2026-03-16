using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardSmithVfx.cs")]
public class NCardSmithVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public static readonly StringName Create = "Create";

		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName PlaySubParticles = "PlaySubParticles";
	}

	public new class PropertyName : Node2D.PropertyName
	{
		public static readonly StringName SfxVolume = "SfxVolume";

		public static readonly StringName _tween = "_tween";

		public static readonly StringName _willPlaySfx = "_willPlaySfx";

		public static readonly StringName _cardNode = "_cardNode";

		public static readonly StringName _cardContainer = "_cardContainer";
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private Tween? _tween;

	public const string smithSfx = "card_smith.mp3";

	private bool _willPlaySfx = true;

	private readonly List<CardModel> _cards = new List<CardModel>();

	private NCard? _cardNode;

	private Control _cardContainer;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_card_smith");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2]
	{
		ScenePath,
		TmpSfx.GetPath("card_smith.mp3")
	});

	public float SfxVolume { get; set; } = 1f;

	public static NCardSmithVfx? Create(IEnumerable<CardModel> cards, bool playSfx = true)
	{
		NCardSmithVfx nCardSmithVfx = Create();
		if (nCardSmithVfx == null)
		{
			return null;
		}
		nCardSmithVfx._cards.AddRange(cards);
		nCardSmithVfx._willPlaySfx = playSfx;
		return nCardSmithVfx;
	}

	public static NCardSmithVfx? Create(NCard card, bool playSfx = true)
	{
		NCardSmithVfx nCardSmithVfx = Create();
		if (nCardSmithVfx == null)
		{
			return null;
		}
		nCardSmithVfx._willPlaySfx = playSfx;
		nCardSmithVfx._cardNode = card;
		return nCardSmithVfx;
	}

	public static NCardSmithVfx? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardSmithVfx>(PackedScene.GenEditState.Disabled);
	}

	public override void _Ready()
	{
		if (_cardNode != null)
		{
			base.GlobalPosition = _cardNode.GlobalPosition;
			base.GlobalScale = _cardNode.Scale;
			TaskHelper.RunSafely(PlayAnimation());
		}
		else if (_cards.Count > 0)
		{
			TaskHelper.RunSafely(PlayAnimation(_cards));
		}
		else
		{
			TaskHelper.RunSafely(PlayAnimation());
		}
	}

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task PlayAnimation()
	{
		if (_willPlaySfx)
		{
			NDebugAudioManager.Instance?.Play("card_smith.mp3", SfxVolume, PitchVariance.Small);
		}
		_tween = CreateTween();
		_tween.Parallel().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark1"));
		}));
		_tween.TweenInterval(0.25);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark2"));
		}));
		_tween.TweenInterval(0.25);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark3"));
		}));
		_tween.TweenInterval(0.4000000059604645);
		await ToSignal(_tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
	}

	private async Task PlayAnimation(IEnumerable<CardModel> cards)
	{
		Control node = GetNode<Control>("%CardContainer");
		List<NCard> cardNodes = new List<NCard>();
		foreach (CardModel card in cards)
		{
			NCard nCard = NCard.Create(card);
			node.AddChildSafely(nCard);
			nCard.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
			cardNodes.Add(nCard);
		}
		_tween = CreateTween();
		foreach (NCard item in cardNodes)
		{
			_tween.Parallel().TweenProperty(item, "scale", Vector2.One * 1f, 0.25).From(Vector2.Zero)
				.SetEase(Tween.EaseType.Out)
				.SetTrans(Tween.TransitionType.Cubic);
		}
		if (_willPlaySfx)
		{
			_tween.Chain().TweenCallback(Callable.From(delegate
			{
				NDebugAudioManager.Instance?.Play("card_smith.mp3", SfxVolume, PitchVariance.Small);
			}));
		}
		_tween.Parallel().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark1"));
		}));
		_tween.Parallel().TweenCallback(Callable.From(delegate
		{
			NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 180f + Rng.Chaotic.NextFloat(-10f, 10f));
		}));
		foreach (NCard item2 in cardNodes)
		{
			_tween.Parallel().TweenProperty(item2, "rotation_degrees", 20, 0.05000000074505806).SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.Out);
		}
		_tween.TweenInterval(0.25);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark2"));
		}));
		foreach (NCard item3 in cardNodes)
		{
			_tween.Parallel().TweenProperty(item3, "rotation_degrees", -10, 0.05000000074505806).SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.Out);
		}
		_tween.Parallel().TweenCallback(Callable.From(delegate
		{
			NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 180f + Rng.Chaotic.NextFloat(-10f, 10f));
		}));
		_tween.TweenInterval(0.25);
		_tween.Chain().TweenCallback(Callable.From(delegate
		{
			PlaySubParticles(GetNode<Control>("Spark3"));
		}));
		foreach (NCard item4 in cardNodes)
		{
			_tween.Parallel().TweenProperty(item4, "rotation_degrees", 5, 0.05000000074505806).SetTrans(Tween.TransitionType.Elastic)
				.SetEase(Tween.EaseType.Out);
		}
		_tween.Parallel().TweenCallback(Callable.From(delegate
		{
			NGame.Instance?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short, 180f + Rng.Chaotic.NextFloat(-10f, 10f));
		}));
		_tween.TweenInterval(0.4000000059604645);
		await ToSignal(_tween, Tween.SignalName.Finished);
		if (cardNodes[0].IsInsideTree() && _cards[0].Pile == null)
		{
			_tween = CreateTween();
			foreach (NCard item5 in cardNodes)
			{
				_tween.SetParallel().TweenProperty(item5, "scale", Vector2.Zero, 0.15000000596046448);
			}
			await ToSignal(_tween, Tween.SignalName.Finished);
		}
		else
		{
			if (!cardNodes[0].IsInsideTree())
			{
				return;
			}
			for (int i = 0; i < cardNodes.Count; i++)
			{
				Vector2 targetPosition = cardNodes[i].Model.Pile.Type.GetTargetPosition(cardNodes[i]);
				Vector2 globalPosition = cardNodes[i].GlobalPosition;
				cardNodes[i].Reparent(this);
				cardNodes[i].GlobalPosition = globalPosition;
				NCardFlyVfx nCardFlyVfx = NCardFlyVfx.Create(cardNodes[i], targetPosition, isAddingToPile: false, cardNodes[i].Model.Owner.Character.TrailPath);
				NRun.Instance?.GlobalUi.TopBar.TrailContainer.AddChildSafely(nCardFlyVfx);
				if (nCardFlyVfx.SwooshAwayCompletion != null && i == cardNodes.Count - 1)
				{
					await nCardFlyVfx.SwooshAwayCompletion.Task;
				}
			}
			this.QueueFreeSafely();
		}
	}

	private void PlaySubParticles(Node node)
	{
		foreach (CpuParticles2D item in node.GetChildren().OfType<CpuParticles2D>())
		{
			item.Emitting = true;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "card", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false),
			new PropertyInfo(Variant.Type.Bool, "playSfx", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.PlaySubParticles, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NCardSmithVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCardSmithVfx>(Create());
			return true;
		}
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
		if (method == MethodName.PlaySubParticles && args.Count == 1)
		{
			PlaySubParticles(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<NCardSmithVfx>(Create(VariantUtils.ConvertTo<NCard>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1])));
			return true;
		}
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NCardSmithVfx>(Create());
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
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.PlaySubParticles)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.SfxVolume)
		{
			SfxVolume = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._willPlaySfx)
		{
			_willPlaySfx = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			_cardNode = VariantUtils.ConvertTo<NCard>(in value);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			_cardContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SfxVolume)
		{
			value = VariantUtils.CreateFrom<float>(SfxVolume);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		if (name == PropertyName._willPlaySfx)
		{
			value = VariantUtils.CreateFrom(in _willPlaySfx);
			return true;
		}
		if (name == PropertyName._cardNode)
		{
			value = VariantUtils.CreateFrom(in _cardNode);
			return true;
		}
		if (name == PropertyName._cardContainer)
		{
			value = VariantUtils.CreateFrom(in _cardContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._willPlaySfx, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardNode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName.SfxVolume, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.SfxVolume, Variant.From<float>(SfxVolume));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
		info.AddProperty(PropertyName._willPlaySfx, Variant.From(in _willPlaySfx));
		info.AddProperty(PropertyName._cardNode, Variant.From(in _cardNode));
		info.AddProperty(PropertyName._cardContainer, Variant.From(in _cardContainer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.SfxVolume, out var value))
		{
			SfxVolume = value.As<float>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value2))
		{
			_tween = value2.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._willPlaySfx, out var value3))
		{
			_willPlaySfx = value3.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._cardNode, out var value4))
		{
			_cardNode = value4.As<NCard>();
		}
		if (info.TryGetProperty(PropertyName._cardContainer, out var value5))
		{
			_cardContainer = value5.As<Control>();
		}
	}
}
