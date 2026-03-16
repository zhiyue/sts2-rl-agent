namespace MegaCrit.Sts2.Core.MonsterMoves.Intents;

public class HiddenIntent : AbstractIntent
{
	public override IntentType IntentType => IntentType.Hidden;

	protected override string IntentPrefix => "HIDDEN";

	protected override string? SpritePath => null;

	public override bool HasIntentTip => false;
}
