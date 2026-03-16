using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;

namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class StatusIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.StatusCard;

	protected override LocString IntentLabelFormat => new LocString("intents", "FORMAT_STATUS_CARD_COUNT");

	protected override string SpritePath => "atlases/intent_atlas.sprites/intent_status_card.tres";

	protected override string IntentPrefix => "STATUS";

	public int CardCount { get; }

	public StatusIntent(int count)
	{
		CardCount = count;
	}

	public override LocString GetIntentLabel(IEnumerable<Creature> _, Creature __)
	{
		LocString intentLabelFormat = IntentLabelFormat;
		intentLabelFormat.Add("CardCount", CardCount);
		return intentLabelFormat;
	}

	protected override LocString GetIntentDescription(IEnumerable<Creature> targets, Creature owner)
	{
		LocString intentDescription = base.GetIntentDescription(targets, owner);
		intentDescription.Add("CardCount", CardCount);
		return intentDescription;
	}
}
