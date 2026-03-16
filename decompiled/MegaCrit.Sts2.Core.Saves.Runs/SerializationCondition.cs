namespace MegaCrit.Sts2.Core.Saves.Runs;

public enum SerializationCondition
{
	AlwaysSave,
	SaveIfNotPropertyDefault,
	SaveIfNotTypeDefault,
	SaveIfNotCollectionEmptyOrNull
}
