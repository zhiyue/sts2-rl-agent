using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes;

[ScriptPath("res://src/Core/Nodes/NRun.cs")]
public class NRun : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _Process = "_Process";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName SetCurrentRoom = "SetCurrentRoom";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName CombatRoom = "CombatRoom";

		public static readonly StringName TreasureRoom = "TreasureRoom";

		public static readonly StringName EventRoom = "EventRoom";

		public static readonly StringName RestSiteRoom = "RestSiteRoom";

		public static readonly StringName MapRoom = "MapRoom";

		public static readonly StringName MerchantRoom = "MerchantRoom";

		public static readonly StringName GlobalUi = "GlobalUi";

		public static readonly StringName RunMusicController = "RunMusicController";

		public static readonly StringName _cardScene = "_cardScene";

		public static readonly StringName _roomContainer = "_roomContainer";

		public static readonly StringName _testButton = "_testButton";

		public static readonly StringName _seedLabel = "_seedLabel";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const string _scenePath = "res://scenes/run.tscn";

	[Export(PropertyHint.None, "")]
	private PackedScene _cardScene;

	private RunState _state;

	private NSceneContainer _roomContainer;

	private Button _testButton;

	private MegaLabel _seedLabel;

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/run.tscn");

	public static NRun? Instance => NGame.Instance?.CurrentRunNode;

	public NCombatRoom? CombatRoom
	{
		get
		{
			Control currentScene = _roomContainer.CurrentScene;
			if (!(currentScene is NCombatRoom result))
			{
				if (currentScene is NEventRoom nEventRoom)
				{
					return nEventRoom.EmbeddedCombatRoom;
				}
				return null;
			}
			return result;
		}
	}

	public NTreasureRoom? TreasureRoom => _roomContainer.CurrentScene as NTreasureRoom;

	public NEventRoom? EventRoom => _roomContainer.CurrentScene as NEventRoom;

	public NRestSiteRoom? RestSiteRoom => _roomContainer.CurrentScene as NRestSiteRoom;

	public NMapRoom? MapRoom => _roomContainer.CurrentScene as NMapRoom;

	public NMerchantRoom? MerchantRoom => _roomContainer.CurrentScene as NMerchantRoom;

	public NGlobalUi GlobalUi { get; private set; }

	public NRunMusicController RunMusicController { get; private set; }

	public ScreenStateTracker ScreenStateTracker { get; private set; }

	public static NRun Create(RunState state)
	{
		NRun nRun = PreloadManager.Cache.GetScene("res://scenes/run.tscn").Instantiate<NRun>(PackedScene.GenEditState.Disabled);
		nRun._state = state;
		return nRun;
	}

	public override void _Ready()
	{
		_roomContainer = GetNode<NSceneContainer>("%RoomContainer");
		GlobalUi = GetNode<NGlobalUi>("%GlobalUi");
		GlobalUi.Initialize(_state);
		ScreenStateTracker = new ScreenStateTracker(GlobalUi.MapScreen, GlobalUi.CapstoneContainer, GlobalUi.Overlays);
		RunMusicController = GetNode<NRunMusicController>("%RunMusicController");
		RunMusicController.SetRunState(_state);
		RunMusicController.UpdateMusic();
		_seedLabel = GetNode<MegaLabel>("%DebugSeed");
		_seedLabel.SetTextAutoSize(_state.Rng.StringSeed);
	}

	public override void _Process(double delta)
	{
		RunManager.Instance.NetService.Update();
	}

	public override void _Notification(int what)
	{
		if ((long)what == 1006)
		{
			RunManager.Instance.CleanUp(graceful: false);
		}
	}

	public void SetCurrentRoom(Control? node)
	{
		if (node != null)
		{
			_roomContainer.SetCurrentScene(node);
			ActiveScreenContext.Instance.Update();
		}
	}

	public void ShowGameOverScreen(SerializableRun serializableRun)
	{
		NCapstoneContainer.Instance.Close();
		NMapScreen.Instance.Close();
		NOverlayStack.Instance.Push(NGameOverScreen.Create(_state, serializableRun));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._Process, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetCurrentRoom, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
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
		if (method == MethodName._Process && args.Count == 1)
		{
			_Process(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetCurrentRoom && args.Count == 1)
		{
			SetCurrentRoom(VariantUtils.ConvertTo<Control>(in args[0]));
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
		if (method == MethodName._Process)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.SetCurrentRoom)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.GlobalUi)
		{
			GlobalUi = VariantUtils.ConvertTo<NGlobalUi>(in value);
			return true;
		}
		if (name == PropertyName.RunMusicController)
		{
			RunMusicController = VariantUtils.ConvertTo<NRunMusicController>(in value);
			return true;
		}
		if (name == PropertyName._cardScene)
		{
			_cardScene = VariantUtils.ConvertTo<PackedScene>(in value);
			return true;
		}
		if (name == PropertyName._roomContainer)
		{
			_roomContainer = VariantUtils.ConvertTo<NSceneContainer>(in value);
			return true;
		}
		if (name == PropertyName._testButton)
		{
			_testButton = VariantUtils.ConvertTo<Button>(in value);
			return true;
		}
		if (name == PropertyName._seedLabel)
		{
			_seedLabel = VariantUtils.ConvertTo<MegaLabel>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.CombatRoom)
		{
			value = VariantUtils.CreateFrom<NCombatRoom>(CombatRoom);
			return true;
		}
		if (name == PropertyName.TreasureRoom)
		{
			value = VariantUtils.CreateFrom<NTreasureRoom>(TreasureRoom);
			return true;
		}
		if (name == PropertyName.EventRoom)
		{
			value = VariantUtils.CreateFrom<NEventRoom>(EventRoom);
			return true;
		}
		if (name == PropertyName.RestSiteRoom)
		{
			value = VariantUtils.CreateFrom<NRestSiteRoom>(RestSiteRoom);
			return true;
		}
		if (name == PropertyName.MapRoom)
		{
			value = VariantUtils.CreateFrom<NMapRoom>(MapRoom);
			return true;
		}
		if (name == PropertyName.MerchantRoom)
		{
			value = VariantUtils.CreateFrom<NMerchantRoom>(MerchantRoom);
			return true;
		}
		if (name == PropertyName.GlobalUi)
		{
			value = VariantUtils.CreateFrom<NGlobalUi>(GlobalUi);
			return true;
		}
		if (name == PropertyName.RunMusicController)
		{
			value = VariantUtils.CreateFrom<NRunMusicController>(RunMusicController);
			return true;
		}
		if (name == PropertyName._cardScene)
		{
			value = VariantUtils.CreateFrom(in _cardScene);
			return true;
		}
		if (name == PropertyName._roomContainer)
		{
			value = VariantUtils.CreateFrom(in _roomContainer);
			return true;
		}
		if (name == PropertyName._testButton)
		{
			value = VariantUtils.CreateFrom(in _testButton);
			return true;
		}
		if (name == PropertyName._seedLabel)
		{
			value = VariantUtils.CreateFrom(in _seedLabel);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cardScene, PropertyHint.ResourceType, "PackedScene", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._roomContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._testButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._seedLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CombatRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.TreasureRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EventRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RestSiteRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MapRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.MerchantRoom, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.GlobalUi, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.RunMusicController, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.GlobalUi, Variant.From<NGlobalUi>(GlobalUi));
		info.AddProperty(PropertyName.RunMusicController, Variant.From<NRunMusicController>(RunMusicController));
		info.AddProperty(PropertyName._cardScene, Variant.From(in _cardScene));
		info.AddProperty(PropertyName._roomContainer, Variant.From(in _roomContainer));
		info.AddProperty(PropertyName._testButton, Variant.From(in _testButton));
		info.AddProperty(PropertyName._seedLabel, Variant.From(in _seedLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.GlobalUi, out var value))
		{
			GlobalUi = value.As<NGlobalUi>();
		}
		if (info.TryGetProperty(PropertyName.RunMusicController, out var value2))
		{
			RunMusicController = value2.As<NRunMusicController>();
		}
		if (info.TryGetProperty(PropertyName._cardScene, out var value3))
		{
			_cardScene = value3.As<PackedScene>();
		}
		if (info.TryGetProperty(PropertyName._roomContainer, out var value4))
		{
			_roomContainer = value4.As<NSceneContainer>();
		}
		if (info.TryGetProperty(PropertyName._testButton, out var value5))
		{
			_testButton = value5.As<Button>();
		}
		if (info.TryGetProperty(PropertyName._seedLabel, out var value6))
		{
			_seedLabel = value6.As<MegaLabel>();
		}
	}
}
