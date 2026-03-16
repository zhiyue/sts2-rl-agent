using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Quality;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Multiplayer/NMultiplayerNetworkProblemIndicator.cs")]
public class NMultiplayerNetworkProblemIndicator : TextureRect
{
	public new class MethodName : TextureRect.MethodName
	{
		public static readonly StringName Initialize = "Initialize";
	}

	public new class PropertyName : TextureRect.PropertyName
	{
		public static readonly StringName IsShown = "IsShown";

		public static readonly StringName _peerId = "_peerId";

		public static readonly StringName _tween = "_tween";
	}

	public new class SignalName : TextureRect.SignalName
	{
	}

	private const float _qualityScoreToShowAt = 350f;

	private ulong _peerId;

	private Tween? _tween;

	public bool IsShown { get; private set; }

	public void Initialize(ulong peerId)
	{
		if (RunManager.Instance.NetService.Type.IsMultiplayer())
		{
			_peerId = peerId;
			TaskHelper.RunSafely(UpdateLoop());
		}
	}

	private async Task UpdateLoop()
	{
		while (RunManager.Instance.NetService.IsConnected && this.IsValid())
		{
			ConnectionStats statsForPeer = RunManager.Instance.NetService.GetStatsForPeer(_peerId);
			if (statsForPeer == null)
			{
				break;
			}
			float num = statsForPeer.PingMsec / (1f - statsForPeer.PacketLoss);
			bool flag = num >= 350f;
			if (!IsShown && flag)
			{
				_tween?.Kill();
				base.Visible = true;
				base.Modulate = Colors.White;
			}
			else if (IsShown && flag)
			{
				NUiFlashVfx nUiFlashVfx = NUiFlashVfx.Create(base.Texture, Colors.White);
				this.AddChildSafely(nUiFlashVfx);
				TaskHelper.RunSafely(nUiFlashVfx.StartVfx());
			}
			else if (IsShown && !flag)
			{
				_tween?.Kill();
				_tween = CreateTween();
				_tween.TweenProperty(this, "modulate:a", 0f, 0.5);
			}
			IsShown = flag;
			await Task.Delay(2000);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.Initialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "peerId", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Initialize && args.Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<ulong>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Initialize)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.IsShown)
		{
			IsShown = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._peerId)
		{
			_peerId = VariantUtils.ConvertTo<ulong>(in value);
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
		if (name == PropertyName.IsShown)
		{
			value = VariantUtils.CreateFrom<bool>(IsShown);
			return true;
		}
		if (name == PropertyName._peerId)
		{
			value = VariantUtils.CreateFrom(in _peerId);
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
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName._peerId, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._tween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.IsShown, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.IsShown, Variant.From<bool>(IsShown));
		info.AddProperty(PropertyName._peerId, Variant.From(in _peerId));
		info.AddProperty(PropertyName._tween, Variant.From(in _tween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.IsShown, out var value))
		{
			IsShown = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._peerId, out var value2))
		{
			_peerId = value2.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName._tween, out var value3))
		{
			_tween = value3.As<Tween>();
		}
	}
}
