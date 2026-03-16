using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class SceneHelper
{
	public static string GetScenePath(string innerPath)
	{
		if (innerPath.StartsWith('/'))
		{
			string text = innerPath;
			innerPath = text.Substring(1, text.Length - 1);
		}
		return "res://scenes/" + innerPath + ".tscn";
	}

	private static PackedScene Load(string innerPath)
	{
		string scenePath = GetScenePath(innerPath);
		return ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Reuse);
	}

	public static T Instantiate<T>(string innerPath) where T : Node
	{
		return Load(innerPath).Instantiate<T>(PackedScene.GenEditState.Disabled);
	}
}
