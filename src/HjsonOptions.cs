using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Hjson
{
  /// <summary>Options for Save.</summary>
  public class HjsonOptions
  {
    /// <summary>Keep white space and comments.</summary>
    public bool KeepWsc { get; set; }

    /// <summary>Show braces at the root level.</summary>
    public bool EmitRootBraces { get; set; }
  }
}
