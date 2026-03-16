using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

[ScriptPath("res://src/Core/Nodes/Vfx/Utilities/NHitStop.cs")]
public class NHitStop : Node
{
	public new class MethodName : Node.MethodName
	{
		public static readonly StringName DoHitStop = "DoHitStop";

		public static readonly StringName SetTimeScale = "SetTimeScale";

		public static readonly StringName EaseForStrength = "EaseForStrength";

		public static readonly StringName SecondsForDuration = "SecondsForDuration";
	}

	public new class PropertyName : Node.PropertyName
	{
	}

	public new class SignalName : Node.SignalName
	{
	}

	private const float _minTimeScale = 0.1f;

	private CancellationTokenSource? _cancelToken;

	public void DoHitStop(ShakeStrength strength, ShakeDuration duration)
	{
		_cancelToken?.Cancel();
		_cancelToken = new CancellationTokenSource();
		TaskHelper.RunSafely(HitStopTask(EaseForStrength(strength), SecondsForDuration(duration)));
	}

	private async Task HitStopTask(Ease.Functions easing, float seconds)
	{
		SetTimeScale(0.1f);
		ulong lastTicks = Time.GetTicksMsec();
		float timer = 0f;
		while (timer <= seconds)
		{
			await GetTree().ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			if (_cancelToken?.IsCancellationRequested ?? false)
			{
				return;
			}
			timer += (float)(Time.GetTicksMsec() - lastTicks) / 1000f;
			float num = Ease.Interpolate(timer / seconds, easing);
			float timeScale = Mathf.Min(0.1f + num * 0.9f, 1f);
			SetTimeScale(timeScale);
			lastTicks = Time.GetTicksMsec();
		}
		SetTimeScale(1f);
	}

	private void SetTimeScale(float timeScale)
	{
		Engine.SetTimeScale(timeScale);
	}

	private Ease.Functions EaseForStrength(ShakeStrength strength)
	{
		return strength switch
		{
			ShakeStrength.VeryWeak => Ease.Functions.CircIn, 
			ShakeStrength.Weak => Ease.Functions.SineIn, 
			ShakeStrength.Medium => Ease.Functions.QuadIn, 
			ShakeStrength.Strong => Ease.Functions.QuartIn, 
			ShakeStrength.TooMuch => Ease.Functions.ExpoIn, 
			_ => throw new ArgumentOutOfRangeException("strength", strength, null), 
		};
	}

	private float SecondsForDuration(ShakeDuration duration)
	{
		return duration switch
		{
			ShakeDuration.Short => 0.15f, 
			ShakeDuration.Normal => 0.3f, 
			ShakeDuration.Long => 0.6f, 
			ShakeDuration.Forever => 2f, 
			_ => throw new ArgumentOutOfRangeException("duration", duration, null), 
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.DoHitStop, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetTimeScale, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "timeScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.EaseForStrength, new PropertyInfo(Variant.Type.Int, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "strength", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SecondsForDuration, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DoHitStop && args.Count == 2)
		{
			DoHitStop(VariantUtils.ConvertTo<ShakeStrength>(in args[0]), VariantUtils.ConvertTo<ShakeDuration>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetTimeScale && args.Count == 1)
		{
			SetTimeScale(VariantUtils.ConvertTo<float>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EaseForStrength && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<Ease.Functions>(EaseForStrength(VariantUtils.ConvertTo<ShakeStrength>(in args[0])));
			return true;
		}
		if (method == MethodName.SecondsForDuration && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<float>(SecondsForDuration(VariantUtils.ConvertTo<ShakeDuration>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.DoHitStop)
		{
			return true;
		}
		if (method == MethodName.SetTimeScale)
		{
			return true;
		}
		if (method == MethodName.EaseForStrength)
		{
			return true;
		}
		if (method == MethodName.SecondsForDuration)
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
