using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Models
{
	public class Case
	{
		[Key]
		public int CaseID { get; set; }

		[Display(Name = "Udgivet")]
		public bool IsPublished { get; set; } = false;

		[Display(Name = "Overskrift")]
		public string Titel { get; set; }

		[Display(Name = "Detaljer"), MaxLength(200)]
		public string Details { get; set; }

		[Display(Name = "Kommentar/Bemærkninger")]
		public string Comments { get; set; }
		
		[Display(Name = "Vej")]
		public string Street { get; set; }


		// Navigation Properties for Postalcode and Town.
		[ForeignKey("Postal"), MaxLength(4)]
		public string PostalCode { get; set; }		// FK For Postal.
		public Postal Postal { get; set; }      // Navigation Property For Postal.


		[DataType(DataType.DateTime), Display(Name = "Oprettet")]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:MM}", ApplyFormatInEditMode = true)]
		public DateTime Created { get; set; } = DateTime.Now;


		[DataType(DataType.DateTime), Display(Name = "Udgivet")]
		[DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:MM}", ApplyFormatInEditMode = true)]
		public DateTime? Published { get; set; }
		

		// Navigation Properties for IdentityUser (Photographer).
		[Display(Name = "Fotograf")]
		public string PhotographerID { get; set; }					// FK for IdentityUser Photographer.
		public virtual ApplicationUser Photographer { get; set; }      // Navigation property for IdentityUser.


		// Navigation Properties for ImageAssets.
		[Display(Name = "Billeder")]
		public ICollection<ImageAssets> ImageAssets { get; set; }	// Collection of ImageAssets - one to many.


		// Navigation Properties for VideoAssets.
		[Display(Name = "Videoer")]
		public ICollection<VideoAssets> VideoAssets { get; set; }	// Collection of VideoAssets - one to many.
	}
}
