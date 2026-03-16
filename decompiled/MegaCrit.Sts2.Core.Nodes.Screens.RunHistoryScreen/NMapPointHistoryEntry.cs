using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

[ScriptPath("res://src/Core/Nodes/Screens/RunHistoryScreen/NMapPointHistoryEntry.cs")]
public class NMapPointHistoryEntry : NClickableControl
{
	public new class MethodName : NClickableControl.MethodName
	{
		public new static readonly StringName _Ready = "_Ready";

		public new static readonly StringName OnFocus = "OnFocus";

		public new static readonly StringName OnUnfocus = "OnUnfocus";

		public static readonly StringName Highlight = "Highlight";

		public static readonly StringName Unhighlight = "Unhighlight";

		public static readonly StringName GetSfxVolume = "GetSfxVolume";

		public static readonly StringName HurryUp = "HurryUp";

		public static readonly StringName SetupForAnimation = "SetupForAnimation";
	}

	public new class PropertyName : NClickableControl.PropertyName
	{
		public static readonly StringName FloorNum = "FloorNum";

		public static readonly StringName _baseScale = "_baseScale";

		public static readonly StringName _texture = "_texture";

		public static readonly StringName _outline = "_outline";

		public static readonly StringName _questIcon = "_questIcon";

		public static readonly StringName _animateInTween = "_animateInTween";

		public static readonly StringName _hoverTween = "_hoverTween";

		public static readonly StringName _baseAngle = "_baseAngle";

		public static readonly StringName _hurryUp = "_hurryUp";
	}

	public new class SignalName : NClickableControl.SignalName
	{
	}

	private Vector2 _baseScale = Vector2.One * 0.7f;

	private RunHistory _runHistory;

	private MapPointHistoryEntry _entry;

	private RunHistoryPlayer? _player;

	private TextureRect _texture;

	private TextureRect _outline;

	private TextureRect _questIcon;

	private Tween? _animateInTween;

	private Tween? _hoverTween;

	private float _baseAngle;

	private bool _hurryUp;

	private static string ScenePath => SceneHelper.GetScenePath("screens/run_history_screen/map_point_history_entry");

	public static IEnumerable<string> AssetPaths => GetAssetPaths();

	public int FloorNum { get; private set; }

	private static IEnumerable<string> GetAssetPaths()
	{
		yield return ScenePath;
		RoomType[] array = new RoomType[6]
		{
			RoomType.Monster,
			RoomType.Elite,
			RoomType.Event,
			RoomType.Shop,
			RoomType.Treasure,
			RoomType.RestSite
		};
		RoomType[] array2 = array;
		foreach (RoomType roomType in array2)
		{
			string roomIconPath = ImageHelper.GetRoomIconPath(MapPointType.Monster, roomType, null);
			if (roomIconPath != null)
			{
				yield return roomIconPath;
			}
			string roomIconOutlinePath = ImageHelper.GetRoomIconOutlinePath(MapPointType.Monster, roomType, null);
			if (roomIconOutlinePath != null)
			{
				yield return roomIconOutlinePath;
			}
		}
		RoomType[] array3 = new RoomType[4]
		{
			RoomType.Monster,
			RoomType.Elite,
			RoomType.Shop,
			RoomType.Treasure
		};
		array2 = array3;
		foreach (RoomType roomType in array2)
		{
			string roomIconPath2 = ImageHelper.GetRoomIconPath(MapPointType.Unknown, roomType, null);
			if (roomIconPath2 != null)
			{
				yield return roomIconPath2;
			}
			string roomIconOutlinePath2 = ImageHelper.GetRoomIconOutlinePath(MapPointType.Unknown, roomType, null);
			if (roomIconOutlinePath2 != null)
			{
				yield return roomIconOutlinePath2;
			}
		}
		foreach (EncounterModel encounter in ModelDb.AllEncounters.Where((EncounterModel e) => e.RoomType == RoomType.Boss))
		{
			string roomIconPath3 = ImageHelper.GetRoomIconPath(MapPointType.Boss, RoomType.Boss, encounter.Id);
			if (roomIconPath3 != null)
			{
				yield return roomIconPath3;
			}
			string roomIconOutlinePath3 = ImageHelper.GetRoomIconOutlinePath(MapPointType.Boss, RoomType.Boss, encounter.Id);
			if (roomIconOutlinePath3 != null)
			{
				yield return roomIconOutlinePath3;
			}
		}
		foreach (AncientEventModel ancient in ModelDb.AllAncients)
		{
			string roomIconPath4 = ImageHelper.GetRoomIconPath(MapPointType.Ancient, RoomType.Event, ancient.Id);
			if (roomIconPath4 != null)
			{
				yield return roomIconPath4;
			}
			string roomIconOutlinePath4 = ImageHelper.GetRoomIconOutlinePath(MapPointType.Ancient, RoomType.Event, ancient.Id);
			if (roomIconOutlinePath4 != null)
			{
				yield return roomIconOutlinePath4;
			}
		}
	}

	public override void _Ready()
	{
		_baseAngle = Rng.Chaotic.NextGaussianFloat(0f, 1f, 0f, 5f);
		_texture = GetNode<TextureRect>("%Icon");
		_outline = GetNode<TextureRect>("%Outline");
		_questIcon = GetNode<TextureRect>("%QuestIcon");
		_texture.RotationDegrees = (Rng.Chaotic.NextBool() ? _baseAngle : (0f - _baseAngle));
		MapPointType mapPointType = _entry.MapPointType;
		RoomType roomType = _entry.Rooms.First().RoomType;
		MapPointType mapPointType2 = _entry.MapPointType;
		bool flag = (uint)(mapPointType2 - 7) <= 1u;
		string roomIconPath = ImageHelper.GetRoomIconPath(mapPointType, roomType, flag ? _entry.Rooms.First().ModelId : null);
		MapPointType mapPointType3 = _entry.MapPointType;
		RoomType roomType2 = _entry.Rooms.First().RoomType;
		mapPointType2 = _entry.MapPointType;
		flag = (uint)(mapPointType2 - 7) <= 1u;
		string roomIconOutlinePath = ImageHelper.GetRoomIconOutlinePath(mapPointType3, roomType2, flag ? _entry.Rooms.First().ModelId : null);
		if (roomIconPath != null)
		{
			_texture.Visible = true;
			_texture.Texture = PreloadManager.Cache.GetCompressedTexture2D(roomIconPath);
		}
		else
		{
			_texture.Visible = false;
		}
		if (roomIconOutlinePath != null)
		{
			_outline.Visible = true;
			_outline.Texture = PreloadManager.Cache.GetCompressedTexture2D(roomIconOutlinePath);
		}
		else
		{
			_outline.Visible = false;
		}
		_questIcon.Visible = false;
		Color modulate = base.Modulate;
		modulate.A = 1f;
		base.Modulate = modulate;
		ConnectSignals();
	}

	public static NMapPointHistoryEntry Create(RunHistory history, MapPointHistoryEntry entry, int floorNum)
	{
		NMapPointHistoryEntry nMapPointHistoryEntry = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NMapPointHistoryEntry>(PackedScene.GenEditState.Disabled);
		nMapPointHistoryEntry._runHistory = history;
		nMapPointHistoryEntry._entry = entry;
		nMapPointHistoryEntry.FloorNum = floorNum;
		return nMapPointHistoryEntry;
	}

	public void SetPlayer(RunHistoryPlayer player)
	{
		_player = player;
		_questIcon.Visible = _entry.GetEntry(_player.Id).CompletedQuests.Count > 0;
	}

	protected override void OnFocus()
	{
		if (_player == null)
		{
			throw new InvalidOperationException("Player has not been set!");
		}
		Highlight();
		HoverTipAlignment alignment = HoverTip.GetHoverTipAlignment(this);
		NHoverTipSet tip = NHoverTipSet.CreateAndShowMapPointHistory(this, NMapPointHistoryHoverTip.Create(FloorNum, _player.Id, _entry));
		Callable.From(delegate
		{
			tip.SetAlignment(this, alignment);
		}).CallDeferred();
		tip.GlobalPosition += Vector2.Down * 96f;
	}

	protected override void OnUnfocus()
	{
		Unhighlight();
		NHoverTipSet.Remove(this);
	}

	public void Highlight()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(_texture, "scale", _baseScale * 1.5f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_texture, "rotation_degrees", 0f, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_outline, "modulate", StsColors.halfTransparentWhite, 0.05).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public void Unhighlight()
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween().SetParallel();
		_hoverTween.TweenProperty(_texture, "scale", _baseScale, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_texture, "rotation_degrees", _baseAngle, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
		_hoverTween.TweenProperty(_outline, "modulate", StsColors.quarterTransparentBlack, 1.0).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
	}

	public async Task AnimateIn(int index)
	{
		base.Visible = true;
		base.Scale = Vector2.One * 0.01f;
		_animateInTween?.Kill();
		_animateInTween = CreateTween().SetParallel();
		_animateInTween.TweenProperty(this, "scale", Vector2.One, _hurryUp ? 0.05 : 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Spring);
		_animateInTween.TweenProperty(this, "modulate", Colors.White, _hurryUp ? 0.05 : 0.1);
		TaskHelper.RunSafely(DoAnimateInEffects());
		if (_hurryUp)
		{
			await Cmd.Wait(0.05f);
			return;
		}
		float num = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 0.4f : 0.5f);
		float to = 0.2f;
		int num2 = ((SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast) ? 5 : 9);
		float weight = (float)index / (float)(index + num2);
		float seconds = Mathf.Lerp(num, to, weight);
		await Cmd.Wait(seconds);
	}

	private async Task DoAnimateInEffects()
	{
		PlayerMapPointHistoryEntry entry = _entry.GetEntry(_player.Id);
		RoomType roomType = _entry.Rooms.Last().RoomType;
		if (_runHistory.MapPointHistory.Last().Last() == _entry && !_runHistory.Win)
		{
			SfxCmd.Play("event:/sfx/block_break");
			NDebugAudioManager.Instance?.Play("STS_DeathStinger_v4_Short_SFX.mp3", 0.75f * GetSfxVolume());
			return;
		}
		if ((uint)(roomType - 1) <= 2u)
		{
			await DoCombatAnimateInEffects(roomType);
			return;
		}
		switch (roomType)
		{
		case RoomType.Shop:
			SfxCmd.Play("event:/sfx/npcs/merchant/merchant_welcome");
			break;
		case RoomType.Treasure:
			SfxCmd.Play("event:/sfx/ui/gold/gold_2", GetSfxVolume());
			break;
		case RoomType.RestSite:
			if (entry.RestSiteChoices.Contains("SMITH"))
			{
				NGame.Instance.ScreenRumble(ShakeStrength.Medium, ShakeDuration.Short, RumbleStyle.Rumble);
				NDebugAudioManager.Instance?.Play("card_smith.mp3", GetSfxVolume(), PitchVariance.Small);
			}
			else if (entry.RestSiteChoices.Contains("HEAL"))
			{
				NDebugAudioManager.Instance?.Play("SOTE_SFX_SleepBlanket_v1.mp3", GetSfxVolume(), PitchVariance.Medium);
			}
			else if (entry.RestSiteChoices.Contains("DIG"))
			{
				NDebugAudioManager.Instance.Play("sts_sfx_shovel_v1.mp3", GetSfxVolume(), PitchVariance.Small);
			}
			else if (entry.RestSiteChoices.Contains("HATCH"))
			{
				SfxCmd.Play("event:/sfx/byrdpip/byrdpip_attack");
			}
			else if (entry.RestSiteChoices.Contains("LIFT"))
			{
				NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
			}
			else if (!entry.RestSiteChoices.Contains("MEND"))
			{
			}
			break;
		case RoomType.Event:
			SfxCmd.Play("event:/sfx/ui/clicks/ui_hover");
			break;
		}
	}

	private async Task DoCombatAnimateInEffects(RoomType roomType)
	{
		CharacterModel byId = ModelDb.GetById<CharacterModel>(_player.Character);
		ShakeStrength? shakeStrength = null;
		switch (roomType)
		{
		case RoomType.Monster:
			shakeStrength = ShakeStrength.Weak;
			await PlaySfx(GetSmallHitSfx(byId));
			break;
		case RoomType.Elite:
			shakeStrength = ShakeStrength.Medium;
			await PlaySfx(GetBigHitSfx(byId));
			break;
		case RoomType.Boss:
			shakeStrength = ShakeStrength.Strong;
			await PlaySfx(GetBigHitSfx(byId));
			break;
		}
		if (shakeStrength.HasValue)
		{
			NGame.Instance.ScreenRumble(shakeStrength.Value, ShakeDuration.Normal, RumbleStyle.Rumble);
		}
		await Cmd.Wait(0.25f);
		foreach (ModelId monsterId in _entry.Rooms.Last().MonsterIds)
		{
			MonsterModel byId2 = ModelDb.GetById<MonsterModel>(monsterId);
			if (byId2.HasDeathSfx)
			{
				SfxCmd.Play(byId2.DeathSfx);
				await Cmd.Wait(0.25f);
			}
		}
	}

	private List<string> GetSmallHitSfx(CharacterModel character)
	{
		int num;
		Span<string> span;
		int index;
		if (!(character is Defect))
		{
			if (!(character is Ironclad))
			{
				if (!(character is Necrobinder))
				{
					if (!(character is Regent))
					{
						if (character is Silent)
						{
							num = 1;
							List<string> list = new List<string>(num);
							CollectionsMarshal.SetCount(list, num);
							span = CollectionsMarshal.AsSpan(list);
							index = 0;
							span[index] = "slash_attack.mp3";
							return list;
						}
						return new List<string>();
					}
					index = 1;
					List<string> list2 = new List<string>(index);
					CollectionsMarshal.SetCount(list2, index);
					span = CollectionsMarshal.AsSpan(list2);
					num = 0;
					span[num] = "slash_attack.mp3";
					return list2;
				}
				num = 1;
				List<string> list3 = new List<string>(num);
				CollectionsMarshal.SetCount(list3, num);
				span = CollectionsMarshal.AsSpan(list3);
				index = 0;
				span[index] = "slash_attack.mp3";
				return list3;
			}
			index = 1;
			List<string> list4 = new List<string>(index);
			CollectionsMarshal.SetCount(list4, index);
			span = CollectionsMarshal.AsSpan(list4);
			num = 0;
			span[num] = "blunt_attack.mp3";
			return list4;
		}
		num = 1;
		List<string> list5 = new List<string>(num);
		CollectionsMarshal.SetCount(list5, num);
		span = CollectionsMarshal.AsSpan(list5);
		index = 0;
		span[index] = "slash_attack.mp3";
		return list5;
	}

	private List<string> GetBigHitSfx(CharacterModel character)
	{
		int num;
		Span<string> span;
		int num2;
		if (!(character is Defect))
		{
			if (!(character is Ironclad))
			{
				if (!(character is Necrobinder))
				{
					if (!(character is Regent))
					{
						if (character is Silent)
						{
							num = 2;
							List<string> list = new List<string>(num);
							CollectionsMarshal.SetCount(list, num);
							span = CollectionsMarshal.AsSpan(list);
							num2 = 0;
							span[num2] = "dagger_throw.mp3";
							num2++;
							span[num2] = "dagger_throw.mp3";
							return list;
						}
						return new List<string>();
					}
					num2 = 1;
					List<string> list2 = new List<string>(num2);
					CollectionsMarshal.SetCount(list2, num2);
					span = CollectionsMarshal.AsSpan(list2);
					num = 0;
					span[num] = "heavy_attack.mp3";
					return list2;
				}
				num = 1;
				List<string> list3 = new List<string>(num);
				CollectionsMarshal.SetCount(list3, num);
				span = CollectionsMarshal.AsSpan(list3);
				num2 = 0;
				span[num2] = "heavy_attack.mp3";
				return list3;
			}
			num2 = 1;
			List<string> list4 = new List<string>(num2);
			CollectionsMarshal.SetCount(list4, num2);
			span = CollectionsMarshal.AsSpan(list4);
			num = 0;
			span[num] = "heavy_attack.mp3";
			return list4;
		}
		num = 1;
		List<string> list5 = new List<string>(num);
		CollectionsMarshal.SetCount(list5, num);
		span = CollectionsMarshal.AsSpan(list5);
		num2 = 0;
		span[num2] = "lightning_orb_evoke.mp3";
		return list5;
	}

	private async Task PlaySfx(List<string> sfxPaths)
	{
		for (int i = 0; i < sfxPaths.Count; i++)
		{
			string text = sfxPaths[i];
			if (text.StartsWith("event:"))
			{
				SfxCmd.Play(text, GetSfxVolume());
			}
			else
			{
				NDebugAudioManager.Instance?.Play(text, GetSfxVolume(), PitchVariance.Medium);
			}
			if (i < sfxPaths.Count - 1)
			{
				await Cmd.Wait(0.1f);
			}
		}
	}

	private float GetSfxVolume()
	{
		if (!_hurryUp)
		{
			return 1f;
		}
		return 0.5f;
	}

	public void HurryUp()
	{
		_hurryUp = true;
	}

	public void SetupForAnimation()
	{
		base.Visible = false;
		base.Modulate = StsColors.transparentBlack;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<MethodInfo> GetGodotMethodList()
	{
		List<MethodInfo> list = new List<MethodInfo>(8);
		list.Add(new MethodInfo(MethodName._Ready, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnFocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.OnUnfocus, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Highlight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.Unhighlight, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.GetSfxVolume, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.HurryUp, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
		list.Add(new MethodInfo(MethodName.SetupForAnimation, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null));
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
		if (method == MethodName.OnFocus && args.Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnUnfocus && args.Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Highlight && args.Count == 0)
		{
			Highlight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Unhighlight && args.Count == 0)
		{
			Unhighlight();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetSfxVolume && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<float>(GetSfxVolume());
			return true;
		}
		if (method == MethodName.HurryUp && args.Count == 0)
		{
			HurryUp();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.SetupForAnimation && args.Count == 0)
		{
			SetupForAnimation();
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
		if (method == MethodName.OnFocus)
		{
			return true;
		}
		if (method == MethodName.OnUnfocus)
		{
			return true;
		}
		if (method == MethodName.Highlight)
		{
			return true;
		}
		if (method == MethodName.Unhighlight)
		{
			return true;
		}
		if (method == MethodName.GetSfxVolume)
		{
			return true;
		}
		if (method == MethodName.HurryUp)
		{
			return true;
		}
		if (method == MethodName.SetupForAnimation)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.FloorNum)
		{
			FloorNum = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			_baseScale = VariantUtils.ConvertTo<Vector2>(in value);
			return true;
		}
		if (name == PropertyName._texture)
		{
			_texture = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._outline)
		{
			_outline = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._questIcon)
		{
			_questIcon = VariantUtils.ConvertTo<TextureRect>(in value);
			return true;
		}
		if (name == PropertyName._animateInTween)
		{
			_animateInTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(in value);
			return true;
		}
		if (name == PropertyName._baseAngle)
		{
			_baseAngle = VariantUtils.ConvertTo<float>(in value);
			return true;
		}
		if (name == PropertyName._hurryUp)
		{
			_hurryUp = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.FloorNum)
		{
			value = VariantUtils.CreateFrom<int>(FloorNum);
			return true;
		}
		if (name == PropertyName._baseScale)
		{
			value = VariantUtils.CreateFrom(in _baseScale);
			return true;
		}
		if (name == PropertyName._texture)
		{
			value = VariantUtils.CreateFrom(in _texture);
			return true;
		}
		if (name == PropertyName._outline)
		{
			value = VariantUtils.CreateFrom(in _outline);
			return true;
		}
		if (name == PropertyName._questIcon)
		{
			value = VariantUtils.CreateFrom(in _questIcon);
			return true;
		}
		if (name == PropertyName._animateInTween)
		{
			value = VariantUtils.CreateFrom(in _animateInTween);
			return true;
		}
		if (name == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom(in _hoverTween);
			return true;
		}
		if (name == PropertyName._baseAngle)
		{
			value = VariantUtils.CreateFrom(in _baseAngle);
			return true;
		}
		if (name == PropertyName._hurryUp)
		{
			value = VariantUtils.CreateFrom(in _hurryUp);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal new static List<PropertyInfo> GetGodotPropertyList()
	{
		List<PropertyInfo> list = new List<PropertyInfo>();
		list.Add(new PropertyInfo(Variant.Type.Vector2, PropertyName._baseScale, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Int, PropertyName.FloorNum, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._texture, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._outline, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._questIcon, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._animateInTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Object, PropertyName._hoverTween, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Float, PropertyName._baseAngle, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		list.Add(new PropertyInfo(Variant.Type.Bool, PropertyName._hurryUp, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false));
		return list;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.FloorNum, Variant.From<int>(FloorNum));
		info.AddProperty(PropertyName._baseScale, Variant.From(in _baseScale));
		info.AddProperty(PropertyName._texture, Variant.From(in _texture));
		info.AddProperty(PropertyName._outline, Variant.From(in _outline));
		info.AddProperty(PropertyName._questIcon, Variant.From(in _questIcon));
		info.AddProperty(PropertyName._animateInTween, Variant.From(in _animateInTween));
		info.AddProperty(PropertyName._hoverTween, Variant.From(in _hoverTween));
		info.AddProperty(PropertyName._baseAngle, Variant.From(in _baseAngle));
		info.AddProperty(PropertyName._hurryUp, Variant.From(in _hurryUp));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.FloorNum, out var value))
		{
			FloorNum = value.As<int>();
		}
		if (info.TryGetProperty(PropertyName._baseScale, out var value2))
		{
			_baseScale = value2.As<Vector2>();
		}
		if (info.TryGetProperty(PropertyName._texture, out var value3))
		{
			_texture = value3.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._outline, out var value4))
		{
			_outline = value4.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._questIcon, out var value5))
		{
			_questIcon = value5.As<TextureRect>();
		}
		if (info.TryGetProperty(PropertyName._animateInTween, out var value6))
		{
			_animateInTween = value6.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._hoverTween, out var value7))
		{
			_hoverTween = value7.As<Tween>();
		}
		if (info.TryGetProperty(PropertyName._baseAngle, out var value8))
		{
			_baseAngle = value8.As<float>();
		}
		if (info.TryGetProperty(PropertyName._hurryUp, out var value9))
		{
			_hurryUp = value9.As<bool>();
		}
	}
}
