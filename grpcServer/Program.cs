using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using M.Resolver;
using wpar = Google.Protobuf.WellKnownTypes;
namespace grpcServer
{
	class Program
	{
		private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
		private const int Port = 8000;
		static Server server;

		public static void Main(string[] _)
		{
			server = new Server
			{
				Services = { resolver.BindService(new ResolverImpl()) },
				Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
			};

			server.Start();
			Console.WriteLine("Server listening on port " + Port);
			Console.WriteLine("Press Ctlr + C to stop the server...");

			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
			_closing.WaitOne();
		}

		protected static void OnExit(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine("Server is stopping");

			server.ShutdownAsync().Wait();

			Console.WriteLine("Server stopped");
			Console.WriteLine();

			_closing.Set();
		}
	}
	class ResolverImpl : resolver.resolverBase
	{
		private string[] myIps;

		private string hostName;

		public string HostName
		{
			get
			{
				if (string.IsNullOrEmpty(hostName))
					hostName = Dns.GetHostName();
				return hostName;
			}
		}
		public string[] MyIps
		{
			get
			{
				if (myIps is null)
					myIps = Dns.GetHostEntry(HostName).AddressList
					   .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
					   .Select(x => x.ToString())
					   .ToArray();
				return myIps;
			}
		}

		public override Task<IpsResponse> FirstMethod(wpar.StringValue request, ServerCallContext context)
		{
			IpsResponse t = new IpsResponse() { HostName = HostName };
			t.Ips.AddRange(MyIps);
			return Task.FromResult(t);
		}
	}
}
