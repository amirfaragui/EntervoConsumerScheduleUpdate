using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Entrvo
{
  using System.Text;

  public static class EnumHelper
  {
    public static IEnumerable<SelectListItem> GetValues<T>() where T : Enum
    {
      var realModelType = typeof(T);
      var values = Enum.GetValues(realModelType).Cast<Enum>();

      TypeConverter converter = TypeDescriptor.GetConverter(realModelType);

      return values.Select(value => new SelectListItem
      {
        Text = converter.ConvertToString(value).BreakByWord(),
        Value = Convert.ToInt32(value).ToString(),
      }).ToList();
    }

    public static IEnumerable<SelectListItem> GetValues<T>(Func<T, bool> predict) where T : Enum
    {
      var realModelType = typeof(T);
      var values = Enum.GetValues(realModelType).Cast<Enum>().Where(i =>
      {
        return predict((T)Enum.Parse(typeof(T), Convert.ToInt32(i).ToString()));
      });

      TypeConverter converter = TypeDescriptor.GetConverter(realModelType);

      return values.Select(value => new SelectListItem
      {
        Text = converter.ConvertToString(value).BreakByWord(),
        Value = Convert.ToInt32(value).ToString(),
      }).ToList();
    }

    public static IEnumerable<SelectListItem> GetNames<T>() where T : Enum
    {
      var realModelType = typeof(T);
      var values = Enum.GetValues(realModelType).Cast<Enum>();

      TypeConverter converter = TypeDescriptor.GetConverter(realModelType);

      return values.Select(value => new SelectListItem
      {
        Text = converter.ConvertToString(value).BreakByWord(),
        Value = value.ToString(),
      }).ToList();
    }

    public static IEnumerable<SelectListItem> GetNames<T>(Func<T, bool> predict) where T : Enum
    {
      var realModelType = typeof(T);
      var values = Enum.GetValues(realModelType).Cast<Enum>().Where(i =>
      {
        return predict((T)Enum.Parse(typeof(T), Convert.ToInt32(i).ToString()));
      });

      TypeConverter converter = TypeDescriptor.GetConverter(realModelType);

      return values.Select(value => new SelectListItem
      {
        Text = converter.ConvertToString(value).BreakByWord(),
        Value = value.ToString(),
      }).ToList();
    }
  }
}

namespace System.Text
{
  public static class StringEntension
  {
    static readonly Regex regex = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

    public static string BreakByWord(this string source)
    {
      return regex.Replace(source, " ");
    }
  }
}
