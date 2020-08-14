using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class VideoAssets
	{
		[Key, Display(Name = "Video ID")]
		public int VideoAssetsID { get; set; }

		[Display(Name = "Filnavn"), Required]
		public string VideoAssetsFileName { get; set; }


		// Navigation Properties...
		[ForeignKey("Case"), Display(Name = "Sags nr.")]
		public int CaseID { get; set; }			// ForiengKey for Case.
		public Case Case { get; set; }			// Navigation property for Case.
	}
}
