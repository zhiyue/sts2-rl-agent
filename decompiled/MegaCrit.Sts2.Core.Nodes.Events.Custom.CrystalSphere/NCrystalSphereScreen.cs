using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Events.Custom.CrystalSphereEvent;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Events.Custom.CrystalSphere;

[ScriptPath("res://src/Core/Nodes/Events/Custom/CrystalSphere/NCrystalSphereScreen.cs")]
public class NCrystalSphereScreen : Control, IOverlayScreen, IScreenContext
{
	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public static readonly StringName SetBigDivination = "SetBigDivination";

		public static readonly StringName SetSmallDivination = "SetSmallDivination";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName OnHoverCell = "OnHoverCell";

		public static readonly StringName OnUnhoverCell = "OnUnhoverCell";

		public static readonly StringName UpdateDivinationsLeft = "UpdateDivinationsLeft";

		public static readonly StringName OnMinigameFinished = "OnMinigameFinished";

		public static readonly StringName OnProceedButtonPressed = "OnProceedButtonPressed";

		public static readonly StringName AfterOverlayOpened = "AfterOverlayOpened";

		public static readonly StringName AfterOverlayClosed = "AfterOverlayClosed";

		public static readonly StringName AfterOverlayShown = "AfterOverlayShown";

		public static readonly StringName AfterOverlayHidden = "AfterOverlayHidden";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName ScreenType = "ScreenType";

		public static readonly StringName UseSharedBackstop = "UseSharedBackstop";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName _itemsContainer = "_itemsContainer";

		public static readonly StringName _cellContainer = "_cellContainer";

		public static readonly StringName _bigDivinationButton = "_bigDivinationButton";

		public static readonly StringName _smallDivinationButton = "_smallDivinationButton";

		public static readonly StringName _divinationsLeftLabel = "_divinationsLeftLabel";

		public static readonly StringName _mask = "_mask";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _instructionsTitleLabel = "_instructionsTitleLabel";

		public static readonly StringName _instructionsDescriptionLabel = "_instructionsDescriptionLabel";

		public static readonly StringName _instructionsContainer = "_instructionsContainer";

		public static readonly StringName _dialogue = "_dialogue";

		public static readonly StringName _fadeTween = "_fadeTween";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private readonly LocString _instructionsTitleLoc = new LocString("events", "CRYSTAL_SPHERE.minigame.instructions.title");

	private readonly LocString _instructionsDescriptionLoc = new LocString("events", "CRYSTAL_SPHERE.minigame.instructions.description");

	private readonly LocString _divinationsRemainLoc = new LocString("events", "CRYSTAL_SPHERE.minigame.divinationsRemain");

	private const string _scenePath = "res://scenes/events/custom/crystal_sphere/crystal_sphere_screen.tscn";

	private CrystalSphereMinigame _entity;

	private Control _itemsContainer;

	private Control _cellContainer;

	private NDivinationButton _bigDivinationButton;

	private NDivinationButton _smallDivinationButton;

	private MegaRichTextLabel _divinationsLeftLabel;

	private NCrystalSphereMask _mask;

	private NProceedButton _proceedButton;

	private MegaRichTextLabel _instructionsTitleLabel;

	private MegaRichTextLabel _instructionsDescriptionLabel;

	private Control _instructionsContainer;

	private NCrystalSphereDialogue _dialogue;

	private Tween? _fadeTween;

	public NetScreenType ScreenType => NetScreenType.None;

	public bool UseSharedBackstop => false;

	public Control DefaultFocusedControl
	{
		get
		{
			List<NCrystalSphereCell> list = (from c in _cellContainer.GetChildren().OfType<NCrystalSphereCell>()
				where c.Entity.IsHidden
				select c).ToList();
			return list[list.Count / 2];
		}
	}

	public static NCrystalSphereScreen ShowScreen(CrystalSphereMinigame grid)
	{
		NCrystalSphereScreen nCrystalSphereScreen = PreloadManager.Cache.GetScene("res://scenes/events/custom/crystal_sphere/crystal_sphere_screen.tscn").Instantiate<NCrystalSphereScreen>(PackedScene.GenEditState.Disabled);
		nCrystalSphereScreen._entity = grid;
		NOverlayStack.Instance.Push(nCrystalSphereScreen);
		return nCrystalSphereScreen;
	}

	public override void _Ready()
	{
		_itemsContainer = GetNode<Control>("%Items");
		_cellContainer = GetNode<Control>("%Cells");
		_bigDivinationButton = GetNode<NDivinationButton>("%BigDivinationButton");
		_smallDivinationButton = GetNode<NDivinationButton>("%SmallDivinationButton");
		_bigDivinationButton.SetLabel(new LocString("events", "CRYSTAL_SPHERE.button.DIVINATION_LABEL_BIG"));
		_smallDivinationButton.SetLabel(new LocString("events", "CRYSTAL_SPHERE.button.DIVINATION_LABEL_SMALL"));
		_divinationsLeftLabel = GetNode<MegaRichTextLabel>("%DivinationsLeft");
		_mask = GetNode<NCrystalSphereMask>("%ScryMask");
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		_instructionsTitleLabel = GetNode<MegaRichTextLabel>("%InstructionsTitle");
		_instructionsDescriptionLabel = GetNode<MegaRichTextLabel>("%InstructionsDescription");
		_instructionsContainer = GetNode<Control>("%Instructions");
		_instructionsTitleLabel.SetTextAutoSize(_instructionsTitleLoc.GetFormattedText());
		_instructionsDescriptionLabel.SetTextAutoSize(_instructionsDescriptionLoc.GetFormattedText());
		_dialogue = GetNode<NCrystalSphereDialogue>("%Dialogue");
		Vector2 vector = Vector2.One * -(57 * _entity.GridSize.X) * 0.5f;
		NCrystalSphereCell[,] array = new NCrystalSphereCell[_entity.GridSize.X, _entity.GridSize.Y];
		for (int i = 0; i < _entity.GridSize.X; i++)
		{
			for (int j = 0; j < _entity.GridSize.Y; j++)
			{
				NCrystalSphereCell cell = NCrystalSphereCell.Create(_entity.cells[i, j], _mask);
				_cellContainer.AddChildSafely(cell);
				array[i, j] = cell;
				cell.Position = vector + 57f * new Vector2(i, j);
				cell.Connect(Control.SignalName.MouseEntered, Callable.From(delegate
				{
					OnHoverCell(cell);
				}));
				cell.Connect(Control.SignalName.MouseExited, Callable.From(delegate
				{
					OnUnhoverCell(cell);
				}));
				cell.Connect(Control.SignalName.FocusEntered, Callable.From(delegate
				{
					OnHoverCell(cell);
				}));
				cell.Connect(Control.SignalName.FocusExited, Callable.From(delegate
				{
					OnUnhoverCell(cell);
				}));
				cell.Connect(NClickableControl.SignalName.MouseReleased, Callable.From<InputEvent>(delegate
				{
					TaskHelper.RunSafely(OnCellClicked(cell));
				}));
				cell.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>(delegate
				{
					TaskHelper.RunSafely(OnCellClicked(cell));
				}));
			}
		}
		foreach (CrystalSphereItem item in _entity.Items)
		{
			NCrystalSphereItem nCrystalSphereItem = NCrystalSphereItem.Create(item);
			nCrystalSphereItem.Size = item.Size * 57;
			_itemsContainer.AddChildSafely(nCrystalSphereItem);
			nCrystalSphereItem.Position = vector + 57f * new Vector2(item.Position.X, item.Position.Y);
			item.Revealed += OnItemRevealed;
		}
		_bigDivinationButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(SetBigDivination));
		_smallDivinationButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(SetSmallDivination));
		_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnProceedButtonPressed));
		_smallDivinationButton.SetHotkeys(new string[1] { MegaInput.viewExhaustPileAndTabRight });
		_bigDivinationButton.SetHotkeys(new string[1] { MegaInput.viewDeckAndTabLeft });
		UpdateDivinationsLeft();
		_proceedButton.Disable();
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		for (int num = 0; num < array.GetLength(0); num++)
		{
			for (int num2 = 0; num2 < array.GetLength(1); num2++)
			{
				Control control = array[num, num2];
				control.FocusNeighborTop = ((num2 > 0) ? array[num, num2 - 1].GetPath() : array[num, num2].GetPath());
				control.FocusNeighborBottom = ((num2 < array.GetLength(1) - 1) ? array[num, num2 + 1].GetPath() : array[num, num2].GetPath());
				control.FocusNeighborLeft = ((num > 0) ? array[num - 1, num2].GetPath() : array[num, num2].GetPath());
				control.FocusNeighborRight = ((num < array.GetLength(0) - 1) ? array[num + 1, num2].GetPath() : array[num, num2].GetPath());
			}
		}
	}

	private void SetBigDivination(NButton obj)
	{
		_bigDivinationButton.SetActive(isActive: true);
		_smallDivinationButton.SetActive(isActive: false);
		_entity.SetTool(CrystalSphereMinigame.CrystalSphereToolType.Big);
	}

	private void SetSmallDivination(NButton obj)
	{
		_smallDivinationButton.SetActive(isActive: true);
		_bigDivinationButton.SetActive(isActive: false);
		_entity.SetTool(CrystalSphereMinigame.CrystalSphereToolType.Small);
	}

	public override void _EnterTree()
	{
		_entity.DivinationCountChanged += UpdateDivinationsLeft;
		_entity.Finished += OnMinigameFinished;
	}

	public override void _ExitTree()
	{
		_entity.DivinationCountChanged -= UpdateDivinationsLeft;
		_entity.Finished -= OnMinigameFinished;
		_fadeTween?.Kill();
		foreach (CrystalSphereItem item in _entity.Items)
		{
			item.Revealed -= OnItemRevealed;
		}
		_entity.ForceMinigameEnd();
	}

	private void OnItemRevealed(CrystalSphereItem item)
	{
		if (item.IsGood)
		{
			_dialogue.PlayGood();
		}
		else
		{
			_dialogue.PlayBad();
		}
	}

	private async Task OnCellClicked(NCrystalSphereCell cell)
	{
		if (_entity.DivinationCount > 0)
		{
			UpdateDivinationsLeft();
			await _entity.CellClicked(cell.Entity);
			List<NCrystalSphereCell> source = (from c in _cellContainer.GetChildren().OfType<NCrystalSphereCell>()
				where c.Entity.IsHidden
				select c).ToList();
			source.OrderBy((NCrystalSphereCell c1) => new Vector2I(cell.Entity.X, cell.Entity.Y).DistanceTo(new Vector2I(c1.Entity.X, c1.Entity.Y))).First().TryGrabFocus();
		}
	}

	private void OnHoverCell(NCrystalSphereCell cell)
	{
		if (!_entity.IsFinished)
		{
			_entity.SetHoveredCell(cell.Entity);
		}
	}

	private void OnUnhoverCell(NCrystalSphereCell cell)
	{
		_entity.UnsetHoveredCell();
	}

	private void UpdateDivinationsLeft()
	{
		_divinationsRemainLoc.Add("Count", _entity.DivinationCount);
		_divinationsLeftLabel.Text = _divinationsRemainLoc.GetFormattedText() ?? "";
	}

	private void OnMinigameFinished()
	{
		_bigDivinationButton.Visible = false;
		_smallDivinationButton.Visible = false;
		_divinationsLeftLabel.Visible = false;
		_instructionsContainer.Visible = false;
		_proceedButton.Visible = true;
		_dialogue.PlayEnd();
		_proceedButton.Enable();
		NMapScreen.Instance.SetTravelEnabled(enabled: true);
	}

	private void OnProceedButtonPressed(NButton _)
	{
		TaskHelper.RunSafely(RunManager.Instance.ProceedFromTerminalRewardsScreen());
	}

	public void AfterOverlayOpened()
	{
		_itemsContainer.Visible = false;
		_fadeTween?.FastForwardToCompletion();
		_fadeTween = CreateTween();
		_fadeTween.TweenProperty(this, "modulate:a", 1.0, 0.5).From(0f);
		_fadeTween.Chain().TweenCallback(Callable.From(delegate
		{
			_dialogue.PlayStart();
			_itemsContainer.Visible = true;
		}));
	}

	public void AfterOverlayClosed()
	{
		_fadeTween?.FastForwardToCompletion();
		this.QueueFreeSafely();
	}

	public void AfterOverlayShown()
	{
		if (_entity.IsFinished)
		{
			_proceedButton.Enable();
		}
	}

	public void AfterOverlayHidden()
	{
		_proceedButton.Disable();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(14);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetBigDivination, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetSmallDivination, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "obj", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnHoverCell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnUnhoverCell, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "cell", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateDivinationsLeft, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnMinigameFinished, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "_", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayOpened, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayClosed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayShown, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AfterOverlayHidden, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.SetBigDivination && args.Count == 1)
		{
			SetBigDivination(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetSmallDivination && args.Count == 1)
		{
			SetSmallDivination(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
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
		if (method == MethodName.OnHoverCell && args.Count == 1)
		{
			OnHoverCell(VariantUtils.ConvertTo<NCrystalSphereCell>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnhoverCell && args.Count == 1)
		{
			OnUnhoverCell(VariantUtils.ConvertTo<NCrystalSphereCell>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateDivinationsLeft && args.Count == 0)
		{
			UpdateDivinationsLeft();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnMinigameFinished && args.Count == 0)
		{
			OnMinigameFinished();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed && args.Count == 1)
		{
			OnProceedButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayOpened && args.Count == 0)
		{
			AfterOverlayOpened();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayClosed && args.Count == 0)
		{
			AfterOverlayClosed();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayShown && args.Count == 0)
		{
			AfterOverlayShown();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AfterOverlayHidden && args.Count == 0)
		{
			AfterOverlayHidden();
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
		if (method == MethodName.SetBigDivination)
		{
			return true;
		}
		if (method == MethodName.SetSmallDivination)
		{
			return true;
		}
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.OnHoverCell)
		{
			return true;
		}
		if (method == MethodName.OnUnhoverCell)
		{
			return true;
		}
		if (method == MethodName.UpdateDivinationsLeft)
		{
			return true;
		}
		if (method == MethodName.OnMinigameFinished)
		{
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayOpened)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayClosed)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayShown)
		{
			return true;
		}
		if (method == MethodName.AfterOverlayHidden)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName._itemsContainer)
		{
			_itemsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._cellContainer)
		{
			_cellContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._bigDivinationButton)
		{
			_bigDivinationButton = VariantUtils.ConvertTo<NDivinationButton>(in value);
			return true;
		}
		if (name == PropertyName._smallDivinationButton)
		{
			_smallDivinationButton = VariantUtils.ConvertTo<NDivinationButton>(in value);
			return true;
		}
		if (name == PropertyName._divinationsLeftLabel)
		{
			_divinationsLeftLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._mask)
		{
			_mask = VariantUtils.ConvertTo<NCrystalSphereMask>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._instructionsTitleLabel)
		{
			_instructionsTitleLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._instructionsDescriptionLabel)
		{
			_instructionsDescriptionLabel = VariantUtils.ConvertTo<MegaRichTextLabel>(in value);
			return true;
		}
		if (name == PropertyName._instructionsContainer)
		{
			_instructionsContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._dialogue)
		{
			_dialogue = VariantUtils.ConvertTo<NCrystalSphereDialogue>(in value);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			_fadeTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ScreenType)
		{
			value = VariantUtils.CreateFrom<NetScreenType>(ScreenType);
			return true;
		}
		if (name == PropertyName.UseSharedBackstop)
		{
			value = VariantUtils.CreateFrom<bool>(UseSharedBackstop);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			value = VariantUtils.CreateFrom<Control>(DefaultFocusedControl);
			return true;
		}
		if (name == PropertyName._itemsContainer)
		{
			value = VariantUtils.CreateFrom(in _itemsContainer);
			return true;
		}
		if (name == PropertyName._cellContainer)
		{
			value = VariantUtils.CreateFrom(in _cellContainer);
			return true;
		}
		if (name == PropertyName._bigDivinationButton)
		{
			value = VariantUtils.CreateFrom(in _bigDivinationButton);
			return true;
		}
		if (name == PropertyName._smallDivinationButton)
		{
			value = VariantUtils.CreateFrom(in _smallDivinationButton);
			return true;
		}
		if (name == PropertyName._divinationsLeftLabel)
		{
			value = VariantUtils.CreateFrom(in _divinationsLeftLabel);
			return true;
		}
		if (name == PropertyName._mask)
		{
			value = VariantUtils.CreateFrom(in _mask);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._instructionsTitleLabel)
		{
			value = VariantUtils.CreateFrom(in _instructionsTitleLabel);
			return true;
		}
		if (name == PropertyName._instructionsDescriptionLabel)
		{
			value = VariantUtils.CreateFrom(in _instructionsDescriptionLabel);
			return true;
		}
		if (name == PropertyName._instructionsContainer)
		{
			value = VariantUtils.CreateFrom(in _instructionsContainer);
			return true;
		}
		if (name == PropertyName._dialogue)
		{
			value = VariantUtils.CreateFrom(in _dialogue);
			return true;
		}
		if (name == PropertyName._fadeTween)
		{
			value = VariantUtils.CreateFrom(in _fadeTween);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._itemsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._cellContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._bigDivinationButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._smallDivinationButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._divinationsLeftLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._mask, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._instructionsTitleLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._instructionsDescriptionLabel, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._instructionsContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._dialogue, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._fadeTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.ScreenType, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName.UseSharedBackstop, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName._itemsContainer, Variant.From(in _itemsContainer));
		info.AddProperty(PropertyName._cellContainer, Variant.From(in _cellContainer));
		info.AddProperty(PropertyName._bigDivinationButton, Variant.From(in _bigDivinationButton));
		info.AddProperty(PropertyName._smallDivinationButton, Variant.From(in _smallDivinationButton));
		info.AddProperty(PropertyName._divinationsLeftLabel, Variant.From(in _divinationsLeftLabel));
		info.AddProperty(PropertyName._mask, Variant.From(in _mask));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._instructionsTitleLabel, Variant.From(in _instructionsTitleLabel));
		info.AddProperty(PropertyName._instructionsDescriptionLabel, Variant.From(in _instructionsDescriptionLabel));
		info.AddProperty(PropertyName._instructionsContainer, Variant.From(in _instructionsContainer));
		info.AddProperty(PropertyName._dialogue, Variant.From(in _dialogue));
		info.AddProperty(PropertyName._fadeTween, Variant.From(in _fadeTween));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName._itemsContainer, out var value))
		{
			_itemsContainer = value.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._cellContainer, out var value2))
		{
			_cellContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._bigDivinationButton, out var value3))
		{
			_bigDivinationButton = value3.As<NDivinationButton>();
		}
		if (info.TryGetProperty(PropertyName._smallDivinationButton, out var value4))
		{
			_smallDivinationButton = value4.As<NDivinationButton>();
		}
		if (info.TryGetProperty(PropertyName._divinationsLeftLabel, out var value5))
		{
			_divinationsLeftLabel = value5.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._mask, out var value6))
		{
			_mask = value6.As<NCrystalSphereMask>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value7))
		{
			_proceedButton = value7.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._instructionsTitleLabel, out var value8))
		{
			_instructionsTitleLabel = value8.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._instructionsDescriptionLabel, out var value9))
		{
			_instructionsDescriptionLabel = value9.As<MegaRichTextLabel>();
		}
		if (info.TryGetProperty(PropertyName._instructionsContainer, out var value10))
		{
			_instructionsContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._dialogue, out var value11))
		{
			_dialogue = value11.As<NCrystalSphereDialogue>();
		}
		if (info.TryGetProperty(PropertyName._fadeTween, out var value12))
		{
			_fadeTween = value12.As<Tween>();
		}
	}
}
