using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// Base for every authored definition (items, units, aliens, recipes, beats).
    /// Definitions are immutable at runtime: game code reads them through the public
    /// properties only, which makes them safe to read from any thread once loaded.
    /// </summary>
    public abstract class DefinitionBase : ScriptableObject
    {
        // Ids are stable keys for saves, telemetry and logs: lower_snake_case with a type prefix.
        private static readonly Regex IdPattern = new Regex("^[a-z][a-z0-9]*(_[a-z0-9]+)*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [Header("Identity")]
        [Tooltip("Stable id used by saves, telemetry and logs (lower_snake_case, e.g. itm_shock_prod).")]
        [SerializeField] internal string id = string.Empty;

        [SerializeField] internal string displayName = string.Empty;

        [TextArea(2, 6)]
        [SerializeField] internal string description = string.Empty;

        [SerializeField] internal Sprite icon;

        [SerializeField] internal GameObject prefab;

        [SerializeField] internal string[] tags = Array.Empty<string>();

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;
        public IReadOnlyList<string> Tags => tags ?? Array.Empty<string>();

        /// <summary>Validates this definition and appends any issues to <paramref name="report"/>.</summary>
        public void Validate(ValidationReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                report.Error(this, nameof(id), "is empty.");
            }
            else if (!IdPattern.IsMatch(id))
            {
                report.Error(this, nameof(id), $"'{id}' must be lower_snake_case.");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                report.Warning(this, nameof(displayName), "is empty.");
            }

            ValidateDefinition(report);
        }

        /// <summary>Type-specific validation. Must not mutate the definition.</summary>
        protected abstract void ValidateDefinition(ValidationReport report);

        public override string ToString() => $"{GetType().Name}({id})";
    }
}
