using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester {
    internal sealed class TesterMoveToSlotExecutor : JobExecutor {
        private readonly int Deck;
        private readonly int Slot;

        protected override string JobName => "tester-move-to-slot";

        protected override void SetupJob() {
            this.Job.Push((object)this.Slot);
            this.Job.Push((object)this.Deck);
        }

        internal TesterMoveToSlotExecutor(HardwareService service, int deck, int slot)
          : base(service) {
            this.Deck = deck;
            this.Slot = slot;
        }
    }
}
