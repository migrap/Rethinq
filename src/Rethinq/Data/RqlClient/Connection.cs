using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rethinq.Data.RqlClient.Versions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Automatonymous;

namespace Rethinq.Data.RqlClient {
    public class Connection {
        private Subject<Segment> _segments = new Subject<Segment>();
        private CancellationTokenSource _connectCancellationTokenSource = new CancellationTokenSource();
        private static Func<byte[], int, int, string> GetString = (b, i, c) => Encoding.UTF8.GetString(b, i, c);
        private Socket _socket;
        private StateMachine _machine = new StateMachine();
        private Context _context = new Context();

        public Connection() {
            _machine.TransitionToState(_context, x => x.Initial);

            _machine.StateChanged.Subscribe(x => {
                var buffer = new byte[_context.Length];
                Buffer.BlockCopy(_context.Buffer, _context.Offset, buffer, 0, buffer.Length);
                buffer = buffer.ToLittleEndian();

                if(x.Current == _machine.Initial) {
                    _context.Length = 0;
                    _context.Offset = 0;
                    _context.Token = 0;
                }
                else if (x.Current == _machine.Token) {
                    _context.Token = BitConverter.ToUInt64(buffer, 0);
                }
                else if (x.Current == _machine.Length) {
                    _context.Length = BitConverter.ToInt32(buffer, 0);
                }
                else if (x.Current == _machine.Response) {
                    var json = GetString(buffer, 0, buffer.Length);
                }
            });

            _machine.StateChanged.Where(x => x.Current.Name == "Response").Subscribe(x => {
                _machine.TransitionToState(_context, sm => sm.Initial);
            });
        }

        public Connection(params EndPoint[] endPoints)
            : this() {
            EndPoints = endPoints;
        }

        public IEnumerable<EndPoint> EndPoints { get; set; }

        private async Task ConnectAsync() {
            await ConnectAsync(_connectCancellationTokenSource.Token);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken) {
            var endPoints = EndPoints.SelectMany<EndPoint, IPEndPoint>(x => {
                if (x is IPEndPoint) {
                    return Enumerable.Repeat(x as IPEndPoint, 1);
                }
                else if (x is DnsEndPoint) {
                    var port = (x as DnsEndPoint).Port;
                    var addresses = Dns.GetHostAddresses((x as DnsEndPoint).Host);
                    return addresses.Select(address => new IPEndPoint(address, port));
                }
                return Enumerable.Empty<IPEndPoint>();
            });


            foreach (var endPoint in endPoints) {
                try {
                    await ConnectAsync(endPoint, cancellationToken);
                }
                catch { }
            }
        }

        public async Task ConnectAsync(params Func<Connection, Func<EndPoint>>[] endPoints) {
            EndPoints = endPoints.Select(x => x(this)()).ToArray();
            await ConnectAsync();
        }

        protected virtual async Task ConnectAsync(IPEndPoint endPoint, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested) {
                throw new TaskCanceledException();
            }

            var buffer = new byte[8096];
            var socket = (Socket)null;
            var completed = default(EventHandler<SocketAsyncEventArgs>);
            var receive = (SocketAsyncEventArgs)null;

            try {
                var abort = new Action(() => {
                    if (null != receive) {
                        receive.Completed -= completed;
                        receive.Dispose();
                        receive = null;
                    }
                    if (null == socket) {
                        socket.Close();
                        socket.Dispose();
                        socket = null;
                    }
                });

                using (cancellationToken.Register(abort)) {
                    socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    await socket.ConnectAsync(endPoint);

                    receive = new SocketAsyncEventArgs {
                        RemoteEndPoint = endPoint,
                    };

                    receive.SetBuffer(buffer, 0, buffer.Length);

                    receive.Completed += completed = (s, e) => {
                        Completed(e);
                        socket.ReceiveAsync(e);
                    };

                    socket.Handshake(handshake: h => h(version: v3));

                    var transfered = socket.Receive(buffer, SocketFlags.None);

                    ProcessReceive(buffer, 0, transfered, (segment) => {
                        var response = GetString(segment.Buffer, segment.Offset, segment.Count);
                        if(false == response.Equals("SUCCESS", StringComparison.InvariantCultureIgnoreCase)) {
                            throw new Exception("Unexpected authentication response; expected SUCCESS but got: " + response);
                        }
                        socket.ReceiveAsync(receive);
                    });

                    _socket = socket;
                }
            }
            catch { }
        }

        public async Task SendAsync(byte[] buffer, int offset, int length) {
            _socket.SendAsync(_ => _
                .Buffer(buffer)
                .Offset(offset)
                .Length(length)
            );
        }

        protected virtual void Completed(SocketAsyncEventArgs e) {
            switch (e.LastOperation) {
                case SocketAsyncOperation.Connect:
                    //ProcessConnect(e);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    //ProcessSend(e);
                    break;
                case SocketAsyncOperation.Disconnect:
                    //ProcessDisconnect(e);
                    break;
                default:
                    //OnError(new Exception("Invalid operation completed"));
                    break;
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e) {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0) {
                ProcessReceive(e.Buffer, e.Offset, e.BytesTransferred);
            }
            else if (e.SocketError == SocketError.ConnectionReset) {
                ConnectAsync();
            }
            else {
                //OnError(new SocketException((int)e.SocketError));
            }
        }

        private void ProcessReceive(byte[] buffer, int offset, int transferred) {
            const int Token = 8;
            const int Length = 4;           
            _context.Buffer = buffer;
            _context.Offset = offset;


            while (transferred > 0) {
                if (_context.State == _machine.Initial && transferred >= Token) {
                    _context.Length = Token;
                    _machine.RaiseEvent(_context, x => x.Receive);
                    transferred -= Token;
                    _context.Offset += Token;
                }
                if (_context.State == _machine.Token && transferred >= Length) {
                    _context.Length = Length;
                    _machine.RaiseEvent(_context, x => x.Receive);
                    transferred -= Length;
                    _context.Offset += Length;
                }
                if(_context.State == _machine.Length && transferred >= _context.Length) {
                    _machine.RaiseEvent(_context, x => x.Receive);
                    transferred -= _context.Length;
                }
            }
        }

        private void ProcessReceive(byte[] buffer, int offset, int transferred, Action<Segment> callback) {
            var previous = offset;
            for (int i = offset; i < (offset + transferred); ++i) {
                if (buffer[i] == 0x00) {
                    var segment = new Segment(buffer, previous, i - previous);
                    callback(segment);
                    previous = i;
                }
            }
        }
    }

    internal class Message {
        public byte[] Buffer;
        public Socket Socket;
        public int Count;
    }

    public delegate void HandshakeDelegate(Version version = null, Authkey authkey = null);

    public class Version {
        private byte[] _buffer;

        internal Version(byte[] buffer) {
            _buffer = buffer;
        }

        public static implicit operator Version(byte[] value) {
            return new Version(value);
        }

        public static implicit operator byte[] (Version value) {
            return value._buffer;
        }
    }

    public static class Versions {
        public static readonly Version v3 = (Version)new byte[] { 0x3e, 0xe8, 0x75, 0x5f };
    }

    public class Authkey {
        public static readonly Authkey Empty = new Authkey(String.Empty);

        private string _value;
        private byte[] _buffer;

        internal Authkey(string value) {
            _value = value;
            _buffer = Encoding.ASCII.GetBytes(value);
        }

        private Authkey(byte[] buffer) {
            _buffer = buffer;
        }

        public override string ToString() {
            return _value;
        }

        public static explicit operator byte[] (Authkey value) {
            return value._buffer;
        }

        public static implicit operator string (Authkey value) {
            return value._value;
        }

        public static implicit operator Authkey(string value) {
            return new Authkey(value);
        }
    }

    public class Segment {
        private byte[] _buffer;
        private int _offset;
        private int _count;

        public Segment(byte[] buffer) : this(buffer, 0, buffer.Length) {
        }

        public Segment(byte[] buffer, int offset, int count) {
            if (null == buffer) {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset", "Non-negative number required.");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count", "Non-negative number required.");
            }
            if ((buffer.Length - offset) < count) {
                throw new ArgumentException("out of bounds", "offset");
            }

            _buffer = buffer;
            _offset = offset;
            _count = count;
        }

        public byte[] Buffer {
            get { return _buffer; }
        }

        public int Offset {
            get { return _offset; }
        }

        public int Count {
            get { return _count; }
        }

        public override bool Equals(object obj) {
            return (obj is Segment) ? Equals((Segment)obj) : false;
        }

        public bool Equals(Segment obj) {
            return ((_buffer == obj.Buffer) && (_offset == obj.Offset) && (_count == obj.Count));
        }

        public override int GetHashCode() {
            return ((_buffer.GetHashCode() ^ _offset) ^ _count);
        }

        public static bool operator ==(Segment a, Segment b) {
            return a.Equals(b);
        }

        public static bool operator !=(Segment a, Segment b) {
            return !(a.Equals(b));
        }

        public static implicit operator byte[] (Segment value) {
            return value.Buffer;
        }

        public static implicit operator Segment(byte[] value) {
            return new Segment(value);
        }

        public override string ToString() {
            return (new { Offset = _offset, Count = _count }).ToString();
        }
    }

    public static partial class Extensions {
        public static Task ConnectAsync(this Socket socket, IPEndPoint endPoint) {
            var tcs = new TaskCompletionSource<bool>();
            var completed = default(EventHandler<SocketAsyncEventArgs>);
            var saea = new SocketAsyncEventArgs {
                RemoteEndPoint = endPoint
            };
            saea.Completed += completed = (s, e) => {
                e.Completed -= completed;
                if (e.SocketError == SocketError.Success) {
                    tcs.SetResult(true);
                }
                else {
                    tcs.SetException(new SocketException((int)e.SocketError));
                }
            };

            if (false == socket.ConnectAsync(saea)) {
                completed(socket, saea);
            }

            return tcs.Task;
        }

        public static void Handshake(this Socket socket, Action<HandshakeDelegate> handshake) {
            handshake((version, authekey) => {
                var json = new byte[] { 0xc7, 0x70, 0x69, 0x7e };
                socket.Send(version);
                socket.Send(authekey);
                socket.Send(json);
            });
        }

        internal static void Send(this Socket socket, Authkey authkey) {
            if (authkey.IsNullOrWhiteSpace()) {
                socket.Send(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4, SocketFlags.None);
            }
            else {
                var buffer = (byte[])authkey;
                var length = BitConverter.GetBytes(buffer.Length).ToLittleEndian();
                socket.Send(length);
                socket.Send(buffer);
            }
        }

        internal static bool IsNullOrWhiteSpace(this Authkey self) {
            return (null == self || string.IsNullOrWhiteSpace(self));
        }

        internal static byte[] ToLittleEndian(this byte[] self) {
            if (false == BitConverter.IsLittleEndian) {
                Array.Reverse(self);
            }
            return self;
        }

        public static void Send(this Socket self, Func<Socket, Func<byte[]>> version) {
            self.Send(version(self)());
        }

        private static readonly byte[] _v03 = new byte[] { };

        internal static byte[] V03(this Socket self) {
            return _v03;
        }

        internal static void SendAsync(this Socket socket, Action<ISocketAsyncEventArgsBuilder> builder) {
            var b = new SocketAsyncEventArgsBuilder();
            builder(b);
            var saea = b.Build();
            socket.SendAsync(saea);
        }
    }

    internal interface ISocketAsyncEventArgsBuilder {
        ISocketAsyncEventArgsBuilder Buffer(byte[] value);
        ISocketAsyncEventArgsBuilder Offset(int value);
        ISocketAsyncEventArgsBuilder Length(int value);
    }

    internal class SocketAsyncEventArgsBuilder : ISocketAsyncEventArgsBuilder {
        private byte[] _buffer;
        private int? _length;
        private int? _offset;

        public ISocketAsyncEventArgsBuilder Buffer(byte[] value) {
            _buffer = value;
            return this;
        }

        public ISocketAsyncEventArgsBuilder Length(int value) {
            _length = value;
            return this;
        }

        public ISocketAsyncEventArgsBuilder Offset(int value) {
            _offset = value;
            return this;
        }

        public SocketAsyncEventArgs Build() {
            var saea = new SocketAsyncEventArgs();
            var buffer = _buffer;
            var length = (null == _length) ? _buffer.Length : _length.Value;
            var offset = (null == _offset) ? 0 : _offset.Value;

            saea.SetBuffer(buffer, offset, length);

            return saea;
        }
    }
}
