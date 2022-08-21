using System.Diagnostics;

/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */



//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(push, 1)

public class UdpMsg
{
   public enum MsgType
   {
	  Invalid = 0,
	  SyncRequest = 1,
	  SyncReply = 2,
	  Input = 3,
	  QualityReport = 4,
	  QualityReply = 5,
	  KeepAlive = 6,
	  InputAck = 7
   }

   public class connect_status
   {
//C++ TO C# CONVERTER TODO TASK: C# does not allow bit fields:
	  public uint disconnected:1;
//C++ TO C# CONVERTER TODO TASK: C# does not allow bit fields:
	  public int last_frame:31;
   }

//C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
   public class AnonymousClass
   {
	  public ushort magic;
	  public ushort sequence_number;
	  public byte type; // packet type
   }
   public AnonymousClass hdr = new AnonymousClass();
//C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
//   union
//   {
//	  struct
//	  {
//		 uint random_request; // please reply back with this random data
//		 ushort remote_magic;
//		 byte remote_endpoint;
//	  }
//	  sync_request;
//
//	  struct
//	  {
//		 uint random_reply; // OK, here's your random data back
//	  }
//	  sync_reply;
//
//	  struct
//	  {
//		 sbyte frame_advantage; // what's the other guy's frame advantage?
//		 uint ping;
//	  }
//	  quality_report;
//
//	  struct
//	  {
//		 uint pong;
//	  }
//	  quality_reply;
//
//	  struct
//	  {
//		 connect_status peer_connect_status[DefineConstants.UDP_MSG_MAX_PLAYERS];
//
//		 uint start_frame;
//
//		 int disconnect_requested:1;
//		 int ack_frame:31;
//
//		 ushort num_bits;
//		 byte input_size; // XXX: shouldn't be in every single packet!
//		 byte bits[DefineConstants.MAX_COMPRESSED_BITS]; // must be last
//	  }
//	  input;
//
//	  struct
//	  {
//		 int ack_frame:31;
//	  }
//	  input_ack;
//
//   }
//C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
//   u;

   public int PacketSize()
   {
	  return sizeof(UdpMsg.AnonymousClass) + PayloadSize();
   }

   public int PayloadSize()
   {
	  int size;

	  switch (hdr.type)
	  {
	  case MsgType.SyncRequest:
		  return sizeof(UdpMsg.AnonymousStruct.AnonymousClass2);
	  case MsgType.SyncReply:
		  return sizeof(UdpMsg.AnonymousStruct.AnonymousClass3);
	  case MsgType.QualityReport:
		  return sizeof(UdpMsg.AnonymousStruct.AnonymousClass4);
	  case MsgType.QualityReply:
		  return sizeof(UdpMsg.AnonymousStruct.AnonymousClass5);
	  case MsgType.InputAck:
		  return sizeof(UdpMsg.AnonymousStruct.AnonymousClass7);
	  case MsgType.KeepAlive:
		  return 0;
	  case MsgType.Input:
		 size = (int)((string) u.input.bits - (string) u.input);
		 size += (u.input.num_bits + 7) / 8;
		 return size;
	  }
	  Debug.Assert(false);
	  return 0;
   }

   public UdpMsg(MsgType t)
   {
	   hdr.type = (byte)t;
   }
}

//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
//#pragma pack(pop)

