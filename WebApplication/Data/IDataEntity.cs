using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using T2WebApplication.Models;

namespace T2Importer.DAL
{ 
  public interface IDataEntity
  {
    public int GetId();
  }

  static class CustomerMappingExtensions
  {
    public static bool TrySetValue<T>(this T customer, PropertyInfo property, ImportColumnModel column, out object value) where T : class, IDataEntity
    {
      value = default;
      var type = property.PropertyType;

      var underlyingType = Nullable.GetUnderlyingType(type);
      if (underlyingType != null)
      {
        if (string.IsNullOrWhiteSpace(column.Text))
        {
          property.SetValue(customer, null);
          return true;
        }
        type = underlyingType;
      }

      if (type.IsEnum)
      {
        if (!Enum.TryParse(type, column.Text, true, out var enumValue)) return false;
        property.SetValue(customer, enumValue);
        return true;
      }

      if (type == typeof(bool) && long.TryParse(column.Text, out var lValue))
      {
        var bValue = lValue == 1;
        property.SetValue(customer, bValue);
        return true;
      }

      if (type == typeof(Decimal) && Decimal.TryParse(column.Text, out var dValue))
      {
        property.SetValue(customer, dValue);
        return true;
      }


      if (type.IsPrimitive)
      {
        var converter = TypeDescriptor.GetConverter(type);
        try
        {
          var primitiveValue = converter.ConvertFromString(column.Text);
          property.SetValue(customer, primitiveValue);
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }

      if (type == typeof(DateTimeOffset))
      {
        if (!string.IsNullOrEmpty(column.Format))
        {
          if (DateTimeOffset.TryParseExact(column.Text, column.Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateValue))
          {
            property.SetValue(customer, dateValue);
            return true;
          }
        }
        else
        {
          if (DateTimeOffset.TryParse(column.Text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateValue))
          {
            property.SetValue(customer, dateValue);
            return true;
          }
        }
        return false;
      }

      if (type == typeof(DateTime))
      {
        if (!string.IsNullOrEmpty(column.Format))
        {
          if (DateTime.TryParseExact(column.Text, column.Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateValue))
          {
            property.SetValue(customer, dateValue);
            return true;
          }
        }
        else
        {
          if (DateTime.TryParse(column.Text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dateValue))
          {
            property.SetValue(customer, dateValue);
            return true;
          }
        }
        return false;
      }

      if (type == typeof(TimeSpan))
      {
        if (TimeSpan.TryParse(column.Text, CultureInfo.InvariantCulture, out var timeValue))
        {
          property.SetValue(customer, timeValue);
          return true;
        }
        return false;
      }

      if (type == typeof(string))
      {
        property.SetValue(customer, column.Text);
        return true;
      }

      return false;

    }
  }

}
