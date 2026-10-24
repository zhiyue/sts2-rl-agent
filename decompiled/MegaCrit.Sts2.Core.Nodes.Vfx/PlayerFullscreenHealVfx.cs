using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes.Vfx;

/// <summary>
/// Creates fullscreen VFX that occurs when healing at an event.
/// </summary>
public static class PlayerFullscreenHealVfx
{
	private static readonly string _scenePath = SceneHelper.GetScenePath("vfx/vfx_cross_heal_fullscreen");

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>(_scenePath);

	public static void Play(Player player, decimal healAmount, Control? vfxContainer)
	{
		if (!TestMode.IsOn && !(healAmount < 1m) && vfxContainer != null)
		{
			float num = Ease.QuadOut((float)(healAmount / (decimal)player.Creature.MaxHp));
			Color green = StsColors.green;
			green.A = Mathf.Max(num * 0.8f, 0.4f);
			NSmokyVignetteVfx nSmokyVignetteVfx = NSmokyVignetteVfx.Create(green, new Color(0f, 1f, 0f, 0.33f));
			if (nSmokyVignetteVfx != null)
			{
				vfxContainer.AddChildSafely(nSmokyVignetteVfx);
			}
			NVfxParticleSystem nVfxParticleSystem = PreloadManager.Cache.GetScene(_scenePath).Instantiate<NVfxParticleSystem>(PackedScene.GenEditState.Disabled);
			Rect2 viewportRect = NGame.Instance.GetViewportRect();
			nVfxParticleSystem.GlobalPosition = viewportRect.Size * 0.5f;
			GpuParticles2D node = nVfxParticleSystem.GetNode<GpuParticles2D>("beam");
			ParticleProcessMaterial particleProcessMaterial = (ParticleProcessMaterial)node.ProcessMaterial;
			particleProcessMaterial.EmissionBoxExtents = new Vector3(viewportRect.Size.X / 2f, viewportRect.Size.Y / 2f, 1f);
			int amount = Mathf.RoundToInt(num * 40f + 10f);
			node.Amount = amount;
			vfxContainer.AddChildSafely(nVfxParticleSystem);
		}
	}
}
