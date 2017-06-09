using System;
namespace Message.Server.BattleManager.Protocol.BM2B {

	public partial class MSG_BM2B_HEARTBEAT {}
	public partial class MSG_BM2B_RETRUN_REGISTER {}
	public partial class MSG_BM2B_TEST {}
	public class Api {
		static public void GenerateId() {
			Engine.Foundation.Id<Message.Server.BattleManager.Protocol.BM2B.MSG_BM2B_HEARTBEAT>.Value = 0x02030001;
			Engine.Foundation.Id<Message.Server.BattleManager.Protocol.BM2B.MSG_BM2B_RETRUN_REGISTER>.Value = 0x02030002;
			Engine.Foundation.Id<Message.Server.BattleManager.Protocol.BM2B.MSG_BM2B_TEST>.Value = 0x02039999;
		}
	}

}
namespace Server.BattleManager.Protocol {
	partial class BM2B {
		static public void GenerateId() {
			Engine.Foundation.Id<Server.BattleManager.Protocol.BM2B>.Value = 0x02030000;
		}
	}
}
