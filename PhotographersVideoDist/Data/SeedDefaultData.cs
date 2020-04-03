using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public static class SeedDefaultData
	{
		public static void SeedData(string connectionString)
		{
			var dataFile = "/tmp/CSV/Postnummer-Danmark-BULK-CSV.csv";
				//Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SeedData", "Postnummer-Danmark-BULK-CSV.csv");
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(string.Format("BULK INSERT {0}", "Postals"));
			sb.AppendFormat(string.Format(" FROM '{0}'", dataFile));
			sb.AppendFormat(string.Format(" WITH ( FORMAT = 'CSV', FIELDTERMINATOR = ';', ROWTERMINATOR = '0x0a' );"));
			string sqlQuery = sb.ToString();
			using SqlConnection sqlConn = new SqlConnection(connectionString);
			sqlConn.Open();
			using SqlCommand sqlCmd = new SqlCommand(sqlQuery, sqlConn);

			try
			{
				sqlCmd.ExecuteNonQuery();
				sqlConn.Close();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
