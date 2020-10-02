using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotographersVideoDist.Hubs
{
	public class FtpProgressHub : Hub
	{
		public async Task AssociateJob(string jobId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
		}
	}
}
