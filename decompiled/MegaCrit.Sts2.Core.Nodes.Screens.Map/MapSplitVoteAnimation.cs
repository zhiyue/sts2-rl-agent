using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Nodes.Screens.Map;

public class MapSplitVoteAnimation
{
	private NMapScreen _mapScreen;

	private RunState _runState;

	private Dictionary<MapCoord, NMapPoint> _mapPointDictionary;

	private int _ticks;

	private Player _winner;

	private List<Player> _sortedPlayers;

	private Player? _currentlyHighlightedPlayer;

	public MapSplitVoteAnimation(NMapScreen mapScreen, RunState runState, Dictionary<MapCoord, NMapPoint> mapPointDictionary)
	{
		_mapScreen = mapScreen;
		_runState = runState;
		_mapPointDictionary = mapPointDictionary;
	}

	public async Task TryPlay(MapCoord selectedCoord)
	{
		MapCoord? mapCoord = null;
		bool flag = true;
		List<Player> list = new List<Player>();
		foreach (KeyValuePair<Player, MapCoord?> item in _mapScreen.PlayerVoteDictionary)
		{
			if (!mapCoord.HasValue)
			{
				mapCoord = item.Value;
			}
			else if (item.Value != mapCoord)
			{
				flag = false;
			}
			if (item.Value == selectedCoord)
			{
				list.Add(item.Key);
			}
		}
		if (!flag)
		{
			Rng rng = new Rng((uint)HashCode.Combine(_runState.Rng.Seed, _runState.ActFloor));
			_ticks = rng.NextInt(12, 18);
			float num = rng.NextFloat(0.05f, 0.3f);
			_winner = rng.NextItem(list);
			_sortedPlayers = (from p in (from p in _mapScreen.PlayerVoteDictionary.ToList()
					where p.Value.HasValue
					select p).OrderBy((KeyValuePair<Player, MapCoord?> p) => p.Value, Comparer<MapCoord?>.Create(MapCoordComparer)).ThenBy((KeyValuePair<Player, MapCoord?> p) => _mapPointDictionary[p.Value.Value].VoteContainer.GetVoteIndex(p.Key))
				select p.Key).ToList();
			Tween tween = _mapScreen.CreateTween();
			tween.TweenMethod(Callable.From<float>(TickSplitVoteAnimation), 0f, 1f, 1.2000000476837158).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
			tween.TweenInterval(num);
			await _mapScreen.ToSignal(tween, Tween.SignalName.Finished);
			_mapPointDictionary[selectedCoord].VoteContainer.BouncePlayers();
		}
	}

	private int MapCoordComparer(MapCoord? a, MapCoord? b)
	{
		if (a == b)
		{
			return 0;
		}
		if (!a.HasValue)
		{
			return 1;
		}
		if (!b.HasValue)
		{
			return -1;
		}
		if (a.Value.col != b.Value.col)
		{
			return a.Value.col.CompareTo(b.Value.col);
		}
		if (a.Value.row != b.Value.row)
		{
			return a.Value.row.CompareTo(b.Value.row);
		}
		return 0;
	}

	private void TickSplitVoteAnimation(float value)
	{
		int num = Mathf.RoundToInt(value * (float)_ticks);
		int num2 = _sortedPlayers.IndexOf(_winner) - (_ticks - num);
		int index = (num2 % _sortedPlayers.Count + _sortedPlayers.Count) % _sortedPlayers.Count;
		Player player = _sortedPlayers[index];
		if (_currentlyHighlightedPlayer != player)
		{
			HighlightPlayer(_sortedPlayers[index]);
			NDebugAudioManager.Instance.Play("map_split_tick.mp3", 0.15f, PitchVariance.Small);
		}
	}

	private void HighlightPlayer(Player? player)
	{
		if (_currentlyHighlightedPlayer != player)
		{
			if (_currentlyHighlightedPlayer != null)
			{
				MapCoord value = _mapScreen.PlayerVoteDictionary[_currentlyHighlightedPlayer].Value;
				_mapPointDictionary[value].VoteContainer.SetPlayerHighlighted(_currentlyHighlightedPlayer, isHighlighted: false);
			}
			_currentlyHighlightedPlayer = player;
			if (player != null)
			{
				MapCoord value2 = _mapScreen.PlayerVoteDictionary[player].Value;
				_mapPointDictionary[value2].VoteContainer.SetPlayerHighlighted(player, isHighlighted: true);
			}
		}
	}
}
