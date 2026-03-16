using System;

namespace MegaCrit.Sts2.Core.Exceptions;

public class SaveException : Exception
{
	public SaveException(string message)
		: base(message)
	{
	}
}
