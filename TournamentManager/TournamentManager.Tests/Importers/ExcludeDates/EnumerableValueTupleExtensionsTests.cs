using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers.ExcludeDates;

[TestFixture]
internal class EnumerableValueTupleExtensionsTests
{
    [Test]
    public void IntegerRanges_ShouldBeConsecutive()
    {
        // Unsorted list of integers with missing numbers
        var intList = new List<int>
        {
            // group #3: number 7 missing
            10,
            9,
            8,
            // group #1
            2,
            3,
            4,
            // group #2: number 5 missing
            6
        };

        var ranges = intList.ConsecutiveRanges().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(ranges, Has.Count.EqualTo(3));
            Assert.That(ranges[0], Is.EqualTo((2, 4)));
            Assert.That(ranges[1], Is.EqualTo((6, 6)));
            Assert.That(ranges[2], Is.EqualTo((8, 10)));
        });
    }

    [Test]
    public void DateOnlyRanges_ShouldBeConsecutive()
    {
        // Unsorted list of DateTime with missing dates
        var dateOnlyList = new List<DateOnly>
        {
            // group #3: number 7 Oct missing
            new (2024, 10, 10),
            new (2024, 10, 9),
            new (2024, 10, 8),
            // group #1
            new (2024, 10, 2),
            new (2024, 10, 3),
            new (2024, 10, 4),
            // group #2: number 5 Oct missing
            new (2024, 10, 6)
        };

        var ranges = dateOnlyList.ConsecutiveRanges().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(ranges, Has.Count.EqualTo(3));
            Assert.That(ranges[0], Is.EqualTo((new DateOnly(2024, 10, 2), new DateOnly(2024, 10, 4))));
            Assert.That(ranges[1], Is.EqualTo((new DateOnly(2024, 10, 6), new DateOnly(2024, 10, 6))));
            Assert.That(ranges[2], Is.EqualTo((new DateOnly(2024, 10, 8), new DateOnly(2024, 10, 10))));
        });
    }
}


