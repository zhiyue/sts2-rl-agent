using System.ComponentModel;
using Godot;
using Godot.Bridge;

namespace MegaCrit.Sts2.GameInfo;

[ScriptPath("res://src/GameInfo/NGameInfoUploader.cs")]
public class NGameInfoUploader : Node
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

	public static bool IsRunning { get; private set; }

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
