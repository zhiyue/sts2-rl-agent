"""Card system: base, registry, and all character/pool card modules.

Importing this package triggers registration of all card effects.
"""

# Import modules to trigger @register_effect decorators
import sts2_env.cards.ironclad_basic  # noqa: F401
import sts2_env.cards.ironclad  # noqa: F401
import sts2_env.cards.silent  # noqa: F401
import sts2_env.cards.defect  # noqa: F401
import sts2_env.cards.necrobinder  # noqa: F401
import sts2_env.cards.regent  # noqa: F401
import sts2_env.cards.colorless  # noqa: F401
import sts2_env.cards.status  # noqa: F401
