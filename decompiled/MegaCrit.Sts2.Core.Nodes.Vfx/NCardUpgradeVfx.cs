using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NCardUpgradeVfx.cs")]
public class NCardUpgradeVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _ExitTree = "_ExitTree";
	}

	public new class PropertyName : Node2D.PropertyName
	{
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private CardModel _card;

	private CancellationTokenSource? _cts;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_card_upgrade");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NCardUpgradeVfx? Create(CardModel card)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCardUpgradeVfx nCardUpgradeVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NCardUpgradeVfx>(PackedScene.GenEditState.Disabled);
		nCardUpgradeVfx._card = card;
		return nCardUpgradeVfx;
	}

	public override void _Ready()
	{
		TaskHelper.RunSafely(PlayAnimation());
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		_cts?.Dispose();
	}

	private async Task PlayAnimation()
	{
		_cts = new CancellationTokenSource();
		NCard cardNode = NCard.Create(_card);
		this.AddChildSafely(cardNode);
		MoveChild(cardNode, 0);
		cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
		GetNode<CpuParticles2D>("%Particle").Emitting = true;
		Tween tween = CreateTween();
		tween.TweenProperty(cardNode, "scale", Vector2.One * 1f, 0.25).From(Vector2.Zero).SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Cubic);
		await Cmd.Wait(1.75f, _cts.Token);
		Vector2 targetPosition = _card.Pile.Type.GetTargetPosition(cardNode);
		NCardFlyVfx nCardFlyVfx = NCardFlyVfx.Create(cardNode, targetPosition, isAddingToPile: false, _card.Owner.Character.TrailPath);
		((_card.Pile.Type != PileType.Deck) ? NCombatRoom.Instance?.CombatVfxContainer : NRun.Instance?.GlobalUi.TopBar.TrailContainer)?.AddChildSafely(nCardFlyVfx);
		if (nCardFlyVfx?.SwooshAwayCompletion != null)
		{
			await nCardFlyVfx.SwooshAwayCompletion.Task;
		}
		if (!_cts.IsCancellationRequested)
		{
			this.QueueFreeSafely();
		}
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
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
