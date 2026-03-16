using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace MegaCrit.Sts2.Core.Nodes.Debug;

[ScriptPath("res://src/Core/Nodes/Debug/NSceneBootstrapper.cs")]
public class NSceneBootstrapper : Node
{
	public new class MethodName : Node.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";
	}

	public new class PropertyName : Node.PropertyName
	{
		public static readonly StringName _openConsole = "_openConsole";

		public static readonly StringName _game = "_game";
	}

	public new class SignalName : Node.SignalName
	{
	}

	private bool _openConsole;

	private NGame _game;

	public override void _Ready()
	{
		if (GetParent() is NGame game)
		{
			_game = game;
		}
		else
		{
			_game = SceneHelper.Instantiate<NGame>("game");
			_game.StartOnMainMenu = false;
			this.AddChildSafely(_game);
		}
		TaskHelper.RunSafely(StartNewRun());
	}

	private async Task StartNewRun()
	{
		Type type = BootstrapSettingsUtil.Get();
		if (type == null)
		{
			Log.Error("No type implementing IBootstrapSettings found in the project! To use the bootstrap scene, copy src/Core/Nodes/Debug/BootstrapSettings.cs.template and rename it to BootstrapSettings.cs");
			return;
		}
		IBootstrapSettings settings = (IBootstrapSettings)Activator.CreateInstance(type);
		if (settings.Language != null)
		{
			LocManager.Instance.SetLanguage(settings.Language);
		}
		PreloadManager.Enabled = settings.DoPreloading;
		string seed = settings.Seed ?? SeedHelper.GetRandomSeed();
		List<ActModel> list = ActModel.GetDefaultList().ToList();
		list[0] = settings.Act;
		RunState runState = RunState.CreateForNewRun(new global::_003C_003Ez__ReadOnlySingleElementList<Player>(Player.CreateForNewRun(settings.Character, SaveManager.Instance.GenerateUnlockStateFromProgress(), 1uL)), list.Select((ActModel a) => a.ToMutable()).ToList(), settings.Modifiers, settings.Ascension, seed);
		RunManager.Instance.SetUpNewSinglePlayer(runState, settings.SaveRunHistory);
		await PreloadManager.LoadRunAssets(new global::_003C_003Ez__ReadOnlySingleElementList<CharacterModel>(settings.Character));
		RunManager.Instance.Launch();
		_game.RootSceneContainer.SetCurrentScene(NRun.Create(runState));
		await RunManager.Instance.SetActInternal(0);
		RunManager.Instance.RunLocationTargetedBuffer.OnRunLocationChanged(runState.CurrentLocation);
		RunManager.Instance.MapSelectionSynchronizer.OnRunLocationChanged(runState.CurrentLocation);
		await settings.Setup(runState.Players[0]);
		switch (settings.RoomType)
		{
		case RoomType.Unassigned:
			await RunManager.Instance.EnterAct(0);
			break;
		case RoomType.Treasure:
		case RoomType.Shop:
		case RoomType.RestSite:
			await RunManager.Instance.EnterRoomDebug(settings.RoomType);
			RunManager.Instance.ActionExecutor.Unpause();
			break;
		case RoomType.Event:
		{
			AbstractRoom abstractRoom = await RunManager.Instance.EnterRoomDebug(settings.RoomType, MapPointType.Unassigned, settings.Event);
			if (abstractRoom != null && abstractRoom.IsVictoryRoom)
			{
				runState.CurrentActIndex = runState.Acts.Count - 1;
			}
			break;
		}
		default:
			await RunManager.Instance.EnterRoomDebug(settings.RoomType, MapPointType.Unassigned, settings.RoomType.IsCombatRoom() ? settings.Encounter.ToMutable() : null);
			break;
		}
		if (_openConsole)
		{
			NDevConsole node = GetNode<NDevConsole>("/root/DevConsole/ConsoleScreen");
			node.ShowConsole();
			node.MakeFullScreen();
			node.SetBackgroundColor(Colors.White);
		}
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
		if (name == PropertyName._openConsole)
		{
			_openConsole = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._game)
		{
			_game = VariantUtils.ConvertTo<NGame>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._openConsole)
		{
			value = VariantUtils.CreateFrom(in _openConsole);
			return true;
		}
		if (name == PropertyName._game)
		{
			value = VariantUtils.CreateFrom(in _game);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._openConsole, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._game, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._openConsole, Variant.From(in _openConsole));
		info.AddProperty(PropertyName._game, Variant.From(in _game));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._openConsole, out var value))
		{
			_openConsole = value.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._game, out var value2))
		{
			_game = value2.As<NGame>();
		}
	}
}
