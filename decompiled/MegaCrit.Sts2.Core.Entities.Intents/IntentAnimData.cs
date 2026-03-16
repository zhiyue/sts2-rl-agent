using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Helpers;

namespace MegaCrit.Sts2.Core.Entities.Intents;

public static class IntentAnimData
{
	private struct InternalData
	{
		public string[] frames;

		public InternalData(string prefix, int frameCount)
		{
			frames = new string[frameCount];
			for (int i = 0; i < frameCount; i++)
			{
				frames[i] = ImageHelper.GetImagePath($"atlases/intent_atlas.sprites/{prefix}_{i.ToString().PadLeft(2, '0')}.tres");
			}
		}

		public InternalData(string singleFrameName)
		{
			frames = new string[1];
			frames[0] = ImageHelper.GetImagePath("atlases/intent_atlas.sprites/" + singleFrameName + ".tres");
		}
	}

	public const string attack1 = "attack_1";

	public const string attack2 = "attack_2";

	public const string attack3 = "attack_3";

	public const string attack4 = "attack_4";

	public const string attack5 = "attack_5";

	public const string buff = "buff";

	public const string cardDebuff = "card_debuff";

	public const string deathBlow = "death_blow";

	public const string debuff = "debuff";

	public const string defend = "defend";

	public const string escape = "escape";

	public const string heal = "heal";

	public const string hidden = "hidden";

	public const string sleep = "sleep";

	public const string status = "status";

	public const string stun = "stun";

	public const string summon = "summon";

	public const string unknown = "unknown";

	private static readonly Dictionary<string, InternalData> _data = new Dictionary<string, InternalData>
	{
		{
			"attack_1",
			new InternalData("attack/intent_attack_1")
		},
		{
			"attack_2",
			new InternalData("attack/intent_attack_2")
		},
		{
			"attack_3",
			new InternalData("attack/intent_attack_3")
		},
		{
			"attack_4",
			new InternalData("attack/intent_attack_4")
		},
		{
			"attack_5",
			new InternalData("attack/intent_attack_5")
		},
		{
			"buff",
			new InternalData("buff/intent_buff", 30)
		},
		{
			"card_debuff",
			new InternalData("card_debuff/intent_carddebuff", 15)
		},
		{
			"death_blow",
			new InternalData("intent_death_blow")
		},
		{
			"debuff",
			new InternalData("debuff/intent_megadebuff", 11)
		},
		{
			"defend",
			new InternalData("defend/intent_defend", 45)
		},
		{
			"escape",
			new InternalData("escape/intent_escape", 40)
		},
		{
			"heal",
			new InternalData("heal/intent_heal", 45)
		},
		{
			"hidden",
			new InternalData("intent_hidden")
		},
		{
			"sleep",
			new InternalData("sleep/intent_sleep", 16)
		},
		{
			"status",
			new InternalData("status/intent_statuscard", 19)
		},
		{
			"stun",
			new InternalData("stun/intent_stunned", 16)
		},
		{
			"summon",
			new InternalData("summon/intent_summon", 25)
		},
		{
			"unknown",
			new InternalData("unknown/intent_unknown", 30)
		}
	};

	public static IEnumerable<string> AssetPaths => _data.Values.SelectMany((InternalData v) => v.frames);

	public static string GetAnimationFrame(string animation, int frame)
	{
		return _data[animation].frames[frame];
	}

	public static int GetAnimationFrameCount(string animation)
	{
		return _data[animation].frames.Length;
	}
}
