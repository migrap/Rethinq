using Remotion.Linq.Parsing.Structure;
using Rethinq;
using Rethinq.Data.RqlClient;
using Rethinq.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Automatonymous;
using System.Reactive.Linq;
using System.Net.Sockets;
using Migrap.Net.Sockets;
using Migrap.Linq;
using System.Threading;
using System.Reflection;
using System.Collections;
using Migrap;
using System.Collections.Concurrent;

namespace Sandbox {
    class Program {
        static void Main(string[] args) {

            var token = new CancellationTokenSource().Token;
            var endPoint = new DnsEndPoint("rethink.migrap.dev", 8080);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var result = ConnectAsync(socket, endPoint).Result;

            //var socket = new SocketClient(_ => _
            //    .AddressFamily(AddressFamily.InterNetwork)
            //    .SocketType(SocketType.Stream)
            //    .ProtocolType(ProtocolType.Tcp)
            //    .EndPoint(endPoint)
            //);
            //socket.Connecting += (s, e) => {
            //    Console.WriteLine("Connecting: " + e.RemoteEndPoint);
            //};
            //socket.Connected += (s, e) => {                
            //    Console.WriteLine("Connected: " + e.RemoteEndPoint);
            //    (s as SocketClient).Receive();
            //};
            //socket.Disconnected += (s, e) => {
            //    Console.WriteLine("Disconnected: " + e.RemoteEndPoint);
            //    (s as SocketClient).Connect();
            //};
            //socket.ConnectAsync().Wait();

            //while (true) {
            //    Console.ReadLine();
            //    socket.IsConnected();
            //}




            //var addresses = Dns.GetHostAddresses("rethink.migrap.dev");
            //var endPoints = addresses.Select(x => new IPEndPoint(x, 28015)).ToArray();
            //var index = 0;
            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //Task.Factory.StartNew(() => ConnectAsync(socket, endPoints[index]));


            Console.ReadLine();
        }

        private static async Task<SocketError> ConnectAsync(Socket socket, EndPoint endPoint) {
            //var awaitable = new Dawn.Net.Sockets.SocketAwaitable();
            //awaitable.RemoteEndPoint = endPoint;
            //var result = await Dawn.Net.Sockets.SocketEx.ConnectAsync(socket, awaitable);

            var awaitable = new SocketAwaitable();
            awaitable.RemoteEndPoint = endPoint;

            return await socket.ConnectAsync(awaitable);
        }
    }

    public class Connection {

    }

    public static partial class Extensions {
        public static EndPoint Courier(this Connection connection) {
            return new DnsEndPoint("10.0.1.75", 28015);
        }

        public static EndPoint Pugna(this Connection connection) {
            return new DnsEndPoint("10.0.1.80", 28015);
        } 
    }

    class Customers {
        public string Name { get; set; }
        public string City { get; set; }
    }

    class People {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SegmentManager {
        const int LogMin = 5;   // Anything smaller than 1 << log_cut goes into the first bucket
        //private List<byte[]>[] _buffers = new List<byte[]>[32 - LogMin];
        private BlockingBufferManager[] _buffers = new BlockingBufferManager[32 - LogMin];
        static int Index(uint n, int min = 5) {
            var pos = 0;
            if (n >= 1 << 16) {
                n >>= 16;
                pos += 16;
            }
            if (n >= 1 << 8) {
                n >>= 8;
                pos += 8;
            }
            if (n >= 1 << 4) {
                n >>= 4;
                pos += 4;
            }
            if (n >= 1 << 2) {
                n >>= 2;
                pos += 2;
            }
            if (n >= 1 << 1) {
                pos += 1;
            }

            var index = ((n == 0) ? (-1) : pos);
            if (index > min) {
                index -= min;
            }
            return index;
        }

        private int _maxBufferSize;

        public SegmentManager(int maxBufferSize = 8 * 1024 * 1024) {
            _maxBufferSize = maxBufferSize.NearestPower2();
        }

        public ArraySegment<byte> Acquire(int bufferSize) {
            return GetOrCreate(bufferSize).Acquire();
        }

        public void Release(ArraySegment<byte> value) {
            GetOrCreate(value.Count).Release(value);
        }

        private BlockingBufferManager GetOrCreate(int bufferSize) {
            bufferSize = bufferSize.NearestPower2();
            var index = Index((uint)bufferSize);

            var manager = _buffers[index];
            if (null == manager) {
                manager = _buffers[index] = new BlockingBufferManager(bufferSize, _maxBufferSize / bufferSize);
            }

            return manager;
        }
    }

    public class BufferManager {
        const int LogMin = 5;   // Anything smaller than 1 << log_cut goes into the first bucket
        private int _maxBufferSize;
        private int _hits;
        private List<byte[]>[] _buffers = new List<byte[]>[32 - LogMin];

        static int log2(uint n) {
            var pos = 0;
            if (n >= 1 << 16) {
                n >>= 16;
                pos += 16;
            }
            if (n >= 1 << 8) {
                n >>= 8;
                pos += 8;
            }
            if (n >= 1 << 4) {
                n >>= 4;
                pos += 4;
            }
            if (n >= 1 << 2) {
                n >>= 2;
                pos += 2;
            }
            if (n >= 1 << 1) {
                pos += 1;
            }

            return ((n == 0) ? (-1) : pos);
        }

        public BufferManager(int maxBufferSize) {
            _maxBufferSize = maxBufferSize;
        }

        public void Clear() {
            foreach (var buffer in _buffers) {
                if (null == buffer) {
                    continue;
                }
                buffer.Clear();
            }
            Array.Clear(_buffers, 0, _buffers.Length);
        }

        public byte[] Acquire(int bufferSize) {
            if (bufferSize < 0 || (_maxBufferSize >= 0 && bufferSize > _maxBufferSize)) {
                throw new ArgumentOutOfRangeException();
            }

            int l2 = log2((uint)bufferSize);
            if (l2 > LogMin) {
                l2 -= LogMin;
            }

            List<byte[]> returned = _buffers[l2];
            if (returned == null || returned.Count == 0) {
                return new byte[bufferSize];
            }

            foreach (var e in returned) {
                if (e.Length >= bufferSize) {
                    _hits++;
                    returned.Remove(e);
                    return e;
                }
            }
            return new byte[bufferSize];
        }

        public void Release(byte[] buffer) {
            if (null == buffer) {
                return;
            }

            uint size = (uint)buffer.Length;
            int l2 = log2(size);

            if (l2 > LogMin) {
                l2 -= LogMin;
            }

            var returned = _buffers[l2];
            if (returned == null) {
                returned = _buffers[l2] = new List<byte[]>();
            }

            returned.Add(buffer);
        }
    }
}