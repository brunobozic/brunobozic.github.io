# WoW ↔ Corporate: Parallels in Guild/Raid Leadership and Organizational Life

A research project examining how leadership, organization, and group dynamics
observed over 15+ years of **World of Warcraft guild and raid leadership** mirror
the structures and behaviors found in **corporate/organizational life**.

## The thesis

The patterns that make (or break) a high-end raiding guild — recruitment,
retention, officer politics, burnout, loot/reward systems, accountability,
communication under pressure — are not gaming quirks. They are the same
human-coordination problems organizations face, expressed in a compressed,
high-tempo, voluntary environment. Because participation is voluntary and
feedback is fast, guilds often surface these dynamics *more clearly* than the
workplace does.

## How this project is built

Each idea starts as a **first-hand observation** from the author's guild-leadership
experience. Every observation goes through the same pipeline:

1. **Capture** — record the raw observation from lived experience.
2. **Corroborate (WoW side)** — find blog posts, forum threads, guild-leadership
   guides, and other sources that independently describe the same phenomenon.
3. **Parallel (corporate side)** — find research papers, management literature,
   news, and industry writing describing the equivalent dynamic at work.
4. **Synthesize** — assess how strong the parallel really is (strong / partial /
   weak / disanalogous) and write it up.
5. **Accumulate** — repeat until there are enough well-supported cases to write
   the article.
6. **Write** — assemble the final article from the strongest cases.

## Repository layout

```
wow-corporate-parallels/
├── README.md              ← you are here (human overview)
├── CLAUDE.md              ← operating instructions for Claude (the workflow, conventions)
├── MEMORY.md              ← running index + progress log (source of truth for state)
├── knowledge-base/
│   ├── index.md           ← catalog of all cases with status
│   ├── themes.md          ← cross-cutting themes emerging across cases
│   └── cases/
│       └── CASE-XXX-slug/ ← one folder per observation/parallel pair
│           ├── observation.md       (WoW side + corroborating evidence)
│           ├── corporate-parallel.md (corporate research)
│           ├── synthesis.md          (the parallel drawn + strength rating)
│           └── sources.md            (all citations for this case)
├── templates/
│   └── case-template/     ← copy this to start a new case
└── article/
    ├── outline.md         ← evolving article structure
    └── drafts/            ← article drafts
```

## Status

Project scaffolding is in place. Awaiting the first observation from the author.
See `MEMORY.md` for the live case index and next steps.
