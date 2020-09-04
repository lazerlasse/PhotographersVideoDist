using FluentFTP;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Controllers;
using PhotographersVideoDist.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public class MediaFilesFTPClient
	{
		private IDataProtector DataProtector { get; set; }
		private ILogger<CasesController> Logger { get; set; }
		private string Username { get; set; }
		private string EncryptedPassword { get; set; }
		private string FtpUrl { get; set; }
		private string FtpRemoteDir { get; set; }

		public MediaFilesFTPClient(IDataProtectionProvider dataProtectionProvider, ILogger<CasesController> logger, string username, string encryptedPassword, string ftpUrl, string ftpRemoteDir)
		{
			DataProtector = dataProtectionProvider.CreateProtector("FTPAccountModel");
			Username = username;
			EncryptedPassword = encryptedPassword;
			FtpUrl = ftpUrl;
			FtpRemoteDir = ftpRemoteDir;
			Logger = logger;
		}

		public async Task UploadFiles(List<string> filesToUpload)
		{
			// Generete default cancellation token.
			var token = new CancellationToken();

			// Create new ftp client.
			using var ftp = new FtpClient(FtpUrl, Username, DataProtector.Unprotect(EncryptedPassword));

			try
			{
				// Connect to server async.
				await ftp.ConnectAsync(token);

				Progress<FtpProgress> progress = new Progress<FtpProgress>(p =>
			   {
				   if (p.Progress == 1)
				   {
					   Logger.LogInformation("Filerne blev uploadet til FTP Serveren.");
				   }
				   else
				   {
					   Logger.LogInformation("Sender filer til FTP: " + p.Progress + "%");
				   }
			   });



				// upload many files, skip if they already exist on server
				await ftp.UploadFilesAsync(filesToUpload, FtpRemoteDir, FtpRemoteExists.Append, false, FtpVerify.None, FtpError.None, token, progress);


				Logger.LogInformation("Filerne blev sendt til FTP serveren med succes: ");
			}
			catch (Exception ex)
			{
				Logger.LogError("Der skete en uventet fejl i forsøget på at uploade filerne til FTP serveren: " + ex.Message);
			}
		}
	}
}
