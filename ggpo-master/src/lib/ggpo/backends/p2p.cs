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
public class Peer2PeerBackend : struct GGPOSession, IPollSink, Udp.Callbacks
{
   public Peer2PeerBackend(GGPOSessionCallbacks cb, string gamename, ushort localport, int num_players, int input_size)
   {
	   this._num_players = num_players;
	   this._input_size = input_size;
	   this._sync = _local_connect_status;
	   this._disconnect_timeout = Globals.DEFAULT_DISCONNECT_TIMEOUT;
	   this._disconnect_notify_start = Globals.DEFAULT_DISCONNECT_NOTIFY_START;
	   this._num_spectators = 0;
	   this._next_spectator_frame = 0;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _callbacks = *cb;
	  _callbacks.CopyFrom(cb);
	  _synchronizing = true;
	  _next_recommended_sleep = 0;

	  /*
	   * Initialize the synchronziation layer
	   */
	  Sync.Config config = new Sync.Config();
	  config.num_players = num_players;
	  config.input_size = input_size;
	  config.callbacks = _callbacks;
	  config.num_prediction_frames = MAX_PREDICTION_FRAMES;
	  _sync.Init(config);

	  /*
	   * Initialize the UDP port
	   */
	  _udp.Init(localport, _poll, this);

	  _endpoints = Arrays.InitializeWithDefaultInstances<UdpProtocol>(_num_players);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_local_connect_status, 0, sizeof(UdpMsg.connect_status));
	  for (int i = 0; i < ARRAY_SIZE(_local_connect_status); i++)
	  {
		 _local_connect_status[i].last_frame = -1;
	  }

	  /*
	   * Preload the ROM
	   */
	  _callbacks.begin_game(gamename);
   }

   public override void Dispose()
   {
	  Arrays.DeleteArray(_endpoints);
	   base.Dispose();
   }


   public virtual GGPOErrorCode DoPoll(int timeout)
   {
	  if (!_sync.InRollback())
	  {
		 _poll.Pump(0);

		 PollUdpProtocolEvents();

		 if (!_synchronizing)
		 {
			_sync.CheckSimulation(timeout);

			// notify all of our endpoints of their local frame number for their
			// next connection quality report
			int current_frame = _sync.GetFrameCount();
			for (int i = 0; i < _num_players; i++)
			{
			   _endpoints[i].SetLocalFrameNumber(current_frame);
			}

			int total_min_confirmed;
			if (_num_players <= 2)
			{
			   total_min_confirmed = Poll2Players(current_frame);
			}
			else
			{
			   total_min_confirmed = PollNPlayers(current_frame);
			}

			Log("last confirmed frame in p2p backend is %d.\n", total_min_confirmed);
			if (total_min_confirmed >= 0)
			{
			   Debug.Assert(total_min_confirmed != int.MaxValue);
			   if (_num_spectators > 0)
			   {
				  while (_next_spectator_frame <= total_min_confirmed)
				  {
					 Log("pushing frame %d to spectators.\n", _next_spectator_frame);

					 GameInput input = new GameInput();
					 input.frame = _next_spectator_frame;
					 input.size = _input_size * _num_players;
					 _sync.GetConfirmedInputs(input.bits, _input_size * _num_players, _next_spectator_frame);
					 for (int i = 0; i < _num_spectators; i++)
					 {
						_spectators[i].SendInput(input);
					 }
					 _next_spectator_frame++;
				  }
			   }
			   Log("setting confirmed frame in sync to %d.\n", total_min_confirmed);
			   _sync.SetLastConfirmedFrame(total_min_confirmed);
			}

			// send timesync notifications if now is the proper time
			if (current_frame > _next_recommended_sleep)
			{
			   int interval = 0;
			   for (int i = 0; i < _num_players; i++)
			   {
				  interval = MAX(interval, _endpoints[i].RecommendFrameDelay());
			   }

			   if (interval > 0)
			   {
				  GGPOEvent info = new GGPOEvent();
				  info.code = GGPOEventCode.GGPO_EVENTCODE_TIMESYNC;
				  info.u.timesync.frames_ahead = interval;
				  _callbacks.on_event(info);
				  _next_recommended_sleep = (int)current_frame + Globals.RECOMMENDATION_INTERVAL;
			   }
			}
			// XXX: this is obviously a farce...
			if (timeout != 0)
			{
			   Sleep(1);
			}
		 }
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode AddPlayer(GGPOPlayer player, GGPOPlayerHandle handle)
   {
	  if (player.type == GGPOPlayerType.GGPO_PLAYERTYPE_SPECTATOR)
	  {
		 return AddSpectator(ref player.u.remote.ip_address, player.u.remote.port);
	  }

	  int queue = player.player_num - 1;
	  if (player.player_num < 1 || player.player_num > _num_players)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE;
	  }
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *handle = QueueToPlayerHandle(queue);
	  handle.CopyFrom(QueueToPlayerHandle(queue));

	  if (player.type == GGPOPlayerType.GGPO_PLAYERTYPE_REMOTE)
	  {
		 AddRemotePlayer(ref player.u.remote.ip_address, player.u.remote.port, queue);
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode AddLocalInput(GGPOPlayerHandle player, object values, int size)
   {
	  int queue;
	  GameInput input = new GameInput();
	  GGPOErrorCode result;

	  if (_sync.InRollback())
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_IN_ROLLBACK;
	  }
	  if (_synchronizing)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_NOT_SYNCHRONIZED;
	  }

	  result = PlayerHandleToQueue(new GGPOPlayerHandle(player), ref queue);
	  if (!((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
	  {
		 return result;
	  }

	  input.init(-1, (string)values, size);

	  // Feed the input for the current frame into the synchronzation layer.
	  if (!_sync.AddLocalInput(queue, input))
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_PREDICTION_THRESHOLD;
	  }

	  if (input.frame != GameInput.NullFrame)
	  { // xxx: <- comment why this is the case
		 // Update the local connect status state to indicate that we've got a
		 // confirmed local frame for this player.  this must come first so it
		 // gets incorporated into the next packet we send.

		 Log("setting local connect status for local queue %d to %d", queue, input.frame);
		 _local_connect_status[queue].last_frame = input.frame;

		 // Send the input to all the remote players.
		 for (int i = 0; i < _num_players; i++)
		 {
			if (_endpoints[i].IsInitialized())
			{
			   _endpoints[i].SendInput(input);
			}
		 }
	  }

	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SyncInput(object values, int size, ref int disconnect_flags)
   {
	  int flags;

	  // Wait until we've started to return inputs.
	  if (_synchronizing)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_NOT_SYNCHRONIZED;
	  }
	  flags = _sync.SynchronizeInputs(values, size);
	  if (disconnect_flags != 0)
	  {
		 disconnect_flags = flags;
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode IncrementFrame()
   {
	  Log("End of frame (%d)...\n", _sync.GetFrameCount());
	  _sync.IncrementFrame();
	  DoPoll(0);
	  PollSyncEvents();

	  return GGPOErrorCode.GGPO_OK;
   }


   /*
    * Called only as the result of a local decision to disconnect.  The remote
    * decisions to disconnect are a result of us parsing the peer_connect_settings
    * blob in every endpoint periodically.
    */
   public virtual GGPOErrorCode DisconnectPlayer(GGPOPlayerHandle player)
   {
	  int queue;
	  GGPOErrorCode result;

	  result = PlayerHandleToQueue(new GGPOPlayerHandle(player), ref queue);
	  if (!((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
	  {
		 return result;
	  }

	  if (_local_connect_status[queue].disconnected)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_PLAYER_DISCONNECTED;
	  }

	  if (!_endpoints[queue].IsInitialized())
	  {
		 int current_frame = _sync.GetFrameCount();
		 // xxx: we should be tracking who the local player is, but for now assume
		 // that if the endpoint is not initalized, this must be the local player.
		 Log("Disconnecting local player %d at frame %d by user request.\n", queue, _local_connect_status[queue].last_frame);
		 for (int i = 0; i < _num_players; i++)
		 {
			if (_endpoints[i].IsInitialized())
			{
			   DisconnectPlayerQueue(i, current_frame);
			}
		 }
	  }
	  else
	  {
		 Log("Disconnecting queue %d at frame %d by user request.\n", queue, _local_connect_status[queue].last_frame);
		 DisconnectPlayerQueue(queue, _local_connect_status[queue].last_frame);
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode GetNetworkStats(GGPONetworkStats stats, GGPOPlayerHandle player)
   {
	  int queue;
	  GGPOErrorCode result;

	  result = PlayerHandleToQueue(new GGPOPlayerHandle(player), ref queue);
	  if (!((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
	  {
		 return result;
	  }

//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(stats, 0, sizeof * stats);
	  _endpoints[queue].GetNetworkStats(stats);

	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SetFrameDelay(GGPOPlayerHandle player, int delay)
   {
	  int queue;
	  GGPOErrorCode result;

	  result = PlayerHandleToQueue(new GGPOPlayerHandle(player), ref queue);
	  if (!((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
	  {
		 return result;
	  }
	  _sync.SetFrameDelay(queue, delay);
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SetDisconnectTimeout(int timeout)
   {
	  _disconnect_timeout = timeout;
	  for (int i = 0; i < _num_players; i++)
	  {
		 if (_endpoints[i].IsInitialized())
		 {
			_endpoints[i].SetDisconnectTimeout(_disconnect_timeout);
		 }
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SetDisconnectNotifyStart(int timeout)
   {
	  _disconnect_notify_start = timeout;
	  for (int i = 0; i < _num_players; i++)
	  {
		 if (_endpoints[i].IsInitialized())
		 {
			_endpoints[i].SetDisconnectNotifyStart(_disconnect_notify_start);
		 }
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual void OnMsg(sockaddr_in from, UdpMsg msg, int len)
   {
	  for (int i = 0; i < _num_players; i++)
	  {
		 if (_endpoints[i].HandlesMsg(from, msg))
		 {
			_endpoints[i].OnMsg(msg, len);
			return;
		 }
	  }
	  for (int i = 0; i < _num_spectators; i++)
	  {
		 if (_spectators[i].HandlesMsg(from, msg))
		 {
			_spectators[i].OnMsg(msg, len);
			return;
		 }
	  }
   }

   protected GGPOErrorCode PlayerHandleToQueue(GGPOPlayerHandle player, ref int queue)
   {
	  int offset = ((int)player - 1);
	  if (offset < 0 || offset >= _num_players)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_INVALID_PLAYER_HANDLE;
	  }
	  queue = offset;
	  return GGPOErrorCode.GGPO_OK;
   }

   protected GGPOPlayerHandle QueueToPlayerHandle(int queue)
   {
	   return (GGPOPlayerHandle)(queue + 1);
   }
   protected GGPOPlayerHandle QueueToSpectatorHandle(int queue)
   {
	   return (GGPOPlayerHandle)(queue + 1000);
   } // out of range of the player array, basically
   protected void DisconnectPlayerQueue(int queue, int syncto)
   {
	  GGPOEvent info = new GGPOEvent();
	  int framecount = _sync.GetFrameCount();

	  _endpoints[queue].Disconnect();

	  Log("Changing queue %d local connect status for last frame from %d to %d on disconnect request (current: %d).\n", queue, _local_connect_status[queue].last_frame, syncto, framecount);

	  _local_connect_status[queue].disconnected = 1;
	  _local_connect_status[queue].last_frame = syncto;

	  if (syncto < framecount)
	  {
		 Log("adjusting simulation to account for the fact that %d disconnected @ %d.\n", queue, syncto);
		 _sync.AdjustSimulation(syncto);
		 Log("finished adjusting simulation.\n");
	  }

	  info.code = GGPOEventCode.GGPO_EVENTCODE_DISCONNECTED_FROM_PEER;
	  info.u.disconnected.player = QueueToPlayerHandle(queue);
	  _callbacks.on_event(info);

	  CheckInitialSync();
   }

   protected void PollSyncEvents()
   {
	  Sync.Event e = new Sync.Event();
	  while (_sync.GetEvent(e))
	  {
		 OnSyncEvent(e);
	  }
	  return;
   }

   protected void PollUdpProtocolEvents()
   {
	  UdpProtocol.Event evt = new UdpProtocol.Event();
	  for (int i = 0; i < _num_players; i++)
	  {
		 while (_endpoints[i].GetEvent(evt))
		 {
			OnUdpProtocolPeerEvent(evt, i);
		 }
	  }
	  for (int i = 0; i < _num_spectators; i++)
	  {
		 while (_spectators[i].GetEvent(evt))
		 {
			OnUdpProtocolSpectatorEvent(evt, i);
		 }
	  }
   }

   protected void CheckInitialSync()
   {
	  int i;

	  if (_synchronizing)
	  {
		 // Check to see if everyone is now synchronized.  If so,
		 // go ahead and tell the client that we're ok to accept input.
		 for (i = 0; i < _num_players; i++)
		 {
			// xxx: IsInitialized() must go... we're actually using it as a proxy for "represents the local player"
			if (_endpoints[i].IsInitialized() && !_endpoints[i].IsSynchronized() && !_local_connect_status[i].disconnected)
			{
			   return;
			}
		 }
		 for (i = 0; i < _num_spectators; i++)
		 {
			if (_spectators[i].IsInitialized() && !_spectators[i].IsSynchronized())
			{
			   return;
			}
		 }

		 GGPOEvent info = new GGPOEvent();
		 info.code = GGPOEventCode.GGPO_EVENTCODE_RUNNING;
		 _callbacks.on_event(info);
		 _synchronizing = false;
	  }
   }

   protected int Poll2Players(int current_frame)
   {
	  int i;

	  // discard confirmed frames as appropriate
	  int total_min_confirmed = MAX_INT;
	  for (i = 0; i < _num_players; i++)
	  {
		 bool queue_connected = true;
		 if (_endpoints[i].IsRunning())
		 {
			int ignore;
			queue_connected = _endpoints[i].GetPeerConnectStatus(i, ref ignore);
		 }
		 if (!_local_connect_status[i].disconnected)
		 {
			total_min_confirmed = MIN(_local_connect_status[i].last_frame, total_min_confirmed);
		 }
		 Log("  local endp: connected = %d, last_received = %d, total_min_confirmed = %d.\n", !_local_connect_status[i].disconnected, _local_connect_status[i].last_frame, total_min_confirmed);
		 if (!queue_connected && !_local_connect_status[i].disconnected)
		 {
			Log("disconnecting i %d by remote request.\n", i);
			DisconnectPlayerQueue(i, total_min_confirmed);
		 }
		 Log("  total_min_confirmed = %d.\n", total_min_confirmed);
	  }
	  return total_min_confirmed;
   }

   protected int PollNPlayers(int current_frame)
   {
	  int i;
	  int queue;
	  int last_received;

	  // discard confirmed frames as appropriate
	  int total_min_confirmed = MAX_INT;
	  for (queue = 0; queue < _num_players; queue++)
	  {
		 bool queue_connected = true;
		 int queue_min_confirmed = MAX_INT;
		 Log("considering queue %d.\n", queue);
		 for (i = 0; i < _num_players; i++)
		 {
			// we're going to do a lot of logic here in consideration of endpoint i.
			// keep accumulating the minimum confirmed point for all n*n packets and
			// throw away the rest.
			if (_endpoints[i].IsRunning())
			{
			   bool connected = _endpoints[i].GetPeerConnectStatus(queue, ref last_received);

			   queue_connected = queue_connected && connected;
			   queue_min_confirmed = MIN(last_received, queue_min_confirmed);
			   Log("  endpoint %d: connected = %d, last_received = %d, queue_min_confirmed = %d.\n", i, connected, last_received, queue_min_confirmed);
			}
			else
			{
			   Log("  endpoint %d: ignoring... not running.\n", i);
			}
		 }
		 // merge in our local status only if we're still connected!
		 if (!_local_connect_status[queue].disconnected)
		 {
			queue_min_confirmed = MIN(_local_connect_status[queue].last_frame, queue_min_confirmed);
		 }
		 Log("  local endp: connected = %d, last_received = %d, queue_min_confirmed = %d.\n", !_local_connect_status[queue].disconnected, _local_connect_status[queue].last_frame, queue_min_confirmed);

		 if (queue_connected)
		 {
			total_min_confirmed = MIN(queue_min_confirmed, total_min_confirmed);
		 }
		 else
		 {
			// check to see if this disconnect notification is further back than we've been before.  If
			// so, we need to re-adjust.  This can happen when we detect our own disconnect at frame n
			// and later receive a disconnect notification for frame n-1.
			if (!_local_connect_status[queue].disconnected || _local_connect_status[queue].last_frame > queue_min_confirmed)
			{
			   Log("disconnecting queue %d by remote request.\n", queue);
			   DisconnectPlayerQueue(queue, queue_min_confirmed);
			}
		 }
		 Log("  total_min_confirmed = %d.\n", total_min_confirmed);
	  }
	  return total_min_confirmed;
   }

   protected void AddRemotePlayer(ref string ip, ushort port, int queue)
   {
	  /*
	   * Start the state machine (xxx: no)
	   */
	  _synchronizing = true;

	  _endpoints[queue].Init(_udp, _poll, queue, ref ip, port, _local_connect_status);
	  _endpoints[queue].SetDisconnectTimeout(_disconnect_timeout);
	  _endpoints[queue].SetDisconnectNotifyStart(_disconnect_notify_start);
	  _endpoints[queue].Synchronize();
   }

   protected GGPOErrorCode AddSpectator(ref string ip, ushort port)
   {
	  if (_num_spectators == DefineConstants.GGPO_MAX_SPECTATORS)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_TOO_MANY_SPECTATORS;
	  }
	  /*
	   * Currently, we can only add spectators before the game starts.
	   */
	  if (!_synchronizing)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_INVALID_REQUEST;
	  }
	  int queue = _num_spectators++;

	  _spectators[queue].Init(_udp, _poll, queue + 1000, ref ip, port, _local_connect_status);
	  _spectators[queue].SetDisconnectTimeout(_disconnect_timeout);
	  _spectators[queue].SetDisconnectNotifyStart(_disconnect_notify_start);
	  _spectators[queue].Synchronize();

	  return GGPOErrorCode.GGPO_OK;
   }

   protected virtual void OnSyncEvent(Sync.Event e)
   {
   }
   protected virtual void OnUdpProtocolEvent(UdpProtocol.Event evt, GGPOPlayerHandle handle)
   {
	  GGPOEvent info = new GGPOEvent();

	  switch (evt.type)
	  {
	  case UdpProtocol.Event.Connected:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTED_TO_PEER;
		 info.u.connected.player = handle;
		 _callbacks.on_event(info);
		 break;
	  case UdpProtocol.Event.Synchronizing:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZING_WITH_PEER;
		 info.u.synchronizing.player = handle;
		 info.u.synchronizing.count = evt.u.synchronizing.count;
		 info.u.synchronizing.total = evt.u.synchronizing.total;
		 _callbacks.on_event(info);
		 break;
	  case UdpProtocol.Event.Synchronzied:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZED_WITH_PEER;
		 info.u.synchronized.player = handle;
		 _callbacks.on_event(info);

		 CheckInitialSync();
		 break;

	  case UdpProtocol.Event.NetworkInterrupted:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTION_INTERRUPTED;
		 info.u.connection_interrupted.player = handle;
		 info.u.connection_interrupted.disconnect_timeout = evt.u.network_interrupted.disconnect_timeout;
		 _callbacks.on_event(info);
		 break;

	  case UdpProtocol.Event.NetworkResumed:
		 info.code = GGPOEventCode.GGPO_EVENTCODE_CONNECTION_RESUMED;
		 info.u.connection_resumed.player = handle;
		 _callbacks.on_event(info);
		 break;
	  }
   }

   protected virtual void OnUdpProtocolPeerEvent(UdpProtocol.Event evt, int queue)
   {
	  OnUdpProtocolEvent(evt, QueueToPlayerHandle(queue));
	  switch (evt.type)
	  {
		 case UdpProtocol.Event.Input:
			if (!_local_connect_status[queue].disconnected)
			{
			   int current_remote_frame = _local_connect_status[queue].last_frame;
			   int new_remote_frame = evt.u.input.input.frame;
			   Debug.Assert(current_remote_frame == -1 || new_remote_frame == (current_remote_frame + 1));

			   _sync.AddRemoteInput(queue, evt.u.input.input);
			   // Notify the other endpoints which frame we received from a peer
			   Log("setting remote connect status for queue %d to %d\n", queue, evt.u.input.input.frame);
			   _local_connect_status[queue].last_frame = evt.u.input.input.frame;
			}
			break;

	  case UdpProtocol.Event.Disconnected:
		 DisconnectPlayer(QueueToPlayerHandle(queue));
		 break;
	  }
   }

   protected virtual void OnUdpProtocolSpectatorEvent(UdpProtocol.Event evt, int queue)
   {
	  GGPOPlayerHandle handle = QueueToSpectatorHandle(queue);
	  OnUdpProtocolEvent(evt, new GGPOPlayerHandle(handle));

	  GGPOEvent info = new GGPOEvent();

	  switch (evt.type)
	  {
	  case UdpProtocol.Event.Disconnected:
		 _spectators[queue].Disconnect();

		 info.code = GGPOEventCode.GGPO_EVENTCODE_DISCONNECTED_FROM_PEER;
		 info.u.disconnected.player = handle;
		 _callbacks.on_event(info);

		 break;
	  }
   }

   protected GGPOSessionCallbacks _callbacks = new GGPOSessionCallbacks();
   protected Poll _poll = new Poll();
   protected Sync _sync = new Sync();
   protected Udp _udp = new Udp();
   protected UdpProtocol[] _endpoints;
   protected UdpProtocol[] _spectators = Arrays.InitializeWithDefaultInstances<UdpProtocol>(DefineConstants.GGPO_MAX_SPECTATORS);
   protected int _num_spectators;
   protected int _input_size;

   protected bool _synchronizing;
   protected int _num_players;
   protected int _next_recommended_sleep;

   protected int _next_spectator_frame;
   protected int _disconnect_timeout;
   protected int _disconnect_notify_start;

   protected UdpMsg.connect_status[] _local_connect_status = Arrays.InitializeWithDefaultInstances<connect_status>(DefineConstants.UDP_MSG_MAX_PLAYERS);
}