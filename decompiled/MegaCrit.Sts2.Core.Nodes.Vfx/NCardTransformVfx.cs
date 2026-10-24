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
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// Manages full card transformations that take place in the center of the screen
/// (ie transformations for cards in the deck/draw/discard)
/// Centers the card and plays the transform animation. The actual transform animation
/// logic lives in NCardTransformShineVfx
/// </summary>
[ScriptPath("res://src/Core/Nodes/Vfx/NCardTransformVfx.cs")]
public class NCardTransformVfx : Node2D
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Node2D.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Node2D.PropertyName
	{
		/// <summary>
		/// Cached name for the '_tween' field.
		/// </summary>
		public static readonly StringName _tween = "_tween";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Node2D.SignalName
	{
	}

	private Tween? _tween;

	private CardModel _startCard;

	private CardModel _endCard;

	private IEnumerable<RelicModel>? _relicsToFlash;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_card_transform");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NCardTransformVfx? Create(CardModel startCard, CardModel endCard, IEnumerable<RelicModel>? relicsToFlash)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardTransformVfx nCardTransformVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardTransformVfx>(PackedScene.GenEditState.Disabled);
		nCardTransformVfx._startCard = startCard;
		nCardTransformVfx._endCard = endCard;
		nCardTransformVfx._relicsToFlash = relicsToFlash;
		return nCardTransformVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlayAnimation());
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

	public override void _ExitTree()
	{
		_tween?.Kill();
	}

	private async Task PlayAnimation()
	{
		SfxCmd.Play("event:/sfx/ui/cards/card_transform");
		Control node = GetNode<Control>("%CardContainer");
		NCard cardNode = NCard.Create(_startCard);
		node.AddChildSafely(cardNode);
		cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		_tween = CreateTween();
		_tween.TweenProperty(cardNode, "scale", Vector2.One * 1f, 0.25).From(Vector2.Zero).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		if (!(await WaitAndInterruptIfNecessary(0.75f, cardNode)))
		{
			this.QueueFreeSafely();
			return;
		}
		NCardTransformShineVfx nCardTransformShineVfx = NCardTransformShineVfx.Create(cardNode, _endCard, _relicsToFlash);
		if (nCardTransformShineVfx != null)
		{
			await nCardTransformShineVfx.PlayAnimation();
		}
		if (!(await WaitAndInterruptIfNecessary(0.3f, cardNode)))
		{
			this.QueueFreeSafely();
			return;
		}
		if (_relicsToFlash != null)
		{
			foreach (RelicModel item in _relicsToFlash)
			{
				item.Flash();
				cardNode.FlashRelicOnCard(item);
			}
		}
		if (!(await WaitAndInterruptIfNecessary(0.5f, cardNode)))
		{
			this.QueueFreeSafely();
			return;
		}
		if (_endCard.Pile == null)
		{
			this.QueueFreeSafely();
			return;
		}
		cardNode.Reparent(this);
		cardNode.Position = Vector2.Zero;
		NCardFlyVfx nCardFlyVfx = NCardFlyVfx.Create(cardNode, _endCard.Pile.Type, isAddingToPile: false, _endCard.Owner.Character.TrailPath);
		((_endCard.Pile.Type != PileType.Deck) ? NCombatRoom.Instance?.CombatVfxContainer : NRun.Instance?.GlobalUi.TopBar.TrailContainer)?.AddChildSafely(nCardFlyVfx);
		if (nCardFlyVfx?.SwooshAwayCompletion != null)
		{
			await nCardFlyVfx.SwooshAwayCompletion.Task;
		}
		this.QueueFreeSafely();
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
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
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
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._tween, out var value))
		{
			_tween = value.As<Tween>();
		}
	}
}
