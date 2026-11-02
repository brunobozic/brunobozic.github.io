using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    /// <summary>The swarm's ECS-style storage: struct-of-arrays, pooled slots, generation-checked handles.</summary>
    public sealed class SwarmStoreTests
    {
        [Test]
        public void Spawn_PacksAliensDensely()
        {
            var swarm = new SwarmStore(capacity: 8);
            var a = swarm.Spawn(AlienKind.Crawler, new RoomId(3), health: 40);
            var b = swarm.Spawn(AlienKind.Leaper, new RoomId(5), health: 60);

            Assert.That(swarm.Count, Is.EqualTo(2));
            Assert.That(swarm.IsAlive(a) && swarm.IsAlive(b), Is.True);
            Assert.That(swarm.RoomOf(b), Is.EqualTo(new RoomId(5)));
            Assert.That(swarm.KindOf(a), Is.EqualTo(AlienKind.Crawler));
            Assert.That(swarm.HealthOf(b), Is.EqualTo(60));
        }

        [Test]
        public void Despawn_ReusesTheSlot_AndStaleHandlesDie()
        {
            var swarm = new SwarmStore(capacity: 4);
            var first = swarm.Spawn(AlienKind.Crawler, new RoomId(0), 40);
            Assert.That(swarm.Despawn(first), Is.True);
            Assert.That(swarm.Despawn(first), Is.False, "already gone");

            var second = swarm.Spawn(AlienKind.Husk, new RoomId(1), 60);
            Assert.That(second.Slot, Is.EqualTo(first.Slot), "the slot is pooled");
            Assert.That(second.Generation, Is.Not.EqualTo(first.Generation));
            Assert.That(swarm.IsAlive(first), Is.False, "an old handle can't reach the new alien");
            Assert.That(swarm.IsAlive(second), Is.True);
        }

        [Test]
        public void Despawn_KeepsTheDenseArrayPacked()
        {
            var swarm = new SwarmStore(capacity: 4);
            var a = swarm.Spawn(AlienKind.Crawler, new RoomId(0), 40);
            var b = swarm.Spawn(AlienKind.Crawler, new RoomId(1), 40);
            var c = swarm.Spawn(AlienKind.Crawler, new RoomId(2), 40);

            swarm.Despawn(a);
            Assert.That(swarm.Count, Is.EqualTo(2));
            Assert.That(swarm.RoomOf(c), Is.EqualTo(new RoomId(2)), "moved, but its handle still finds it");
            Assert.That(swarm.RoomOf(b), Is.EqualTo(new RoomId(1)));

            var rooms = new System.Collections.Generic.List<int>();
            for (var i = 0; i < swarm.Count; i++) rooms.Add(swarm.DenseRoom(i).Index);
            Assert.That(rooms, Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void Capacity_GrowsWhenNeeded()
        {
            var swarm = new SwarmStore(capacity: 2);
            for (var i = 0; i < 100; i++) swarm.Spawn(AlienKind.Crawler, new RoomId(i % 4), 40);
            Assert.That(swarm.Count, Is.EqualTo(100));
            Assert.That(swarm.Capacity, Is.GreaterThanOrEqualTo(100));
        }

        [Test]
        public void Damage_KillsAtZero()
        {
            var swarm = new SwarmStore(capacity: 2);
            var a = swarm.Spawn(AlienKind.Crawler, new RoomId(0), 40);
            Assert.That(swarm.Damage(a, 15), Is.False);
            Assert.That(swarm.HealthOf(a), Is.EqualTo(25));
            Assert.That(swarm.Damage(a, 30), Is.True, "lethal");
            Assert.That(swarm.IsAlive(a), Is.False);
        }

        [Test]
        public void CountIn_CountsAliensPerRoom()
        {
            var swarm = new SwarmStore(capacity: 8);
            swarm.Spawn(AlienKind.Crawler, new RoomId(1), 40);
            swarm.Spawn(AlienKind.Crawler, new RoomId(1), 40);
            swarm.Spawn(AlienKind.Spitter, new RoomId(2), 40);
            Assert.That(swarm.CountIn(new RoomId(1)), Is.EqualTo(2));
            Assert.That(swarm.CountIn(new RoomId(3)), Is.Zero);
        }
    }
}
