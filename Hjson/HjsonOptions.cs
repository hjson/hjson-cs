using System.Collections.Generic;
using System.Linq;

namespace Hjson
{
	/// <summary>Options for Save.</summary>
	public class HjsonOptions
	{
		private IHjsonDsfProvider[] dsf;

		/// <summary>Initializes a new instance of this class.</summary>
		public HjsonOptions() => this.EmitRootBraces = true;

		/// <summary>Keep white space and comments.</summary>
		public bool KeepWsc { get; set; }

		/// <summary>Show braces at the root level (default true).</summary>
		public bool EmitRootBraces { get; set; }

		/// <summary>
		/// Gets or sets DSF providers.
		/// </summary>
		public IEnumerable<IHjsonDsfProvider> DsfProviders
		{
			get => this.dsf ?? Enumerable.Empty<IHjsonDsfProvider>();
			set => this.dsf = value.ToArray();
		}
	}
}
