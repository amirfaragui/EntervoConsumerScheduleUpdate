using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entrvo.DAL.Attributes
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
  public class AuditIgnoreAttribute: Attribute
  {
  }
}
