namespace MegaCrit.Sts2.Core.Entities.Powers;

public enum PowerInstanceType
{
	None,
	/// <summary>
	/// An instanced power adds a new instance of itself when it's applied, rather than stacking on existing instances.
	/// For example, <see cref="T:MegaCrit.Sts2.Core.Models.Powers.TheBombPower" /> is instanced, since if you play a second <see cref="T:MegaCrit.Sts2.Core.Models.Powers.TheBombPower" />, you want another power ticking down
	/// from 3.
	/// </summary>
	Instanced,
	/// <summary>
	/// Creates one instance per applier. Additional applications from the same applier stack onto that applier's existing instance
	/// rather than creating a new one. For example, <see cref="T:MegaCrit.Sts2.Core.Models.Powers.OblivionPower" /> needs individual instances for each player that
	/// applies it, BUT instances from the same player should stack
	/// </summary>
	InstancedPerApplier
}
