using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace MegaCrit.Sts2.Core.Modding;

public delegate IEnumerable<AbstractModel> RunHookSubscriptionDelegate(RunState runState);
