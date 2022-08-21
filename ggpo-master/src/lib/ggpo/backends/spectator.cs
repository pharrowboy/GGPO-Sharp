using System.Diagnostics;

/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */

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
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
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


//C++ TO C# CONVERTER TODO TASK: Multiple inheritance is not available in C#:
public class SpectatorBackend : struct GGPOSession, IPollSink, Udp.Callbacks
{
   public SpectatorBackend(GGPOSessionCallbacks cb, string gamename, ushort localport, int num_players, int input_size, ref string hostip, u_short hostport)
   {
	   this._num_players = num_players;
	   this._input_size = input_size;
	   this._next_input_to_send = 0;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _callbacks = *cb;
	  _callbacks.CopyFrom(cb);
	  _synchronizing = true;

	  for (int i = 0; i < ARRAY_SIZE(_inputs); i++)
	  {
		 _inputs[i].frame = -1;
	  }

	  /*
	   * Initialize the UDP port
	   */
	  _udp.Init(localport, _poll, this);

	  /*
	   * Init the host endpoint
	   */
	  _host.Init(_udp, _poll, 0, ref hostip, new u_short(hostport), null);
	  _host.Synchronize();

	  /*
	   * Preload the ROM
	   */
	  _callbacks.begin_game(gamename);
   }

   public override void Dispose()
   {
	   base.Dispose();
   }


   public virtual GGPOErrorCode DoPoll(int timeout)
   {
	  _poll.Pump(0);

	  PollUdpProtocolEvents();
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode AddPlayer(GGPOPlayer player, GGPOPlayerHandle handle)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode AddLocalInput(GGPOPlayerHandle player, object values, int size)
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public virtual GGPOErrorCode SyncInput(object values, int size, ref int disconnect_flags)
   {
	  // Wait until we've started to return inputs.
	  if (_synchronizing)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_NOT_SYNCHRONIZED;
	  }

	  GameInput input = _inputs[_next_input_to_send % DefineConstants.SPECTATOR_FRAME_BUFFER_SIZE];
	  if (input.frame < _next_input_to_send)
	  {
		 // Haven't received the input from the host yet.  Wait
		 return GGPOErrorCode.GGPO_ERRORCODE_PREDICTION_THRESHOLD;
	  }
	  if (input.frame > _next_input_to_send)
	  {
		 // The host is way way way far ahead of the spectator.  How'd this
		 // happen?  Anyway, the input we need is gone forever.
		 return GGPOErrorCode.GGPO_ERRORCODE_GENERAL_FAILURE;
	  }

	  Debug.Assert(size >= _input_size * _num_players);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
	  memcpy(values, input.bits, _input_size * _num_players);
	  if (disconnect_flags != 0)
	  {
		 disconnect_flags = null; // xxx: should get them from the host!
	  }
	  _next_input_to_send++;

	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode IncrementFrame()
   {
	  Log("End of frame (%d)...\n", _next_input_to_send - 1);
	  DoPoll(0);
	  PollUdpProtocolEvents();

	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode DisconnectPlayer(GGPOPlayerHandle handle)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode GetNetworkStats(GGPONetworkStats stats, GGPOPlayerHandle handle)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode SetFrameDelay(GGPOPlayerHandle player, int delay)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode SetDisconnectTimeout(int timeout)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode SetDisconnectNotifyStart(int timeout)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }

   public virtual void OnMsg(sockaddr_in from, UdpMsg msg, int len)
   {
	  if (_host.HandlesMsg(from, msg))
	  {
		 _host.OnMsg(msg, len);
	  }
   }

   protected void PollUdpProtocolEvents()
   {
	  UdpProtocol.Event evt = new UdpProtocol.Event();
	  while (_host.GetEvent(evt))
	  {
		 OnUdpProtocolEvent(evt);
	  }
   }

//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//   void CheckInitialSync();

   protected void OnUdpProtocolEvent(UdpProtocol.Event evt)
   {
	  GGPOEvent info = new GGPOEvent();

	  switch (evt.type)
	  {
	  case UdpProtocol.Event.Connected:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTED_TO_PEER;
		 info.u.connected.player = 0;
		 _callbacks.on_event(info);
		 break;
	  case UdpProtocol.Event.Synchronizing:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZING_WITH_PEER;
		 info.u.synchronizing.player = 0;
		 info.u.synchronizing.count = evt.u.synchronizing.count;
		 info.u.synchronizing.total = evt.u.synchronizing.total;
		 _callbacks.on_event(info);
		 break;
	  case UdpProtocol.Event.Synchronzied:
		 if (_synchronizing)
		 {
			info.code = GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZED_WITH_PEER;
			info.u.synchronized.player = 0;
			_callbacks.on_event(info);

			info.code = GGPOEventCode.GGPO_EVENTCODE_RUNNING;
			_callbacks.on_event(info);
			_synchronizing = false;
		 }
		 break;

	  case UdpProtocol.Event.NetworkInterrupted:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTION_INTERRUPTED;
		 info.u.connection_interrupted.player = 0;
		 info.u.connection_interrupted.disconnect_timeout = evt.u.network_interrupted.disconnect_timeout;
		 _callbacks.on_event(info);
		 break;

	  case UdpProtocol.Event.NetworkResumed:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTION_RESUMED;
		 info.u.connection_resumed.player = 0;
		 _callbacks.on_event(info);
		 break;

	  case UdpProtocol.Event.Disconnected:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_DISCONNECTED_FROM_PEER;
		 info.u.disconnected.player = 0;
		 _callbacks.on_event(info);
		 break;

	  case UdpProtocol.Event.Input:
		 GameInput input = evt.u.input.input;

		 _host.SetLocalFrameNumber(input.frame);
		 _host.SendInputAck();
		 _inputs[input.frame % DefineConstants.SPECTATOR_FRAME_BUFFER_SIZE] = input;
		 break;
	  }
   }

   protected GGPOSessionCallbacks _callbacks = new GGPOSessionCallbacks();
   protected Poll _poll = new Poll();
   protected Udp _udp = new Udp();
   protected UdpProtocol _host = new UdpProtocol();
   protected bool _synchronizing;
   protected int _input_size;
   protected int _num_players;
   protected int _next_input_to_send;
   protected GameInput[] _inputs = Arrays.InitializeWithDefaultInstances<GameInput>(DefineConstants.SPECTATOR_FRAME_BUFFER_SIZE);
}

