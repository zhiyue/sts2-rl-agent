using System;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

public interface IScreenHandler : IHandler
{
	Type ScreenType { get; }
}
