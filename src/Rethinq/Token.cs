using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq {
    internal class Token : Oxidation<ulong> {
        private readonly ushort _identifier;

        public Token(ushort identifier)
            : this(identifier, new DateTimeOffset(2014, 1, 1, 0, 0, 0, TimeSpan.Zero)) {
        }

        public Token(ushort identifier,DateTimeOffset epoch)
            : base(epoch) {
            _identifier = identifier;
        }

        public override ulong Oxidize() {
            Update();
            return (Oxidized << 32) + (ulong)(_identifier << 16) + Counter;
        }
    }

    internal abstract class Oxidation<T> {
        public static readonly DateTimeOffset DefaultEpoch = new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        protected ushort Counter;
        protected readonly DateTimeOffset Epoch;
        protected ulong Oxidized;

        protected Oxidation(DateTimeOffset epoch) {
            Counter = 0;
            Epoch = epoch;
            Oxidized = Now();
        }

        public abstract T Oxidize();

        protected void Update() {
            var now = Now();

            if (Oxidized > now) {
                throw new ApplicationException("Clock is running backwards");
            }

            Counter = (ushort)((Oxidized < now) ? 0 : Counter + 1);
            Oxidized = now;
        }

        private ulong Now() {
            return (ulong)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }
    }
}
