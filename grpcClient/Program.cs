using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace grpcClient
{
	class Program
	{
		private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
		private const string _address = "grpc-server";
		//private const string _address = "localhost";
		private const int DeadlineMs = 1000;


		private static Channel _channel;
		private static Channel Channel
		{
			get
			{
				if (_channel is null)
					_channel = new Channel(_address, 8000, ChannelCredentials.Insecure);

				for (int i = 0; i < 3 && _channel.State != ChannelState.Ready; i++)
				{
					try
					{
						_channel.ConnectAsync(DateTime.UtcNow.AddMilliseconds(DeadlineMs)).Wait();
					}
					catch
					{
						_channel = new Channel(_address, 8000, ChannelCredentials.Insecure);
					}
				}
				return _channel;
			}
		}

		public static M.Resolver.resolver.resolverClient _client;
		public static M.Resolver.resolver.resolverClient Client {
			get
			{
				if (_client is null || _channel.State != ChannelState.Ready)
					_client = new M.Resolver.resolver.resolverClient(Channel);
				return _client;
			}
		}

		public static void Main(string[] _)
		{
			Task.Run(() =>
			{
				Stopwatch swGetChannel = new Stopwatch();
				Stopwatch swGetResponse = new Stopwatch();
				Google.Protobuf.WellKnownTypes.StringValue request = new Google.Protobuf.WellKnownTypes.StringValue() { Value = "Mike" };
				while (true)
				{
					try
					{
						swGetChannel.Restart();
						M.Resolver.resolver.resolverClient client = Client;
						swGetChannel.Stop();

						swGetResponse.Restart();
						M.Resolver.IpsResponse reply = client.FirstMethod(request, deadline: DateTime.UtcNow.AddMilliseconds(DeadlineMs));
						swGetResponse.Stop();

						Console.WriteLine($"Took [{swGetChannel.ElapsedMilliseconds.ToString("{0:000}")}" +
							$" / {swGetResponse.ElapsedMilliseconds.ToString("{0:000}")}]" +
							$" - {reply.HostName}: [{string.Join(", ", reply.Ips)}]");

						System.Threading.Thread.Sleep(1000);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Loop ex: {ex.Message}");
					}
				}
			});

			Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
			_closing.WaitOne();
		}

		protected static void OnExit(object sender, ConsoleCancelEventArgs args)
		{
			Console.WriteLine("client is stopping");
			_closing.Set();
		}
	}
}
