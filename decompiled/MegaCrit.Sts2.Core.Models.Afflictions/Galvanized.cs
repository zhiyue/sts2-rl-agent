namespace MegaCrit.Sts2.Core.Models.Afflictions;

/// <summary>
/// Most of this Affliction's logic lives in <see cref="T:MegaCrit.Sts2.Core.Models.Powers.GalvanicPower" />.
/// </summary>
public sealed class Galvanized : AfflictionModel
{
	public override bool IsStackable => true;

	public override bool HasExtraCardText => true;
}
