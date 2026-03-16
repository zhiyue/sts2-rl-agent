using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

[ScriptPath("res://src/Core/Nodes/Vfx/NMonsterDeathVfx.cs")]
public class NMonsterDeathVfx : Node2D
{
	public new class MethodName : Node2D.MethodName
	{
	}

	public new class PropertyName : Node2D.PropertyName
	{
	}

	public new class SignalName : Node2D.SignalName
	{
	}

	private const string _deathSfx = "event:/sfx/enemy/enemy_fade";

	private const float _refLength = 0.1f;

	private const float _refTweenDuration = 2.5f;

	private const float _minTweenDuration = 2.5f;

	private const float _tweenStartValue = 0f;

	private const string _shaderParamThreshold = "shader_parameter/threshold";

	private List<NCreature> _creatureNodes;

	private List<Control> _hitboxes;

	private CancellationToken _cancelToken;

	private static string ScenePath => SceneHelper.GetScenePath("vfx/vfx_monster_death");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(ScenePath);

	public static NMonsterDeathVfx? Create(NCreature creatureNode, CancellationToken cancelToken)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			return null;
		}
		if (cancelToken.IsCancellationRequested)
		{
			return null;
		}
		NMonsterDeathVfx nMonsterDeathVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NMonsterDeathVfx>(PackedScene.GenEditState.Disabled);
		int num = 1;
		List<NCreature> list = new List<NCreature>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<NCreature> span = CollectionsMarshal.AsSpan(list);
		int index = 0;
		span[index] = creatureNode;
		nMonsterDeathVfx._creatureNodes = list;
		nMonsterDeathVfx._cancelToken = cancelToken;
		index = 1;
		List<Control> list2 = new List<Control>(index);
		CollectionsMarshal.SetCount(list2, index);
		Span<Control> span2 = CollectionsMarshal.AsSpan(list2);
		num = 0;
		span2[num] = creatureNode.Hitbox;
		nMonsterDeathVfx._hitboxes = list2;
		return nMonsterDeathVfx;
	}

	public static NMonsterDeathVfx? Create(List<NCreature> creatureNodes)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NMonsterDeathVfx nMonsterDeathVfx = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NMonsterDeathVfx>(PackedScene.GenEditState.Disabled);
		nMonsterDeathVfx._creatureNodes = creatureNodes;
		nMonsterDeathVfx._cancelToken = default(CancellationToken);
		nMonsterDeathVfx._hitboxes = creatureNodes.Select((NCreature c) => c.Hitbox).ToList();
		return nMonsterDeathVfx;
	}

	public async Task PlayVfx()
	{
		if (_cancelToken.IsCancellationRequested)
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			this.QueueFreeSafely();
			return;
		}
		SubViewport node = GetNode<SubViewport>("Viewport");
		Rect2? rect = null;
		for (int i = 0; i < _creatureNodes.Count; i++)
		{
			NCreature nCreature = _creatureNodes[i];
			NCreatureVisuals visuals = nCreature.Visuals;
			MegaSprite spineBody = visuals.SpineBody;
			Vector2 scale = visuals.Body.Scale;
			Vector2 vector = nCreature.Entity.Monster?.ExtraDeathVfxPadding ?? MonsterModel.defaultDeathVfxPadding;
			if (visuals.HasSpineAnimation)
			{
				Vector2 scale2 = instance.SceneContainer.Scale;
				Rect2 bounds = spineBody.GetSkeleton().GetBounds();
				Rect2 rect2 = new Rect2(bounds.Position * scale * scale2, bounds.Size * scale * scale2);
				Vector2 vector2 = new Vector2(Math.Min(rect2.Position.X, rect2.End.X), Math.Min(rect2.Position.Y, rect2.End.Y));
				Vector2 vector3 = new Vector2(Math.Max(rect2.Position.X, rect2.End.X), Math.Max(rect2.Position.Y, rect2.End.Y));
				Vector2 vector4 = vector3 - vector2;
				Vector2 vector5 = vector4 * vector;
				Vector2 vector6 = (vector5 - vector4) * 0.5f;
				Rect2 rect3 = new Rect2(visuals.Body.GlobalPosition + vector2 - vector6, vector5);
				rect = (rect.HasValue ? new Rect2?(rect.Value.Merge(rect3)) : new Rect2?(rect3));
			}
			else
			{
				Control control = _hitboxes[i];
				Vector2 vector7 = control.Size * control.Scale;
				Vector2 vector8 = vector7 * vector;
				Vector2 vector9 = (vector8 - vector7) * 0.5f;
				Rect2 rect4 = new Rect2(control.GlobalPosition - vector9, vector8);
				rect = (rect.HasValue ? new Rect2?(rect.Value.Merge(rect4)) : new Rect2?(rect4));
			}
		}
		base.GlobalPosition = rect.Value.Position + rect.Value.Size * 0.5f;
		Vector2 size = rect.Value.Size;
		int num = Mathf.RoundToInt(Mathf.Max(size.X, size.Y));
		size = new Vector2(num, num);
		node.Size = new Vector2I(num, num) * 2;
		node.Size2DOverride = new Vector2I(num, num);
		GetNode<Sprite2D>("Visual").Scale /= 2f * instance.SceneContainer.Scale;
		Vector2 vector10 = base.GlobalPosition - size * 0.5f;
		foreach (NCreature creatureNode in _creatureNodes)
		{
			if (GodotObject.IsInstanceValid(creatureNode.Visuals.Body))
			{
				Vector2 globalPosition = creatureNode.Visuals.Body.GlobalPosition;
				creatureNode.Visuals.Body.Reparent(node);
				if (GodotObject.IsInstanceValid(creatureNode.Visuals.Body))
				{
					creatureNode.Visuals.Body.Position = globalPosition - vector10;
				}
			}
		}
		node.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
		await PlayVfxInternal();
	}

	private async Task PlayVfxInternal()
	{
		SfxCmd.Play("event:/sfx/enemy/enemy_fade");
		SubViewport node = GetNode<SubViewport>("Viewport");
		Sprite2D node2 = GetNode<Sprite2D>("Visual");
		CpuParticles2D node3 = GetNode<CpuParticles2D>("%Particles");
		node3.EmissionSphereRadius = (float)node.Size.Y / 4f;
		node3.Emitting = true;
		float num = 0.1f * GetViewportRect().Size.X;
		float num2 = Math.Min((float)node.Size.X / num * 2.5f, 2.5f);
		using Tween tween = CreateTween();
		tween.TweenProperty(node2.Material, "shader_parameter/threshold", 0f, num2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFreeSafely();
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
