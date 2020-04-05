using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class Postal
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Key, MaxLength(4), Display(Name = "Postnummer"), DataType(DataType.PostalCode)]
		public string PostalCode { get; set; }
		
		[Display(Name = "By")]
		public string Town { get; set; }
	}
}
