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

public class SyncTestBackend : struct GGPOSession, System.IDisposable
{
   public SyncTestBackend(GGPOSessionCallbacks cb, ref string gamename, int frames, int num_players)
   {
	   this._sync = null;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _callbacks = *cb;
	  _callbacks.CopyFrom(cb);
	  _num_players = num_players;
	  _check_distance = frames;
	  _last_verified = 0;
	  _rollingback = false;
	  _running = false;
	  _logfp = null;
	  _current_input.erase();
	  strcpy_s(_game, gamename);

	  /*
	   * Initialize the synchronziation layer
	   */
	  Sync.Config config = new Sync.Config();
	  config.callbacks = _callbacks;
	  config.num_prediction_frames = MAX_PREDICTION_FRAMES;
	  _sync.Init(config);

	  /*
	   * Preload the ROM
	   */
	  _callbacks.begin_game(gamename);
   }

   public virtual void Dispose()
   {
   }

   public virtual GGPOErrorCode DoPoll(int timeout)
   {
	  if (!_running)
	  {
		 GGPOEvent info = new GGPOEvent();

		 info.code = GGPOEventCode.GGPO_EVENTCODE_RUNNING;
		 _callbacks.on_event(info);
		 _running = true;
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode AddPlayer(GGPOPlayer player, GGPOPlayerHandle handle)
   {
	  if (player.player_num < 1 || player.player_num > _num_players)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE;
	  }
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *handle = (GGPOPlayerHandle)(player->player_num - 1);
	  handle.CopyFrom((GGPOPlayerHandle)(player.player_num - 1));
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode AddLocalInput(GGPOPlayerHandle player, object values, int size)
   {
	  if (!_running)
	  {
		 return GGPOErrorCode.GGPO_ERRORCODE_NOT_SYNCHRONIZED;
	  }

	  int index = (int)player;
	  for (int i = 0; i < size; i++)
	  {
		 _current_input.bits[(index * size) + i] |= ((string)values)[i];
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SyncInput(object values, int size, ref int disconnect_flags)
   {
	  BeginLog(false ? 1 : 0);
	  if (_rollingback)
	  {
		 _last_input = _saved_frames.front().input;
	  }
	  else
	  {
		 if (_sync.GetFrameCount() == 0)
		 {
			_sync.SaveCurrentFrame();
		 }
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _last_input = _current_input;
		 _last_input.CopyFrom(_current_input);
	  }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
	  memcpy(values, _last_input.bits, size);
	  if (disconnect_flags != 0)
	  {
		 disconnect_flags = null;
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode IncrementFrame()
   {
	  _sync.IncrementFrame();
	  _current_input.erase();

	  Log("End of frame(%d)...\n", _sync.GetFrameCount());
	  EndLog();

	  if (_rollingback)
	  {
		 return GGPOErrorCode.GGPO_OK;
	  }

	  int frame = _sync.GetFrameCount();
	  // Hold onto the current frame in our queue of saved states.  We'll need
	  // the checksum later to verify that our replay of the same frame got the
	  // same results.
	  SavedInfo info = new SavedInfo();
	  info.frame = frame;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: info.input = _last_input;
	  info.input.CopyFrom(_last_input);
	  info.cbuf = _sync.GetLastSavedFrame().cbuf;
//C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
	  info.buf = (string)malloc(info.cbuf);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
	  memcpy(info.buf, _sync.GetLastSavedFrame().buf, info.cbuf);
	  info.checksum = _sync.GetLastSavedFrame().checksum;
	  _saved_frames.push(info);

	  if (frame - _last_verified == _check_distance)
	  {
		 // We've gone far enough ahead and should now start replaying frames.
		 // Load the last verified frame and set the rollback flag to true.
		 _sync.LoadFrame(_last_verified);

		 _rollingback = true;
		 while (!_saved_frames.empty())
		 {
			_callbacks.advance_frame(0);

			// Verify that the checksumn of this frame is the same as the one in our
			// list.
			info = _saved_frames.front();
			_saved_frames.pop();

			if (info.frame != _sync.GetFrameCount())
			{
			   RaiseSyncError("Frame number %d does not match saved frame number %d", info.frame, frame);
			}
			int checksum = _sync.GetLastSavedFrame().checksum;
			if (info.checksum != checksum)
			{
			   LogSaveStates(info);
			   RaiseSyncError("Checksum for frame %d does not match saved (%d != %d)", frame, checksum, info.checksum);
			}
			printf("Checksum %08d for frame %d matches.\n", checksum, info.frame);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
			free(info.buf);
		 }
		 _last_verified = frame;
		 _rollingback = false;
	  }

	  return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode Logv(ref string fmt, va_list list)
   {
	  if (_logfp != null)
	  {
		 vfprintf(_logfp, fmt, list);
	  }
	  return GGPOErrorCode.GGPO_OK;
   }

   protected class SavedInfo
   {
	  public int frame;
	  public int checksum;
	  public string buf;
	  public int cbuf;
	  public GameInput input = new GameInput();
   }

   protected void RaiseSyncError(string fmt, params object[] LegacyParamArray)
   {
	  string buf = new string(new char[1024]);
//	  va_list args;
	  int ParamCount = -1;
//	  va_start(args, fmt);
	  vsprintf_s(buf, ARRAY_SIZE(buf), fmt, args);
//	  va_end(args);

	  puts(buf);
	  OutputDebugStringA(buf);
	  EndLog();
	  DebugBreak();
   }

   protected void BeginLog(int saving)
   {
	  EndLog();

	  string filename = new string(new char[MAX_PATH]);
	  CreateDirectoryA("synclogs", null);
	  sprintf_s(filename, ARRAY_SIZE(filename), "synclogs\\%s-%04d-%s.log", saving != 0 ? "state" : "log", _sync.GetFrameCount(), _rollingback ? "replay" : "original");

	   fopen_s(_logfp, filename, "w");
   }

   protected void EndLog()
   {
	  if (_logfp != null)
	  {
		 fprintf(_logfp, "Closing log file.\n");
		 fclose(_logfp);
		 _logfp = null;
	  }
   }

   protected void LogSaveStates(SavedInfo info)
   {
	  string filename = new string(new char[MAX_PATH]);
	  sprintf_s(filename, ARRAY_SIZE(filename), "synclogs\\state-%04d-original.log", _sync.GetFrameCount());
	  _callbacks.log_game_state(filename, (byte)info.buf, info.cbuf);

	  sprintf_s(filename, ARRAY_SIZE(filename), "synclogs\\state-%04d-replay.log", _sync.GetFrameCount());
	  _callbacks.log_game_state(filename, _sync.GetLastSavedFrame().buf, _sync.GetLastSavedFrame().cbuf);
   }

   protected GGPOSessionCallbacks _callbacks = new GGPOSessionCallbacks();
   protected Sync _sync = new Sync();
   protected int _num_players;
   protected int _check_distance;
   protected int _last_verified;
   protected bool _rollingback;
   protected bool _running;
   protected FILE _logfp;
   protected string _game = new string(new char[128]);

   protected GameInput _current_input = new GameInput();
   protected GameInput _last_input = new GameInput();
   protected RingBuffer<SavedInfo, 32> _saved_frames = new RingBuffer<SavedInfo, 32>();
}
