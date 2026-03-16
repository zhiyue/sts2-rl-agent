using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

public interface ICardSelector
{
	Task<IEnumerable<CardModel>> CardsSelected();
}
