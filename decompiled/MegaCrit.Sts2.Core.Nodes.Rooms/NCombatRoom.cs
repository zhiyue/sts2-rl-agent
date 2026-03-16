using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.addons.mega_text;

namespace MegaCrit.Sts2.Core.Nodes.Rooms;

[ScriptPath("res://src/Core/Nodes/Rooms/NCombatRoom.cs")]
public class NCombatRoom : Control, IScreenContext, IRoomWithProceedButton
{
	private struct PlayerAndPets
	{
		public NCreature player;

		public List<NCreature> pets;
	}

	public new class MethodName : Control.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName _EnterTree = "_EnterTree";

		public new static readonly StringName _ExitTree = "_ExitTree";

		public static readonly StringName SubscribeToCombatEvents = "SubscribeToCombatEvents";

		public static readonly StringName AdjustCreatureScaleForAspectRatio = "AdjustCreatureScaleForAspectRatio";

		public static readonly StringName CreateAllyNodes = "CreateAllyNodes";

		public static readonly StringName CreateEnemyNodes = "CreateEnemyNodes";

		public static readonly StringName RemoveCreatureNode = "RemoveCreatureNode";

		public static readonly StringName UpdateCreatureNavigation = "UpdateCreatureNavigation";

		public static readonly StringName OnActiveScreenUpdated = "OnActiveScreenUpdated";

		public static readonly StringName EnableControllerNavigation = "EnableControllerNavigation";

		public static readonly StringName RandomizeEnemyScalesAndHues = "RandomizeEnemyScalesAndHues";

		public static readonly StringName RadialBlur = "RadialBlur";

		public static readonly StringName SetWaitingForOtherPlayersOverlayVisible = "SetWaitingForOtherPlayersOverlayVisible";

		public static readonly StringName OnProceedButtonPressed = "OnProceedButtonPressed";
	}

	public new class PropertyName : Control.PropertyName
	{
		public static readonly StringName Ui = "Ui";

		public static readonly StringName SceneContainer = "SceneContainer";

		public static readonly StringName BgContainer = "BgContainer";

		public static readonly StringName Background = "Background";

		public static readonly StringName ProceedButton = "ProceedButton";

		public static readonly StringName BackCombatVfxContainer = "BackCombatVfxContainer";

		public static readonly StringName CombatVfxContainer = "CombatVfxContainer";

		public static readonly StringName CreatedMsec = "CreatedMsec";

		public static readonly StringName Mode = "Mode";

		public static readonly StringName EncounterSlots = "EncounterSlots";

		public static readonly StringName DefaultFocusedControl = "DefaultFocusedControl";

		public static readonly StringName FocusedControlFromTopBar = "FocusedControlFromTopBar";

		public static readonly StringName _allyContainer = "_allyContainer";

		public static readonly StringName _enemyContainer = "_enemyContainer";

		public static readonly StringName _radialBlur = "_radialBlur";

		public static readonly StringName _proceedButton = "_proceedButton";

		public static readonly StringName _waitingForOtherPlayersOverlay = "_waitingForOtherPlayersOverlay";

		public static readonly StringName _window = "_window";
	}

	public new class SignalName : Control.SignalName
	{
	}

	private const float _centerSafeZone = 150f;

	private const float _defaultPadding = 70f;

	private const float _minimumAutoPadding = 5f;

	private const float _minAlternatingYPos = 40f;

	private const float _maxAlternatingYPos = 60f;

	private const float _alternateYPosBeginPadding = 30f;

	private const float _yPos = 200f;

	private static readonly LocString _waitingLoc = new LocString("gameplay_ui", "MULTIPLAYER_WAITING");

	private readonly List<NCreature> _creatureNodes = new List<NCreature>();

	private readonly List<NCreature> _removingCreatureNodes = new List<NCreature>();

	private Control _allyContainer;

	private Control _enemyContainer;

	private const string _scenePath = "res://scenes/rooms/combat_room.tscn";

	private NRadialBlurVfx _radialBlur;

	private NProceedButton _proceedButton;

	private Control _waitingForOtherPlayersOverlay;

	private ICombatRoomVisuals _visuals;

	private Window _window;

	public static NCombatRoom? Instance => NRun.Instance?.CombatRoom;

	public NCombatUi Ui { get; private set; }

	public IEnumerable<NCreature> CreatureNodes => _creatureNodes;

	public IEnumerable<NCreature> RemovingCreatureNodes => _removingCreatureNodes.Where(GodotObject.IsInstanceValid);

	public Control SceneContainer { get; private set; }

	private Control BgContainer { get; set; }

	public NCombatBackground Background { get; private set; }

	public NProceedButton ProceedButton => _proceedButton;

	public Node BackCombatVfxContainer { get; private set; }

	public Control CombatVfxContainer { get; private set; }

	public ulong CreatedMsec { get; private set; }

	public CombatRoomMode Mode { get; private set; }

	private Control? EncounterSlots { get; set; }

	public static IEnumerable<string> AssetPaths => new global::_003C_003Ez__ReadOnlySingleElementList<string>("res://scenes/rooms/combat_room.tscn");

	public Control DefaultFocusedControl => Ui.Hand.CardHolderContainer;

	public Control? FocusedControlFromTopBar => _creatureNodes.FirstOrDefault(delegate(NCreature c)
	{
		if (c != null && c.IsInteractable)
		{
			Control hitbox = c.Hitbox;
			if (hitbox != null)
			{
				return hitbox.FocusMode == FocusModeEnum.All;
			}
		}
		return false;
	})?.Hitbox ?? DefaultFocusedControl;

	public event Action? ProceedButtonPressed;

	public static NCombatRoom? Create(ICombatRoomVisuals visuals, CombatRoomMode mode)
	{
		if (TestMode.IsOn)
		{
			return null;
		}
		NCombatRoom nCombatRoom = PreloadManager.Cache.GetScene("res://scenes/rooms/combat_room.tscn").Instantiate<NCombatRoom>(PackedScene.GenEditState.Disabled);
		nCombatRoom._visuals = visuals;
		nCombatRoom.Mode = mode;
		return nCombatRoom;
	}

	public override void _Ready()
	{
		Ui = GetNode<NCombatUi>("%CombatUi");
		Ui.Deactivate();
		SceneContainer = GetNode<Control>("%CombatSceneContainer");
		_allyContainer = GetNode<Control>("%AllyContainer");
		_enemyContainer = GetNode<Control>("%EnemyContainer");
		BackCombatVfxContainer = GetNode<Node2D>("%BackCombatVfxContainer");
		CombatVfxContainer = GetNode<Control>("%CombatVfxContainer");
		_radialBlur = GetNode<NRadialBlurVfx>("RadialBlur");
		_waitingForOtherPlayersOverlay = GetNode<Control>("%WaitingForOtherPlayers");
		_waitingForOtherPlayersOverlay.GetNode<MegaLabel>("Label").SetTextAutoSize(_waitingLoc.GetRawText());
		_proceedButton = GetNode<NProceedButton>("%ProceedButton");
		_proceedButton.UpdateText(NProceedButton.ProceedLoc);
		if (Mode == CombatRoomMode.VisualOnly && NEventRoom.Instance == null)
		{
			_proceedButton.Visible = true;
			_proceedButton.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(OnProceedButtonPressed));
			_proceedButton.Enable();
		}
		BgContainer = GetNode<Control>("%BgContainer");
		CreatedMsec = Time.GetTicksMsec();
		Log.Info($"Creating NCombatRoom with mode={Mode} encounter={_visuals.Encounter.Id.Entry}.");
		CreateAllyNodes();
		if (Mode != CombatRoomMode.FinishedCombat)
		{
			CreateEnemyNodes();
		}
		if (Mode != CombatRoomMode.ActiveCombat)
		{
			foreach (NCreature creatureNode in _creatureNodes)
			{
				creatureNode.Hitbox.FocusMode = FocusModeEnum.None;
				SetCreatureIsInteractable(creatureNode.Entity, on: false);
			}
		}
		SceneContainer.Scale = Vector2.One * _visuals.Encounter.GetCameraScaling();
		SceneContainer.Position += _visuals.Encounter.GetCameraOffset();
		SceneContainer.ZIndex = -10;
		CombatVfxContainer.ZIndex = -9;
		_window = GetTree().Root;
		_window.Connect(Viewport.SignalName.SizeChanged, Callable.From(AdjustCreatureScaleForAspectRatio));
		AdjustCreatureScaleForAspectRatio();
		NGame.Instance.SetScreenShakeTarget(SceneContainer);
	}

	public static Rng GenerateBackgroundRngForCurrentPoint(IRunState state)
	{
		uint num = 0u;
		if (state.CurrentMapCoord.HasValue)
		{
			num = (uint)(state.CurrentMapCoord.Value.row + state.CurrentMapCoord.Value.col * 747);
		}
		return new Rng(state.Rng.Seed + num);
	}

	public override void _EnterTree()
	{
		ActiveScreenContext.Instance.Updated += OnActiveScreenUpdated;
		if (Mode == CombatRoomMode.ActiveCombat)
		{
			SubscribeToCombatEvents();
		}
	}

	public override void _ExitTree()
	{
		NGame.Instance.ClearScreenShakeTarget();
		ActiveScreenContext.Instance.Updated -= OnActiveScreenUpdated;
		CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
		CombatManager.Instance.CombatEnded -= RestrictControllerNavigation;
		CombatManager.Instance.CombatWon -= RestrictControllerNavigation;
	}

	private void SubscribeToCombatEvents()
	{
		CombatManager.Instance.CombatSetUp += OnCombatSetUp;
		CombatManager.Instance.CombatEnded += RestrictControllerNavigation;
		CombatManager.Instance.CombatWon += RestrictControllerNavigation;
	}

	private void OnCombatSetUp(CombatState state)
	{
		Ui.Activate(((CombatRoom)_visuals).CombatState);
		if (Background == null)
		{
			SetUpBackground(state.RunState);
		}
	}

	public void SetUpBackground(IRunState state)
	{
		if (Background != null)
		{
			Log.Warn("Tried to set up background twice!");
		}
		Background = _visuals.Encounter.CreateBackground(_visuals.Act, GenerateBackgroundRngForCurrentPoint(state));
		BgContainer.AddChildSafely(Background);
	}

	private void AdjustCreatureScaleForAspectRatio()
	{
		_allyContainer.Scale = Vector2.One;
		_enemyContainer.Scale = Vector2.One;
		_enemyContainer.Position = SceneContainer.Size * 0.5f;
		float num = 0f;
		foreach (NCreature creatureNode in _creatureNodes)
		{
			num = Math.Max(creatureNode.GlobalPosition.X + creatureNode.Visuals.Bounds.Size.X * 0.5f * SceneContainer.Scale.X, num);
		}
		num += 15f;
		if (num > base.Size.X)
		{
			float num2 = base.Size.X / num;
			_allyContainer.Scale = Vector2.One * num2;
			_enemyContainer.Scale = Vector2.One * num2;
			_enemyContainer.Position += Vector2.Left * (num - base.Size.X) * num2;
		}
	}

	private void CreateAllyNodes()
	{
		List<Creature> allies = _visuals.Allies.ToList();
		foreach (Creature item in allies)
		{
			AddCreature(item);
		}
		PositionPlayersAndPets(_creatureNodes.Where((NCreature c) => allies.Contains(c.Entity)).ToList(), _visuals.Encounter.GetCameraScaling(), _visuals.Encounter.FullyCenterPlayers);
	}

	private void CreateEnemyNodes()
	{
		if (_visuals.Encounter.HasScene)
		{
			EncounterSlots = _visuals.Encounter.CreateScene();
		}
		if (EncounterSlots != null)
		{
			_enemyContainer.AddChildSafely(EncounterSlots);
			EncounterSlots.Position -= new Vector2(1920f, 1080f) * 0.5f;
		}
		List<Creature> enemies = _visuals.Enemies.ToList();
		foreach (Creature item in enemies)
		{
			AddCreature(item);
		}
		List<NCreature> creatures = _creatureNodes.Where((NCreature c) => enemies.Contains(c.Entity)).ToList();
		if (EncounterSlots != null)
		{
			PositionCreaturesWithSlots(creatures);
		}
		else
		{
			PositionEnemies(creatures, _visuals.Encounter.GetCameraScaling());
		}
		RandomizeEnemyScalesAndHues();
	}

	private void PositionCreaturesWithSlots(List<NCreature> creatures)
	{
		foreach (NCreature creature in creatures)
		{
			string slotName = creature.Entity.SlotName;
			creature.GlobalPosition = EncounterSlots.GetNode<Marker2D>(slotName).GlobalPosition;
		}
	}

	private void PositionEnemies(List<NCreature> creatures, float scaling)
	{
		float num = 960f / scaling;
		float num2 = 70f;
		float num3 = creatures.Sum((NCreature n) => n.Visuals.Bounds.Size.X);
		float num4 = num3 + (float)(creatures.Count - 1) * num2;
		float val = (num - num4) * 0.5f;
		val = Math.Max(val, 150f);
		float num5 = 0f;
		if (val + num4 > num)
		{
			num2 = Math.Max((num - 150f - num3) / (float)(creatures.Count - 1), 5f);
			num4 = num3 + (float)(creatures.Count - 1) * num2;
			val = (num - num4) * 0.5f;
			if (num2 < 30f)
			{
				num5 = float.Lerp(60f, 40f, (num2 - 5f) / 25f);
			}
			if (val + num4 > num)
			{
				Log.Warn("Creatures for current encounter (" + _visuals.Encounter.Title.GetFormattedText() + ") are being displayed off-screen because they are too wide!");
			}
		}
		for (int num6 = 0; num6 < creatures.Count; num6++)
		{
			NCreature nCreature = creatures[num6];
			nCreature.Position = new Vector2(val + nCreature.Visuals.Bounds.Size.X * 0.5f, 200f - ((num6 % 2 != 0) ? num5 : 0f));
			val += nCreature.Visuals.Bounds.Size.X + num2;
		}
	}

	public static void PositionPlayersAndPets(List<NCreature> creatureNodes, float scaling, bool fullyCenterPlayers)
	{
		List<PlayerAndPets> list = new List<PlayerAndPets>();
		foreach (NCreature creatureNode in creatureNodes)
		{
			if (creatureNode.Entity.IsPlayer)
			{
				PlayerAndPets item = new PlayerAndPets
				{
					player = creatureNode,
					pets = new List<NCreature>()
				};
				if (LocalContext.IsMe(creatureNode.Entity))
				{
					list.Insert(0, item);
				}
				else
				{
					list.Add(item);
				}
			}
		}
		foreach (NCreature creature in creatureNodes)
		{
			if (!creature.Entity.IsPlayer)
			{
				list.First((PlayerAndPets p) => p.player.Entity.Player == creature.Entity.PetOwner).pets.Add(creature);
			}
		}
		float num = 960f / scaling;
		float num2 = 70f;
		int num3 = (int)Math.Ceiling(Math.Sqrt(list.Count));
		int num4 = (int)Math.Ceiling((double)list.Count / (double)num3);
		float num5 = creatureNodes.Take(num3).Sum((NCreature n) => n.Visuals.Bounds.Size.X);
		float num6 = num5 + (float)(num3 - 1) * num2;
		float num7 = num5 * 0.33f;
		float num8 = ((num4 > 1) ? (num7 / (float)(num4 - 1)) : 0f);
		float num9 = ((num4 > 1) ? (120f / (float)(num4 - 1)) : 0f);
		float num10;
		if (fullyCenterPlayers)
		{
			num10 = creatureNodes.First((NCreature c) => c.Entity.IsPlayer).Visuals.Bounds.Size.X * -0.5f;
		}
		else
		{
			num10 = (num - num6) * 0.5f;
			num10 = Math.Max(num10, 150f);
			if (list.Count >= num3 * 2)
			{
				num5 += num7;
			}
			if (num10 + num6 > num)
			{
				num2 = (num - 150f - num5) / (float)(num3 - 1);
				num6 = num5 + (float)(num3 - 1) * num2;
				num10 = (num - num6) * 0.5f;
			}
		}
		for (int num11 = 0; num11 < num3; num11++)
		{
			float targetXPos = num10 + num8 * (float)num11;
			for (int num12 = 0; num12 < num3; num12++)
			{
				int num13 = num11 * num3 + num12;
				if (num13 >= list.Count)
				{
					break;
				}
				PlayerAndPets playerAndPets = list[num13];
				NCreature player = playerAndPets.player;
				List<NCreature> pets = playerAndPets.pets;
				player.Position = new Vector2(0f - targetXPos - player.Visuals.Bounds.Size.X * 0.5f, 200f - num9 * (float)num11);
				if (LocalContext.IsMe(player.Entity) && player.Entity.Player.Character is Necrobinder)
				{
					NCreature osty = null;
					for (int num14 = 0; num14 < pets.Count; num14++)
					{
						NCreature nCreature = pets[num14];
						if (nCreature.Entity.Monster is Osty)
						{
							osty = nCreature;
							pets.RemoveAt(num14);
							break;
						}
					}
					PositionLocalPlayerOsty(ref targetXPos, player.Position.Y, player, osty);
				}
				float num15 = ((pets.Count > 1) ? (player.Visuals.Bounds.Size.X / (float)(pets.Count - 1)) : 0f);
				for (int num16 = 0; num16 < pets.Count; num16++)
				{
					NCreature nCreature2 = pets[num16];
					nCreature2.Position = new Vector2(0f - targetXPos + 20f - (float)num16 * num15 - nCreature2.Visuals.Bounds.Size.X * 0.5f, player.Position.Y + 10f);
				}
				if (num11 > 0)
				{
					playerAndPets.player.Visuals.Modulate = new Color(0.5f, 0.5f, 0.5f);
					foreach (NCreature item2 in pets)
					{
						item2.Visuals.Modulate = new Color(0.5f, 0.5f, 0.5f);
					}
				}
				targetXPos += playerAndPets.player.Visuals.Bounds.Size.X + num2;
			}
		}
		foreach (PlayerAndPets item3 in list)
		{
			item3.player.GetParent().MoveChild(item3.player, 0);
			for (int num17 = 0; num17 < item3.pets.Count; num17++)
			{
				NCreature nCreature3 = item3.pets[num17];
				nCreature3.GetParent().MoveChild(nCreature3, num17 + 1);
				if (!LocalContext.IsMe(item3.player.Entity))
				{
					nCreature3.Visuals.Bounds.Visible = false;
				}
			}
		}
	}

	private static void PositionLocalPlayerOsty(ref float targetXPos, float playerYPosition, NCreature player, NCreature? osty)
	{
		Vector2 position = player.Position;
		position.X = player.Position.X - 150f;
		player.Position = position;
		if (osty != null)
		{
			osty.Position = new Vector2(0f - targetXPos, playerYPosition) + NCreature.GetOstyOffsetFromPlayer(osty.Entity);
		}
		targetXPos += 100f;
	}

	public NCreature? GetCreatureNode(Creature? creature)
	{
		if (creature != null)
		{
			return CreatureNodes.FirstOrDefault((NCreature c) => c.Entity == creature);
		}
		return null;
	}

	public void RemoveCreatureNode(NCreature node)
	{
		_creatureNodes.Remove(node);
		_removingCreatureNodes.Add(node);
		UpdateCreatureNavigation();
		if (GetViewport().GuiGetFocusOwner() == node.Hitbox)
		{
			_creatureNodes[0].Hitbox.TryGrabFocus();
		}
		TaskHelper.RunSafely(RemoveCreatureWhenGone(node));
	}

	private async Task RemoveCreatureWhenGone(NCreature node)
	{
		if (node.DeathAnimationTask != null)
		{
			await node.DeathAnimationTask;
		}
		_removingCreatureNodes.Remove(node);
	}

	public void AddCreature(Creature creature)
	{
		NCreature nCreature = NCreature.Create(creature);
		_creatureNodes.Add(nCreature);
		if (creature.IsPlayer || creature.PetOwner != null)
		{
			_allyContainer.AddChildSafely(nCreature);
		}
		else
		{
			_enemyContainer.AddChildSafely(nCreature);
		}
		if (creature.SlotName != null)
		{
			if (EncounterSlots == null)
			{
				throw new InvalidOperationException($"Creature {creature} has slot name '{creature.SlotName}' but NCombatRoom.EncounterSlots is null.");
			}
			nCreature.GlobalPosition = EncounterSlots.GetNode<Marker2D>(creature.SlotName).GlobalPosition;
		}
		UpdateCreatureNavigation();
		if (creature.PetOwner == null)
		{
			return;
		}
		NCreature creatureNode = GetCreatureNode(creature.PetOwner.Creature);
		Player player = creatureNode.Entity.Player;
		List<NCreature> list = _creatureNodes.Where((NCreature c) => c.Entity.PetOwner == player && (!(c.Entity.Monster is Osty) || !LocalContext.IsMe(player))).ToList();
		nCreature.GetParent().MoveChild(nCreature, creatureNode.GetIndex() + 1);
		if (creature.Monster is Osty && LocalContext.IsMe(player))
		{
			nCreature.OstyScaleToSize(creature.MaxHp, 0f);
			nCreature.GetParent().MoveChild(nCreature, creatureNode.GetIndex());
			return;
		}
		float num = ((list.Count > 1) ? (creatureNode.Visuals.Bounds.Size.X / (float)(list.Count - 1)) : 0f);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			NCreature nCreature2 = list[num2];
			nCreature2.Position = new Vector2(creatureNode.Position.X - 20f + (float)num2 * num + nCreature2.Visuals.Bounds.Size.X * 0.5f, creatureNode.Position.Y + 10f);
			nCreature2.ToggleIsInteractable(on: false);
		}
		if (creatureNode.Position.Y < 199f)
		{
			nCreature.Visuals.Modulate = new Color(0.5f, 0.5f, 0.5f);
		}
	}

	public void SetCreatureIsInteractable(Creature? creature, bool on)
	{
		GetCreatureNode(creature)?.ToggleIsInteractable(on);
		UpdateCreatureNavigation();
	}

	private void UpdateCreatureNavigation()
	{
		List<NCreature> list = (from n in _creatureNodes
			where n.IsInteractable
			orderby n.GlobalPosition.X
			select n).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			Control hitbox = list[num].Hitbox;
			NodePath path;
			if (num <= 0)
			{
				path = list[list.Count - 1].Hitbox.GetPath();
			}
			else
			{
				path = list[num - 1].Hitbox.GetPath();
			}
			hitbox.FocusNeighborLeft = path;
			list[num].Hitbox.FocusNeighborRight = ((num < list.Count - 1) ? list[num + 1].Hitbox.GetPath() : list[0].Hitbox.GetPath());
			list[num].Hitbox.FocusNeighborBottom = Ui.Hand.CardHolderContainer.GetPath();
			list[num].Hitbox.FocusNeighborTop = list[num].Hitbox.GetPath();
			list[num].UpdateNavigation();
		}
		Ui.Hand.CardHolderContainer.FocusNeighborTop = Instance.CreatureNodes.FirstOrDefault()?.Hitbox.GetPath();
	}

	private void OnActiveScreenUpdated()
	{
		if (CombatManager.Instance.IsInProgress)
		{
			this.UpdateControllerNavEnabled();
			if (ActiveScreenContext.Instance.IsCurrent(this))
			{
				Ui.Enable();
			}
			else
			{
				Ui.Disable();
			}
		}
	}

	private void RestrictControllerNavigation(CombatRoom _)
	{
		RestrictControllerNavigation(Array.Empty<Control>());
	}

	public void RestrictControllerNavigation(IEnumerable<Control> whitelist)
	{
		foreach (NCreature creatureNode in _creatureNodes)
		{
			Control hitbox = creatureNode.Hitbox;
			creatureNode.Hitbox.FocusMode = (FocusModeEnum)(whitelist.Contains(hitbox) ? 2 : 0);
			creatureNode.Hitbox.FocusNeighborBottom = creatureNode.Hitbox.GetPath();
		}
		Ui.Hand.DisableControllerNavigation();
	}

	public void EnableControllerNavigation()
	{
		foreach (NCreature creatureNode in _creatureNodes)
		{
			creatureNode.Hitbox.FocusMode = FocusModeEnum.All;
			creatureNode.Hitbox.FocusNeighborBottom = Ui.Hand.CardHolderContainer.GetPath();
		}
		Ui.Hand.EnableControllerNavigation();
	}

	private void RandomizeEnemyScalesAndHues()
	{
		Dictionary<Type, List<NCreature>> dictionary = new Dictionary<Type, List<NCreature>>();
		foreach (NCreature creatureNode in _creatureNodes)
		{
			if (creatureNode.Entity.Side != CombatSide.Player && creatureNode.Entity.Monster != null)
			{
				Type type = creatureNode.Entity.Monster.GetType();
				if (!dictionary.TryGetValue(type, out var value))
				{
					value = (dictionary[type] = new List<NCreature>());
				}
				value.Add(creatureNode);
			}
		}
		foreach (KeyValuePair<Type, List<NCreature>> item in dictionary)
		{
			if (item.Value.Count == 1)
			{
				continue;
			}
			foreach (NCreature item2 in item.Value)
			{
				MonsterModel monster = item2.Entity.Monster;
				int value2 = monster.Creature.MonsterMaxHpBeforeModification.Value;
				float value3 = ((monster.MaxInitialHp != monster.MinInitialHp) ? (((float)(value2 - monster.MinInitialHp) / (float)(monster.MaxInitialHp - monster.MinInitialHp) - 0.5f) * 2f) : 0f);
				value3 = Math.Clamp(value3, 0f, 1f);
				float weight = Math.Max(item2.Visuals.Bounds.Size.X, item2.Visuals.Bounds.Size.Y);
				float amount = Math.Clamp(Mathf.InverseLerp(250f, 100f, weight), 0f, 1f);
				float num = float.Lerp(0.1f, 0.15f, amount);
				item2.SetScaleAndHue(1f + value3 * num, Rng.Chaotic.NextFloat(0.05f));
			}
		}
	}

	public void RadialBlur(VfxPosition vfxPosition = VfxPosition.Center)
	{
		_radialBlur.Activate(vfxPosition);
	}

	public void ShakeOstyIfDead(Player owner)
	{
		_creatureNodes.FirstOrDefault((NCreature c) => c.Entity.Monster is Osty && c.Entity.PetOwner == owner)?.AnimShake();
	}

	public void PlaySplashVfx(Creature target, Color tint)
	{
		NCreature creatureNode = GetCreatureNode(target);
		if (creatureNode != null)
		{
			Control combatVfxContainer = CombatVfxContainer;
			combatVfxContainer.AddChildSafely(NSplashVfx.Create(creatureNode.GetBottomOfHitbox(), tint));
			combatVfxContainer.AddChildSafely(NLiquidOverlayVfx.Create(target, tint));
		}
	}

	public void SetWaitingForOtherPlayersOverlayVisible(bool visible)
	{
		_waitingForOtherPlayersOverlay.Visible = visible;
	}

	private void OnProceedButtonPressed(NButton button)
	{
		_proceedButton.Disable();
		this.ProceedButtonPressed?.Invoke();
	}

	public void TransitionToActiveCombat(CombatRoom combatRoom)
	{
		if (Mode != CombatRoomMode.VisualOnly)
		{
			throw new InvalidOperationException($"Cannot transition to {"ActiveCombat"} from {Mode}.");
		}
		Mode = CombatRoomMode.ActiveCombat;
		_visuals = combatRoom;
		foreach (NCreature creatureNode in _creatureNodes)
		{
			creatureNode.Hitbox.FocusMode = FocusModeEnum.All;
			SetCreatureIsInteractable(creatureNode.Entity, on: true);
		}
		SubscribeToCombatEvents();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(15);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._EnterTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName._ExitTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SubscribeToCombatEvents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.AdjustCreatureScaleForAspectRatio, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateAllyNodes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.CreateEnemyNodes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RemoveCreatureNode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "node", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.UpdateCreatureNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnActiveScreenUpdated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.EnableControllerNavigation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RandomizeEnemyScalesAndHues, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.RadialBlur, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Int, "vfxPosition", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.SetWaitingForOtherPlayersOverlayVisible, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Bool, "visible", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
		}, null));
		list.Add(new MethodInfo(MethodName.OnProceedButtonPressed, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, "button", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Control"), exported: false)
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
		if (method == MethodName.SubscribeToCombatEvents && args.Count == 0)
		{
			SubscribeToCombatEvents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AdjustCreatureScaleForAspectRatio && args.Count == 0)
		{
			AdjustCreatureScaleForAspectRatio();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateAllyNodes && args.Count == 0)
		{
			CreateAllyNodes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CreateEnemyNodes && args.Count == 0)
		{
			CreateEnemyNodes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveCreatureNode && args.Count == 1)
		{
			RemoveCreatureNode(VariantUtils.ConvertTo<NCreature>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UpdateCreatureNavigation && args.Count == 0)
		{
			UpdateCreatureNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated && args.Count == 0)
		{
			OnActiveScreenUpdated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.EnableControllerNavigation && args.Count == 0)
		{
			EnableControllerNavigation();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RandomizeEnemyScalesAndHues && args.Count == 0)
		{
			RandomizeEnemyScalesAndHues();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RadialBlur && args.Count == 1)
		{
			RadialBlur(VariantUtils.ConvertTo<VfxPosition>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetWaitingForOtherPlayersOverlayVisible && args.Count == 1)
		{
			SetWaitingForOtherPlayersOverlayVisible(VariantUtils.ConvertTo<bool>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed && args.Count == 1)
		{
			OnProceedButtonPressed(VariantUtils.ConvertTo<NButton>(in args[0]));
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
		if (method == MethodName._EnterTree)
		{
			return true;
		}
		if (method == MethodName._ExitTree)
		{
			return true;
		}
		if (method == MethodName.SubscribeToCombatEvents)
		{
			return true;
		}
		if (method == MethodName.AdjustCreatureScaleForAspectRatio)
		{
			return true;
		}
		if (method == MethodName.CreateAllyNodes)
		{
			return true;
		}
		if (method == MethodName.CreateEnemyNodes)
		{
			return true;
		}
		if (method == MethodName.RemoveCreatureNode)
		{
			return true;
		}
		if (method == MethodName.UpdateCreatureNavigation)
		{
			return true;
		}
		if (method == MethodName.OnActiveScreenUpdated)
		{
			return true;
		}
		if (method == MethodName.EnableControllerNavigation)
		{
			return true;
		}
		if (method == MethodName.RandomizeEnemyScalesAndHues)
		{
			return true;
		}
		if (method == MethodName.RadialBlur)
		{
			return true;
		}
		if (method == MethodName.SetWaitingForOtherPlayersOverlayVisible)
		{
			return true;
		}
		if (method == MethodName.OnProceedButtonPressed)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.Ui)
		{
			Ui = VariantUtils.ConvertTo<NCombatUi>(in value);
			return true;
		}
		if (name == PropertyName.SceneContainer)
		{
			SceneContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.BgContainer)
		{
			BgContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.Background)
		{
			Background = VariantUtils.ConvertTo<NCombatBackground>(in value);
			return true;
		}
		if (name == PropertyName.BackCombatVfxContainer)
		{
			BackCombatVfxContainer = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		if (name == PropertyName.CombatVfxContainer)
		{
			CombatVfxContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName.CreatedMsec)
		{
			CreatedMsec = VariantUtils.ConvertTo<ulong>(in value);
			return true;
		}
		if (name == PropertyName.Mode)
		{
			Mode = VariantUtils.ConvertTo<CombatRoomMode>(in value);
			return true;
		}
		if (name == PropertyName.EncounterSlots)
		{
			EncounterSlots = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._allyContainer)
		{
			_allyContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._enemyContainer)
		{
			_enemyContainer = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._radialBlur)
		{
			_radialBlur = VariantUtils.ConvertTo<NRadialBlurVfx>(in value);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			_proceedButton = VariantUtils.ConvertTo<NProceedButton>(in value);
			return true;
		}
		if (name == PropertyName._waitingForOtherPlayersOverlay)
		{
			_waitingForOtherPlayersOverlay = VariantUtils.ConvertTo<Control>(in value);
			return true;
		}
		if (name == PropertyName._window)
		{
			_window = VariantUtils.ConvertTo<Window>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.Ui)
		{
			value = VariantUtils.CreateFrom<NCombatUi>(Ui);
			return true;
		}
		Control from;
		if (name == PropertyName.SceneContainer)
		{
			from = SceneContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.BgContainer)
		{
			from = BgContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Background)
		{
			value = VariantUtils.CreateFrom<NCombatBackground>(Background);
			return true;
		}
		if (name == PropertyName.ProceedButton)
		{
			value = VariantUtils.CreateFrom<NProceedButton>(ProceedButton);
			return true;
		}
		if (name == PropertyName.BackCombatVfxContainer)
		{
			value = VariantUtils.CreateFrom<Node>(BackCombatVfxContainer);
			return true;
		}
		if (name == PropertyName.CombatVfxContainer)
		{
			from = CombatVfxContainer;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CreatedMsec)
		{
			value = VariantUtils.CreateFrom<ulong>(CreatedMsec);
			return true;
		}
		if (name == PropertyName.Mode)
		{
			value = VariantUtils.CreateFrom<CombatRoomMode>(Mode);
			return true;
		}
		if (name == PropertyName.EncounterSlots)
		{
			from = EncounterSlots;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.DefaultFocusedControl)
		{
			from = DefaultFocusedControl;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.FocusedControlFromTopBar)
		{
			from = FocusedControlFromTopBar;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._allyContainer)
		{
			value = VariantUtils.CreateFrom(in _allyContainer);
			return true;
		}
		if (name == PropertyName._enemyContainer)
		{
			value = VariantUtils.CreateFrom(in _enemyContainer);
			return true;
		}
		if (name == PropertyName._radialBlur)
		{
			value = VariantUtils.CreateFrom(in _radialBlur);
			return true;
		}
		if (name == PropertyName._proceedButton)
		{
			value = VariantUtils.CreateFrom(in _proceedButton);
			return true;
		}
		if (name == PropertyName._waitingForOtherPlayersOverlay)
		{
			value = VariantUtils.CreateFrom(in _waitingForOtherPlayersOverlay);
			return true;
		}
		if (name == PropertyName._window)
		{
			value = VariantUtils.CreateFrom(in _window);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Ui, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._allyContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._enemyContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.SceneContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BgContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.Background, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._radialBlur, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._proceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.ProceedButton, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._waitingForOtherPlayersOverlay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.BackCombatVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.CombatVfxContainer, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.CreatedMsec, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.Mode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.EncounterSlots, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._window, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.DefaultFocusedControl, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName.FocusedControlFromTopBar, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.Ui, Variant.From<NCombatUi>(Ui));
		info.AddProperty(PropertyName.SceneContainer, Variant.From<Control>(SceneContainer));
		info.AddProperty(PropertyName.BgContainer, Variant.From<Control>(BgContainer));
		info.AddProperty(PropertyName.Background, Variant.From<NCombatBackground>(Background));
		info.AddProperty(PropertyName.BackCombatVfxContainer, Variant.From<Node>(BackCombatVfxContainer));
		info.AddProperty(PropertyName.CombatVfxContainer, Variant.From<Control>(CombatVfxContainer));
		info.AddProperty(PropertyName.CreatedMsec, Variant.From<ulong>(CreatedMsec));
		info.AddProperty(PropertyName.Mode, Variant.From<CombatRoomMode>(Mode));
		info.AddProperty(PropertyName.EncounterSlots, Variant.From<Control>(EncounterSlots));
		info.AddProperty(PropertyName._allyContainer, Variant.From(in _allyContainer));
		info.AddProperty(PropertyName._enemyContainer, Variant.From(in _enemyContainer));
		info.AddProperty(PropertyName._radialBlur, Variant.From(in _radialBlur));
		info.AddProperty(PropertyName._proceedButton, Variant.From(in _proceedButton));
		info.AddProperty(PropertyName._waitingForOtherPlayersOverlay, Variant.From(in _waitingForOtherPlayersOverlay));
		info.AddProperty(PropertyName._window, Variant.From(in _window));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.Ui, out var value))
		{
			Ui = value.As<NCombatUi>();
		}
		if (info.TryGetProperty(PropertyName.SceneContainer, out var value2))
		{
			SceneContainer = value2.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.BgContainer, out var value3))
		{
			BgContainer = value3.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.Background, out var value4))
		{
			Background = value4.As<NCombatBackground>();
		}
		if (info.TryGetProperty(PropertyName.BackCombatVfxContainer, out var value5))
		{
			BackCombatVfxContainer = value5.As<Node>();
		}
		if (info.TryGetProperty(PropertyName.CombatVfxContainer, out var value6))
		{
			CombatVfxContainer = value6.As<Control>();
		}
		if (info.TryGetProperty(PropertyName.CreatedMsec, out var value7))
		{
			CreatedMsec = value7.As<ulong>();
		}
		if (info.TryGetProperty(PropertyName.Mode, out var value8))
		{
			Mode = value8.As<CombatRoomMode>();
		}
		if (info.TryGetProperty(PropertyName.EncounterSlots, out var value9))
		{
			EncounterSlots = value9.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._allyContainer, out var value10))
		{
			_allyContainer = value10.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._enemyContainer, out var value11))
		{
			_enemyContainer = value11.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._radialBlur, out var value12))
		{
			_radialBlur = value12.As<NRadialBlurVfx>();
		}
		if (info.TryGetProperty(PropertyName._proceedButton, out var value13))
		{
			_proceedButton = value13.As<NProceedButton>();
		}
		if (info.TryGetProperty(PropertyName._waitingForOtherPlayersOverlay, out var value14))
		{
			_waitingForOtherPlayersOverlay = value14.As<Control>();
		}
		if (info.TryGetProperty(PropertyName._window, out var value15))
		{
			_window = value15.As<Window>();
		}
	}
}
