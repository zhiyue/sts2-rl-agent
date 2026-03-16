using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace MegaCrit.Sts2.Core.Nodes.Reaction;

[ScriptPath("res://src/Core/Nodes/Reaction/NReactionContainer.cs")]
public class NReactionContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName DeinitializeNetworking = "DeinitializeNetworking";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName DoLocalReaction = "DoLocalReaction";

		public static readonly StringName DoRemoteReaction = "DoRemoteReaction";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName InMultiplayer = "InMultiplayer";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private ReactionSynchronizer? _synchronizer;

	public bool InMultiplayer
	{
		get
		{
			if (_synchronizer != null)
			{
				return _synchronizer.NetService.Type != NetGameType.Singleplayer;
			}
			return false;
		}
	}

	public void InitializeNetworking(INetGameService netService)
	{
		if (_synchronizer != null)
		{
			DeinitializeNetworking();
		}
		_synchronizer = new ReactionSynchronizer(netService, this);
		_synchronizer.NetService.Disconnected += NetServiceDisconnected;
	}

	private void NetServiceDisconnected(NetErrorInfo _)
	{
		DeinitializeNetworking();
	}

	public void DeinitializeNetworking()
	{
		if (_synchronizer != null)
		{
			_synchronizer.NetService.Disconnected -= NetServiceDisconnected;
			_synchronizer.Dispose();
			_synchronizer = null;
		}
	}

	public override void _ExitTree()
	{
		DeinitializeNetworking();
	}

	public void DoLocalReaction(Texture2D tex, Vector2 position)
	{
		NReaction nReaction = NReaction.Create(tex);
		this.AddChildSafely(nReaction);
		nReaction.GlobalPosition = position - nReaction.Size / 2f;
		nReaction.BeginAnim();
		_synchronizer?.SendLocalReaction(nReaction.Type, position);
	}

	public void DoRemoteReaction(ReactionType type, Vector2 position)
	{
		NReaction nReaction = NReaction.Create(type);
		this.AddChildSafely(nReaction);
		nReaction.GlobalPosition = position - nReaction.Size / 2f;
		nReaction.BeginAnim();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName.DeinitializeNetworking, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DoLocalReaction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "tex", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Texture2D"), exported: false),
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.DoRemoteReaction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
			new PropertyInfo(Variant.Type.Vector2, "position", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.DeinitializeNetworking && args.Count == 0)
		{
			DeinitializeNetworking();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoLocalReaction && args.Count == 2)
		{
			DoLocalReaction(VariantUtils.ConvertTo<Texture2D>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DoRemoteReaction && args.Count == 2)
		{
			DoRemoteReaction(VariantUtils.ConvertTo<ReactionType>(in args[0]), VariantUtils.ConvertTo<Vector2>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.DeinitializeNetworking)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.DoLocalReaction)
		{
			return true;
		}
		if (method == MethodName.DoRemoteReaction)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.InMultiplayer)
		{
			value = VariantUtils.CreateFrom<bool>(InMultiplayer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.InMultiplayer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
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
