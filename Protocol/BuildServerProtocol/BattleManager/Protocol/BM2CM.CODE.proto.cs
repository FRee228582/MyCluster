//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from: BM2CM.CODE.proto
namespace Message.Server.BattleManager.Protocol.BM2CM
{
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MSG_BM2CM_HEARTBEAT")]
  public partial class MSG_BM2CM_HEARTBEAT : global::ProtoBuf.IExtensible
  {
    public MSG_BM2CM_HEARTBEAT() {}
    
    private int _serverId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"serverId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int serverId
    {
      get { return _serverId; }
      set { _serverId = value; }
    }
    private int _subId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"subId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int subId
    {
      get { return _subId; }
      set { _subId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
  [global::System.Serializable, global::ProtoBuf.ProtoContract(Name=@"MSG_BM2CM_REGISTER")]
  public partial class MSG_BM2CM_REGISTER : global::ProtoBuf.IExtensible
  {
    public MSG_BM2CM_REGISTER() {}
    
    private int _areaId;
    [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name=@"areaId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int areaId
    {
      get { return _areaId; }
      set { _areaId = value; }
    }
    private int _serverId;
    [global::ProtoBuf.ProtoMember(2, IsRequired = true, Name=@"serverId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int serverId
    {
      get { return _serverId; }
      set { _serverId = value; }
    }
    private int _subId;
    [global::ProtoBuf.ProtoMember(3, IsRequired = true, Name=@"subId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
    public int subId
    {
      get { return _subId; }
      set { _subId = value; }
    }
    private global::ProtoBuf.IExtension extensionObject;
    global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
      { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
  }
  
}