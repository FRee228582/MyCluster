#ifndef Message_Server_Battle_Protocol_B2CM_h
#define Message_Server_Battle_Protocol_B2CM_h

namespace Message { namespace Server { namespace Battle { namespace Protocol { namespace B2CM {
class MSG_B2CM_HEARTBEAT;
class MSG_B2CM_REGISTER;
} } } } }

const uint32 engine::id<Message::Server::Battle::Protocol::B2CM::MSG_B2CM_HEARTBEAT>::value = 0x03010001;
const uint32 engine::id<Message::Server::Battle::Protocol::B2CM::MSG_B2CM_REGISTER>::value = 0x03010002;

namespace Server { namespace Battle { namespace Protocol { class B2CM; } } }
const uint32 engine::id<Server::Battle::Protocol::B2CM>::value = 0x03010000;
#endif