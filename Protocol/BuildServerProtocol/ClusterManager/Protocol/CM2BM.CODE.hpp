#ifndef Message_Server_ClusterManager_Protocol_CM2BM_h
#define Message_Server_ClusterManager_Protocol_CM2BM_h

namespace Message { namespace Server { namespace ClusterManager { namespace Protocol { namespace CM2BM {
class MSG_CM2BM_HEARTBEAT;
class MSG_CM2BM_RETRUN_REGISTER;
} } } } }

const uint32 engine::id<Message::Server::ClusterManager::Protocol::CM2BM::MSG_CM2BM_HEARTBEAT>::value = 0x01020001;
const uint32 engine::id<Message::Server::ClusterManager::Protocol::CM2BM::MSG_CM2BM_RETRUN_REGISTER>::value = 0x01020002;

namespace Server { namespace ClusterManager { namespace Protocol { class CM2BM; } } }
const uint32 engine::id<Server::ClusterManager::Protocol::CM2BM>::value = 0x01020000;
#endif