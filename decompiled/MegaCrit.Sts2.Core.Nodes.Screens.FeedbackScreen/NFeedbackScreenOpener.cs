using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;

namespace MegaCrit.Sts2.Core.Nodes.Screens.FeedbackScreen;

[ScriptPath("res://src/Core/Nodes/Screens/FeedbackScreen/NFeedbackScreenOpener.cs")]
public class NFeedbackScreenOpener : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public new static readonly StringName _Input = "_Input";
	}

	public new class PropertyName : Node.PropertyName
	{
	}

	public new class SignalName : Node.SignalName
	{
	}

	public static NFeedbackScreenOpener Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _ExitTree()
	{
		Instance = null;
	}

	public override void _Input(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey { Pressed: not false } inputEventKey && inputEventKey.Keycode == Key.F2 && !NGame.Instance.FeedbackScreen.Visible && !(NCapstoneContainer.Instance?.CurrentCapstoneScreen is NCapstoneSubmenuStack { ScreenType: NetScreenType.Feedback }))
		{
			TaskHelper.RunSafely(OpenFeedbackScreen());
		}
	}

	public async Task OpenFeedbackScreen()
	{
		Image screenshot = GetViewport().GetTexture().GetImage();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		NGame.Instance.GetInspectCardScreen().Close();
		NGame.Instance.GetInspectRelicScreen().Close();
		NSendFeedbackScreen feedbackScreen = NGame.Instance.FeedbackScreen;
		feedbackScreen.SetScreenshot(screenshot);
		NGame.Instance.FeedbackScreen.Open();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(3);
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
		}, null));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._EnterTree && args.Count == 0)
		{
			_EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName._Input)
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
