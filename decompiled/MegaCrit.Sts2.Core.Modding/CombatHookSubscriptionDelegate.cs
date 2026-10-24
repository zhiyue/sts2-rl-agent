using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;

namespace MegaCrit.Sts2.Core.Modding;

public delegate IEnumerable<AbstractModel> CombatHookSubscriptionDelegate(CombatState combatState);
