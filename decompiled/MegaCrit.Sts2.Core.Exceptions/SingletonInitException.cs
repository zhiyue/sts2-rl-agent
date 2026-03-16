using System;

namespace MegaCrit.Sts2.Core.Exceptions;

public class SingletonInitException : Exception
{
	public SingletonInitException()
		: base("The singleton was used before initialization.")
	{
	}
}
