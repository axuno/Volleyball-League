using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class MatchRuleEntityExtensionTests
{
    [TestCase(false, 3, 3)]
    [TestCase(true, 3, 5)]
    [TestCase(true, 2, 3)]
    public void Calculate_Max_Number_Of_Sets(bool isBestOf, int numOfSets, int expected)
    {
        var rule = new MatchRuleEntity { BestOf = isBestOf, NumOfSets = numOfSets };
        Assert.AreEqual(expected, rule.MaxNumOfSets());
    }
}
