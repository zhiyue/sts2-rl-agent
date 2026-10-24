using System;

namespace MegaCrit.Sts2.Core.Exceptions;

public class SoftlockException : Exception
{
	public SoftlockException(string message)
		: base(message)
	{
	}
}
