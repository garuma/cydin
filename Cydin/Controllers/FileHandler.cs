using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.IO;
using Cydin.Builder;
using Cydin.Models;

namespace Cydin.Controllers
{
	public class FileHandler : IRouteHandler, IHttpHandler
	{
		string devStatus;
		
		public FileHandler (string devStatus)
		{
			this.devStatus = devStatus;
		}
		
		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return this;
		}

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest (HttpContext context)
		{
			string path = context.Request.Path;
			int i = path.IndexOf ('/');
			if (i != -1)
				i = path.IndexOf ('/', i + 1);
			if (i == -1) {
				WriteNotFound (context);
				return;
			}

			path = path.Substring (i + 1);
			if (path.Length == 0) {
				WriteNotFound (context);
				return;
			}
			
			string basePath = Path.GetFullPath (BuildService.AddinsPath);

			path = path.Replace ('/', Path.DirectorySeparatorChar);
			string subPath = path;
			path = Path.GetFullPath (Path.Combine (BuildService.AddinsPath, Path.Combine (devStatus, path)));
			if (!path.StartsWith (basePath)) {
				WriteNotFound (context);
				return;
			}

			if (Directory.Exists (path))
				path = Path.Combine (path, "index.html");
			
			if (!File.Exists (path)) {
				WriteNotFound (context);
				return;
			}
			
			string ext = Path.GetExtension (path);
			
			if (ext == ".mpack")
				context.Response.AddHeader ("Content-Type", "application/x-mpack");
			else if (ext.EndsWith ("-mpack"))
				context.Response.AddHeader ("Content-Type", "application/x-" + ext.Substring (1));
			
			context.Response.TransmitFile (path);
			
			if (path.EndsWith (".mpack")) {
				string fileId = subPath;
				fileId = fileId.Replace (Path.DirectorySeparatorChar, '/');
				fileId = fileId.Substring (0, fileId.Length - 6).Trim ('/');
				using (UserModel m = UserModel.GetCurrent ()) {
					m.IncDownloadCount (fileId);
				}
			}
		}

		void WriteNotFound (HttpContext context)
		{
			context.Response.StatusCode = 404;
		}
	}
}