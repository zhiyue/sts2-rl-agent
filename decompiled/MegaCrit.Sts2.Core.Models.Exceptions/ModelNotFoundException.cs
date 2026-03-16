using System;

namespace MegaCrit.Sts2.Core.Models.Exceptions;

public class ModelNotFoundException : Exception
{
	public ModelNotFoundException(ModelId id)
		: base($"Model id={id} not found")
	{
	}
}
