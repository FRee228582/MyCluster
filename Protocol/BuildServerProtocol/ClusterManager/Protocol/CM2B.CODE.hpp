#ifndef Message_Server_ClusterManager_Protocol_CM2B_h
#define Message_Server_ClusterManager_Protocol_CM2B_h

namespace Message { namespace Server { namespace ClusterManager { namespace Protocol { namespace CM2B {
class MSG_CM2B_HEARTBEAT;
class MSG_CM2B_RETRUN_REGISTER;
} } } } }

const uint32 engine::id<Message::Server::ClusterManager::Protocol::CM2B::MSG_CM2B_HEARTBEAT>::value = 0x01030001;
const uint32 engine::id<Message::Server::ClusterManager::Protocol::CM2B::MSG_CM2B_RETRUN_REGISTER>::value = 0x01030002;

namespace Server { namespace ClusterManager { namespace Protocol { class CM2B; } } }
const uint32 engine::id<Server::ClusterManager::Protocol::CM2B>::value = 0x01030000;
#endif