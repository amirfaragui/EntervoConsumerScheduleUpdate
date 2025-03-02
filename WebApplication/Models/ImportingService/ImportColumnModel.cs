using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Entrvo.DAL;

namespace T2WebApplication.Models
{
  public class ImportColumnModel
  {
    public string Text { get; set; }
    public string Column { get; set; }
    public string Format { get; set; }
    public Type? DataType { get; set; }
  }

}
