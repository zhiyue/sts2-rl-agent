using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Rooms;

public class BackgroundAssets
{
	public string BackgroundScenePath { get; }

	public List<string> BgLayers { get; }

	public string? FgLayer { get; }

	public IEnumerable<string> AssetPaths => from s in new string[2]
		{
			BackgroundScenePath,
			FgLayer ?? string.Empty
		}.Concat(BgLayers)
		where !string.IsNullOrWhiteSpace(s)
		select s;

	public BackgroundAssets(string title, Rng rng)
	{
		string text = "res://scenes/backgrounds/" + title + "/layers";
		using DirAccess dirAccess = DirAccess.Open(text);
		if (dirAccess == null)
		{
			throw new InvalidOperationException("could not find directory " + text);
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		List<string> list = new List<string>();
		dirAccess.ListDirBegin();
		string next = dirAccess.GetNext();
		while (next != "")
		{
			if (dirAccess.CurrentIsDir())
			{
				throw new InvalidOperationException("there should be no other directories within the layers directory");
			}
			if (next.Contains("_fg_"))
			{
				list.Add(text + "/" + next);
			}
			else
			{
				if (!next.Contains("_bg_"))
				{
					throw new InvalidOperationException("files must either contain '_fg_' or '_bg_'");
				}
				string text2 = next.Split("_bg_")[1];
				string key = text2.Split("_")[0];
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new List<string>());
				}
				dictionary[key].Add(text + "/" + next);
			}
			next = dirAccess.GetNext();
		}
		string scenePath = SceneHelper.GetScenePath($"backgrounds/{title}/{title}_background");
		BackgroundScenePath = scenePath;
		BgLayers = SelectRandomBackgroundAssetLayers(rng, dictionary);
		FgLayer = SelectRandomForegroundAssetLayer(rng, list.ToArray());
	}

	private static List<string> SelectRandomBackgroundAssetLayers(Rng rng, Dictionary<string, List<string>> bgLayers)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, List<string>> item2 in bgLayers.OrderBy<KeyValuePair<string, List<string>>, string>((KeyValuePair<string, List<string>> kv) => kv.Key))
		{
			item2.Deconstruct(out var _, out var value);
			List<string> items = value;
			string item = rng.NextItem(items);
			list.Add(item);
		}
		return list;
	}

	private static string? SelectRandomForegroundAssetLayer(Rng rng, IEnumerable<string> fgLayer)
	{
		return rng.NextItem(fgLayer);
	}
}
