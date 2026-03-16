using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NTransition.cs")]
public class NTransition : ColorRect
{
	public new class MethodName : ColorRect.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : ColorRect.PropertyName
	{
		public static readonly StringName InTransition = "InTransition";

		public static readonly StringName _initialGradientYPosition = "_initialGradientYPosition";

		public static readonly StringName _targetGradientYPosition = "_targetGradientYPosition";

		public static readonly StringName _gradientTransition = "_gradientTransition";

		public static readonly StringName _simpleTransition = "_simpleTransition";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : ColorRect.SignalName
	{
	}

	private static readonly StringName _threshold = new StringName("threshold");

	private const string _fightTransitionPath = "res://materials/transitions/fight_transition_mat.tres";

	private const string _fadeTransitionPath = "res://materials/transitions/fade_transition_mat.tres";

	private float _initialGradientYPosition;

	private float _targetGradientYPosition;

	private Control _gradientTransition;

	private Control _simpleTransition;

	private Tween? _tween;

	public bool InTransition { get; private set; }

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "res://materials/transitions/fight_transition_mat.tres", "res://materials/transitions/fade_transition_mat.tres" });

	public override void _Ready()
	{
		_gradientTransition = GetNode<Control>("GradientTransition");
		_simpleTransition = GetNode<Control>("SimpleTransition");
		_initialGradientYPosition = _gradientTransition.Position.Y;
		_targetGradientYPosition = 0f;
	}

	public async Task FadeOut(float time = 0.8f, string transitionPath = "res://materials/transitions/fade_transition_mat.tres", CancellationToken? cancelToken = null)
	{
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			InTransition = true;
			base.Visible = false;
			return;
		}
		InTransition = true;
		Control simpleTransition = _simpleTransition;
		Color modulate = _simpleTransition.Modulate;
		modulate.A = 0f;
		simpleTransition.Modulate = modulate;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		_tween.TweenProperty(_simpleTransition, "modulate:a", 1f, time).SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Quad);
		if (!(base.Material is ShaderMaterial shaderMaterial))
		{
			Log.Warn("NTransition.Material is null or not a ShaderMaterial (actual: " + (base.Material?.GetType().Name ?? "null") + "). Skipping transition.");
		}
		else
		{
			if (shaderMaterial.GetShaderParameter(_threshold).AsInt32() == 1)
			{
				return;
			}
			base.Material = PreloadManager.Cache.GetMaterial(transitionPath);
			Material material = base.Material;
			if (!(material is ShaderMaterial transitionMaterial))
			{
				Log.Warn("NTransition.Material failed to load from cache (path: " + transitionPath + "). Skipping transition.");
				return;
			}
			transitionMaterial.SetShaderParameter(_threshold, 0);
			double t = 0.0;
			while (t < (double)time)
			{
				if (cancelToken.HasValue && cancelToken.GetValueOrDefault().IsCancellationRequested)
				{
					_tween?.FastForwardToCompletion();
					break;
				}
				transitionMaterial.SetShaderParameter(_threshold, 1.0 - ((double)time - t));
				t += GetProcessDeltaTime();
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
			base.MouseFilter = MouseFilterEnum.Stop;
			transitionMaterial.SetShaderParameter(_threshold, 1);
		}
	}

	public async Task FadeIn(float time = 0.8f, string transitionPath = "res://materials/transitions/fade_transition_mat.tres", CancellationToken? cancelToken = null)
	{
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			base.Visible = false;
			InTransition = false;
			base.MouseFilter = MouseFilterEnum.Ignore;
			return;
		}
		_tween?.Kill();
		Control simpleTransition = _simpleTransition;
		Color modulate = _simpleTransition.Modulate;
		modulate.A = 0f;
		simpleTransition.Modulate = modulate;
		if (!(base.Material is ShaderMaterial shaderMaterial))
		{
			Log.Warn("NTransition.Material is null or not a ShaderMaterial (actual: " + (base.Material?.GetType().Name ?? "null") + "). Skipping transition.");
			InTransition = false;
			return;
		}
		if (shaderMaterial.GetShaderParameter(_threshold).AsInt32() == 0)
		{
			InTransition = false;
			return;
		}
		base.Material = PreloadManager.Cache.GetMaterial(transitionPath);
		Material material = base.Material;
		if (!(material is ShaderMaterial transitionMaterial))
		{
			Log.Warn("NTransition.Material failed to load from cache (path: " + transitionPath + "). Skipping transition.");
			InTransition = false;
			return;
		}
		transitionMaterial.SetShaderParameter(_threshold, 1);
		base.MouseFilter = MouseFilterEnum.Ignore;
		double t = 0.0;
		while (t < (double)time)
		{
			if (cancelToken.HasValue && cancelToken.GetValueOrDefault().IsCancellationRequested)
			{
				_tween?.FastForwardToCompletion();
				break;
			}
			transitionMaterial.SetShaderParameter(_threshold, Ease.CubicIn((float)((double)time - t)));
			t += GetProcessDeltaTime();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (t / (double)time > 0.75)
			{
				InTransition = false;
			}
		}
		InTransition = false;
		transitionMaterial.SetShaderParameter(_threshold, 0);
		base.MouseFilter = MouseFilterEnum.Ignore;
	}

	public async Task RoomFadeOut()
	{
		InTransition = true;
		if (!TestMode.IsOn && SaveManager.Instance.PrefsSave.FastMode != FastModeType.Instant)
		{
			Control simpleTransition = _simpleTransition;
			Color modulate = _simpleTransition.Modulate;
			modulate.A = 0f;
			simpleTransition.Modulate = modulate;
			Control gradientTransition = _gradientTransition;
			modulate = _gradientTransition.Modulate;
			modulate.A = 1f;
			gradientTransition.Modulate = modulate;
			Control gradientTransition2 = _gradientTransition;
			Vector2 position = _gradientTransition.Position;
			position.Y = _initialGradientYPosition;
			gradientTransition2.Position = position;
			_tween?.Kill();
			_tween = CreateTween().SetParallel();
			if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Normal)
			{
				_tween.TweenProperty(_gradientTransition, "position:y", _targetGradientYPosition, 0.6).SetDelay(0.5);
				_tween.TweenProperty(_simpleTransition, "modulate:a", 1f, 0.6).SetDelay(0.5);
			}
			else if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast)
			{
				_tween.TweenProperty(_simpleTransition, "modulate:a", 1f, 0.3).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad)
					.SetDelay(0.3);
			}
			await ToSignal(_tween, Tween.SignalName.Finished);
		}
	}

	public async Task RoomFadeIn(bool showTransition = true)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		Color modulate;
		if (!showTransition || SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
		{
			Control simpleTransition = _simpleTransition;
			modulate = _simpleTransition.Modulate;
			modulate.A = 0f;
			simpleTransition.Modulate = modulate;
		}
		if (!(base.Material is ShaderMaterial shaderMaterial))
		{
			Log.Warn("NTransition.Material is null or not a ShaderMaterial (actual: " + (base.Material?.GetType().Name ?? "null") + "). Skipping transition.");
			InTransition = false;
			return;
		}
		shaderMaterial.SetShaderParameter(_threshold, 0);
		Control gradientTransition = _gradientTransition;
		modulate = _gradientTransition.Modulate;
		modulate.A = 0f;
		gradientTransition.Modulate = modulate;
		Control simpleTransition2 = _simpleTransition;
		modulate = _simpleTransition.Modulate;
		modulate.A = 1f;
		simpleTransition2.Modulate = modulate;
		_tween?.Kill();
		_tween = CreateTween().SetParallel();
		if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast)
		{
			_tween.TweenProperty(_simpleTransition, "modulate:a", 0f, 0.3);
			base.MouseFilter = MouseFilterEnum.Ignore;
		}
		else
		{
			_tween.TweenProperty(_simpleTransition, "modulate:a", 0f, 0.8);
			_tween.TweenCallback(Callable.From(delegate
			{
				base.MouseFilter = MouseFilterEnum.Ignore;
				InTransition = false;
			})).SetDelay(0.2);
		}
		await ToSignal(_tween, Tween.SignalName.Finished);
		InTransition = false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.InTransition)
		{
			InTransition = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._initialGradientYPosition)
		{
			_initialGradientYPosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._targetGradientYPosition)
		{
			_targetGradientYPosition = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._gradientTransition)
		{
			_gradientTransition = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._simpleTransition)
		{
			_simpleTransition = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InTransition)
		{
			value = VariantUtils.CreateFrom<bool>(InTransition);
			return true;
		}
		if (name == PropertyName._initialGradientYPosition)
		{
			value = VariantUtils.CreateFrom(in _initialGradientYPosition);
			return true;
		}
		if (name == PropertyName._targetGradientYPosition)
		{
			value = VariantUtils.CreateFrom(in _targetGradientYPosition);
			return true;
		}
		if (name == PropertyName._gradientTransition)
		{
			value = VariantUtils.CreateFrom(in _gradientTransition);
			return true;
		}
		if (name == PropertyName._simpleTransition)
		{
			value = VariantUtils.CreateFrom(in _simpleTransition);
			return true;
		}
		if (name == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom(in _tween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InTransition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._initialGradientYPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._targetGradientYPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._gradientTransition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._simpleTransition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.InTransition, Variant.From<bool>(InTransition));
		info.AddProperty(PropertyName._initialGradientYPosition, Variant.From(in _initialGradientYPosition));
		info.AddProperty(PropertyName._targetGradientYPosition, Variant.From(in _targetGradientYPosition));
		info.AddProperty(PropertyName._gradientTransition, Variant.From(in _gradientTransition));
		info.AddProperty(PropertyName._simpleTransition, Variant.From(in _simpleTransition));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.InTransition, out var value))
		{
			InTransition = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._initialGradientYPosition, out var value2))
		{
			_initialGradientYPosition = value2.As<float>();
		}
		if (info.TryGetProperty(PropertyName._targetGradientYPosition, out var value3))
		{
			_targetGradientYPosition = value3.As<float>();
		}
		if (info.TryGetProperty(PropertyName._gradientTransition, out var value4))
		{
			_gradientTransition = value4.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._simpleTransition, out var value5))
		{
			_simpleTransition = value5.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value6))
		{
			_tween = value6.As<Tween>();
		}
	}
}
