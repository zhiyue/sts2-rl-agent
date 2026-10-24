using System;
using System.Collections.Generic;

namespace MegaCrit.Sts2.Core.Debug;

/// <summary>
/// Implementation of semver. Mostly used for comparing versions.
/// https://semver.org
/// </summary>
public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
	private enum ParseState
	{
		None,
		Major,
		Minor,
		Patch,
		Prerelease,
		Metadata
	}

	public int Major { get; }

	public int Minor { get; }

	public int Patch { get; }

	public List<string>? Prerelease { get; }

	public string? Metadata { get; }

	public SemanticVersion(int major, int minor, int patch, string? metadata = null, List<string>? prerelease = null)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
		Metadata = metadata;
		Prerelease = prerelease;
	}

	public static bool TryFromString(string str, out SemanticVersion? version)
	{
		try
		{
			version = FromString(str);
			return true;
		}
		catch (Exception ex) when (((ex is InvalidOperationException || ex is FormatException) ? 1 : 0) != 0)
		{
			version = null;
			return false;
		}
	}

	public static SemanticVersion FromString(string version)
	{
		int major = 0;
		int minor = 0;
		int patch = 0;
		List<string> list = null;
		string metadata = null;
		int num = 0;
		ParseState parseState = ParseState.None;
		for (int i = 0; i < version.Length; i++)
		{
			if (i == 0 && version[i] == 'v')
			{
				num++;
				continue;
			}
			if (version[i] == '.')
			{
				switch (parseState)
				{
				case ParseState.Major:
				{
					int num2 = num;
					major = int.Parse(version.Substring(num2, i - num2));
					parseState = ParseState.Minor;
					break;
				}
				case ParseState.Minor:
				{
					int num2 = num;
					minor = int.Parse(version.Substring(num2, i - num2));
					parseState = ParseState.Patch;
					break;
				}
				case ParseState.Patch:
					throw new InvalidOperationException($"Version {version} has a . in an invalid place! Parse state is {parseState}, index is {i}");
				case ParseState.Prerelease:
				{
					List<string> list2 = list;
					int num2 = num;
					list2.Add(version.Substring(num2, i - num2));
					break;
				}
				}
				num = i + 1;
				continue;
			}
			if (version[i] == '-')
			{
				if (parseState != ParseState.Patch)
				{
					throw new InvalidOperationException($"Version {version} has a - in an invalid place! Parse state is {parseState}, index is {i}");
				}
				int num2 = num;
				patch = int.Parse(version.Substring(num2, i - num2));
				parseState = ParseState.Prerelease;
				list = new List<string>();
				num = i + 1;
				continue;
			}
			if (version[i] == '+')
			{
				int num2;
				switch (parseState)
				{
				case ParseState.Patch:
					num2 = num;
					patch = int.Parse(version.Substring(num2, i - num2));
					break;
				case ParseState.Prerelease:
				{
					List<string> list3 = list;
					num2 = num;
					list3.Add(version.Substring(num2, i - num2));
					break;
				}
				default:
					throw new InvalidOperationException($"Version {version} has a + in an invalid place! Parse state is {parseState}, index is {i}");
				}
				string text = version;
				num2 = i + 1;
				metadata = text.Substring(num2, text.Length - num2);
				parseState = ParseState.Metadata;
				break;
			}
			if (parseState == ParseState.None)
			{
				parseState = ParseState.Major;
			}
		}
		switch (parseState)
		{
		case ParseState.Patch:
		{
			string text = version;
			int num2 = num;
			patch = int.Parse(text.Substring(num2, text.Length - num2));
			break;
		}
		case ParseState.Prerelease:
		{
			List<string> list4 = list;
			string text = version;
			int num2 = num;
			list4.Add(text.Substring(num2, text.Length - num2));
			break;
		}
		default:
			throw new InvalidOperationException($"Version terminated in an invalid place! Parse state is {parseState}");
		case ParseState.Metadata:
			break;
		}
		return new SemanticVersion(major, minor, patch, metadata, list);
	}

	public int CompareTo(SemanticVersion? other)
	{
		if (this == other)
		{
			return 0;
		}
		if (other == null)
		{
			return 1;
		}
		int num = Major.CompareTo(other.Major);
		if (num != 0)
		{
			return num;
		}
		int num2 = Minor.CompareTo(other.Minor);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = Patch.CompareTo(other.Patch);
		if (num3 != 0)
		{
			return num3;
		}
		int num4 = Prerelease?.Count ?? 0;
		int num5 = other.Prerelease?.Count ?? 0;
		if (num4 == 0 && num5 == 0)
		{
			return 0;
		}
		if (num4 > 0 && num5 == 0)
		{
			return -1;
		}
		if (num4 == 0 && num5 > 0)
		{
			return 1;
		}
		for (int i = 0; i < Math.Min(num4, num5); i++)
		{
			if (!(Prerelease[i] == other.Prerelease[i]))
			{
				int result;
				bool flag = int.TryParse(Prerelease[i], out result);
				int result2;
				bool flag2 = int.TryParse(other.Prerelease[i], out result2);
				if (!flag && !flag2)
				{
					return string.Compare(Prerelease[i], other.Prerelease[i], StringComparison.Ordinal);
				}
				if (flag && !flag2)
				{
					return -1;
				}
				if (!flag && flag2)
				{
					return 1;
				}
				if (flag && flag2)
				{
					return result.CompareTo(result2);
				}
			}
		}
		return num4.CompareTo(num5);
	}

	public bool Equals(SemanticVersion? other)
	{
		return CompareTo(other) == 0;
	}

	public override string ToString()
	{
		string text = $"v{Major}.{Minor}.{Patch}";
		if (Prerelease != null)
		{
			text = text + "-" + string.Join(".", Prerelease);
		}
		if (Metadata != null)
		{
			text = text + "+" + Metadata;
		}
		return text;
	}
}
