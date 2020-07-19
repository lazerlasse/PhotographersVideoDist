using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class VideoAssets
	{
		[Key]
		public int VideoAssetsID { get; set; }

		[Display(Name = "Filnavn")]
		public string VideoAssetsFileName { get; set; }

		[Display(Name = "Sti")]
		public string VideoAssetsFilePath { get; set; }


		// Navigation Properties...
		public int CaseID { get; set; }			// ForiengKey for Case.
		public Case Case { get; set; }			// Navigation property for Case.
	}
}
