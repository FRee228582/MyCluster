#ifndef Message_Server_BattleManager_Protocol_BM2B_h
#define Message_Server_BattleManager_Protocol_BM2B_h

namespace Message { namespace Server { namespace BattleManager { namespace Protocol { namespace BM2B {
class MSG_BM2B_HEARTBEAT;
class MSG_BM2B_RETRUN_REGISTER;
class MSG_BM2B_TEST;
} } } } }

const uint32 engine::id<Message::Server::BattleManager::Protocol::BM2B::MSG_BM2B_HEARTBEAT>::value = 0x02030001;
const uint32 engine::id<Message::Server::BattleManager::Protocol::BM2B::MSG_BM2B_RETRUN_REGISTER>::value = 0x02030002;
const uint32 engine::id<Message::Server::BattleManager::Protocol::BM2B::MSG_BM2B_TEST>::value = 0x02039999;

namespace Server { namespace BattleManager { namespace Protocol { class BM2B; } } }
const uint32 engine::id<Server::BattleManager::Protocol::BM2B>::value = 0x02030000;
#endif