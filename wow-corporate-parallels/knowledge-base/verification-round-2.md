# Deep-Research Round 2 — Primary-Source Verification of the Article's Citations

_Date: 2026-07-26. Triggered by the Draft-2 fact-check pass on
`article/drafts/the-healer-always-quits-first.md`._

## Why this exists

Round 1 research (2026-07-25) gathered most citations from **search snippets and
abstracts**, because publisher pages (HBR, IBM, SAGE, *Science*, ScienceDirect, ACM)
repeatedly returned `403`. Every case file and `framing-guild-as-lab.md` carried a
"re-verify wording before quoting" caveat. This round takes the **load-bearing claims
that actually appear in the article** and checks each against an authoritative source
(publisher page, author's own PDF, or a first-party press release). The point is not to
re-list the bibliography — it is to find the claims that were **wrong, over-stated, or
unsupported**, and fix them.

## Verdict table

| # | Claim as used in the article | Source | Verdict |
|---|------------------------------|--------|---------|
| 1 | In-game leadership carried into **volunteer** orgs but had **no** link to leadership rank in **companies** (~18k players) | Lu, Shen & Williams (2014), *Computers in Human Behavior* 35:54–60 | ✅ **Verified verbatim.** "The linkage between players' relational activities in MMOGs and their offline leadership was found in voluntary organizations, but not in companies." |
| 2 | Loyal workers selectively targeted for unpaid extra work; ~1,400 managers; fictional "John" | Stanley, Neck & Neck (2023), *J. Experimental Social Psychology* 105:104442 | ✅ **Verified.** Title is "Loyal **workers**…"; ~1,400 managers; loyalty framing raised willingness to assign unpaid labor. |
| 3 | Mandatory overtime, specifically, tied to nurses' intent-to-leave; authors urged a ban | Bae (2024), *International Journal of Public Health* (SSPH+) 1607068 | ✅ **Verified.** "Only mandatory overtime was significantly related to intent to leave"; "labor policy should ban mandatory overtime." |
| 4 | 67% of production-supervisor postings demanded a degree vs 16% of incumbents holding one | Fuller & Raman (2017), *Dismissed by Degrees* (HBS / Accenture / Grads of Life) | ✅ **Verified.** 2015 data; a "degree gap" of 51 points. |
| 5 | 37% of externally-appointed CEOs personally connected to the hiring board | Kim & Low (2026), "Who Hires Whom? Connected Hiring in the CEO Labor Market," *J. Empirical Finance* 87 | ⚠️ **Half-verified — framing corrected.** 37% is real and "significantly higher than expected after adjusting for network sizes." BUT the paper finds connected CEOs **enhance firm value** and are concentrated at "informationally opaque firms / less established CEOs," **not** "where the record was weakest." The draft's cronyism spin was **not supported** and has been cut. See note below. |
| 6 | Talent hoarding: promotion applications rose ~123% when managers' ability to hoard was reduced | Hägele (2022), "Talent Hoarding in Organizations," arXiv 2206.15098 | ✅ **Verified.** Manager rotation → +123% in promotion applications. |
| 7 | PIP mentions on Glassdoor "rose eightfold" | WorkLife.news (2024), "Are you being 'quietly cut' with a PIP?" citing Glassdoor (CASE-004 source C10) | ⚠️ **Thinly sourced — cut.** Not fabricated: it traces to one secondary-journalism source citing Glassdoor. But the underlying Glassdoor figure is not independently retrievable and no primary dataset was found, so it's a single-secondary-source claim — fair to cut from a piece otherwise built on primary research. |
| 8 | Concertive control: self-managing teams police each other harder than managers | Barker (1993), "Tightening the Iron Cage," *Administrative Science Quarterly* 38:408–437 | ✅ Canonical; accurately used. |
| 9 | Noncompetes bind ~18% of US workers, ~38% ever signed, binding even where unenforceable | Starr, Prescott & Bishara (2021), *J. Law & Economics* 64:53–84 (2014 survey, 11k+ workers) | ✅ **Verified.** 18% currently bound; 38% ever-signed; effect present where unenforceable. |
| 10 | Challenging voice lowers performance ratings vs supportive voice; mediated by loyalty/threat | Burris (2012), "The Risks and Rewards of Speaking Up," *Academy of Management Journal* 55(4):851–875 | ✅ **Verified verbatim,** including the loyalty/threat mediation. |
| 11 | "Shooting the Messenger" — eleven experiments; blameless messengers still penalized | John, Blunden & Liu (2019), *J. Experimental Psychology: General* 148(4) | ✅ **Verified.** "The eleven experiments provide evidence…" |
| 12 | Escalation of commitment strongest for decisions you are personally responsible for; meta-analysis of ~140 studies | Sleesman, Conlon, McNamara & Miles (2012), "Cleaning Up the Big Muddy," *AMJ* 55:541–562 | ✅ **Verified.** (k = 141; "~140" is fair. Personal responsibility a key determinant.) |
| 13 | Narcissism predicts leadership **emergence** but not **effectiveness** | Grijalva et al. (2015), *Personnel Psychology* 68:1–47 | ✅ **Verified verbatim.** (Effectiveness is curvilinear; the linear "≈ no relationship" claim is accurate.) |
| 14 | MMO players average ~20 hrs/week; grind described as a paid second job | Yee, Daedalus Project (N≈2,000 for the hours item; ~40k overall) | ✅ **Verified.** ~21 hrs/week (22.3 F / 19.0 M). |

## The two substantive corrections

### A. The CEO connected-hiring study (Kim & Low) does not say what the draft implied
The article used the 37% figure to illustrate **cronyism** — "connected hires were most
common exactly where the candidate's record was weakest," i.e. the network protects weak
insiders. The actual paper argues the **opposite valence**: connected hiring *reduces
bilateral information asymmetry*, connected CEOs *enhance firm value* relative to
unconnected ones, take *lower* initial pay, and match *at least as well* (similar
separation rates). The defensible claim is narrower and is what the draft now says:
**a third of the most powerful hires walk in through a personal relationship the
rank-and-file candidate will never be offered** — an *access-asymmetry* point, not a
*competence* point. Do **not** cite this study as evidence that connected executives are
worse; it says they aren't. (This is a genuine "the source complicates the story"
finding, logged per the project's honesty rule.)

### B. The PIP "eightfold" stat is too thin to carry — here is a sturdier replacement
The "eightfold" figure is **not fabricated** — it traces to WorkLife.news (2024) citing
Glassdoor (CASE-004 source C10). But it is a **single secondary source** and the
underlying Glassdoor dataset is not independently retrievable, which is below the bar for
a piece that otherwise cites primary research — so it stays cut. If the author wants a
*number* rather than the current qualitative claim, these are sturdier and citable:
- **HR Acuity** benchmark: ~43.6 of every 1,000 US workers were subject to formal
  performance procedures in 2023, ~30% above 2020. (Vendor benchmark — label as such.)
- **Amazon** placed thousands of employees per month into the initial PIP phase in the
  months before cutting ~27,000 roles (Nov 2022–Mar 2023) — a concrete, reported example
  of PIP-as-managed-exit. (Business press.)
The article currently makes the qualitative claim only (workers read a PIP as a
pre-written termination), which is well-supported and needs no number.

## Net effect on the article (Draft 2, committed)
- **Restored** the precise transfer-study wording in two places (was over-softened in the
  first Draft-2 pass) now that Lu/Shen/Williams (2014) is verified.
- **Kept cut:** the CEO "weakest record" spin (#5) and the PIP "eightfold" stat (#7).
- Everything else in the article's citation set is confirmed accurate as written.

## Follow-ups (non-blocking)
- Prefer author-PDF or publisher DOIs over snippets when the article gets formatted for
  publication; the DOIs above are the canonical anchors.
- Psychological-contract-breach "meta-analysis of dozens of studies" (used in Move 3) is
  Zhao et al. (2007), *Personnel Psychology* 60:647–680 — verify directly if a specific
  effect size is ever quoted (the article keeps it qualitative, which is safe).
- Asch "three-quarters … at least once" is canonical (Asch 1951/1956); fine as phrased.
