using NUnit.Framework;
using TournamentManager.ModelValidators;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class ValidatorTests
{
    private class SimplisticValidator : AbstractValidator<string, long, int>
    {
        public SimplisticValidator(string m, long l) : base(m, l)
        {
            Facts.Add(new Fact<int>
            {
                Id = 1,
                FieldNames = new []{ m + 1 },
                Type = FactType.Critical,
                Enabled = true,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = "Any message " + m,
                        Success = true
                    })

            });

            Facts.Add(new Fact<int>
            {
                Id = 2,
                FieldNames = new[] { m + 2 },
                Type = FactType.Error,
                Enabled = true,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = "Any message " + m,
                        Success = true
                    })

            });

            Facts.Add(new Fact<int>
            {
                Id = 3,
                FieldNames = new[] { m + 3 },
                Type = FactType.Warning,
                Enabled = false,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = "Any message " + m,
                        Success = true
                    })

            });
        }
    }

    [Test]
    public void Disabled_Fact_Is_Not_Executed()
    {
        var sv = new SimplisticValidator("model", 123456789);
        var fact = sv.Facts.First();
        fact.CheckAsync = (cancellationToken) => throw new ArithmeticException();
        Assert.Throws<ArithmeticException>(() => fact.CheckAsync(CancellationToken.None));
        fact.Enabled = false;
        Task.FromResult(sv.CheckAsync(fact.Id, CancellationToken.None));
        Assert.That(fact.Exception, Is.Null); // check is not executed
    }

    [Test]
    public void Set_Get_Fact_Properties()
    {
        var sv = new SimplisticValidator("model", 123456789);
        var fact = sv.Facts.First();
        var oldType = fact.Type;
        fact.Type = oldType == FactType.Error ? FactType.Warning : FactType.Error;
        Assert.Multiple(() =>
        {
            Assert.That(fact.Type, Is.Not.EqualTo(oldType));
            Assert.That(fact.FieldNames.Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void Fact_Check_throws_Exception()
    {
        var sv = new SimplisticValidator("model", 123456789);
        var fact = sv.Facts.First();
        fact.CheckAsync = (cancellationToken) => throw new ArgumentNullException();
        Task.FromResult(sv.CheckAsync(fact.Id, CancellationToken.None));
        Assert.That(!fact.Success && fact.IsChecked && fact.Exception is ArgumentNullException, Is.True);
    }

    [Test]
    public void Fact_Reset()
    {
        var sv = new SimplisticValidator("model", 123456789);
        var anyFact = sv.Facts.First();
        anyFact.IsChecked = anyFact.Success = true;
        anyFact.Exception = new AccessViolationException();
        anyFact.Reset();
        Assert.That(!anyFact.IsChecked && !anyFact.Success && anyFact.Exception == null, Is.True);
    }

    [Test]
    public void Validator_All_Facts_Reset()
    {
        var sv = new SimplisticValidator("model", 123456789);
        sv.Facts.ToList().ForEach(f => f.IsChecked = f.Success = true);
        Assert.That(sv.Reset().All(f => !f.IsChecked && !f.Success && f.Exception == null), Is.True);
    }
        
    [Test]
    public void Check_All_Facts_Stop_After_Critical()
    {
        var sv = CreateValidatorWithErrorForType(FactType.Critical);
            
        var result = sv.CheckAsync(CancellationToken.None).Result;
        Assert.That(result, Has.Count.EqualTo(1));

        var firstResult = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstResult.Type, Is.EqualTo(FactType.Critical));
            Assert.That(firstResult.Success, Is.EqualTo(false));
            Assert.That(result.Count != sv.Facts.Count && firstResult.Message == "error " + sv.Model &&
                          firstResult.Enabled && firstResult.Id == 1 && firstResult.IsChecked, Is.True);
        });
    }

    [Test]
    public void Check_All_Facts_Stop_After_Error()
    {
        var sv = CreateValidatorWithErrorForType(FactType.Error);

        var result = sv.CheckAsync(CancellationToken.None).Result;
        Assert.That(result, Has.Count.EqualTo(1));

        var firstResult = result[0];
        Assert.Multiple(() =>
        {
            Assert.That(firstResult.Type, Is.EqualTo(FactType.Error));
            Assert.That(firstResult.Success, Is.EqualTo(false));
            Assert.That(result.Count != sv.Facts.Count && firstResult.Message == "error " + sv.Model &&
                          firstResult.Enabled && firstResult.Id == 2 && firstResult.IsChecked, Is.True);
        });
    }

    [Test]
    public void Check_All_Facts_Only_Warning()
    {
        var sv = CreateValidatorWithErrorForType(FactType.Warning);

        var result = sv.CheckAsync(CancellationToken.None).Result;
        Assert.That(result, Has.Count.EqualTo(sv.Facts.Count));

        Assert.Multiple(() =>
        {
            Assert.That(result.Count(f => !f.Success && f.Type == FactType.Warning), Is.EqualTo(1));
            Assert.That(result.Count(f => f.Type == FactType.Critical), Is.EqualTo(1));
            Assert.That(result.Count(f => f.Type == FactType.Error), Is.EqualTo(1));

            Assert.That(sv.GetFailedFacts(), Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Check_All_Facts()
    {
        var sv = new SimplisticValidator("The model", 123456789);
        sv.Facts.Last().Enabled = true;

        var result = sv.CheckAsync(CancellationToken.None).Result;
        Assert.That(result, Has.Count.EqualTo(sv.Facts.Count));
    }

    [Test]
    public void Get_Enabled_Facts()
    {
        var sv = new SimplisticValidator("The model", 123456789);
        Assert.That(sv.GetEnabledFacts(), Has.Count.EqualTo(2));
    }

    private static SimplisticValidator CreateValidatorWithErrorForType(FactType factType)
    {
        var sv = new SimplisticValidator("The model", 123456789);
        Assert.That(sv.Facts, Has.Count.EqualTo(3));

        var fact = sv.Facts.First(f => f.Type == factType);
        fact.Enabled = true;
        fact.CheckAsync = (cancellationToken) => Task.FromResult(new FactResult
        {
            Message = "error " + sv.Model,
            Success = false
        });
        return sv;
    }
}
