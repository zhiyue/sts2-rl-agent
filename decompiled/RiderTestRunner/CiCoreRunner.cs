using System.ComponentModel;
using Godot;
using Godot.Bridge;

namespace RiderTestRunner;

[ScriptPath("res://RiderTestRunner/CiCoreRunner.cs")]
public class CiCoreRunner : Node
{
	public new class MethodName : Node.MethodName
	{
	}

	public new class PropertyName : Node.PropertyName
	{
	}

	public new class SignalName : Node.SignalName
	{
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
