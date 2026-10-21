using RoR2;

namespace Augmentum
{
    static class OptionPickupFix
    {
        private static bool _initialized;

        public static void Init()
        {
            if (_initialized)
                return;
            _initialized = true;

            // PickupDisplays that keep a preset model (dontInstantiatePickupModel, e.g. option pickups) must never have it destroyed.
            // RebuildModel no longer calls DestroyModel directly; it now happens when the model reference is swapped,
            // so the old IL patch on RebuildModel had nothing to match. Guarding DestroyModel itself covers every path.
            On.RoR2.PickupDisplay.DestroyModel += PickupDisplay_DestroyModel;
        }

        private static void PickupDisplay_DestroyModel(On.RoR2.PickupDisplay.orig_DestroyModel orig, PickupDisplay self)
        {
            if (self && self.dontInstantiatePickupModel)
                return;
            orig(self);
        }
    }
}