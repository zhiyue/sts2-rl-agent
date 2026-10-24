namespace MegaCrit.Sts2.Core.Saves.Runs;

public enum SerializationCondition
{
	/// <summary>
	/// Property is always serialized and deserialized no matter what.
	/// </summary>
	AlwaysSave,
	/// <summary>
	/// Property is only serialized and deserialized if Object.Equals returns false when comparing the current value of
	/// the property and the default value of the property when the declaring type's default constructor is called.
	/// </summary>
	SaveIfNotPropertyDefault,
	/// <summary>
	/// Property is only serialized and deserialized if Object.Equals returns false when comparing the current value of
	/// the property and default(T), where T is the property of the type.
	/// </summary>
	SaveIfNotTypeDefault,
	/// <summary>
	/// Property is only serialized and deserialized if it is a collection that is not empty, or if it is not null.
	/// </summary>
	SaveIfNotCollectionEmptyOrNull
}
