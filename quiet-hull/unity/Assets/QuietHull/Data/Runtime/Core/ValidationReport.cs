using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>One problem found while validating a definition.</summary>
    public readonly struct ValidationIssue
    {
        public ValidationIssue(ValidationSeverity severity, string definitionId, string definitionType, string field, string message)
        {
            Severity = severity;
            DefinitionId = definitionId ?? string.Empty;
            DefinitionType = definitionType ?? string.Empty;
            Field = field ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public ValidationSeverity Severity { get; }
        public string DefinitionId { get; }
        public string DefinitionType { get; }
        public string Field { get; }
        public string Message { get; }

        public override string ToString() => $"[{Severity}] {DefinitionType} '{DefinitionId}'.{Field}: {Message}";
    }

    /// <summary>
    /// Collects validation issues. Not thread-safe by design: one report per validation pass,
    /// owned by the caller. Validation itself is read-only on the definitions.
    /// </summary>
    public sealed class ValidationReport
    {
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        public IReadOnlyList<ValidationIssue> Issues => _issues;
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool HasErrors => ErrorCount > 0;

        public void Error(DefinitionBase owner, string field, string message) => Add(ValidationSeverity.Error, owner, field, message);

        public void Warning(DefinitionBase owner, string field, string message) => Add(ValidationSeverity.Warning, owner, field, message);

        public void Positive(DefinitionBase owner, string field, float value)
        {
            if (!(value > 0f))
            {
                Error(owner, field, $"must be > 0 (was {value}).");
            }
        }

        public void NonNegative(DefinitionBase owner, string field, float value)
        {
            if (value < 0f || float.IsNaN(value))
            {
                Error(owner, field, $"must be >= 0 (was {value}).");
            }
        }

        public void InRange(DefinitionBase owner, string field, float value, float min, float max)
        {
            if (value < min || value > max || float.IsNaN(value))
            {
                Error(owner, field, $"must be in [{min}, {max}] (was {value}).");
            }
        }

        public void NotNull(DefinitionBase owner, string field, UnityEngine.Object value)
        {
            // Unity overloads == for destroyed objects, so use it rather than ReferenceEquals.
            if (value == null)
            {
                Error(owner, field, "is not assigned.");
            }
        }

        public void NotEmpty<T>(DefinitionBase owner, string field, IReadOnlyCollection<T> values)
        {
            if (values == null || values.Count == 0)
            {
                Error(owner, field, "must contain at least one entry.");
            }
        }

        public void Merge(ValidationReport other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            foreach (var issue in other._issues)
            {
                AddIssue(issue);
            }
        }

        private void Add(ValidationSeverity severity, DefinitionBase owner, string field, string message)
        {
            var id = owner != null ? owner.Id : string.Empty;
            var type = owner != null ? owner.GetType().Name : string.Empty;
            AddIssue(new ValidationIssue(severity, id, type, field, message));
        }

        private void AddIssue(ValidationIssue issue)
        {
            _issues.Add(issue);
            if (issue.Severity == ValidationSeverity.Error)
            {
                ErrorCount++;
            }
            else
            {
                WarningCount++;
            }
        }
    }
}
