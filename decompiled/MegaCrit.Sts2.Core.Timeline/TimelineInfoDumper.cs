using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace MegaCrit.Sts2.Core.Timeline;

[ScriptPath("res://src/Core/Timeline/TimelineInfoDumper.cs")]
public class TimelineInfoDumper : Node
{
	public new class MethodName : Node.MethodName
	{
		public static readonly StringName Dump = "Dump";
	}

	public new class PropertyName : Node.PropertyName
	{
	}

	public new class SignalName : Node.SignalName
	{
	}

	public static void Dump()
	{
		List<EpochModel> allEpochs = GetAllEpochs();
		Console.Out.WriteLine("START TIMELINE INFO DUMPER");
		Console.Out.WriteLine("START TIMELINE INFO DUMPER");
		Console.Out.WriteLine("START TIMELINE INFO DUMPER");
		foreach (EpochModel item in allEpochs)
		{
			Console.Out.WriteLine($"\"{item.Id}\", \"{item.Era}\", \"{item.Era}.{item.EraPosition}\", \"{item.Title}\", \"{item.Description.Replace("\r", "").Replace("\n", "")}\", \"{item.UnlockText}\", \"{item.BigPortraitPath}\"");
		}
		Console.Out.WriteLine("END TIMELINE INFO DUMPER");
		Console.Out.WriteLine("END TIMELINE INFO DUMPER");
		Console.Out.WriteLine("END TIMELINE INFO DUMPER");
	}

	public static List<EpochModel> GetAllEpochs()
	{
		return EpochModel.AllEpochIds.Select(EpochModel.Get).ToList();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(1);
		list.Add(new MethodInfo(MethodName.Dump, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, null, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Dump && args.Count == 0)
		{
			Dump();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.Dump && args.Count == 0)
		{
			Dump();
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.Dump)
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
