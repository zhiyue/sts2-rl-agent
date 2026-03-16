using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace MegaCrit.Sts2.Core.Nodes.Combat;

[ScriptPath("res://src/Core/Nodes/Combat/NSelectedHandCardContainer.cs")]
public class NSelectedHandCardContainer : Control
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName Add = "Add";

		public static readonly StringName RefreshHolderPositions = "RefreshHolderPositions";

		public static readonly StringName DeselectHolder = "DeselectHolder";

		public static readonly StringName OnFocus = "OnFocus";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Hand = "Hand";
	}

	public new class SignalName : Control.SignalName
	{
	}

	public NPlayerHand? Hand { get; set; }

	public List<NSelectedHandCardHolder> Holders => GetChildren().OfType<NSelectedHandCardHolder>().ToList();

	public override void _Ready()
	{
		Connect(Control.SignalName.FocusEntered, Callable.From(OnFocus));
	}

	public NSelectedHandCardHolder Add(NHandCardHolder originalHolder)
	{
		NCard cardNode = originalHolder.CardNode;
		Vector2 globalPosition = cardNode.GlobalPosition;
		NSelectedHandCardHolder nSelectedHandCardHolder = NSelectedHandCardHolder.Create(originalHolder);
		nSelectedHandCardHolder.Connect(NCardHolder.SignalName.Pressed, Callable.From<NCardHolder>(DeselectHolder), 4u);
		this.AddChildSafely(nSelectedHandCardHolder);
		RefreshHolderPositions();
		cardNode.GlobalPosition = globalPosition;
		return nSelectedHandCardHolder;
	}

	private void RefreshHolderPositions()
	{
		int count = Holders.Count;
		base.FocusMode = (FocusModeEnum)((count > 0) ? 2 : 0);
		if (count != 0)
		{
			float x = Holders.First().Size.X;
			float num = (0f - x) * (float)(count - 1) / 2f;
			for (int i = 0; i < count; i++)
			{
				Holders[i].Position = new Vector2(num, 0f);
				num += x;
				Holders[i].FocusNeighborLeft = ((i > 0) ? Holders[i - 1].GetPath() : Holders[Holders.Count - 1].GetPath());
				Holders[i].FocusNeighborRight = ((i < Holders.Count - 1) ? Holders[i + 1].GetPath() : Holders[0].GetPath());
			}
		}
	}

	private void DeselectHolder(NCardHolder holder)
	{
		NSelectedHandCardHolder nSelectedHandCardHolder = (NSelectedHandCardHolder)holder;
		NCard cardNode = nSelectedHandCardHolder.CardNode;
		Hand.DeselectCard(cardNode);
		this.RemoveChildSafely(nSelectedHandCardHolder);
		nSelectedHandCardHolder.QueueFreeSafely();
		RefreshHolderPositions();
	}

	public void DeselectCard(CardModel card)
	{
		NSelectedHandCardHolder holder = Holders.First((NSelectedHandCardHolder child) => child.CardNode.Model == card);
		DeselectHolder(holder);
	}

	private void OnFocus()
	{
		Holders.FirstOrDefault()?.TryGrabFocus();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(5);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Add, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "originalHolder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.RefreshHolderPositions, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.DeselectHolder, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "holder", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.Add && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<NSelectedHandCardHolder>(Add(VariantUtils.ConvertTo<NHandCardHolder>(in args[0])));
			return true;
		}
		if (method == MethodName.RefreshHolderPositions && args.Count == 0)
		{
			RefreshHolderPositions();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DeselectHolder && args.Count == 1)
		{
			DeselectHolder(VariantUtils.ConvertTo<NCardHolder>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
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
		if (method == MethodName.Add)
		{
			return true;
		}
		if (method == MethodName.RefreshHolderPositions)
		{
			return true;
		}
		if (method == MethodName.DeselectHolder)
		{
			return true;
		}
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Hand)
		{
			Hand = VariantUtils.ConvertTo<NPlayerHand>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Hand)
		{
			value = VariantUtils.CreateFrom<NPlayerHand>(Hand);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Hand, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Hand, Variant.From<NPlayerHand>(Hand));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Hand, out var value))
		{
			Hand = value.As<NPlayerHand>();
		}
	}
}
