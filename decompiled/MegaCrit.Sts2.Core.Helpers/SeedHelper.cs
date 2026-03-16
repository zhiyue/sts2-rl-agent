using System.Text;
using MegaCrit.Sts2.Core.Random;

namespace MegaCrit.Sts2.Core.Helpers;

public static class SeedHelper
{
	private const string _characters = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

	public const int seedDefaultLength = 10;

	public static string GetRandomSeed(int length = 10)
	{
		string text;
		do
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append(Rng.Chaotic.NextItem("0123456789ABCDEFGHJKLMNPQRSTUVWXYZ"));
			}
			text = stringBuilder.ToString();
		}
		while (BadWordChecker.ContainsBadWord(text));
		return text;
	}

	public static string CanonicalizeSeed(string seed)
	{
		seed = seed.ToUpperInvariant();
		seed = seed.Replace('O', '0');
		seed = seed.Replace('I', '1');
		seed = seed.Trim();
		return seed;
	}
}
