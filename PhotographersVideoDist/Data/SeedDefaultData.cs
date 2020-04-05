using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
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
		public static void SeedData(string connectionString, ApplicationDbContext context)
		{
			if (context.Postals.Any())
			{
				return;
			}

			var dataFile = "/var/lib/mysql-files/Postnummer-Danmark-BULK-CSV.csv";
				//Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SeedData", "Postnummer-Danmark-BULK-CSV.csv");
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(string.Format("LOAD DATA INFILE '{0}'", dataFile));
			sb.AppendFormat(string.Format(" INTO TABLE {0}", "Postals"));
			sb.AppendFormat(string.Format(" FIELDS TERMINATED BY ';'"));
			sb.AppendFormat(string.Format(" LINES TERMINATED BY '\n'"));
			sb.AppendFormat(string.Format(" IGNORE 1 LINES;"));
			string sqlQuery = sb.ToString();
			using MySqlConnection sqlConn = new MySqlConnection(connectionString);
			sqlConn.Open();
			using MySqlCommand sqlCmd = new MySqlCommand(sqlQuery, sqlConn);

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
