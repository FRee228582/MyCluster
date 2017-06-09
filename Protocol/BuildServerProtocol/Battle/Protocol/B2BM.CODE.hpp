#ifndef Message_Server_Battle_Protocol_B2BM_h
#define Message_Server_Battle_Protocol_B2BM_h

namespace Message { namespace Server { namespace Battle { namespace Protocol { namespace B2BM {
class MSG_B2BM_HEARTBEAT;
class MSG_B2BM_REGISTER;
class MSG_B2BM_TEST;
} } } } }

const uint32 engine::id<Message::Server::Battle::Protocol::B2BM::MSG_B2BM_HEARTBEAT>::value = 0x03020001;
const uint32 engine::id<Message::Server::Battle::Protocol::B2BM::MSG_B2BM_REGISTER>::value = 0x03020002;
const uint32 engine::id<Message::Server::Battle::Protocol::B2BM::MSG_B2BM_TEST>::value = 0x03029999;

namespace Server { namespace Battle { namespace Protocol { class B2BM; } } }
const uint32 engine::id<Server::Battle::Protocol::B2BM>::value = 0x03020000;
#endif