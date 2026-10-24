using System;

namespace MegaCrit.Sts2.Core.AutoSlay.Handlers;

/// <summary>
/// Handler for a specific overlay screen type.
/// </summary>
public interface IScreenHandler : IHandler
{
	/// <summary>The screen type this handler can handle.</summary>
	Type ScreenType { get; }
}
