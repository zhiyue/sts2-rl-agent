using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.GameActions.Multiplayer;

public abstract class PlayerChoiceContext
{
	/// <summary>
	/// A stack of models that are involved with this choice context.
	/// A model can invoke other models to do some work, and those models can invoke a player choice. For example, a
	/// <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Survivor" /> may autoplay a Sly'd <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Prepared" />. In these cases, when we display the context
	/// to remote players, we want to show the <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Prepared" /> as the model that is involved in the choice, not
	/// the <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Survivor" />.
	/// </summary>
	private Stack<AbstractModel>? _modelStack;

	public AbstractModel? LastInvolvedModel
	{
		get
		{
			Stack<AbstractModel>? modelStack = _modelStack;
			if (modelStack == null || !modelStack.TryPeek(out AbstractModel result))
			{
				return null;
			}
			return result;
		}
	}

	/// <summary>
	/// Add a new model to the top of the context stack.
	/// For example, while <see cref="T:MegaCrit.Sts2.Core.Models.Cards.Prepared" /> is executing, it will be at the top of the context stack.
	/// If it discards a card with <see cref="F:MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Sly" />, then the Sly'd card should be pushed to the top of
	/// the context stack.
	/// </summary>
	public void PushModel(AbstractModel model)
	{
		if (_modelStack == null)
		{
			_modelStack = new Stack<AbstractModel>();
		}
		_modelStack.Push(model);
	}

	public void PopModel(AbstractModel model)
	{
		AbstractModel result = null;
		if (_modelStack == null || !_modelStack.TryPeek(out result) || result != model)
		{
			Log.Error($"Tried to pop model {model} from stack of player choice context {this} but {result} was on the top of the stack instead! (Stack size: {_modelStack?.Count})");
		}
		else
		{
			_modelStack.Pop();
		}
	}

	public abstract Task SignalPlayerChoiceBegun(PlayerChoiceOptions options);

	public abstract Task SignalPlayerChoiceEnded();
}
