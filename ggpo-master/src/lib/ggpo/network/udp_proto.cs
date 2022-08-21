using System.Diagnostics;

/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRTIMP __declspec(dllexport)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRTIMP __declspec(dllimport)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING)) extern "C" {
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_END_C_HEADER } __pragma(pack(pop))
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER cpp_quote("__pragma(pack(push, _CRT_PACKING))") cpp_quote("extern \"C\" {")
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_END_C_HEADER cpp_quote("}") cpp_quote("__pragma(pack(pop))")
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING))
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define _CRT_END_C_HEADER __pragma(pack(pop))
//C++ TO C# CONVERTER WARNING: Statement interrupted by a preprocessor statement:
//The original statement from the file starts with:
//    __pragma(pack(push, _CRT_PACKING))
//Preprocessor-interrupted statements cannot be handled by this converter.
//The remainder of the header file is ignored.
/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */


//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define GGPO_API __declspec(dllexport)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define GGPO_API __declspec(dllimport)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define GGPO_ERRORLIST GGPO_ERRORLIST_ENTRY(GGPO_OK, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_SUCCESS, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_GENERAL_FAILURE, -1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_SESSION, 1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_PLAYER_HANDLE, 2) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE, 3) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PREDICTION_THRESHOLD, 4) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_UNSUPPORTED, 5) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_NOT_SYNCHRONIZED, 6) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_IN_ROLLBACK, 7) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INPUT_DROPPED, 8) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_DISCONNECTED, 9) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_TOO_MANY_SPECTATORS, 10) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_REQUEST, 11)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define GGPO_ERRORLIST_ENTRY(name, value) name = value,
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define GGPO_SUCCEEDED(result) ((result) == GGPO_ERRORCODE_SUCCESS)
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:

public class UdpProtocol : IPollSink, System.IDisposable
{
   public class Stats
   {
	  public int ping;
	  public int remote_frame_advantage;
	  public int local_frame_advantage;
	  public int send_queue_len;
	  public Udp.Stats udp = new Udp.Stats();
   }

   public class Event
   {
	  public enum Type
	  {
		 Unknown = -1,
		 Connected,
		 Synchronizing,
		 Synchronzied,
		 Input,
		 Disconnected,
		 NetworkInterrupted,
		 NetworkResumed
	  }

	  public Type type;
//C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
//	  union
//	  {
//		 struct
//		 {
//			GameInput input;
//		 }
//		 input;
//		 struct
//		 {
//			int total;
//			int count;
//		 }
//		 synchronizing;
//		 struct
//		 {
//			int disconnect_timeout;
//		 }
//		 network_interrupted;
//	  }
//C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
//	  u;

	  public UdpProtocol.Event(Type t = Type.Unknown)
	  {
		  this.type = new UdpProtocol.Event.Type(t);
	  }
   }

   public virtual bool OnLoopPoll(object cookie)
   {
	  if (_udp == null)
	  {
		 return true;
	  }

	  uint now = Platform.GetCurrentTimeMS();
	  uint next_interval;

	  PumpSendQueue();
	  switch (_current_state)
	  {
	  case State.Syncing:
		 next_interval = (_state.sync.roundtrips_remaining == Globals.NUM_SYNC_PACKETS) ? (uint)Globals.SYNC_FIRST_RETRY_INTERVAL : (uint)Globals.SYNC_RETRY_INTERVAL;
		 if (_last_send_time != 0 && _last_send_time + next_interval < now)
		 {
			Log("No luck syncing after %d ms... Re-queueing sync packet.\n", next_interval);
			SendSyncRequest();
		 }
		 break;

	  case State.Running:
		 // xxx: rig all this up with a timer wrapper
		 if (_state.running.last_input_packet_recv_time == 0 || _state.running.last_input_packet_recv_time + Globals.RUNNING_RETRY_INTERVAL < now)
		 {
			Log("Haven't exchanged packets in a while (last received:%d  last sent:%d).  Resending.\n", _last_received_input.frame, _last_sent_input.frame);
			SendPendingOutput();
			_state.running.last_input_packet_recv_time = now;
		 }

		 if (_state.running.last_quality_report_time == 0 || _state.running.last_quality_report_time + Globals.QUALITY_REPORT_INTERVAL < now)
		 {
			UdpMsg msg = new UdpMsg(UdpMsg.MsgType.QualityReport);
			msg.u.quality_report.ping = Platform.GetCurrentTimeMS();
			msg.u.quality_report.frame_advantage = (byte)_local_frame_advantage;
			SendMsg(msg);
			_state.running.last_quality_report_time = now;
		 }

		 if (_state.running.last_network_stats_interval == 0 || _state.running.last_network_stats_interval + Globals.NETWORK_STATS_INTERVAL < now)
		 {
			UpdateNetworkStats();
			_state.running.last_network_stats_interval = now;
		 }

		 if (_last_send_time != 0 && _last_send_time + Globals.KEEP_ALIVE_INTERVAL < now)
		 {
			Log("Sending keep alive packet\n");
			SendMsg(new UdpMsg(UdpMsg.MsgType.KeepAlive));
		 }

		 if (_disconnect_timeout != 0 && _disconnect_notify_start != 0 && !_disconnect_notify_sent && (_last_recv_time + _disconnect_notify_start < now))
		 {
			Log("Endpoint has stopped receiving packets for %d ms.  Sending notification.\n", _disconnect_notify_start);
			Event e = new Event(Event.NetworkInterrupted);
			e.u.network_interrupted.disconnect_timeout = (int)(_disconnect_timeout - _disconnect_notify_start);
			QueueEvent(e);
			_disconnect_notify_sent = true;
		 }

		 if (_disconnect_timeout != 0 && (_last_recv_time + _disconnect_timeout < now))
		 {
			if (_disconnect_event_sent == 0)
			{
			   Log("Endpoint has stopped receiving packets for %d ms.  Disconnecting.\n", _disconnect_timeout);
			   QueueEvent(new Event(Event.Disconnected));
			   _disconnect_event_sent = true ? 1 : 0;
			}
		 }
		 break;

	  case State.Disconnected:
		 if (_shutdown_timeout < now)
		 {
			Log("Shutting down udp connection.\n");
			_udp = null;
			_shutdown_timeout = 0;
		 }

		 break;
	  }


	  return true;
   }

   public UdpProtocol()
   {
	   this._local_frame_advantage = 0;
	   this._remote_frame_advantage = 0;
	   this._queue = -1;
	   this._magic_number = 0;
	   this._remote_magic_number = 0;
	   this._packets_sent = 0;
	   this._bytes_sent = 0;
	   this._stats_start_time = 0;
	   this._last_send_time = 0;
	   this._shutdown_timeout = 0;
	   this._disconnect_timeout = 0;
	   this._disconnect_notify_start = 0;
	   this._disconnect_notify_sent = false;
	   this._disconnect_event_sent = false ? 1 : 0;
	   this._connected = false;
	   this._next_send_seq = 0;
	   this._next_recv_seq = 0;
	   this._udp = null;
	  _last_sent_input.init(-1, null, 1);
	  _last_received_input.init(-1, null, 1);
	  _last_acked_input.init(-1, null, 1);

//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_state, 0, sizeof (UdpProtocol.AnonymousStruct2));
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_peer_connect_status, 0, sizeof(UdpMsg.connect_status));
	  for (int i = 0; i < ARRAY_SIZE(_peer_connect_status); i++)
	  {
		 _peer_connect_status[i].last_frame = -1;
	  }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_peer_addr, 0, sizeof (sockaddr_in));
	  _oo_packet.msg = null;

	  _send_latency = Platform.GetConfigInt("ggpo.network.delay");
	  _oop_percent = Platform.GetConfigInt("ggpo.oop.percent");
   }

   public virtual void Dispose()
   {
	  ClearSendQueue();
   }

   public void Init(Udp udp, Poll poll, int queue, ref string ip, u_short port, UdpMsg.connect_status status)
   {
	  _udp = udp;
	  _queue = queue;
	  _local_connect_status = status;

	  _peer_addr.sin_family = AF_INET;
	  _peer_addr.sin_port = htons(port);
	  inet_pton(AF_INET, ip, _peer_addr.sin_addr.s_addr);

	  do
	  {
		 _magic_number = (ushort)rand();
	  } while (_magic_number == 0);
	  poll.RegisterLoop(this);
   }

   public void Synchronize()
   {
	  if (_udp != null)
	  {
		 _current_state = State.Syncing;
		 _state.sync.roundtrips_remaining = (uint)Globals.NUM_SYNC_PACKETS;
		 SendSyncRequest();
	  }
   }

   public bool GetPeerConnectStatus(int id, ref int frame)
   {
	  frame = _peer_connect_status[id].last_frame;
	  return !_peer_connect_status[id].disconnected;
   }

   public bool IsInitialized()
   {
	   return _udp != null;
   }
   public bool IsSynchronized()
   {
	   return _current_state == State.Running;
   }
   public bool IsRunning()
   {
	   return _current_state == State.Running;
   }
   public void SendInput(GameInput input)
   {
	  if (_udp != null)
	  {
		 if (_current_state == State.Running)
		 {
			/*
			 * Check to see if this is a good time to adjust for the rift...
			 */
			_timesync.advance_frame(input, _local_frame_advantage, _remote_frame_advantage);

			/*
			 * Save this input packet
			 *
			 * XXX: This queue may fill up for spectators who do not ack input packets in a timely
			 * manner.  When this happens, we can either resize the queue (ug) or disconnect them
			 * (better, but still ug).  For the meantime, make this queue really big to decrease
			 * the odds of this happening...
			 */
			_pending_output.push(input);
		 }
		 SendPendingOutput();
	  }
   }

   public void SendInputAck()
   {
	  UdpMsg msg = new UdpMsg(UdpMsg.MsgType.InputAck);
	  msg.u.input_ack.ack_frame = _last_received_input.frame;
	  SendMsg(msg);
   }

   public bool HandlesMsg(sockaddr_in from, UdpMsg msg)
   {
	  if (_udp == null)
	  {
		 return false;
	  }
	  return _peer_addr.sin_addr.S_un.S_addr == from.sin_addr.S_un.S_addr && _peer_addr.sin_port == from.sin_port;
   }

   public void OnMsg(UdpMsg msg, int len)
   {
	  bool handled = false;
	  DispatchFn[] table = {this.OnInvalid, this.OnSyncRequest, this.OnSyncReply, this.OnInput, this.OnQualityReport, this.OnQualityReply, this.OnKeepAlive, this.OnInputAck};

	  // filter out messages that don't match what we expect
	  ushort seq = msg.hdr.sequence_number;
	  if (msg.hdr.type != UdpMsg.MsgType.SyncRequest && msg.hdr.type != UdpMsg.MsgType.SyncReply)
	  {
		 if (msg.hdr.magic != _remote_magic_number)
		 {
			LogMsg("recv rejecting", msg);
			return;
		 }

		 // filter out out-of-order packets
		 ushort skipped = (ushort)((int)seq - (int)_next_recv_seq);
		 // Log("checking sequence number -> next - seq : %d - %d = %d\n", seq, _next_recv_seq, skipped);
		 if (skipped > Globals.MAX_SEQ_DISTANCE)
		 {
			Log("dropping out of order packet (seq: %d, last seq:%d)\n", seq, _next_recv_seq);
			return;
		 }
	  }

	  _next_recv_seq = seq;
	  LogMsg("recv", msg);
	  if (msg.hdr.type >= ARRAY_SIZE(table))
	  {
		 OnInvalid(msg, len);
	  }
	  else
	  {
		 handled = (table[msg.hdr.type])(msg, len);
	  }
	  if (handled)
	  {
		 _last_recv_time = Platform.GetCurrentTimeMS();
		 if (_disconnect_notify_sent && _current_state == State.Running)
		 {
			QueueEvent(new Event(Event.NetworkResumed));
			_disconnect_notify_sent = false;
		 }
	  }
   }

   private delegate bool DispatchFn(UdpMsg msg, int len);

   public void Disconnect()
   {
	  _current_state = State.Disconnected;
	  _shutdown_timeout = Platform.GetCurrentTimeMS() + Globals.UDP_SHUTDOWN_TIMER;
   }

   public void GetNetworkStats(GGPONetworkStats s)
   {
	  s.network.ping = _round_trip_time;
	  s.network.send_queue_len = _pending_output.size();
	  s.network.kbps_sent = _kbps_sent;
	  s.timesync.remote_frames_behind = _remote_frame_advantage;
	  s.timesync.local_frames_behind = _local_frame_advantage;
   }

   public bool GetEvent(ref UdpProtocol.Event e)
   {
	  if (_event_queue.size() == 0)
	  {
		 return false;
	  }
	  e = _event_queue.front();
	  _event_queue.pop();
	  return true;
   }

//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//   void GGPONetworkStats(Stats stats);
   public void SetLocalFrameNumber(int localFrame)
   {
	  /*
	   * Estimate which frame the other guy is one by looking at the
	   * last frame they gave us plus some delta for the one-way packet
	   * trip time.
	   */
	  int remoteFrame = _last_received_input.frame + (_round_trip_time * 60 / 1000);

	  /*
	   * Our frame advantage is how many frames *behind* the other guy
	   * we are.  Counter-intuative, I know.  It's an advantage because
	   * it means they'll have to predict more often and our moves will
	   * pop more frequenetly.
	   */
	  _local_frame_advantage = remoteFrame - localFrame;
   }

   public int RecommendFrameDelay()
   {
	  // XXX: require idle input should be a configuration parameter
	  return _timesync.recommend_frame_wait_duration(false);
   }

   public void SetDisconnectTimeout(int timeout)
   {
	  _disconnect_timeout = (uint)timeout;
   }

   public void SetDisconnectNotifyStart(int timeout)
   {
	  _disconnect_notify_start = (uint)timeout;
   }

   protected enum State
   {
	  Syncing,
	  Synchronzied,
	  Running,
	  Disconnected
   }
   protected class QueueEntry
   {
	  public int queue_time;
	  public sockaddr_in dest_addr = new sockaddr_in();
	  public UdpMsg msg;

	  public QueueEntry()
	  {
	  }
	  public QueueEntry(int time, sockaddr_in dst, UdpMsg m)
	  {
		  this.queue_time = time;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: this.dest_addr = dst;
		  this.dest_addr.CopyFrom(dst);
		  this.msg = m;
	  }
   }

//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//   bool CreateSocket(int retries);
   protected void UpdateNetworkStats()
   {
	  int now = Platform.GetCurrentTimeMS();

	  if (_stats_start_time == 0)
	  {
		 _stats_start_time = now;
	  }

	  int total_bytes_sent = _bytes_sent + (Globals.UDP_HEADER_SIZE * _packets_sent);
	  float seconds = (float)((now - _stats_start_time) / 1000.0);
	  float Bps = total_bytes_sent / seconds;
	  float udp_overhead = (float)(100.0 * (Globals.UDP_HEADER_SIZE * _packets_sent) / _bytes_sent);

	  _kbps_sent = (int)(Bps / 1024);

	  Log("Network Stats -- Bandwidth: %.2f KBps   Packets Sent: %5d (%.2f pps)   " + "KB Sent: %.2f    UDP Overhead: %.2f %%.\n", _kbps_sent, _packets_sent, (float)_packets_sent * 1000 / (now - _stats_start_time), total_bytes_sent / 1024.0, udp_overhead);
   }

   protected void QueueEvent(in UdpProtocol.Event evt)
   {
	  LogEvent("Queuing event", evt);
	  _event_queue.push(evt);
   }

   protected void ClearSendQueue()
   {
	  while (!_send_queue.empty())
	  {
		 _send_queue.front().msg = null;
		 _send_queue.pop();
	  }
   }

   protected void Log(string fmt, params object[] LegacyParamArray)
   {
	  string buf = new string(new char[1024]);
	  size_t offset = new size_t();
//	  va_list args;

	  sprintf_s(buf, ARRAY_SIZE(buf), "udpproto%d | ", _queue);
	  offset = strlen(buf);
	  int ParamCount = -1;
//	  va_start(args, fmt);
	  vsnprintf(buf + offset, ARRAY_SIZE(buf) - offset - 1, fmt, args);
	  buf = StringFunctions.ChangeCharacter(buf, ARRAY_SIZE(buf) - 1, '\0');
	  global::Log(buf);
//	  va_end(args);
   }

   protected void LogMsg(string prefix, UdpMsg msg)
   {
	  switch (msg.hdr.type)
	  {
	  case UdpMsg.MsgType.SyncRequest:
		 Log("%s sync-request (%d).\n", prefix, msg.u.sync_request.random_request);
		 break;
	  case UdpMsg.MsgType.SyncReply:
		 Log("%s sync-reply (%d).\n", prefix, msg.u.sync_reply.random_reply);
		 break;
	  case UdpMsg.MsgType.QualityReport:
		 Log("%s quality report.\n", prefix);
		 break;
	  case UdpMsg.MsgType.QualityReply:
		 Log("%s quality reply.\n", prefix);
		 break;
	  case UdpMsg.MsgType.KeepAlive:
		 Log("%s keep alive.\n", prefix);
		 break;
	  case UdpMsg.MsgType.Input:
		 Log("%s game-compressed-input %d (+ %d bits).\n", prefix, msg.u.input.start_frame, msg.u.input.num_bits);
		 break;
	  case UdpMsg.MsgType.InputAck:
		 Log("%s input ack.\n", prefix);
		 break;
	  default:
		 Debug.Assert(false && "Unknown UdpMsg type.");
		 break;
	  }
   }

   protected void LogEvent(string prefix, in UdpProtocol.Event evt)
   {
	  switch (evt.type)
	  {
	  case UdpProtocol.Event.Synchronzied:
		 Log("%s (event: Synchronzied).\n", prefix);
		 break;
	  }
   }

   protected void SendSyncRequest()
   {
	  _state.sync.random = (uint)(rand() & 0xFFFF);
	  UdpMsg msg = new UdpMsg(UdpMsg.MsgType.SyncRequest);
	  msg.u.sync_request.random_request = _state.sync.random;
	  SendMsg(msg);
   }

   protected void SendMsg(UdpMsg msg)
   {
	  LogMsg("send", msg);

	  _packets_sent++;
	  _last_send_time = Platform.GetCurrentTimeMS();
	  _bytes_sent += msg.PacketSize();

	  msg.hdr.magic = _magic_number;
	  msg.hdr.sequence_number = _next_send_seq++;

	  _send_queue.push(new QueueEntry(Platform.GetCurrentTimeMS(), _peer_addr, msg));
	  PumpSendQueue();
   }

   protected void PumpSendQueue()
   {
	  while (!_send_queue.empty())
	  {
		 QueueEntry entry = _send_queue.front();

		 if (_send_latency != 0)
		 {
			// should really come up with a gaussian distributation based on the configured
			// value, but this will do for now.
			int jitter = (_send_latency * 2 / 3) + ((rand() % _send_latency) / 3);
			if (Platform.GetCurrentTimeMS() < _send_queue.front().queue_time + jitter)
			{
			   break;
			}
		 }
		 if (_oop_percent != 0 && _oo_packet.msg == null && ((rand() % 100) < _oop_percent))
		 {
			int delay = rand() % (_send_latency * 10 + 1000);
			Log("creating rogue oop (seq: %d  delay: %d)\n", entry.msg.hdr.sequence_number, delay);
			_oo_packet.send_time = Platform.GetCurrentTimeMS() + delay;
			_oo_packet.msg = entry.msg;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _oo_packet.dest_addr = entry.dest_addr;
			_oo_packet.dest_addr.CopyFrom(entry.dest_addr);
		 }
		 else
		 {
			Debug.Assert(entry.dest_addr.sin_addr.s_addr);

			_udp.SendTo(ref (string)entry.msg, entry.msg.PacketSize(), 0, (sockaddr) entry.dest_addr, sizeof (sockaddr_in));

			entry.msg = null;
		 }
		 _send_queue.pop();
	  }
	  if (_oo_packet.msg != null && _oo_packet.send_time < Platform.GetCurrentTimeMS())
	  {
		 Log("sending rogue oop!");
		 _udp.SendTo(ref (string)_oo_packet.msg, _oo_packet.msg.PacketSize(), 0, (sockaddr) _oo_packet.dest_addr, sizeof (sockaddr_in));

		 _oo_packet.msg = null;
		 _oo_packet.msg = null;
	  }
   }

//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//   void DispatchMsg(ref byte buffer, int len);
   protected void SendPendingOutput()
   {
	  UdpMsg msg = new UdpMsg(UdpMsg.MsgType.Input);
	  int i;
	  int j;
	  int offset = 0;
//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
//ORIGINAL LINE: byte *bits;
	  byte bits;
	  GameInput last = new GameInput();

	  if (_pending_output.size() != 0)
	  {
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: last = _last_acked_input;
		 last.CopyFrom(_last_acked_input);
		 bits = msg.u.input.bits;

		 msg.u.input.start_frame = _pending_output.front().frame;
		 msg.u.input.input_size = (byte)_pending_output.front().size;

		 Debug.Assert(last.frame == -1 || last.frame + 1 == msg.u.input.start_frame);
		 for (j = 0; j < _pending_output.size(); j++)
		 {
			GameInput current = _pending_output.item(j);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
			if (memcmp(current.bits, last.bits, current.size) != 0)
			{
			   Debug.Assert((GAMEINPUT_MAX_BYTES * GAMEINPUT_MAX_PLAYERS * 8) < (1 << BITVECTOR_NIBBLE_SIZE));
			   for (i = 0; i < current.size * 8; i++)
			   {
				  Debug.Assert(i < (1 << BITVECTOR_NIBBLE_SIZE));
				  if (current.value(i) != last.value(i))
				  {
					 BitVector_SetBit(msg.u.input.bits, offset);
					 (current.value(i) ? BitVector_SetBit : BitVector_ClearBit)(bits, offset);
					 BitVector_WriteNibblet(bits, i, offset);
				  }
			   }
			}
			BitVector_ClearBit(msg.u.input.bits, offset);
			last = _last_sent_input = current;
		 }
	  }
	  else
	  {
		 msg.u.input.start_frame = 0;
		 msg.u.input.input_size = 0;
	  }
	  msg.u.input.ack_frame = _last_received_input.frame;
	  msg.u.input.num_bits = (ushort)offset;

	  msg.u.input.disconnect_requested = _current_state == State.Disconnected;
	  if (_local_connect_status)
	  {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
		 memcpy(msg.u.input.peer_connect_status, _local_connect_status, sizeof(UdpMsg.connect_status) * DefineConstants.UDP_MSG_MAX_PLAYERS);
	  }
	  else
	  {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
		 memset(msg.u.input.peer_connect_status, 0, sizeof(UdpMsg.connect_status) * DefineConstants.UDP_MSG_MAX_PLAYERS);
	  }

	  Debug.Assert(offset < DefineConstants.MAX_COMPRESSED_BITS);

	  SendMsg(msg);
   }

   protected bool OnInvalid(UdpMsg msg, int len)
   {
	  Debug.Assert(false && "Invalid msg in UdpProtocol");
	  return false;
   }

   protected bool OnSyncRequest(UdpMsg msg, int len)
   {
	  if (_remote_magic_number != 0 && msg.hdr.magic != _remote_magic_number)
	  {
		 Log("Ignoring sync request from unknown endpoint (%d != %d).\n", msg.hdr.magic, _remote_magic_number);
		 return false;
	  }
	  UdpMsg reply = new UdpMsg(UdpMsg.MsgType.SyncReply);
	  reply.u.sync_reply.random_reply = msg.u.sync_request.random_request;
	  SendMsg(reply);
	  return true;
   }

   protected bool OnSyncReply(UdpMsg msg, int len)
   {
	  if (_current_state != State.Syncing)
	  {
		 Log("Ignoring SyncReply while not synching.\n");
		 return msg.hdr.magic == _remote_magic_number;
	  }

	  if (msg.u.sync_reply.random_reply != _state.sync.random)
	  {
		 Log("sync reply %d != %d.  Keep looking...\n", msg.u.sync_reply.random_reply, _state.sync.random);
		 return false;
	  }

	  if (!_connected)
	  {
		 QueueEvent(new Event(Event.Connected));
		 _connected = true;
	  }

	  Log("Checking sync state (%d round trips remaining).\n", _state.sync.roundtrips_remaining);
	  if (--_state.sync.roundtrips_remaining == 0)
	  {
		 Log("Synchronized!\n");
		 QueueEvent(new UdpProtocol.Event(UdpProtocol.Event.Synchronzied));
		 _current_state = State.Running;
		 _last_received_input.frame = -1;
		 _remote_magic_number = msg.hdr.magic;
	  }
	  else
	  {
		 UdpProtocol.Event evt = new UdpProtocol.Event(UdpProtocol.Event.Synchronizing);
		 evt.u.synchronizing.total = Globals.NUM_SYNC_PACKETS;
		 evt.u.synchronizing.count = (int)(Globals.NUM_SYNC_PACKETS - _state.sync.roundtrips_remaining);
		 QueueEvent(evt);
		 SendSyncRequest();
	  }
	  return true;
   }

   protected bool OnInput(UdpMsg msg, int len)
   {
	  /*
	   * If a disconnect is requested, go ahead and disconnect now.
	   */
	  bool disconnect_requested = msg.u.input.disconnect_requested;
	  if (disconnect_requested)
	  {
		 if (_current_state != State.Disconnected && _disconnect_event_sent == 0)
		 {
			Log("Disconnecting endpoint on remote request.\n");
			QueueEvent(new Event(Event.Disconnected));
			_disconnect_event_sent = true ? 1 : 0;
		 }
	  }
	  else
	  {
		 /*
		  * Update the peer connection status if this peer is still considered to be part
		  * of the network.
		  */
		 UdpMsg.connect_status[] remote_status = msg.u.input.peer_connect_status;
		 for (int i = 0; i < ARRAY_SIZE(_peer_connect_status); i++)
		 {
			Debug.Assert(remote_status[i].last_frame >= _peer_connect_status[i].last_frame);
			_peer_connect_status[i].disconnected = _peer_connect_status[i].disconnected || remote_status[i].disconnected;
			_peer_connect_status[i].last_frame = MAX(_peer_connect_status[i].last_frame, remote_status[i].last_frame);
		 }
	  }

	  /*
	   * Decompress the input.
	   */
	  int last_received_frame_number = _last_received_input.frame;
	  if (msg.u.input.num_bits)
	  {
		 int offset = 0;
//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
//ORIGINAL LINE: byte *bits = (byte *)msg->u.input.bits;
		 byte bits = (byte)msg.u.input.bits;
		 int numBits = msg.u.input.num_bits;
		 int currentFrame = msg.u.input.start_frame;

		 _last_received_input.size = msg.u.input.input_size;
		 if (_last_received_input.frame < 0)
		 {
			_last_received_input.frame = msg.u.input.start_frame - 1;
		 }
		 while (offset < numBits)
		 {
			/*
			 * Keep walking through the frames (parsing bits) until we reach
			 * the inputs for the frame right after the one we're on.
			 */
			Debug.Assert(currentFrame <= (_last_received_input.frame + 1));
			bool useInputs = currentFrame == _last_received_input.frame + 1;

			while (BitVector_ReadBit(bits, offset))
			{
			   int on = BitVector_ReadBit(bits, offset);
			   int button = BitVector_ReadNibblet(bits, offset);
			   if (useInputs)
			   {
				  if (on != 0)
				  {
					 _last_received_input.set(button);
				  }
				  else
				  {
					 _last_received_input.clear(button);
				  }
			   }
			}
			Debug.Assert(offset <= numBits);

			/*
			 * Now if we want to use these inputs, go ahead and send them to
			 * the emulator.
			 */
			if (useInputs)
			{
			   /*
			    * Move forward 1 frame in the stream.
			    */
			   string desc = new string(new char[1024]);
			   Debug.Assert(currentFrame == _last_received_input.frame + 1);
			   _last_received_input.frame = currentFrame;

			   /*
			    * Send the event to the emualtor
			    */
			   UdpProtocol.Event evt = new UdpProtocol.Event(UdpProtocol.Event.Input);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: evt.u.input.input = _last_received_input;
			   evt.u.input.input.CopyFrom(_last_received_input);

			   _last_received_input.desc(desc, ARRAY_SIZE(desc));

			   _state.running.last_input_packet_recv_time = Platform.GetCurrentTimeMS();

			   Log("Sending frame %d to emu queue %d (%s).\n", _last_received_input.frame, _queue, desc);
			   QueueEvent(evt);

			}
			else
			{
			   Log("Skipping past frame:(%d) current is %d.\n", currentFrame, _last_received_input.frame);
			}

			/*
			 * Move forward 1 frame in the input stream.
			 */
			currentFrame++;
		 }
	  }
	  Debug.Assert(_last_received_input.frame >= last_received_frame_number);

	  /*
	   * Get rid of our buffered input
	   */
	  while (_pending_output.size() != 0 && _pending_output.front().frame < msg.u.input.ack_frame)
	  {
		 Log("Throwing away pending output frame %d\n", _pending_output.front().frame);
		 _last_acked_input = _pending_output.front();
		 _pending_output.pop();
	  }
	  return true;
   }

   protected bool OnInputAck(UdpMsg msg, int len)
   {
	  /*
	   * Get rid of our buffered input
	   */
	  while (_pending_output.size() != 0 && _pending_output.front().frame < msg.u.input_ack.ack_frame)
	  {
		 Log("Throwing away pending output frame %d\n", _pending_output.front().frame);
		 _last_acked_input = _pending_output.front();
		 _pending_output.pop();
	  }
	  return true;
   }

   protected bool OnQualityReport(UdpMsg msg, int len)
   {
	  // send a reply so the other side can compute the round trip transmit time.
	  UdpMsg reply = new UdpMsg(UdpMsg.MsgType.QualityReply);
	  reply.u.quality_reply.pong = msg.u.quality_report.ping;
	  SendMsg(reply);

	  _remote_frame_advantage = msg.u.quality_report.frame_advantage;
	  return true;
   }

   protected bool OnQualityReply(UdpMsg msg, int len)
   {
	  _round_trip_time = Platform.GetCurrentTimeMS() - msg.u.quality_reply.pong;
	  return true;
   }

   protected bool OnKeepAlive(UdpMsg msg, int len)
   {
	  return true;
   }

   /*
    * Network transmission information
    */
   protected Udp _udp;
   protected sockaddr_in _peer_addr = new sockaddr_in();
   protected ushort _magic_number;
   protected int _queue;
   protected ushort _remote_magic_number;
   protected bool _connected;
   protected int _send_latency;
   protected int _oop_percent;
//C++ TO C# CONVERTER NOTE: Classes must be named in C#, so the following class has been named by the converter:
   protected class AnonymousClass
   {
	  public int send_time;
	  public sockaddr_in dest_addr = new sockaddr_in();
	  public UdpMsg msg;
   }
   protected AnonymousClass _oo_packet = new AnonymousClass();
   protected RingBuffer<QueueEntry, 64> _send_queue = new RingBuffer<QueueEntry, 64>();

   /*
    * Stats
    */
   protected int _round_trip_time;
   protected int _packets_sent;
   protected int _bytes_sent;
   protected int _kbps_sent;
   protected int _stats_start_time;

   /*
    * The state machine
    */
   protected UdpMsg.connect_status[] _local_connect_status;
   protected UdpMsg.connect_status[] _peer_connect_status = Arrays.InitializeWithDefaultInstances<connect_status>(DefineConstants.UDP_MSG_MAX_PLAYERS);

   protected State _current_state;
//C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
//   union
//   {
//	  struct
//	  {
//		 uint roundtrips_remaining;
//		 uint random;
//	  }
//	  sync;
//	  struct
//	  {
//		 uint last_quality_report_time;
//		 uint last_network_stats_interval;
//		 uint last_input_packet_recv_time;
//	  }
//	  running;
//   }
//C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
//   _state;

   /*
    * Fairness.
    */
   protected int _local_frame_advantage;
   protected int _remote_frame_advantage;

   /*
    * Packet loss...
    */
   protected RingBuffer<GameInput, 64> _pending_output = new RingBuffer<GameInput, 64>();
   protected GameInput _last_received_input = new GameInput();
   protected GameInput _last_sent_input = new GameInput();
   protected GameInput _last_acked_input = new GameInput();
   protected uint _last_send_time;
   protected uint _last_recv_time;
   protected uint _shutdown_timeout;
   protected uint _disconnect_event_sent;
   protected uint _disconnect_timeout;
   protected uint _disconnect_notify_start;
   protected bool _disconnect_notify_sent;

   protected ushort _next_send_seq;
   protected ushort _next_recv_seq;

   /*
    * Rift synchronization.
    */
   protected TimeSync _timesync = new TimeSync();

   /*
    * Event queue
    */
   protected RingBuffer<UdpProtocol.Event, 64> _event_queue = new RingBuffer<UdpProtocol.Event, 64>();
}