using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Cards;

/// <summary>
/// Used in Events. Displays original card on left and a card that rapidly cycles through random cards on the right.
/// </summary>
[ScriptPath("res://src/Core/Nodes/Cards/NTransformPreview.cs")]
public class NTransformPreview : Control
{
	/// <summary>
	/// Cached StringNames for the methods contained in this class, for fast lookup.
	/// </summary>
	public new class MethodName : Control.MethodName
	{
		/// <summary>
		/// Cached name for the '_Ready' method.
		/// </summary>
		public new static readonly StringName _Ready = "_Ready";

		/// <summary>
		/// Cached name for the '_ExitTree' method.
		/// </summary>
		public new static readonly StringName _ExitTree = "_ExitTree";

		/// <summary>
		/// Cached name for the 'Uninitialize' method.
		/// </summary>
		public static readonly StringName Uninitialize = "Uninitialize";

		/// <summary>
		/// Cached name for the 'RemoveExistingCards' method.
		/// </summary>
		public static readonly StringName RemoveExistingCards = "RemoveExistingCards";
	}

	/// <summary>
	/// Cached StringNames for the properties and fields contained in this class, for fast lookup.
	/// </summary>
	public new class PropertyName : Control.PropertyName
	{
		/// <summary>
		/// Cached name for the 'SelectedCardPosition' property.
		/// </summary>
		public static readonly StringName SelectedCardPosition = "SelectedCardPosition";

		/// <summary>
		/// Cached name for the '_before' field.
		/// </summary>
		public static readonly StringName _before = "_before";

		/// <summary>
		/// Cached name for the '_after' field.
		/// </summary>
		public static readonly StringName _after = "_after";

		/// <summary>
		/// Cached name for the '_arrows' field.
		/// </summary>
		public static readonly StringName _arrows = "_arrows";
	}

	/// <summary>
	/// Cached StringNames for the signals contained in this class, for fast lookup.
	/// </summary>
	public new class SignalName : Control.SignalName
	{
	}

	private Control _before;

	private Control _after;

	private Control _arrows;

	private CancellationTokenSource? _cancelTokenSource;

	public Vector2 SelectedCardPosition => _before.GlobalPosition;

	public override void _Ready()
	{
		_before = GetNode<Control>("%Before");
		_after = GetNode<Control>("%After");
		_arrows = GetNode<Control>("Arrows");
	}

	public override void _ExitTree()
	{
		_cancelTokenSource?.Cancel();
	}

	public void Initialize(IEnumerable<CardTransformation> cardTransformations)
	{
		RemoveExistingCards();
		_cancelTokenSource?.Cancel();
		List<CardTransformation> list = cardTransformations.ToList();
		float num = _before.GlobalPosition.X - 100f;
		float num2 = Math.Min(num / ((float)list.Count * 300f + (float)(list.Count - 1) * 30f), 1f);
		for (int i = 0; i < list.Count; i++)
		{
			CardTransformation cardTransformation = list[i];
			NPlayerHand nPlayerHand = NCombatRoom.Instance?.Ui.Hand;
			NPreviewCardHolder nPreviewCardHolder = NPreviewCardHolder.Create(NCard.Create(cardTransformation.Original), nPlayerHand == null, nPlayerHand != null);
			_before.AddChildSafely(nPreviewCardHolder);
			nPreviewCardHolder.FocusMode = FocusModeEnum.All;
			nPreviewCardHolder.CardNode.UpdateVisuals(cardTransformation.Original.Pile.Type, CardPreviewMode.Normal);
			nPreviewCardHolder.SetCardScale(Vector2.One * num2);
			int num3 = list.Count - i;
			nPreviewCardHolder.Position = new Vector2((0f - ((float)num3 - 0.5f)) * 300f * num2 - (float)(num3 - 1) * 30f, 0f);
			NCard card = ((cardTransformation.Replacement == null) ? NCard.Create(cardTransformation.Original) : NCard.Create(cardTransformation.Replacement));
			NPreviewCardHolder nPreviewCardHolder2 = NPreviewCardHolder.Create(card, showHoverTips: true, scaleOnHover: false);
			nPreviewCardHolder2.FocusMode = FocusModeEnum.None;
			_after.AddChildSafely(nPreviewCardHolder2);
			nPreviewCardHolder2.CardNode.UpdateVisuals(cardTransformation.Original.Pile.Type, CardPreviewMode.Normal);
			nPreviewCardHolder2.Scale = Vector2.One * num2;
			nPreviewCardHolder2.Position = new Vector2(((float)i + 0.5f) * 300f * num2 + (float)i * 30f, 0f);
			if (cardTransformation.Replacement == null)
			{
				nPreviewCardHolder2.Hitbox.MouseFilter = MouseFilterEnum.Ignore;
				TaskHelper.RunSafely(CycleThroughCards(possibleTransformations: (cardTransformation.ReplacementOptions == null) ? CardFactory.GetDefaultTransformationOptions(cardTransformation.Original, cardTransformation.IsInCombat) : cardTransformation.ReplacementOptions, holder: nPreviewCardHolder2, cardPile: cardTransformation.Original.Pile));
			}
		}
	}

	public void Uninitialize()
	{
		_cancelTokenSource?.Cancel();
	}

	private async Task CycleThroughCards(NPreviewCardHolder holder, CardPile cardPile, IEnumerable<CardModel> possibleTransformations)
	{
		_cancelTokenSource = new CancellationTokenSource();
		List<CardModel> cards = possibleTransformations.ToList();
		cards.UnstableShuffle(Rng.Chaotic);
		int cardIndex = 0;
		while (!_cancelTokenSource.IsCancellationRequested && holder.CardNode != null)
		{
			holder.ReassignToCard(cards[cardIndex], cardPile.Type, null, ModelVisibility.Visible);
			cardIndex++;
			if (cardIndex >= cards.Count)
			{
				cards.UnstableShuffle(Rng.Chaotic);
				cardIndex = 0;
			}
			if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant)
			{
				await Task.Delay(200, _cancelTokenSource.Token);
			}
			else
			{
				await Cmd.Wait(0.2f, _cancelTokenSource.Token, ignoreCombatEnd: true);
			}
		}
	}

	/// <summary>
	/// Helper function to remove any existing preview cards
	/// </summary>
	private void RemoveExistingCards()
	{
		foreach (Node child in _before.GetChildren())
		{
			child.QueueFreeSafely();
		}
		foreach (Node child2 in _after.GetChildren())
		{
			child2.QueueFreeSafely();
		}
	}

	/// <summary>
	/// Get the method information for all the methods declared in this class.
	/// This method is used by Godot to register the available methods in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(4);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Uninitialize, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveExistingCards, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName._Ready && args.Count == 0)
		{
			_Ready();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._ExitTree && args.Count == 0)
		{
			_ExitTree();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Uninitialize && args.Count == 0)
		{
			Uninitialize();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveExistingCards && args.Count == 0)
		{
			RemoveExistingCards();
			ret = default(godot_variant);
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName._Ready)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.Uninitialize)
		{
			return true;
		}
		if (method == MethodName.RemoveExistingCards)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._before)
		{
			_before = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._after)
		{
			_after = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._arrows)
		{
			_arrows = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.SelectedCardPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(SelectedCardPosition);
			return true;
		}
		if (name == PropertyName._before)
		{
			value = VariantUtils.CreateFrom(in _before);
			return true;
		}
		if (name == PropertyName._after)
		{
			value = VariantUtils.CreateFrom(in _after);
			return true;
		}
		if (name == PropertyName._arrows)
		{
			value = VariantUtils.CreateFrom(in _arrows);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	/// <summary>
	/// Get the property information for all the properties declared in this class.
	/// This method is used by Godot to register the available properties in the editor.
	/// Do not call this method.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._before, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._after, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._arrows, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName.SelectedCardPosition, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._before, Variant.From(in _before));
		info.AddProperty(PropertyName._after, Variant.From(in _after));
		info.AddProperty(PropertyName._arrows, Variant.From(in _arrows));
	}

	/// <inheritdoc />
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._before, out var value))
		{
			_before = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._after, out var value2))
		{
			_after = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._arrows, out var value3))
		{
			_arrows = value3.As<Control>();
		}
	}
}
