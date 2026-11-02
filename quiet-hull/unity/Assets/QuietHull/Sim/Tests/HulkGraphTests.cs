using System;
using NUnit.Framework;

namespace QuietHull.Sim.Tests
{
    public sealed class HulkGraphTests
    {
        private static HulkGraph Chain() =>
            new HulkGraphBuilder()
                .Room("bay", "Shuttle bay")
                .Room("corridor", "Corridor")
                .Room("crusher", "Crusher bay", ambientNoise: 6)
                .Door("bay", "corridor")
                .Door("corridor", "crusher")
                .Build();

        [Test]
        public void Rooms_AreFoundById()
        {
            var g = Chain();
            Assert.That(g.RoomCount, Is.EqualTo(3));
            Assert.That(g.Room(g.IdOf("crusher")).AmbientNoise, Is.EqualTo(6));
            Assert.That(g.Room(g.IdOf("bay")).Name, Is.EqualTo("Shuttle bay"));
        }

        [Test]
        public void Doors_ConnectBothWays()
        {
            var g = Chain();
            var corridor = g.IdOf("corridor");
            Assert.That(g.Neighbours(corridor), Is.EquivalentTo(new[] { g.IdOf("bay"), g.IdOf("crusher") }));
            Assert.That(g.Neighbours(g.IdOf("bay")), Is.EqualTo(new[] { corridor }));
        }

        [Test]
        public void Builder_RejectsDuplicateRooms_AndDoorsToNowhere()
        {
            Assert.Throws<ArgumentException>(() => new HulkGraphBuilder().Room("a", "A").Room("a", "A again").Build());
            Assert.Throws<ArgumentException>(() => new HulkGraphBuilder().Room("a", "A").Door("a", "ghost").Build());
            Assert.Throws<ArgumentException>(() => new HulkGraphBuilder().Room("a", "A").Door("a", "a").Build());
        }

        [Test]
        public void UnknownRoom_Throws()
        {
            Assert.Throws<ArgumentException>(() => Chain().IdOf("nowhere"));
        }
    }
}
