# MEMORY.md — Live project state

> Source of truth for where this project stands. Update at the end of every
> working session. Read this + `CLAUDE.md` at the start of every session.

_Last updated: 2026-07-26_

## Project one-liner

Documenting how WoW guild/raid leadership dynamics parallel corporate
organizational life, one sourced "case" at a time, building toward an article.

## Current phase

**Phase 2 — Article revised.** Six STRONG cases + a framing pass, and **Draft 2 of the
article is written** (editorial + fact-check revision of Draft 1):
`article/drafts/the-healer-always-quits-first.md`.

Scaffolding, six fully-researched cases, a framing pass (guild-as-lab), a ~120-source
bibliography across 10 domains, and a complete first draft are all committed. CASE-003
is the thesis-level engine; CASES 001/002/004/005/006 are the five-move enclosure.

### Article draft (Draft 1 — 2026-07-26)
- **File:** `article/drafts/the-healer-always-quits-first.md` (~4,300 words).
- **Structure:** raid-night hook → "a company with the serial numbers filed off" →
  why the guild is a clean *mirror* (framing, with the honest transfer-limit seeded) →
  the five-move enclosure (coerced in / gatekept / trapped / can't leave / can't speak
  up) → the twist/engine (same seat, same brain) → where the analogy breaks → so-what
  (the tells + psychological safety) → return to the raid-night scene.
- **Framing anchored honestly:** guild = supported *mirror/laboratory* (Bainbridge,
  Castronova, Prax, Shen/Monge/Williams); NOT a leadership pipeline (Lu/Shen/Williams:
  guild leadership predicts volunteer-org, not company, leadership). See
  `knowledge-base/framing-guild-as-lab.md`.
- **Every claim traces** to a case `sources.md`; first-hand recollections labeled as
  such, not dressed as proof.

### Article revision (Draft 2 — 2026-07-26)
Editorial + fact-check pass on Draft 1 (~4,300 → ~4,900 words). Reviewed across ~12
lenses (org-psych, methodology, WoW-domain, prose, structure, red-team, fact-check,
exec-reader, lay-reader, labor-ethics, publishing, legal). Changes committed
(`c7750ee`):
- **Honesty — narrator self-implication.** Added a paragraph in the "same seat, same
  brain" section where the author owns having made the five enclosure moves himself
  (deferred a healer, let "lazy healers" stand, treated the namer as the problem). Closes
  the credibility gap where the "I" narrated the moves from outside. Makes the author
  Exhibit A for the seat thesis.
- **Overclaim softened.** "That healer is almost always right" → early-warning-system
  framing (you won't always agree; a team that punishes the warning just stops hearing
  it). Removes the one line a skeptical exec could seize on.
- **Citation corrections (pending the verification round below):**
  - Dropped the unsupported "vs ~5% by chance" and "most common where the record was
    weakest" around the 37% connected-CEO figure (37% itself is fine; the embellishments
    were not in Kim & Low as recalled).
  - Softened both uses of the Lu/Shen/Williams transfer finding to "weak/uneven" pending
    primary-text verification (NB: KB `framing-guild-as-lab.md` actually cites this
    precisely — Lu, Shen & Williams 2014, CHB 35:54–60 — so the *original* wording may be
    restorable once the paper is confirmed).
  - Cut the thinly-sourced "PIPs rose eightfold on Glassdoor" stat, kept the point.
  - Verified live during revision: talent-hoarding **+123%** = Hägele, *Talent Hoarding
    in Organizations* (arXiv 2206.15098, 2022). Confirmed correct.
- **Craft:** GearScore/40-man era signpost; foreshadowed the hobby-vs-waged-labor caveat
  at the nurse comparison; folded Hughes into Bandura in Move 4; varied a repeated verbal
  tic; bumped draft marker + closing source caveat.
- **Perspective shift (later pass, per author):** de-personalized the narrator from
  *guild leader* ("I ran guilds… recruited, promoted, benched") to *long-time observer*
  ("played since vanilla, ~two decades, watched the same pattern recur across many
  guilds"). Subtitle, intro, and scattered "my guild / we used / led through" markers
  changed to player/observer framing; the raid-night "we/our" (a player was still in
  those raids) kept. The self-implication paragraph was reworked from "I sat in the seat
  and did the moves" to "I watched it across guild after guild under leaders who shared
  nothing but the chair" + light bystander complicity (nodded along, stayed) — which
  actually *strengthens* the "same seat, same brain" thesis (pattern seen across many
  occupants) while keeping honesty.

### Deep-research Round 2 (2026-07-26) — verification of snippet-sourced citations
Triggered by the Draft-2 fact-check: the article's remaining risk was not wrong claims
but **real citations gathered from research snippets that were never checked against
primary texts** (flagged throughout the case files and `framing-guild-as-lab.md`, whose
publisher pages 403'd). Re-reviewed the whole dataset and deep-researched the 14
load-bearing claims against authoritative sources. **Full log:
`knowledge-base/verification-round-2.md`.**

**Result: 12 of 14 verified accurate as written; 2 corrected.**
- ✅ **Verified** (primary/publisher/author-PDF/first-party): Lu-Shen-Williams transfer
  finding (restored precise wording); Stanley et al. loyalty→exploitation; Bae nurse
  mandatory-overtime; Fuller & Raman 67/16 degree inflation; Hägele +123% talent hoarding;
  Starr et al. 18% noncompetes; Burris challenging-voice; John et al. "Shooting the
  Messenger" (11 experiments); Sleesman et al. escalation (k=141); Grijalva narcissism
  emergence-not-effectiveness; Yee ~20 hrs/week; Barker concertive control.
- ⚠️ **Corrected — CEO connected-hiring (Kim & Low, 37%):** 37% and above-chance hold, but
  the paper's valence is *efficiency, not cronyism* (connected CEOs enhance firm value).
  The draft's "most common where the record was weakest" spin and the "~5% by chance"
  figure were **removed**; keep only the access-asymmetry point. (Matches CASE-002's own
  complication #8.)
- ⚠️ **Corrected — PIP "eightfold":** traces to one secondary source (WorkLife.news citing
  Glassdoor), underlying data not independently retrievable → **cut** from the article.
  Sturdier substitutes noted (HR Acuity ~44/1,000 in 2023, up ~30% since 2020; Amazon
  PIP-then-layoff wave).
- **KB updated:** new `verification-round-2.md`; Round-2 notes added to
  `framing-guild-as-lab.md`, CASE-002 `deep-research.md`, CASE-004 `sources.md`.

### Next steps for the article (revision pass — non-blocking)
1. Author read-through: voice/tone, any facts to correct, cuts (it runs long).
2. Before any *public* publication: re-verify the snippet-sourced citations against
   original texts (flagged throughout the case files) — esp. exact stats and the
   `[DOI verify]` items.
3. Decide venue/length; possibly split into a series (one case per post) or tighten to
   ~3,000 words for a single essay.
4. Title decision: current pick "The Healer Always Quits First" (alts in `outline.md`).
5. Optional: more observations → more cases → more sections, if the author wants scope
   beyond the healer arc (e.g., recruitment scams, loot-council corruption, server-first
   race dynamics).

### Deep-research Round 1 (2026-07-25) — what it added
- **New KB artifacts:** `knowledge-base/literature.md` (master bibliography);
  `deep-research.md` in each of CASE-001 and CASE-002.
- **CASE-001 upgrades:** primary loyalty→exploitation paper (Stanley et al. 2023);
  nurse mandatory-overtime→intent-to-leave evidence; motivation-crowding theory;
  Blizzard "Call to Arms" as the clean incentives-vs-coercion example; WoW-guild
  sociology (workification, emotional labor, class-balance→survival).
- **CASE-002 upgrades:** primary *Dismissed by Degrees*; Spence signaling; Collins;
  Kim & Low connected-hiring (37%); Rivera cultural matching; Baert risk-aversion.
  **Conceptual bridge:** Consalvo "gaming capital" ↔ credential/signal.
  **Self-correction:** metric is a *noisy, biased proxy*, not "meaningless."
- **Both:** honest contradicts/complicates sections added (affective vs. exploited
  commitment; gating as rational triage; structured interviews' real validity;
  degree-reset-but-mostly-rhetoric).
- **Follow-ups (non-blocking):** re-verify snippet-based quotes on open network
  before print; confirm flagged authorship (Lukacs 2010; Investment-Model 2014;
  Kim & Low; Baert); pull exact interviewing.io coefficients live.

## Case index

| ID | Slug | Observation (short) | WoW side | Corp side | Synthesis | Rating |
|----|------|--------------------|----------|-----------|-----------|--------|
| CASE-001 | healer-tank-coercion | Scarce healers/tanks met with coercion (DKP/attendance, spec+loot lock-in, self-farmed flasks, unequal inspections, leader reward-hoarding, "family/duty" rhetoric) → attrition & collapse; fixed only when Blizzard changed the system | done | done | done | STRONG |
| CASE-002 | gatekeeping-credential-spiral | Gear bar inflated above what the raid dropped → catch-22 (GearScore/RaiderIO); parallels credential inflation, entry-level catch-22, and the engineer-gauntlet vs exec-hired-over-dinner asymmetry; bar serves gatekeeper's risk-shifting, not objective need | done | done | done | STRONG |
| CASE-003 | persistence-despite-evidence | "9-yr-old" GMs strategize like C-level execs & both persist despite disconfirming evidence — because the *structural seat* (unaccountable authority + ego-investment), not age/personality, makes the cognition. Meta-case for 001/002. Escalation of commitment, CEO narcissism, hubris, power→cognition | done | done | done | STRONG* |
| CASE-004 | hollow-promises-scapegoating | Competent healer kept trapped: false "next season" promises, hoarded ("can't afford to lose a healer"), a punitive "performance improvement plan," blamed for every wipe → psychological-contract breach, talent-hoarding (+123% suppressed promotions), PIP-as-managed-out, blame culture | done | done | done | STRONG |
| CASE-005 | deniable-peer-enforcement | Vilify leavers, outsource policing to peers, keep clean hands via proxies, blacklist those who leave → concertive control (peers police harder, no mgmt fingerprints), moral disengagement/dirty work, work-devotion norm, retaliation + noncompetes. Welds shut CASE-004's exit | done | done | done | STRONG |
| CASE-006 | voice-punished-compliance-over-competence | Healers accurately name exploitation ("unpaid job") and are punished for saying it, recoded as "immature/selfish"; compliant kept over crucial-but-outspoken → challenging voice lowers perf ratings (Burris), blameless-messenger penalty (John et al.), troublemaker-label top silencer, do-gooder derogation. Closes the voice valve | done | done | done | STRONG |

`*` = STRONG with an explicit scope condition. Status legend:
`todo`/`researching`/`drafted`/`done`. Full catalog: `knowledge-base/index.md`.

## Emerging themes

Eleven themes now (see `knowledge-base/themes.md`):
1. **Compliance-vs-incentive substitution.** [CASE-001]
2. **Loyalty/duty rhetoric as extraction.** [CASE-001, CASE-005]
3. **The managerial-caste double standard.** [CASE-001, CASE-002, CASE-004]
4. **Systemic fix, not willpower.** [CASE-001, CASE-002, CASE-003]
5. **Gatekeeping-as-risk-shifting (the credential catch-22).** [CASE-002]
6. **Same seat, same brain** — structure produces leadership cognition. [CASE-003]
   ← **thesis spine.**
7. **Persistence despite disconfirming evidence (escalation of commitment).** [CASE-003]
8. **Trapping the competent** — hollow promises, talent-hoarding, PIP-as-control.
   [CASE-004]
9. **Scapegoating the burdened role (blame culture).** [CASE-004]
10. **Deniable enforcement** — outsourced peer policing + moral disengagement. [CASE-005]
11. **Controlling exit** — vilifying leavers + weaponized reputation. [CASE-005]
12. **Punished voice / compliance over competence** — shoot the messenger; grievance
    recoded as character defect. [CASE-006]

**Structure of the argument now visible:** Themes 6–7 (CASE-003) are the *engine*;
the rest are what that engine *does*. The **managerial-caste double standard** (theme 3)
and **loyalty-as-extraction** (theme 2) are the most visible connective tissue;
**"same seat, same brain"** (theme 6) is the deepest claim / thesis chapter.

**The burdened-role arc is now a COMPLETE ENCLOSURE** (the article's narrative spine):
- **coerced in** (CASE-001) → **scrutinized/gatekept as a lower caste** (CASE-002) →
- **trapped in role** (CASE-004) → **can't leave** (CASE-005) → **can't speak up**
  (CASE-006) →
- all because **the seat makes leaders behave this way** (CASE-003, the engine).

After CASE-006 the burdened worker's only options are silent compliance, a punished
exit, or a punished protest — the enclosure is closed on every side. This is a natural
place to **write the article**; the argument no longer has an obvious structural gap.

## Key decisions

- **2026-07-25** — Adopted case-centric structure: one folder per observation
  pair, each holding `observation.md` (WoW + corroboration), `corporate-parallel.md`,
  `synthesis.md`, and `sources.md`. Cases get sequential IDs `CASE-XXX`.
- **2026-07-25** — Every claim must be sourced with a real URL; unsourceable
  claims are marked `[UNVERIFIED]`. Parallels get a STRONG/PARTIAL/WEAK/DISANALOGOUS
  rating.

## Next steps

1. Author supplies observation #4 (run the full pipeline + deep-research pass).
2. Check new observations against the seven themes; add themes as needed.
3. **Article outline is now unblocked** — three STRONG cases + a clear thesis spine
   (CASE-003 as engine, CASE-001/002 as its effects). Consider drafting
   `article/outline.md` into a real structure when the author is ready, or after a
   few more cases. Recommend CASE-003 as the thesis chapter, CASE-001 as the vivid
   opener, CASE-002 as the "who's even let in" escalation.
4. Follow-ups before final article (non-blocking):
   - CASE-001: cite primary journal article behind ASU loyalty study (C1);
     re-verify W3/W4 druid quotes directly (403'd on fetch); find a cleaner source
     for the "guild collapsed overnight" mechanism.
   - CASE-002: cite the primary Fuller & Raman HBS report ("Dismissed by Degrees")
     directly instead of the AEI summary (C1).
5. Migrate the whole project into the private repo once the author creates it.

## Open questions / parking lot

- Target case count before writing the article? (TBD — quality over a fixed number.)
- Intended audience/venue for the final article? (TBD — assume general reader for now.)
- Author's guild/raid context (era, patch, tier of progression) worth recording as
  background once shared, to frame the observations.
