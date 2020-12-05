using System;
using System.Threading.Tasks;

namespace Axuno.Scripting
{
    public class Processor
    {
        /// <summary>
        /// Uses Roslyn C# compiler scripting to create a <see cref="Predicate{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The code to define a <see cref="Predicate{T}"/>.</param>
        /// <example>
        /// <![CDATA[
        /// // Inspired by: https://www.strathweb.com/2018/01/easy-way-to-create-a-c-lambda-expression-from-a-string-with-roslyn/
        /// var hf = "h => (h.PublicHolidayStateIds.Count == 0 || h.PublicHolidayStateIds.Contains(Axuno.Tools.GermanFederalStates.Id.Bayern)) && (h.Type == Axuno.Tools.GermanHolidays.Type.Public || h.Type == Axuno.Tools.GermanHolidays.Type.Custom || h.Type == Axuno.Tools.GermanHolidays.Type.School)";
        /// var holidayFilterExpression = await Axuno.Scripting.Processor.CreatePredicateAsync<Axuno.Tools.GermanHoliday>(hf);
        /// ]]>
        /// </example>
        /// <returns>Returns the <see cref="Predicate{T}" /> created from the code <paramref name="expression"/> string.</returns>
        public static async Task<Predicate<T>> CreatePredicateAsync<T>(string expression) where T: class
        {
            var options = Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default.AddReferences(typeof(T).Assembly);
            return await Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<Predicate<T>>(expression, options);
        }
    }
}
