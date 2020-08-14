using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class ImageAssets
	{
		[Key, Display(Name = "Billede ID")]
		public int ImageAssetsID { get; set; }

		[Display(Name = "Filnavn"), Required]
		public string ImageFileName { get; set; }

		[Display(Name = "Udvalgt billede")]
		public bool ImageIsPrimary { get; set; } = false;

		
		// Navigation Properties.
		[ForeignKey("Case")]
		public int CaseID { get; set; }		// FK for CaseID.
		public Case Case { get; set; }		// Navigation property for Case.
	}
}
