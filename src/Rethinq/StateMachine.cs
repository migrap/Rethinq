using Automatonymous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rethinq {
    public sealed class StateMachine : AutomatonymousStateMachine<Context> {
        public StateMachine() {
            State(() => Token);
            State(() => Length);
            State(() => Response);

            InstanceState(x => x.State);
            
            Event(() => Receive);

            During(Initial, When(Receive).TransitionTo(Token));
            During(Token, When(Receive).TransitionTo(Length));
            During(Length, When(Receive).TransitionTo(Response));

            StateChanged.Subscribe(x => {
                Console.WriteLine("{0} => {1}", x.Previous, x.Current);
            });
        }

        public Event Receive { get; set; }
        public State Token { get; set; }
        public State Length { get; set; }
        public State Response { get; set; }
    }

    public static partial class Extensions {
        public static IObservable<EventRaised<Context>> EventRaised(this StateMachine stateMachine, Func<StateMachine, Event> eventSelector) {
            return stateMachine.EventRaised(eventSelector(stateMachine));
        }

        public static void TransitionToState(this StateMachine machine, Context context, Func<StateMachine, State> state) {
            machine.TransitionToState(context, state(machine));
        }
    }

    public class Context {
        public State State { get; set; }
        public ulong Token { get; set; }
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }
}
