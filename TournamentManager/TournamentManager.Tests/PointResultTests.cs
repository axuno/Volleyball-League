using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace TournamentManager.Tests;

[TestFixture]
public class PointResultTests
{
    [Test]
    public void Create_Result_From_String_Containing_Only_Separator()
    {
        var pointResult = new PointResult("-", '-');
        new PointResult(null, null) { PointsSeparator = '-' }.Should().BeEquivalentTo(pointResult);
    }

    [TestCase(12, 25)]
    [TestCase(null, null)]
    public void Create_Result_From_Nullable_Int(int? home, int? guest)
    {
        var pointResult = new PointResult(home, guest);

        Assert.That(pointResult.Home == home);
        Assert.That(pointResult.Guest == guest);
        Assert.That(pointResult.ToString(), Is.EqualTo($"{(home != null ? home.Value : "-")}:{(guest != null ? guest.Value : "-")}"));
    }

    [Test]
    public void Add_Integer_Results()
    {
        var pointResult1 = new PointResult("1:25");
        var pointResult2 = new PointResult("25:1");

        var result = pointResult1 + pointResult2;

        Assert.That(result is { Home: 26, Guest: 26 });
    }

    [Test]
    public void Add_Null_Results()
    {
        var pointResult1 = new PointResult("1:25");
        var pointResult2 = new PointResult(null, null);

        var result = pointResult1 + pointResult2;

        Assert.That(result is { Home: 1, Guest: 25 });
    }

    [Test]
    public void Subtract_Integer_Results()
    {
        var pointResult1 = new PointResult("25:23");
        var pointResult2 = new PointResult("23:21");

        var result = pointResult1 - pointResult2;

        Assert.That(result is { Home: 2, Guest: 2 });
    }

    [Test]
    public void Compare_Results_For_Equality()
    {
        var pointResult1 = new PointResult("25:18");
        var pointResult2 = new PointResult("25:18");

        Assert.That(pointResult1 == pointResult2, Is.True);
        Assert.That(pointResult1 != pointResult2, Is.False);
    }

    [TestCase("25:0", "25:1", true)]
    [TestCase("0:25", "25:0", false)]
    [TestCase("0:25", "1:25", false)]
    [TestCase("4:25", "1:25", true)]
    [TestCase("25:4", "25:1", false)]
    public void Compare_Results_A_more_than_B(string a, string b, bool expected)
    {
        var pointResult1 = new PointResult(a);
        var pointResult2 = new PointResult(b);

        Assert.That(pointResult1 > pointResult2, Is.EqualTo(expected));
        Assert.That(pointResult1 < pointResult2, Is.EqualTo(!expected));
    }

    [Test]
    public void Compare_Results_Null_Results()
    {
        var pointResult = new PointResult(25, 1);

        Assert.That(pointResult.CompareTo(null), Is.EqualTo(1));
        Assert.That(pointResult.Compare(null, pointResult), Is.EqualTo(-1));
        Assert.That(pointResult.Compare(null, null), Is.EqualTo(0));
    }
}
