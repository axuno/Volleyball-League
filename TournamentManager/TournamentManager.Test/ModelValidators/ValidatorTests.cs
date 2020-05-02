using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.ModelValidators;

namespace TournamentManager.Tests.ModelValidators
{
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
            Assert.IsNull(fact.Exception); // check is not executed
        }

        [Test]
        public void Set_Get_Fact_Properties()
        {
            var sv = new SimplisticValidator("model", 123456789);
            var fact = sv.Facts.First();
            var oldType = fact.Type;
            fact.Type = oldType == FactType.Error ? FactType.Warning : FactType.Error;
            Assert.AreNotEqual(oldType, fact.Type);
            Assert.IsTrue(fact.FieldNames.Count() == 1);
        }

        [Test]
        public void Fact_Check_throws_Exception()
        {
            var sv = new SimplisticValidator("model", 123456789);
            var fact = sv.Facts.First();
            fact.CheckAsync = (cancellationToken) => throw new ArgumentNullException();
            Task.FromResult(sv.CheckAsync(fact.Id, CancellationToken.None));
            Assert.IsTrue(!fact.Success && fact.IsChecked && fact.Exception is ArgumentNullException);
        }

        [Test]
        public void Fact_Reset()
        {
            var sv = new SimplisticValidator("model", 123456789);
            var anyFact = sv.Facts.First();
            anyFact.IsChecked = anyFact.Success = true;
            anyFact.Exception = new AccessViolationException();
            anyFact.Reset();
            Assert.IsTrue(!anyFact.IsChecked && !anyFact.Success && anyFact.Exception == null);
        }

        [Test]
        public void Validator_All_Facts_Reset()
        {
            var sv = new SimplisticValidator("model", 123456789);
            sv.Facts.ToList().ForEach(f => f.IsChecked = f.Success = true);
            Assert.IsTrue(sv.Reset().All(f => !f.IsChecked && !f.Success && f.Exception == null));
        }
        
        [Test]
        public void Check_All_Facts_Stop_After_Critical()
        {
            var sv = CreateValidatorWithErrorForType(FactType.Critical);
            
            var result = sv.CheckAsync(CancellationToken.None).Result;
            Assert.AreEqual(1, result.Count);

            var firstResult = result[0];
            Assert.AreEqual(FactType.Critical, firstResult.Type);
            Assert.IsTrue(firstResult.Success == false);
            Assert.IsTrue(result.Count != sv.Facts.Count && firstResult.Message == "error " + sv.Model && 
                          firstResult.Enabled && firstResult.Id == 1 && firstResult.IsChecked);
        }

        [Test]
        public void Check_All_Facts_Stop_After_Error()
        {
            var sv = CreateValidatorWithErrorForType(FactType.Error);

            var result = sv.CheckAsync(CancellationToken.None).Result;
            Assert.AreEqual(1, result.Count);

            var firstResult = result[0];
            Assert.AreEqual(FactType.Error, firstResult.Type);
            Assert.IsTrue(firstResult.Success == false);
            Assert.IsTrue(result.Count != sv.Facts.Count && firstResult.Message == "error " + sv.Model &&
                          firstResult.Enabled && firstResult.Id == 2 && firstResult.IsChecked);
        }

        [Test]
        public void Check_All_Facts_Only_Warning()
        {
            var sv = CreateValidatorWithErrorForType(FactType.Warning);

            var result = sv.CheckAsync(CancellationToken.None).Result;
            Assert.AreEqual(sv.Facts.Count, result.Count);

            Assert.AreEqual(1, result.Count(f => !f.Success && f.Type == FactType.Warning));
            Assert.AreEqual(1, result.Count(f => f.Type == FactType.Critical));
            Assert.AreEqual(1, result.Count(f => f.Type == FactType.Error));

            Assert.AreEqual(1, sv.GetFailedFacts().Count);
        }

        [Test]
        public void Check_All_Facts()
        {
            var sv = new SimplisticValidator("The model", 123456789);
            sv.Facts.Last().Enabled = true;

            var result = sv.CheckAsync(CancellationToken.None).Result;
            Assert.AreEqual(sv.Facts.Count, result.Count);
        }

        [Test]
        public void Get_Enabled_Facts()
        {
            var sv = new SimplisticValidator("The model", 123456789);
            Assert.AreEqual(2, sv.GetEnabledFacts().Count);
        }

        private SimplisticValidator CreateValidatorWithErrorForType(FactType factType)
        {
            var sv = new SimplisticValidator("The model", 123456789);
            Assert.AreEqual(3, sv.Facts.Count);

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
}
