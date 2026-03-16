using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Debug.Multiplayer;

[ScriptPath("res://src/Core/Nodes/Debug/Multiplayer/NMultiplayerTestCharacterPaginator.cs")]
public class NMultiplayerTestCharacterPaginator : NPaginator
{
	public new class MethodName : NPaginator.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnIndexChanged = "OnIndexChanged";
	}

	public new class PropertyName : NPaginator.PropertyName
	{
	}

	public new class SignalName : NPaginator.SignalName
	{
	}

	private readonly CharacterModel[] _characters = new CharacterModel[5]
	{
		ModelDb.Character<Ironclad>(),
		ModelDb.Character<Silent>(),
		ModelDb.Character<Regent>(),
		ModelDb.Character<Necrobinder>(),
		ModelDb.Character<Defect>()
	};

	public CharacterModel Character => _characters[_currentIndex];

	public event Action<CharacterModel>? CharacterChanged;

	public override void _Ready()
	{
		ConnectSignals();
		CharacterModel[] characters = _characters;
		foreach (CharacterModel characterModel in characters)
		{
			_options.Add(new LocString("characters", characterModel.Id.Entry + ".title").GetFormattedText());
		}
		_label.Text = _options[_currentIndex];
	}

	protected override void OnIndexChanged(int index)
	{
		_currentIndex = index;
		_label.Text = _characters[index].Title.GetFormattedText();
		this.CharacterChanged?.Invoke(_characters[index]);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnIndexChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
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
		if (method == MethodName.OnIndexChanged && args.Count == 1)
		{
			OnIndexChanged(VariantUtils.ConvertTo<int>(in args[0]));
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
		if (method == MethodName.OnIndexChanged)
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
