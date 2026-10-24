using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

/// <summary>
/// An interface used to define any screen that allows for cards to be chosen
/// (ie NCardSelectionScreen, NChooseACardScreen)
/// </summary>
public interface ICardSelector
{
	Task<IEnumerable<CardModel>> CardsSelected();
}
