using FluentFTP;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Controllers;
using PhotographersVideoDist.Hubs;
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
		private IHubContext<FtpProgressHub> HubContext { get; set; }

		public MediaFilesFTPClient(
			IDataProtectionProvider dataProtectionProvider,
			ILogger<CasesController> logger,
			string username,
			string encryptedPassword,
			string ftpUrl,
			string ftpRemoteDir,
			IHubContext<FtpProgressHub> hubContext)
		{
			DataProtector = dataProtectionProvider.CreateProtector("FTPAccountModel");
			Username = username;
			EncryptedPassword = encryptedPassword;
			FtpUrl = ftpUrl;
			FtpRemoteDir = ftpRemoteDir;
			Logger = logger;
			HubContext = hubContext;
		}

		public async Task UploadFiles(List<string> filesToUpload, string jobId)
		{
			// Generete default cancellation token.
			var token = new CancellationToken();

			// Create new ftp client.
			using var ftp = new FtpClient(FtpUrl, Username, DataProtector.Unprotect(EncryptedPassword));

			// Create the progress notifier for the upload.
			Progress<FtpProgress> progress = new Progress<FtpProgress>(async p =>
			{
				if (p.Progress == 1)
				{
					await HubContext.Clients.Group(jobId).SendAsync("progress", p.Progress, "Færdig", p.TransferSpeed, 0);
					Logger.LogInformation("Filerne blev uploadet til FTP Serveren.");
				}
				else
				{
					await HubContext.Clients.Group(jobId).SendAsync("progress", p.Progress, "Sender fil: " + p.FileIndex + " / " + p.FileCount + " til FTP serveren.", p.TransferSpeedToString(), p.TransferredBytes);
					Logger.LogInformation("Sender fil " + p.FileIndex + " / " + p.FileCount + " til FTP: " + p.Progress + "%");
				}
			});

			ftp.RetryAttempts = 3;
			int succesCount = 0;

			try
			{
				// Connect to server async.
				await ftp.ConnectAsync(token);

				// upload many files, skip if they already exist on server.
				succesCount = await ftp.UploadFilesAsync(filesToUpload, FtpRemoteDir, FtpRemoteExists.Append, false, FtpVerify.Retry, FtpError.Throw, token, progress);
			}
			catch (FtpException ex)
			{
				await HubContext.Clients.Group(jobId).SendAsync("progress", 0, "Fejlet: " + ex.Message, 0, 0);
				Logger.LogError("Der skete en uventet fejl i forsøget på at uploade filerne til FTP serveren: " + ex.Message);
			}

			await HubContext.Clients.Group(jobId).SendAsync("progress", 0, succesCount + " / " + filesToUpload.Count + " Blev uploadet til serveren.", 0, 0);
		}
	}
}
