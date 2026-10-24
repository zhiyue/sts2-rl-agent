using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Helpers;

public static class SpineNodeExtensions
{
	private const int _spineReadyWarnThresholdFrames = 600;

	/// <summary>
	/// Invokes <paramref name="onReady" /> with <paramref name="sprite" />'s animation state once it
	/// exists, waiting across frames if necessary.
	///
	/// Godot runs _Ready() bottom-up (children before parents) and a SpineSprite builds its skeleton
	/// asynchronously, so a VFX node can run before the SpineSprite it drives has created its
	/// animation state. Driving the animation state in that window throws a NullReferenceException and
	/// leaves the VFX uninitialized (PRG-6982). Use this for any animation-state setup that runs from
	/// _Ready, whether <paramref name="sprite" /> is the node's parent or a child/sibling sub-sprite.
	///
	/// If the skeleton never loads (e.g. asset load failure, which NCreatureVisuals handles by
	/// disabling the spine), <paramref name="onReady" /> simply never runs, and a one-time warning is
	/// logged once the wait grows implausibly long. The wait also stops if <paramref name="host" /> or
	/// <paramref name="sprite" />'s node leaves the tree or is freed. The sprite can be freed
	/// independently of the host (e.g. NOrb frees and recreates its sprite on orb replacement while
	/// the NOrb node itself stays in the tree).
	/// </summary>
	public static void RunWhenSpineReady(this Node host, MegaSprite sprite, Action<MegaAnimationState> onReady)
	{
		TaskHelper.RunSafely(WaitForSpineReady(host, sprite, onReady));
	}

	private static async Task WaitForSpineReady(Node host, MegaSprite sprite, Action<MegaAnimationState> onReady)
	{
		int framesWaited = 0;
		while (GodotObject.IsInstanceValid(sprite.BoundObject) && !sprite.IsAnimationStateReady())
		{
			await host.AwaitProcessFrame();
			int num = framesWaited + 1;
			framesWaited = num;
			if (num == 600)
			{
				Log.Warn($"{host.Name}: still waiting for a SpineSprite skeleton after {framesWaited} " + "frames; its animation will not start until the skeleton loads (possible asset load failure).");
			}
		}
		if (GodotObject.IsInstanceValid(sprite.BoundObject))
		{
			onReady(sprite.GetAnimationState());
		}
	}
}
