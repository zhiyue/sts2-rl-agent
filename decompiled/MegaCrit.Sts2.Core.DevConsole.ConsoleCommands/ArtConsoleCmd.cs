using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;

public class ArtConsoleCmd : AbstractConsoleCmd
{
	private struct MissingArt
	{
		public AbstractModel Model { get; }

		public string IntendedPath { get; }

		public string Description { get; }

		public MissingArt(AbstractModel model, string intendedPath, string description)
		{
			Model = model;
			IntendedPath = intendedPath;
			Description = description;
		}
	}

	private static readonly string[] _types = new string[5] { "affliction", "card", "enchantment", "power", "relic" };

	public override string CmdName => "art";

	public override string Args => "<type:string>";

	public override string Description => "Lists all the content of the specified type that is missing art. " + TypesHint;

	public override bool IsNetworked => false;

	private static string TypesHint => "Types: " + string.Join(", ", _types) + ".";

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		if (args.Length < 1)
		{
			return new CmdResult(success: false, CmdName + " requires a type. " + TypesHint);
		}
		string text = args[0].ToLowerInvariant();
		if (text.EndsWith('s'))
		{
			string text2 = text;
			text = text2.Substring(0, text2.Length - 1);
		}
		List<MissingArt> list = new List<MissingArt>();
		switch (text)
		{
		case "affliction":
			foreach (AfflictionModel debugAffliction in ModelDb.DebugAfflictions)
			{
				if (!debugAffliction.HasOverlay)
				{
					list.Add(new MissingArt(debugAffliction, debugAffliction.OverlayPath, debugAffliction.Description.GetRawText()));
				}
			}
			break;
		case "card":
			foreach (CardModel allCard in ModelDb.AllCards)
			{
				if (allCard is DeprecatedCard)
				{
					continue;
				}
				foreach (string allPortraitPath in allCard.AllPortraitPaths)
				{
					var (text3, spriteName) = AtlasResourceLoader.ParsePath(allPortraitPath);
					if (text3 == null || !AtlasManager.HasSprite(text3, spriteName))
					{
						list.Add(new MissingArt(allCard, allCard.PortraitPath, allCard.Description.GetRawText()));
					}
				}
			}
			break;
		case "enchantment":
			foreach (EnchantmentModel debugEnchantment in ModelDb.DebugEnchantments)
			{
				if (!(debugEnchantment is DeprecatedEnchantment) && !(debugEnchantment.IconPath == debugEnchantment.IntendedIconPath))
				{
					list.Add(new MissingArt(debugEnchantment, debugEnchantment.IntendedIconPath, debugEnchantment.Description.GetRawText()));
				}
			}
			break;
		case "power":
			foreach (PowerModel allPower in ModelDb.AllPowers)
			{
				if (!(allPower.IconPath == allPower.PackedIconPath))
				{
					list.Add(new MissingArt(allPower, allPower.PackedIconPath, allPower.Description.GetRawText()));
				}
			}
			break;
		case "relic":
			foreach (RelicModel allRelic in ModelDb.AllRelics)
			{
				if (!(allRelic.IconPath == allRelic.PackedIconPath))
				{
					list.Add(new MissingArt(allRelic, allRelic.PackedIconPath, allRelic.Description.GetRawText()));
				}
			}
			break;
		default:
			return new CmdResult(success: false, "'" + args[0] + "' is not a valid type. " + TypesHint);
		}
		string value = ((list.Count == 1) ? "" : "s");
		StringBuilder stringBuilder = new StringBuilder($"{list.Count} {text}{value} with missing art:\n");
		foreach (MissingArt item in list.OrderBy((MissingArt m) => m.Model.Id.Entry))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(0, 1, stringBuilder2);
			handler.AppendFormatted(item.Model.Id.Entry);
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
			handler.AppendLiteral("* Intended Path: ");
			handler.AppendFormatted(item.IntendedPath);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(17, 1, stringBuilder2);
			handler.AppendLiteral("* Description: \"");
			handler.AppendFormatted(item.Description);
			handler.AppendLiteral("\"");
			stringBuilder5.AppendLine(ref handler);
		}
		return new CmdResult(success: true, stringBuilder.ToString());
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		if (args.Length <= 1)
		{
			List<string> candidates = _types.ToList();
			return CompleteArgument(candidates, Array.Empty<string>(), args.FirstOrDefault() ?? "");
		}
		return new CompletionResult
		{
			Type = CompletionType.Argument,
			ArgumentContext = CmdName
		};
	}
}
