# CLAUDE.md — Operating instructions for this project

This file tells Claude how to work on the **WoW ↔ Corporate parallels** project.
Read this and `MEMORY.md` at the start of every session before doing anything else.

## What we are doing

We are building a research-backed article arguing that leadership and
organizational dynamics in World of Warcraft guild/raid leadership parallel those
in corporate life. The author supplies first-hand observations; Claude
corroborates them, finds the corporate parallel, and documents everything in a
knowledge base of "cases." When enough strong cases exist, we write the article.

## Golden rules

1. **The author's observation is the seed, not the conclusion.** Never fabricate
   what the author "must have meant." Capture exactly what they say, then research.
2. **Everything is sourced.** Every claim on either the WoW side or the corporate
   side must be backed by a real, retrievable source with a URL. No invented
   citations, no invented statistics. If something can't be sourced, mark it
   `[UNVERIFIED]` and say so.
3. **Rate honesty over neatness.** Every parallel gets a strength rating (see
   below). A weak or disanalogous parallel is a valid, useful finding — record it
   as such rather than forcing the analogy.
4. **MEMORY.md is the source of truth for project state.** Update it at the end of
   every working session: case index, statuses, decisions, and next steps.
5. **One case = one folder.** Never mix two distinct observations in one case.

## Workflow for a new observation

When the author gives a new observation:

1. **Assign a case ID.** Next sequential `CASE-XXX` (check `knowledge-base/index.md`).
   Pick a short kebab-case slug. Create the folder by copying `templates/case-template/`.
2. **Fill `observation.md`:**
   - Record the raw observation verbatim (author's words / paraphrase clearly labeled).
   - Do web research to find independent WoW-community corroboration (guild-leadership
     blogs, Reddit/forums like r/wow, r/CompetitiveWoW, Wowhead, Icy Veins community
     posts, old Elitist Jerks-style material, YouTube/creator commentary, academic
     work on MMO guilds — e.g. Nick Yee, Dmitri Williams, Nardi/Harris).
   - Summarize what corroborates, what complicates, and cite each source.
3. **Fill `corporate-parallel.md`:**
   - Identify the corporate/organizational equivalent.
   - Find management research, HBR/MIT Sloan-type articles, peer-reviewed papers,
     books (cite specific authors/works), and news.
   - Summarize the corporate dynamic and cite each source.
4. **Fill `synthesis.md`:**
   - State the parallel in one crisp sentence.
   - Explain the mechanism both share (the *why*, not just the *what*).
   - Note the disanalogies (voluntary vs. paid, anonymity, time compression, etc.).
   - Assign a **strength rating** (see scale).
5. **Fill `sources.md`:** consolidated citation list for the case (URL, title,
   author/site, date accessed, one-line note on what it supports).
6. **Update `knowledge-base/index.md`, `knowledge-base/themes.md`, and `MEMORY.md`.**

## Strength rating scale

- **STRONG** — same underlying mechanism, well-sourced on both sides, disanalogies minor.
- **PARTIAL** — real parallel but with meaningful caveats or thinner sourcing on one side.
- **WEAK** — surface resemblance; mechanisms differ or evidence is thin.
- **DISANALOGOUS** — looks similar but breaks down under scrutiny (still worth recording).

## Research conventions

- Prefer primary and multiple independent sources over a single blog.
- Distinguish **evidence** (studies, data) from **opinion** (blog takes, forum lore).
  Both are usable; label which is which.
- Capture the URL and enough bibliographic detail to re-find and cite the source.
- When a source is paywalled or uncertain, note it rather than paraphrasing blindly.
- Keep an eye out for recurring cross-case themes and log them in `themes.md`.

## Writing conventions for the files

- Markdown, readable, skimmable. Use headings, short paragraphs, bullet lists.
- Quote the author's observations distinctly (blockquote) from research.
- Inline-link sources; also list them in the case's `sources.md`.
- Keep tone analytical and honest — this is a knowledge base, not marketing copy.

## The article (final stage)

Only start once there is a solid base of cases (target: enough STRONG/PARTIAL cases
to support a real argument, not just a list). The outline lives in
`article/outline.md`; drafts in `article/drafts/`. The article draws from the cases
but is written for a general reader who may not play WoW — explain the WoW context
briefly each time.

## Related files

- `MEMORY.md` — live project state, case index, decisions, next steps.
- `knowledge-base/index.md` — catalog of cases and their statuses.
- `knowledge-base/themes.md` — cross-cutting themes.
- `README.md` — human-facing project overview.
