using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NSceneContainer.cs")]
public class NSceneContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public static readonly StringName SetCurrentScene = "SetCurrentScene";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CurrentScene = "CurrentScene";

		public static readonly StringName _currentScene = "_currentScene";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private Control? _currentScene;

	public Control? CurrentScene
	{
		get
		{
			if (_currentScene == null)
			{
				return null;
			}
			if (!GodotObject.IsInstanceValid(_currentScene))
			{
				return null;
			}
			if (_currentScene.IsQueuedForDeletion())
			{
				return null;
			}
			return _currentScene;
		}
		private set
		{
			_currentScene = value;
		}
	}

	public void SetCurrentScene(Control node)
	{
		foreach (Node child in GetChildren())
		{
			this.RemoveChildSafely(child);
			child.QueueFreeSafely();
		}
		CurrentScene = node;
		if (node.GetParent() == null)
		{
			this.AddChildSafely(node);
		}
		else
		{
			node.Reparent(this);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.SetCurrentScene, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.SetCurrentScene && args.Count == 1)
		{
			SetCurrentScene(VariantUtils.ConvertTo<Control>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.SetCurrentScene)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.CurrentScene)
		{
			CurrentScene = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._currentScene)
		{
			_currentScene = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CurrentScene)
		{
			value = VariantUtils.CreateFrom<Control>(CurrentScene);
			return true;
		}
		if (name == PropertyName._currentScene)
		{
			value = VariantUtils.CreateFrom(in _currentScene);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._currentScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CurrentScene, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.CurrentScene, Variant.From<Control>(CurrentScene));
		info.AddProperty(PropertyName._currentScene, Variant.From(in _currentScene));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.CurrentScene, out var value))
		{
			CurrentScene = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._currentScene, out var value2))
		{
			_currentScene = value2.As<Control>();
		}
	}
}
