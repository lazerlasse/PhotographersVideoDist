using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class Postal
	{
		[Key, MaxLength(4), Display(Name = "Postnummer"), DataType(DataType.PostalCode)]
		public int PostalCode { get; set; }
		
		[Display(Name = "By")]
		public string Town { get; set; }
	}
}
