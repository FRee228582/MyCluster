#ifndef Message_Server_BattleManager_Protocol_BM2CM_h
#define Message_Server_BattleManager_Protocol_BM2CM_h

namespace Message { namespace Server { namespace BattleManager { namespace Protocol { namespace BM2CM {
class MSG_BM2CM_HEARTBEAT;
class MSG_BM2CM_REGISTER;
} } } } }

const uint32 engine::id<Message::Server::BattleManager::Protocol::BM2CM::MSG_BM2CM_HEARTBEAT>::value = 0x02010001;
const uint32 engine::id<Message::Server::BattleManager::Protocol::BM2CM::MSG_BM2CM_REGISTER>::value = 0x02010002;

namespace Server { namespace BattleManager { namespace Protocol { class BM2CM; } } }
const uint32 engine::id<Server::BattleManager::Protocol::BM2CM>::value = 0x02010000;
#endif