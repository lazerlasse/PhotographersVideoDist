using FluentFTP;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Data
{
	public class MediaFilesFTPClient
	{
		public static async Task UploadFiles(List<string> filesToUpload, string userName)
		{
			var token = new CancellationToken();

			using (var ftp = new FtpClient("127.0.0.1", userName, "ftptest"))
			{
				await ftp.ConnectAsync(token);

				// upload many files, skip if they already exist on server
				await ftp.UploadFilesAsync(filesToUpload, "/", FtpRemoteExists.Skip);
			}
		}

		public static async Task DeleteFiles(List<string> filesToDelete)
		{

		}

		public static async Task<List<IFormFile>> DownloadFiles()
		{
			return new List<IFormFile>();
		}

		public static async Task<List<string>> ListFilesOnServer()
		{
			return new List<string>();
		} 
	}
}
