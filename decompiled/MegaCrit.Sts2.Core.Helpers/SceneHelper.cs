using Godot;

namespace MegaCrit.Sts2.Core.Helpers;

public static class SceneHelper
{
	/// <summary>
	/// Get the path to the scene with the specified "inner" path.
	/// </summary>
	/// <param name="innerPath">The scene's inner path. For example, for "scenes/ui/button.tscn", pass "ui/button".</param>
	/// <returns>The full path to the scene.</returns>
	public static string GetScenePath(string innerPath)
	{
		if (innerPath.StartsWith('/'))
		{
			string text = innerPath;
			innerPath = text.Substring(1, text.Length - 1);
		}
		return "res://scenes/" + innerPath + ".tscn";
	}

	/// <summary>
	/// Load the packed scene with the specified "inner" path.
	/// </summary>
	/// <param name="innerPath">The scene's inner path. For example, for "scenes/ui/button.tscn", pass "ui/button".</param>
	/// <returns>The PackedScene.</returns>
	private static PackedScene Load(string innerPath)
	{
		string scenePath = GetScenePath(innerPath);
		return ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Reuse);
	}

	/// <summary>
	/// Instantiate the scene with the specified "inner" path.
	/// </summary>
	/// <param name="innerPath">The scene's inner path. For example, for "scenes/ui/button.tscn", pass "ui/button".</param>
	/// <typeparam name="T">The type to cast to. Should be a descendant of <see cref="T:Godot.Node" />.</typeparam>
	/// <returns>The instantiated scene.</returns>
	public static T Instantiate<T>(string innerPath) where T : Node
	{
		return Load(innerPath).Instantiate<T>(PackedScene.GenEditState.Disabled);
	}
}
