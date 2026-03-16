using System;

namespace MegaCrit.Sts2.Core.Saves.Runs;

public class SavedPropertyAttribute : Attribute
{
	public readonly SerializationCondition defaultBehaviour;

	public readonly int order;

	public SavedPropertyAttribute()
	{
		defaultBehaviour = SerializationCondition.AlwaysSave;
	}

	public SavedPropertyAttribute(SerializationCondition defaultBehaviour)
	{
		this.defaultBehaviour = defaultBehaviour;
	}

	public SavedPropertyAttribute(SerializationCondition defaultBehaviour, int order)
	{
		this.defaultBehaviour = defaultBehaviour;
		this.order = order;
	}
}
