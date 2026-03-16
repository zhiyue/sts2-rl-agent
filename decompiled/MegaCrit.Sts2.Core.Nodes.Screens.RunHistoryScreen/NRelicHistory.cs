using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NRelicHistory.cs")]
public class NRelicHistory : VBoxContainer
{
	[Signal]
	public delegate void HoveredEventHandler(NRelicBasicHolder relic);

	[Signal]
	public delegate void UnhoveredEventHandler(NRelicBasicHolder relic);

	public new class MethodName : VBoxContainer.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName OnRelicClicked = "OnRelicClicked";
	}

	public new class PropertyName : VBoxContainer.PropertyName
	{
		public static readonly StringName _headerLabel = "_headerLabel";

		public static readonly StringName _relicsContainer = "_relicsContainer";
	}

	public new class SignalName : VBoxContainer.SignalName
	{
		public static readonly StringName Hovered = "Hovered";

		public static readonly StringName Unhovered = "Unhovered";
	}

	private readonly LocString _relicHeader = new LocString("run_history", "RELIC_HISTORY.header");

	private readonly LocString _relicCategories = new LocString("run_history", "RELIC_HISTORY.categories");

	private MegaRichTextLabel _headerLabel;

	private Control _relicsContainer;

	private HoveredEventHandler backing_Hovered;

	private UnhoveredEventHandler backing_Unhovered;

	public event HoveredEventHandler Hovered
	{
		add
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Combine(backing_Hovered, value);
		}
		remove
		{
			backing_Hovered = (HoveredEventHandler)Delegate.Remove(backing_Hovered, value);
		}
	}

	public event UnhoveredEventHandler Unhovered
	{
		add
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Combine(backing_Unhovered, value);
		}
		remove
		{
			backing_Unhovered = (UnhoveredEventHandler)Delegate.Remove(backing_Unhovered, value);
		}
	}

	public override void _Ready()
	{
		_headerLabel = GetNode<MegaRichTextLabel>("Header");
		_relicsContainer = GetNode<Control>("%RelicsContainer");
	}

	public void LoadRelics(Player player, IEnumerable<SerializableRelic> relics)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Node child in _relicsContainer.GetChildren())
		{
			child.QueueFreeSafely();
		}
		Dictionary<RelicRarity, int> dictionary = new Dictionary<RelicRarity, int>();
		RelicRarity[] values = Enum.GetValues<RelicRarity>();
		foreach (RelicRarity key in values)
		{
			dictionary.Add(key, 0);
		}
		List<SerializableRelic> list = relics.ToList();
		foreach (SerializableRelic item in list)
		{
			RelicModel relicModel;
			try
			{
				relicModel = RelicModel.FromSerializable(item);
			}
			catch (ModelNotFoundException)
			{
				relicModel = ModelDb.Relic<DeprecatedRelic>().ToMutable();
			}
			relicModel.Owner = player;
			NRelicBasicHolder holder = NRelicBasicHolder.Create(relicModel);
			holder.MouseDefaultCursorShape = CursorShape.Help;
			_relicsContainer.AddChildSafely(holder);
			holder.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
			{
				EmitSignal(SignalName.Hovered, holder);
			}));
			holder.Connect(Control.SignalName.FocusExited, Callable.From(delegate
			{
				EmitSignal(SignalName.Unhovered, holder);
			}));
			holder.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
			{
				EmitSignal(SignalName.Hovered, holder);
			}));
			holder.Connect(Control.SignalName.MouseExited, Callable.From(delegate
			{
				EmitSignal(SignalName.Unhovered, holder);
			}));
			holder.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(delegate
			{
				OnRelicClicked(holder.Relic);
			}));
			dictionary[relicModel.Rarity]++;
		}
		_relicHeader.Add("totalRelics", list.Count);
		foreach (KeyValuePair<RelicRarity, int> item2 in dictionary)
		{
			_relicCategories.Add(item2.Key.ToString() + "Relics", item2.Value);
		}
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(20, 1, stringBuilder2);
		handler.AppendLiteral("[gold][b]");
		handler.AppendFormatted(_relicHeader.GetFormattedText());
		handler.AppendLiteral("[/b][/gold]");
		stringBuilder2.Append(ref handler);
		stringBuilder.Append(_relicCategories.GetFormattedText().Trim(','));
		_headerLabel.Text = stringBuilder.ToString();
	}

	private void OnRelicClicked(NRelic node)
	{
		List<RelicModel> list = new List<RelicModel>();
		foreach (NRelicBasicHolder item in _relicsContainer.GetChildren().OfType<NRelicBasicHolder>())
		{
			list.Add(item.Relic.Model);
		}
		NGame.Instance.GetInspectRelicScreen().Open(list, node.Model);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnRelicClicked, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
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
		if (method == MethodName.OnRelicClicked && args.Count == 1)
		{
			OnRelicClicked(VariantUtils.ConvertTo<NRelic>(in args[0]));
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
		if (method == MethodName.OnRelicClicked)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._headerLabel)
		{
			_headerLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._relicsContainer)
		{
			_relicsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName._headerLabel)
		{
			value = VariantUtils.CreateFrom(in _headerLabel);
			return true;
		}
		if (name == PropertyName._relicsContainer)
		{
			value = VariantUtils.CreateFrom(in _relicsContainer);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._headerLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._relicsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._headerLabel, Variant.From(in _headerLabel));
		info.AddProperty(PropertyName._relicsContainer, Variant.From(in _relicsContainer));
		info.AddSignalEventDelegate(SignalName.Hovered, backing_Hovered);
		info.AddSignalEventDelegate(SignalName.Unhovered, backing_Unhovered);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._headerLabel, out var value))
		{
			_headerLabel = value.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._relicsContainer, out var value2))
		{
			_relicsContainer = value2.As<Control>();
		}
		if (info.TryGetSignalEventDelegate<HoveredEventHandler>(SignalName.Hovered, out var value3))
		{
			backing_Hovered = value3;
		}
		if (info.TryGetSignalEventDelegate<UnhoveredEventHandler>(SignalName.Unhovered, out var value4))
		{
			backing_Unhovered = value4;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotSignalList()
	{
		List<MethodInfo> list = new List<MethodInfo>(2);
		list.Add(new MethodInfo(SignalName.Hovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "relic", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(SignalName.Unhovered, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "relic", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		return list;
	}

	protected void EmitSignalHovered(NRelicBasicHolder relic)
	{
		EmitSignal(SignalName.Hovered, relic);
	}

	protected void EmitSignalUnhovered(NRelicBasicHolder relic)
	{
		EmitSignal(SignalName.Unhovered, relic);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RaiseGodotClassSignalCallbacks(in godot_string_name signal, NativeVariantPtrArgs args)
	{
		if (signal == SignalName.Hovered && args.Count == 1)
		{
			backing_Hovered?.Invoke(VariantUtils.ConvertTo<NRelicBasicHolder>(in args[0]));
		}
		else if (signal == SignalName.Unhovered && args.Count == 1)
		{
			backing_Unhovered?.Invoke(VariantUtils.ConvertTo<NRelicBasicHolder>(in args[0]));
		}
		else
		{
			base.RaiseGodotClassSignalCallbacks(in signal, args);
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassSignal(in godot_string_name signal)
	{
		if (signal == SignalName.Hovered)
		{
			return true;
		}
		if (signal == SignalName.Unhovered)
		{
			return true;
		}
		return base.HasGodotClassSignal(in signal);
	}
}
