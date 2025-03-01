using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace System
{
  static class DateExtensions
  {
    public static DateTime CurrentMonthEnd(this DateTime date)
    {
      var thisMonth = new DateTime(date.Year, date.Month, 1);
      var nextMonth = thisMonth.AddMonths(1);
      return nextMonth.AddDays(-1);
    }

    public static DateTime NextMonthEnd(this DateTime date)
    {
      var thisMonth = new DateTime(date.Year, date.Month, 1);
      var nextMonth = thisMonth.AddMonths(2);
      return nextMonth.AddDays(-1);
    }
  }
}
