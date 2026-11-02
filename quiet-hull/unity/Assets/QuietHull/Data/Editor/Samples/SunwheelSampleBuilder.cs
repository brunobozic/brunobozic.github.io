using System;

namespace QuietHull.Data.Editor
{
    /// <summary>
    /// A sample sun-puzzle hulk, the Tremolino (the smuggling boat in Conrad's The Mirror of the Sea):
    /// four facings, and the three rooms the player needs (the T2 fabricator, the cryo vault, the armoury)
    /// are never all open at once, so the job takes several turns. Turning to facing 2 burns out the nest,
    /// and holding it there lets the belt vent the nest completely; holding facing 3 strips the reactor
    /// housing away so the deep hold's radiation drops. Holding facing 0 too long collapses the spine.
    /// </summary>
    public static class SunwheelSampleBuilder
    {
        public const string CatalogId = "cat_sample_sunwheel";

        public static (SunExposureDefinition sun, BeltErosionDefinition belt) Build(IDefinitionFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var make = new DefinitionMaker(factory);
            var sun = make.Def<SunExposureDefinition>("sun_tremolino", "Samples", "Tremolino sun exposure",
                "A slowly tumbling smuggler. Every turn opens some rooms and bakes others.", "sun", "sample");
            sun.facings = 4;
            sun.startFacing = 0;
            sun.turnSeconds = 75f;
            sun.lockIntensity = 3;
            sun.thrusterConsoleId = "console_tremolino_attitude";
            sun.rooms = new[]
            {
                //                         facing:  0  1  2  3
                new RoomExposure("bridge",          1, 0, 1, 2),
                new RoomExposure("fabricator_t2",   3, 2, 1, 0),
                new RoomExposure("armoury",         0, 1, 3, 3),
                new RoomExposure("cryo_vault",      3, 3, 0, 3),
                new RoomExposure("charging_bay",    1, 3, 3, 0),
                new RoomExposure("nest",            2, 1, 3, 3)
            };
            var belt = make.Def<BeltErosionDefinition>("belt_tremolino", "Samples", "Tremolino belt erosion",
                "The Tremolino is drifting through a debris belt. Turn the nest into it to vent the nest; turn the reactor housing into it to strip the radiation from the deep hold.",
                "belt", "sample");
            belt.sunExposure = sun;
            belt.impactsPerMinute = 6f;
            belt.damagePerImpact = 10f;
            belt.impactNoise = 9f;
            belt.sections = new[]
            {
                new ErodingSection("nest_hull", 2, 300f, ErosionOutcome.Breach, string.Empty),
                new ErodingSection("reactor_housing", 3, 600f, ErosionOutcome.StripRadiationSource, "deep_hold"),
                new ErodingSection("armoury_wall", 1, 450f, ErosionOutcome.OpenPassage, "armoury"),
                new ErodingSection("spine_truss", 0, 900f, ErosionOutcome.Collapse, "charging_bay")
            };

            make.WriteCatalog(CatalogId);
            return (sun, belt);
        }
    }
}
