using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One line the ship's AI says at a point on the countdown.</summary>
    [Serializable]
    public struct Announcement
    {
        [Min(0f)] public float atCountdownSeconds;
        [TextArea(1, 3)] public string text;
        [Tooltip("Played through the glitching voice (power failing in that section).")]
        public bool degraded;

        public Announcement(float atCountdownSeconds, string text, bool degraded)
        {
            this.atCountdownSeconds = atCountdownSeconds;
            this.text = text;
            this.degraded = degraded;
        }
    }

    /// <summary>
    /// The ship AI's countdown lines (WARDEN in the prologue). Entries are ordered from the
    /// start of the countdown to the end, so lookups are a forward scan from the last index.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Scripting/Announcement Schedule", fileName = "ann_")]
    public sealed class AnnouncementScheduleDefinition : DefinitionBase
    {
        [SerializeField] internal string speakerName = "WARDEN";
        [SerializeField] internal Announcement[] announcements = Array.Empty<Announcement>();

        public string SpeakerName => speakerName;
        public IReadOnlyList<Announcement> Announcements => announcements ?? Array.Empty<Announcement>();

        /// <summary>
        /// Takes the next announcement once the countdown has reached it. Call every tick in a loop
        /// until it returns false; <paramref name="cursor"/> is owned by the caller (one per mission run),
        /// so the definition itself stays immutable and shareable. Allocation-free.
        /// </summary>
        public bool TryTakeDue(float countdownSeconds, ref int cursor, out Announcement announcement)
        {
            var list = Announcements;
            if (cursor >= 0 && cursor < list.Count && countdownSeconds <= list[cursor].atCountdownSeconds)
            {
                announcement = list[cursor];
                cursor++;
                return true;
            }

            announcement = default;
            return false;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotEmpty(this, nameof(announcements), announcements);

            var previous = float.MaxValue;
            foreach (var line in Announcements)
            {
                if (line.atCountdownSeconds > previous)
                {
                    report.Error(this, nameof(announcements), $"'{line.text}' is out of order (countdown runs down).");
                }

                if (string.IsNullOrWhiteSpace(line.text))
                {
                    report.Error(this, nameof(announcements), $"line at {line.atCountdownSeconds} s is empty.");
                }

                previous = line.atCountdownSeconds;
            }
        }
    }
}
