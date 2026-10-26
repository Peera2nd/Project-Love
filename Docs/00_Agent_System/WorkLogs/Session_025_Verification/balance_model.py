"""Paper model for a proposed seven-day sprint. Not Unity gameplay or balance approval."""
from dataclasses import dataclass, field
import json
from pathlib import Path

NAMES = ["Code", "Art", "Design"]
TARGETS = [80, 72, 64]
CARDS = [[28, 28, 24], [24, 24, 24], [24, 20, 20]]

@dataclass
class Worker:
    stress: int
    cards: list
    index: int = 0
    progress: int = 0
    effort: int = 0
    accepted: int = 0

    def remaining(self):
        return self.cards[self.index] - self.progress if self.index < len(self.cards) else 0

    def add(self, amount):
        actual = min(amount, self.remaining())
        self.progress += actual
        self.effort += actual
        return actual

    def accept(self):
        if self.remaining() == 0 and self.index < len(self.cards):
            self.accepted += self.cards[self.index]
            self.index += 1
            self.progress = 0

    def rate(self):
        return 4 if self.stress < 40 else 3 if self.stress < 70 else 2 if self.stress < 90 else 1


def simulate(name, actions=None, start_stress=30, bug=0, absent_days=(), producer_block=False):
    actions = actions or {}
    workers = [Worker(start_stress, list(cards)) for cards in CARDS]
    energy = 100
    plan_ticks = 0
    blocked = producer_block
    counts = {"help": 0, "plan": 0, "clarify": 0, "care": 0, "rest": 0, "talks": 0}
    unused = 0
    minimum_energy = energy
    ledger = []
    rejected = []
    for day in range(1, 8):
        if day > 1:
            energy = min(100, energy + 40)
        # First daily talk for each present character. No work tick and no stress effect.
        present = [not (i == 0 and day in absent_days) for i in range(3)]
        talk_cost = sum(present) * 3
        if energy < talk_cost:
            rejected.append(f"Day {day}: daily talk energy")
        else:
            energy -= talk_cost
            counts["talks"] += sum(present)
        if day == 4 and bug:
            workers[0].cards.append(bug)
        for phase in range(4):
            action, target = actions.get((day, phase), ("free", None))
            productive = phase < 3
            eligible = [productive and present[i] and w.remaining() > 0 and not (i == 0 and blocked) for i, w in enumerate(workers)]
            skip_worker = None
            cost = {"help": 20, "plan": 15, "clarify": 15, "care": 10, "rest": 0, "free": 0}[action]
            valid = energy >= cost
            if action in ("help", "care"):
                valid = valid and productive and present[target] and (action == "care" or eligible[target])
            if action == "clarify":
                valid = valid and blocked
            if action == "plan":
                valid = valid and plan_ticks == 0
            if not valid:
                rejected.append(f"Day {day} phase {phase}: {action} {target}")
                action = "free"
                cost = 0
            energy -= cost
            helped = 0
            if action == "help":
                w = workers[target]
                helped = w.add(6)
                w.stress = max(0, w.stress - 3)
            elif action == "plan":
                plan_ticks = 2
            elif action == "clarify":
                blocked = False
                eligible[0] = productive and present[0] and workers[0].remaining() > 0
            elif action == "care":
                workers[target].stress = max(0, workers[target].stress - 15)
                skip_worker = target
            elif action == "rest":
                energy = min(100, energy + 30)
            if action != "free":
                counts[action] += 1
            else:
                unused += 1
            outputs = []
            for i, w in enumerate(workers):
                actual = w.add(w.rate() + (1 if plan_ticks and productive else 0)) if eligible[i] and i != skip_worker else 0
                if actual:
                    w.stress = min(100, w.stress + 2)
                outputs.append(actual)
            if productive and plan_ticks:
                plan_ticks -= 1
            # Free batch acceptance at each boundary, no same-tick jump to the next card.
            for w in workers:
                w.accept()
            minimum_energy = min(minimum_energy, energy)
            ledger.append(dict(day=day, phase=phase, action=action, target=target, help=helped, baseline=outputs, energy=energy,
                stress=[w.stress for w in workers], effort=[w.effort for w in workers]))
        for w in workers:
            w.stress = max(0, w.stress - 4)
    goals = [80 + bug, 72, 64]
    return dict(name=name, goals=dict(zip(NAMES, goals)), effort=dict(zip(NAMES, [w.effort for w in workers])),
        accepted=dict(zip(NAMES, [w.accepted for w in workers])), remaining=dict(zip(NAMES, [sum(w.cards)-w.effort for w in workers])),
        stress_end=dict(zip(NAMES, [w.stress for w in workers])), passed=all(w.accepted >= g for w,g in zip(workers,goals)),
        actions=counts, unspent_phase_slots=unused, minimum_energy=minimum_energy, rejected=rejected, ledger=ledger)


def cases():
    mixed = {(d,0): ("help", [0,1,2,0,1][d-1]) for d in range(1,6)}
    bug = dict(mixed)
    bug[7,0] = ("help",0)
    for d in (4,5,6): bug[d,1] = ("help",0)
    missed = {k:v for k,v in mixed.items() if k[0] not in (2,3)}
    missed[6,0] = ("help",2)
    high = {(d,i): ("care",i) for d in (1,2) for i in range(3)}
    for d,i in [(3,0),(4,1),(5,2),(6,0),(7,1)]: high[d,0] = ("help",i)
    high[6,1] = ("help",0); high[7,1] = ("help",0)
    recovery = {(d,i): ("care",i) for d in (1,2,3) for i in range(3)}
    for d in (4,5,6,7): recovery[d,0] = ("help",0)
    recovery[4,1] = ("help",1); recovery[5,1] = ("help",1)
    recovery[4,2] = ("help",2); recovery[6,1] = ("help",1)
    absence = {(d,p): ("help",0) for d,p in [(1,0),(1,2),(2,0),(2,2),(3,0),(7,0),(7,2)]}
    absence[2,1] = ("help",1); absence[3,1] = ("help",2)
    planned = {(d,0): ("plan",None) for d in (1,3,5,7)}
    block = dict(mixed); del block[1,0]; block[2,1] = ("clarify",None)
    block[7,0] = ("help",0); block[7,1] = ("help",0)
    return [simulate("No player work"), simulate("Late focused help",{(7,2):("help",0)}),
        simulate("Mixed daily help",mixed), simulate("Code bug +24 with response",bug,bug=24),
        simulate("Two missed help days",missed), simulate("High stress 75: partial care",high,start_stress=75),
        simulate("High stress 75: deliberate recovery",recovery,start_stress=75),
        simulate("Code worker absent Days 4-6",absence,absent_days=(4,5,6)),
        simulate("Four coordination actions only",planned), simulate("Producer unblock plus focused help",block,producer_block=True)]


if __name__ == "__main__":
    results = cases()
    for r in results:
        assert not r["rejected"], (r["name"], r["rejected"])
        assert len(r["ledger"]) == 28
        assert len({(v["day"], v["phase"]) for v in r["ledger"]}) == 28
        assert r["minimum_energy"] >= 0
        for tick in r["ledger"]:
            assert 0 <= tick["energy"] <= 100
            assert all(0 <= value <= 100 for value in tick["stress"])
            if tick["phase"] == 3: assert tick["baseline"] == [0,0,0]
        for name in NAMES:
            assert r["accepted"][name] <= r["effort"][name] <= r["goals"][name]
    Path(__file__).with_name("results.json").write_text(json.dumps(results,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    for r in results:
        print(r["name"], "effort=",r["effort"],"accepted=",r["accepted"],"pass=",r["passed"],"actions=",r["actions"],"free=",r["unspent_phase_slots"],"minEnergy=",r["minimum_energy"],"rejected=",r["rejected"])
