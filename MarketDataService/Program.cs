using Common;
using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Threading;

namespace MarketDataService
{
    class Program
    {
        public class Service : ServiceBase
        {
            public Service(string serviceName)
            {
                this.ServiceName = serviceName;
            }

            protected override void OnStart(string[] args)
            {
                Program.Start(args);
            }

            protected override void OnStop()
            {
                Program.Stop(this.ServiceName);
            }
        }

        static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                using (var service = new Service("TestService"))
                    ServiceBase.Run(service);
            else
            {
                Start(args);
            }
        }

        private static void Start(string[] args)
        {
            Console.WriteLine("Service started.");

            try
            {
                if (tcpLsn == null)
                {
                    IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, 9100);
                    tcpLsn = new TcpListener(endpoint);
                    tcpLsn.Start();
                    Thread tcpThd = new Thread(new ThreadStart(WaitingForClient));
                    threadHolder.TryAdd(connectId, tcpThd);
                    tcpThd.Start();
                }
                while (true)
                {
                    LoadThread();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Stop(string serviceName)
        {
            Console.WriteLine("Service stoped.");
            tcpLsn.Stop();
            tcpLsn = null;

            Thread tcpThd = (Thread)threadHolder[0];
            tcpThd.Abort();
            threadHolder.TryRemove(0, out Thread thread);
            foreach (TcpClient s in clientHolder.Values)
            {
                if (s.Connected)
                    s.Close();
            }
            foreach (Thread t in threadHolder.Values)
            {
                if (t.IsAlive)
                    t.Abort();
            }
        }

        private static long connectId = 0;
        private static TcpListener tcpLsn;
        static ConcurrentDictionary<long, TcpClient> clientHolder = new ConcurrentDictionary<long, TcpClient>();
        static ConcurrentDictionary<long, Thread> threadHolder = new ConcurrentDictionary<long, Thread>();
        private static int MaxConnected = 400;

        public static void WaitingForClient()
        {
            while (true)
            {
                TcpClient tcpClient = tcpLsn.AcceptTcpClient();
                Console.WriteLine($"Connection request received : {connectId}");
                if (connectId < 10000)
                    Interlocked.Increment(ref connectId);
                else
                    connectId = 1;
                if (clientHolder.Count < MaxConnected)
                {
                    while (clientHolder.ContainsKey(connectId))
                    {
                        Interlocked.Increment(ref connectId);
                    }
                    clientHolder.TryAdd(connectId, tcpClient);
                }
            }
        }

        private static void CloseTheThread(long realId)
        {
            if (clientHolder.ContainsKey(realId))
            {
                Console.WriteLine($"Removing client : {realId}");
                clientHolder.TryRemove(realId, out TcpClient value);
            }
            if (threadHolder.ContainsKey(realId))
            {
                Thread thd = null;
                threadHolder.TryRemove(realId, out thd);
                if (thd != null)
                    thd.Abort();
            }
        }

        public static void LoadThread()
        {
            StockPriceDto stockPrice = null;
            using (StreamReader sr = File.OpenText("SampleData.txt"))
            {
                while (true)
                {
                    if (clientHolder.Count == 0)
                        continue;
                    if (sr.EndOfStream)
                    {
                        sr.BaseStream.Position = 0;
                        sr.DiscardBufferedData();
                    }
                    var line = sr.ReadLine();
                    var data = line.Split(':');
                    if (data.Length == 0)
                        continue;
                    stockPrice = new StockPriceDto(data[0].Trim(), decimal.Parse(data[1].Trim()), DateTime.Now);
                    if (stockPrice == null)
                        break;
                    SendDataToAllClient(stockPrice);
                }
            }
        }

        private static void SendDataToAllClient(StockPriceDto data)
        {
            foreach (var tcpClientkeyVal in clientHolder)
            {
                try
                {
                    if (tcpClientkeyVal.Value.Connected)
                    {
                        var stream = tcpClientkeyVal.Value.GetStream();
                        Serializer.SerializeWithLengthPrefix(stream, data, PrefixStyle.Base128, 1);
                    }
                }
                catch (Exception ex)
                {
                    CloseTheThread(tcpClientkeyVal.Key);
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}