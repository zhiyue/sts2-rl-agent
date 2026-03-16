namespace MegaCrit.Sts2.Core.Models.Afflictions;

public sealed class Galvanized : AfflictionModel
{
	public override bool IsStackable => true;

	public override bool HasExtraCardText => true;
}
