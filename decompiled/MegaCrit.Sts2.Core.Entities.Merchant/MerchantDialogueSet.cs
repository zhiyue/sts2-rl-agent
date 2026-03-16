using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.Entities.Merchant;

public class MerchantDialogueSet
{
	private readonly List<LocString> _welcomeLines = new List<LocString>();

	private readonly List<LocString> _openInventoryLines = new List<LocString>();

	private readonly List<LocString> _foulPotionLines = new List<LocString>();

	private readonly List<LocString> _playerDeadLines = new List<LocString>();

	private readonly List<LocString> _purchaseSuccessLines = new List<LocString>();

	private readonly List<LocString> _purchaseFailureGoldLines = new List<LocString>();

	private readonly List<LocString> _purchaseFailureSpaceLines = new List<LocString>();

	private readonly List<LocString> _purchaseFailureForbiddenLines = new List<LocString>();

	public IReadOnlyList<LocString> WelcomeLines => _welcomeLines;

	public IReadOnlyList<LocString> FoulPotionLines => _foulPotionLines;

	public IReadOnlyList<LocString> PlayerDeadLines => _playerDeadLines;

	public IReadOnlyList<LocString> OpenInventoryLines => _openInventoryLines;

	public static MerchantDialogueSet CreateFromLocStrings(IEnumerable<LocString> locStrings)
	{
		MerchantDialogueSet merchantDialogueSet = new MerchantDialogueSet();
		foreach (LocString locString in locStrings)
		{
			string text = locString.LocEntryKey.Split('.')[^2];
			(text switch
			{
				"welcome" => merchantDialogueSet._welcomeLines, 
				"openInventory" => merchantDialogueSet._openInventoryLines, 
				"foulPotion" => merchantDialogueSet._foulPotionLines, 
				"playerDead" => merchantDialogueSet._playerDeadLines, 
				"purchaseSuccess" => merchantDialogueSet._purchaseSuccessLines, 
				"purchaseFailureGold" => merchantDialogueSet._purchaseFailureGoldLines, 
				"purchaseFailureSpace" => merchantDialogueSet._purchaseFailureSpaceLines, 
				"purchaseFailureForbidden" => merchantDialogueSet._purchaseFailureForbiddenLines, 
				_ => throw new InvalidOperationException("Unexpected merchant dialogue key: " + text), 
			}).Add(locString);
		}
		return merchantDialogueSet;
	}

	public IReadOnlyList<LocString> GetPurchaseSuccessLines(PurchaseStatus status)
	{
		return status switch
		{
			PurchaseStatus.Success => _purchaseSuccessLines, 
			PurchaseStatus.FailureGold => _purchaseFailureGoldLines, 
			PurchaseStatus.FailureSpace => _purchaseFailureSpaceLines, 
			PurchaseStatus.FailureForbidden => _purchaseFailureForbiddenLines, 
			_ => throw new ArgumentOutOfRangeException("status", status, null), 
		};
	}
}
