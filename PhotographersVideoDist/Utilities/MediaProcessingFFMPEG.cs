using Microsoft.Extensions.Logging;
using PhotographersVideoDist.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Utilities
{
	public class MediaProcessingFFMPEG
	{
		private static ILogger _logger { get; set; }

		public MediaProcessingFFMPEG(ILogger logger)
		{
			_logger = logger;
		}

		public List<string> GenerateStillsFromVideo(string videoFileName, int caseID)
		{
			// Generate path for assets folder.
			var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Cases", caseID.ToString());

			// Generate full path for input file.
			var inputFile = Path.Combine(assetsPath, videoFileName);

			// Get filename without extension.
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);


			// Check if running OS are Linux and run process.
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				// Generate the command line args to be executed.
				string linuxCommandToExecute = "ffmpeg -ss 00:00:03 -i " + inputFile + " -vf fps=1/15 -f image2 -vframes 8 " + Path.Combine(assetsPath, fileNameWithoutExtension + "-%03d.jpeg");

				// Run the process on Linux.
				ProcessLinux(linuxCommandToExecute);
			}


			// Check if running OS are Windows and run process.
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// Generate the command line args to be executed.
				string winCommandToExecute = "-ss 00:00:03 -i " + inputFile + " -vf fps=1/15 -f image2 -vframes 8 " + Path.Combine(assetsPath, fileNameWithoutExtension + "-%03d.jpeg");

				// Run the process on Windows.
				ProcessWindows(winCommandToExecute);
			}


			// Get list of generated stills.
			var generatedStills = AssetsFileHandler.SearchForAssetsfilesInAssetsFolder(assetsPath, fileNameWithoutExtension, "jpeg");

			// Return the list with names of the generated stills.
			return generatedStills;
		}

		private static void ProcessWindows(string command)
		{
			// Create new instance of the system process and run the command line in ffmpeg.exe (Windows Only)
			Process winProcess = new Process()
			{
				// Create startinformation for the process to run...
				StartInfo = new ProcessStartInfo()
				{
					CreateNoWindow = false,
					FileName = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg-win64-2019", "bin", "ffmpeg.exe"), // Path to the ffmpeg
					WindowStyle = ProcessWindowStyle.Normal,
					Arguments = command
				}
			};

			// Try run the commandline process or throw an exeption...
			try
			{
				winProcess.Start();
				winProcess.WaitForExit(300000); // wait for exit 10000 miliseconds
			}
			catch (Exception ex)
			{
				_logger.LogError("Der opstod en fejl i forsøget på at generere stills via Windows Process: " + ex.Message);
			}
		}

		private static void ProcessLinux(string command)
		{
			// Create new Linux process to execute the FFMPEG command.
			Process linuxProcess = new Process()
			{
				// Create startinformation for the process to run...
				StartInfo = new ProcessStartInfo()
				{
					CreateNoWindow = false,
					FileName = "/bin/bash",
					WindowStyle = ProcessWindowStyle.Normal,
					Arguments = command
				}
			};

			// Try run the commandline process or throw an exeption...
			try
			{
				linuxProcess.Start();
				linuxProcess.WaitForExit(300000); // wait for exit 10000 miliseconds
			}
			catch (Exception ex)
			{
				_logger.LogError("Der opstod en fejl i forsøget på generere stills via Linux Process: " + ex.Message);
			}
		}
	}
}
