using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Ftue;

/// <summary>
/// This is a popup that lets you know that you have unlocked Multiplayer Ascensions and briefly explains it.
/// This is NOT a true FTUE as disabling tutorials will not prevent this from showing up.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Ftue/NAscensionMultiplayerFtue.cs")]
public class NAscensionMultiplayerFtue : NFtue
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : NFtue.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the 'Create' method.
		/// </summary>
		public static readonly StringName Create = "Create";

		/// <summary>
		/// Cached name for the 'CloseFtue' method.
		/// </summary>
		public new static readonly StringName CloseFtue = "CloseFtue";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : NFtue.PropertyName
	{
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : NFtue.SignalName
	{
	}

	public const string id = "ascension_multiplayer_ftue";

	private static readonly string _scenePath = SceneHelper.GetScenePath("ftue/ascension_multiplayer_ftue");

	public override void _Ready()
	{
		GetNode<MegaLabel>("%Header").SetTextAutoSize(new LocString("ftues", "ASCENSION_MULTIPLAYER_FTUE_TITLE").GetFormattedText());
		GetNode<MegaRichTextLabel>("%Description").SetTextAutoSize(new LocString("ftues", "ASCENSION_MULTIPLAYER_FTUE_DESCRIPTION").GetFormattedText());
		GetNode<MegaRichTextLabel>("%Disclaimer").SetTextAutoSize(new LocString("ftues", "ASCENSION_MULTIPLAYER_FTUE_DISCLAIMER").GetFormattedText());
		GetNode<NButton>("%FtueConfirmButton").Connect(NClickableControl.SignalName.Released, Callable.From((Action<NButton>)CloseFtue));
		Tween tween = CreateTween().SetParallel();
		Color modulate = base.Modulate;
		modulate.A = 0f;
		base.Modulate = modulate;
		tween.TweenProperty(this, "position:y", base.Position.Y, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back)
			.From(base.Position.Y + 100f)
			.SetDelay(1.0);
		tween.TweenProperty(this, "modulate:a", 1f, 0.3).SetEase(Tween.EaseType.Out).SetDelay(1.0);
	}

	public static NAscensionMultiplayerFtue? Create()
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		return PreloadManager.Cache.GetScene(_scenePath).Instantiate<NAscensionMultiplayerFtue>(PackedScene.GenEditState.Disabled);
	}

	private void CloseFtue(NButton _)
	{
		SaveManager.Instance.MarkFtueAsComplete("ascension_multiplayer_ftue");
		CloseFtue();
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Create, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		list.Add(new MethodInfo(MethodName.CloseFtue, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NAscensionMultiplayerFtue>(Create());
			return true;
		}
		if (method == MethodName.CloseFtue && args.Count == 1)
		{
			CloseFtue(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Create && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<NAscensionMultiplayerFtue>(Create());
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName.Create)
		{
			return true;
		}
		if (method == MethodName.CloseFtue)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
	}
}
