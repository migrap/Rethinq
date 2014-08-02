using Automatonymous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Rethinq.Data.RqlClient;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using Rethinq.Data.RqlClient.Converters;

namespace Rethinq {
    public class RethinqConnection {
        [ThreadStatic]
        private static Token _token;
        internal static Token Token {
            get { return _token ?? (_token = new Token((ushort)Thread.CurrentThread.ManagedThreadId)); }
        }

        private static Func<byte[], int, int, string> GetString = (b, i, c) => Encoding.UTF8.GetString(b, i, c);
        private StateMachine _machine = new StateMachine();
        private Context _context = new Context();
        private MemoryStream _stream = new MemoryStream();
        private IDictionary<ulong, TaskCompletionSource<string>> _responses = new ConcurrentDictionary<ulong, TaskCompletionSource<string>>();

        public RethinqConnection() {
            _machine.TransitionToState(_context, x => x.Initial);
            _machine.StateChanged.Subscribe(StateChanged);
            _machine.StateChanged.Where(x => _machine.Response == x.Current)
                .Subscribe(x => _machine.TransitionToState(_context, m => m.Initial));
        }

        public async Task SendAsync(Term term, CancellationToken cancellationToken) {
            var token = Token.Oxidize();
            var query = new Query(token, term, x => x.Start);
            await SendAsync(query, cancellationToken);
        }

        internal async Task SendAsync(Query query, CancellationToken cancellationToken) {
            var tcs = new TaskCompletionSource<string>(query);
            _responses[query.Token] = tcs;

            Action abort = () => {
                if (_responses.Remove(query.Token)) {
                    tcs.SetCanceled();
                }
            };

            Func<Query, byte[]> serialize = (q) => {
                var json = JsonConvert.SerializeObject(q, new QueryConverter(), new TermConverter(), new WhereClauseConverter());
                return Encoding.UTF8.GetBytes(json);
             };

            using (cancellationToken.Register(abort)) {
                var stream = new MemoryStream();

                var buffer = serialize(query);
                var token = query.Token.GetBytes();
                var length = buffer.Length.GetBytes();

                stream.Write(token, 0, token.Length);
                stream.Write(length, 0, length.Length);
                stream.Write(buffer, 0, buffer.Length);

                await Globals.Connetion.SendAsync(stream.GetBuffer(), 0, (int)stream.Position);                
            }
        }

        private void StateChanged(StateChanged<Context> change) {
            var buffer = _context.Buffer;
            var offset = _context.Offset;
            var length = _context.Length;

            if (_machine.Initial == change.Current) {
                _context.Length = 0;
                _context.Offset = 0;
                _context.Token = 0;
            }
            else if (_machine.Token == change.Current) {
                _context.Token = (ulong)buffer.GetToken(offset);
            }
            else if (_machine.Length == change.Current) {
                _context.Length = buffer.GetLength(offset);
            }
            else if (_machine.Response == change.Current) {
                var json = GetString(buffer, offset, length);
                var token = _context.Token;

                if(_responses.TryGetValue(token, out TaskCompletionSource<string> tcs)){
                    _responses.Remove(token);
                    Task.Run(() => tcs.SetResult(json));
                }
            }
        }

        private int SocketReceive(StateMachine machine, Context context, int length) {
            context.Length = length;
            machine.RaiseEvent(context, x => x.Receive);
            context.Offset += length;
            return length;
        }

        private void SocketReceived(byte[] buffer, int offset, int length) {
            _context.Buffer = buffer;
            _context.Offset = offset;

            while (length > 0) {
                if (_machine.Initial == _context.State && length >= 8) {
                    length -= SocketReceive(_machine, _context, 8);
                }
                if (_machine.Token == _context.State && length >= 4) {
                    length -= SocketReceive(_machine, _context, 4);
                }
                if (_machine.Length == _context.State && length >= _context.Length) {
                    length -= SocketReceive(_machine, _context, _context.Length);
                }
            }
        }
    }

    public static partial class Extensions {
        public static int GetLength(this byte[] buffer, int offset = 0) {
            if (BitConverter.IsLittleEndian) {
                return (buffer[offset + 3] << 24 | buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset + 0]);
            }
            else {
                return (buffer[offset + 0] << 24 | buffer[offset + 1] << 16 | buffer[offset + 2] << 8 | buffer[offset + 3]);
            }
        }

        public static ulong GetToken(this byte[] buffer, int offset = 0) {
            if (BitConverter.IsLittleEndian) {
                return (ulong)(buffer[offset + 7] << 56 | buffer[offset + 6] << 48 | buffer[offset + 5] << 40 | buffer[offset + 4] << 32 | buffer[offset + 3] << 24 | buffer[offset + 2] << 16 | buffer[offset + 1] << 8 | buffer[offset + 0]);
            }
            else {
                return (ulong)(buffer[offset + 0] << 56 | buffer[offset + 1] << 48 | buffer[offset + 2] << 40 | buffer[offset + 3] << 32 | buffer[offset + 4] << 24 | buffer[offset + 5] << 16 | buffer[offset + 6] << 8 | buffer[offset + 7]);
            }
        }

        public static byte[] GetBytes(this int self) {
            return BitConverter.GetBytes(self).ToLittleEndian();
        }

        public static byte[] GetBytes(this ulong self) {
            return BitConverter.GetBytes(self).ToLittleEndian();
        }
    }
}