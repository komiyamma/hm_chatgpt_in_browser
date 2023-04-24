using Microsoft.AspNetCore.Components.Server.Circuits;

// 秀丸やブラウザ含め、このアプリと接続しているモノが０になるかどうかを診断するため。
namespace HmChatGptInBrowser
{
    public class CircuitHandlerService : CircuitHandler
    {
        private static int remainingTotalConnections = 0;
        public static int RemainingTotalConnections { get { return remainingTotalConnections; } }


        public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            remainingTotalConnections++;
            var circuitId = circuit.Id;
            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            remainingTotalConnections--;
            var circuitId = circuit.Id;
            return base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
    }
}
