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


//C++ TO C# CONVERTER NOTE: C# has no need of forward class declarations:
//class SyncTestBackend;

public class Sync : System.IDisposable
{
   public class Config
   {
	  public GGPOSessionCallbacks callbacks = new GGPOSessionCallbacks();
	  public int num_prediction_frames;
	  public int num_players;
	  public int input_size;
   }
   public class Event
   {
//C++ TO C# CONVERTER NOTE: Enums must be named in C#, so the following enum has been named by the converter:
	  public enum AnonymousEnum
	  {
		 ConfirmedInput
	  }
	  public AnonymousEnum type;
//C++ TO C# CONVERTER TODO TASK: Unions are not supported in C#:
//	  union
//	  {
//		 struct
//		 {
//			GameInput input;
//		 }
//		 confirmedInput;
//	  }
//C++ TO C# CONVERTER NOTE: Access declarations are not available in C#:
//	  u;
   }

   public Sync(UdpMsg.connect_status connect_status)
   {
	   this._local_connect_status = new UdpMsg.connect_status(connect_status);
	   this._input_queues = new InputQueue(null);
	  _framecount = 0;
	  _last_confirmed_frame = -1;
	  _max_prediction_frames = 0;
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_savedstate, 0, sizeof(Sync.SavedState));
   }

   public virtual void Dispose()
   {
	  /*
	   * Delete frames manually here rather than in a destructor of the SavedFrame
	   * structure so we can efficently copy frames via weak references.
	   */
	  for (int i = 0; i < ARRAY_SIZE(_savedstate.frames); i++)
	  {
		 _callbacks.free_buffer(_savedstate.frames[i].buf);
	  }
	  Arrays.DeleteArray(_input_queues);
	  _input_queues = null;
   }

   public void Init(Sync.Config config)
   {
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _config = config;
	  _config.CopyFrom(config);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _callbacks = config.callbacks;
	  _callbacks.CopyFrom(config.callbacks);
	  _framecount = 0;
	  _rollingback = false;

	  _max_prediction_frames = config.num_prediction_frames;

	  CreateQueues(config);
   }

   public void SetLastConfirmedFrame(int frame)
   {
	  _last_confirmed_frame = frame;
	  if (_last_confirmed_frame > 0)
	  {
		 for (int i = 0; i < _config.num_players; i++)
		 {
			_input_queues[i].DiscardConfirmedFrames(frame - 1);
		 }
	  }
   }

   public void SetFrameDelay(int queue, int delay)
   {
	  _input_queues[queue].SetFrameDelay(delay);
   }

   public bool AddLocalInput(int queue, GameInput input)
   {
	  int frames_behind = _framecount - _last_confirmed_frame;
	  if (_framecount >= _max_prediction_frames && frames_behind >= _max_prediction_frames)
	  {
		 Log("Rejecting input from emulator: reached prediction barrier.\n");
		 return false;
	  }

	  if (_framecount == 0)
	  {
		 SaveCurrentFrame();
	  }

	  Log("Sending undelayed local frame %d to queue %d.\n", _framecount, queue);
	  input.frame = _framecount;
	  _input_queues[queue].AddInput(input);

	  return true;
   }

   public void AddRemoteInput(int queue, GameInput input)
   {
	  _input_queues[queue].AddInput(input);
   }

   public int GetConfirmedInputs(object values, int size, int frame)
   {
	  int disconnect_flags = 0;
	  string output = (string)values;

	  Debug.Assert(size >= _config.num_players * _config.input_size);

//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(output, 0, size);
	  for (int i = 0; i < _config.num_players; i++)
	  {
		 GameInput input = new GameInput();
		 if (_local_connect_status[i].disconnected && frame > _local_connect_status[i].last_frame)
		 {
			disconnect_flags |= (1 << i);
			input.erase();
		 }
		 else
		 {
			_input_queues[i].GetConfirmedInput(frame, input);
		 }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
		 memcpy(output + (i * _config.input_size), input.bits, _config.input_size);
	  }
	  return disconnect_flags;
   }

   public int SynchronizeInputs(object values, int size)
   {
	  int disconnect_flags = 0;
	  string output = (string)values;

	  Debug.Assert(size >= _config.num_players * _config.input_size);

//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(output, 0, size);
	  for (int i = 0; i < _config.num_players; i++)
	  {
		 GameInput input = new GameInput();
		 if (_local_connect_status[i].disconnected && _framecount > _local_connect_status[i].last_frame)
		 {
			disconnect_flags |= (1 << i);
			input.erase();
		 }
		 else
		 {
			_input_queues[i].GetInput(_framecount, input);
		 }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
		 memcpy(output + (i * _config.input_size), input.bits, _config.input_size);
	  }
	  return disconnect_flags;
   }

   public void CheckSimulation(int timeout)
   {
	  int seek_to;
	  if (!CheckSimulationConsistency(ref seek_to))
	  {
		 AdjustSimulation(seek_to);
	  }
   }

   public void AdjustSimulation(int seek_to)
   {
	  int framecount = _framecount;
	  int count = _framecount - seek_to;

	  Log("Catching up\n");
	  _rollingback = true;

	  /*
	   * Flush our input queue and load the last frame.
	   */
	  LoadFrame(seek_to);
	  Debug.Assert(_framecount == seek_to);

	  /*
	   * Advance frame by frame (stuffing notifications back to
	   * the master).
	   */
	  ResetPrediction(_framecount);
	  for (int i = 0; i < count; i++)
	  {
		 _callbacks.advance_frame(0);
	  }
	  Debug.Assert(_framecount == framecount);

	  _rollingback = false;

	  Log("---\n");
   }

   public void IncrementFrame()
   {
	  _framecount++;
	  SaveCurrentFrame();
   }

   public int GetFrameCount()
   {
	   return _framecount;
   }
   public bool InRollback()
   {
	   return _rollingback;
   }

   public bool GetEvent(ref Event e)
   {
	  if (_event_queue.size() != 0)
	  {
		 e = _event_queue.front();
		 _event_queue.pop();
		 return true;
	  }
	  return false;
   }

//C++ TO C# CONVERTER TODO TASK: C# has no concept of a 'friend' class:
//   friend SyncTestBackend;

   protected class SavedFrame
   {
	  public byte[] buf;
	  public int cbuf;
	  public int frame;
	  public int checksum;
	  public SavedFrame()
	  {
		  this.buf = null;
		  this.cbuf = 0;
		  this.frame = -1;
		  this.checksum = 0;
	  }
   }
   protected class SavedState
   {
	  public SavedFrame[] frames = Arrays.InitializeWithDefaultInstances<SavedFrame>(DefineConstants.MAX_PREDICTION_FRAMES + 2);
	  public int head;
   }

   protected void LoadFrame(int frame)
   {
	  // find the frame in question
	  if (frame == _framecount)
	  {
		 Log("Skipping NOP.\n");
		 return;
	  }

	  // Move the head pointer back and load it up
	  _savedstate.head = FindSavedFrameIndex(frame);
	  SavedFrame state = _savedstate.frames + _savedstate.head;

	  Log("=== Loading frame info %d (size: %d  checksum: %08x).\n", state.frame, state.cbuf, state.checksum);

	  Debug.Assert(state.buf && state.cbuf);
	  _callbacks.load_game_state(state.buf, state.cbuf);

	  // Reset framecount and the head of the state ring-buffer to point in
	  // advance of the current frame (as if we had just finished executing it).
	  _framecount = state.frame;
	  _savedstate.head = (_savedstate.head + 1) % ARRAY_SIZE(_savedstate.frames);
   }

   protected void SaveCurrentFrame()
   {
	  /*
	   * See StateCompress for the real save feature implemented by FinalBurn.
	   * Write everything into the head, then advance the head pointer.
	   */
	  SavedFrame state = _savedstate.frames + _savedstate.head;
	  if (state.buf)
	  {
		 _callbacks.free_buffer(state.buf);
		 state.buf = null;
	  }
	  state.frame = _framecount;
	  _callbacks.save_game_state(state.buf, state.cbuf, state.checksum, state.frame);

	  Log("=== Saved frame info %d (size: %d  checksum: %08x).\n", state.frame, state.cbuf, state.checksum);
	  _savedstate.head = (_savedstate.head + 1) % ARRAY_SIZE(_savedstate.frames);
   }

   protected int FindSavedFrameIndex(int frame)
   {
	  int i;
	  int count = ARRAY_SIZE(_savedstate.frames);
	  for (i = 0; i < count; i++)
	  {
		 if (_savedstate.frames[i].frame == frame)
		 {
			break;
		 }
	  }
	  if (i == count)
	  {
		 Debug.Assert(false);
	  }
	  return i;
   }

   protected Sync.SavedFrame GetLastSavedFrame()
   {
	  int i = _savedstate.head - 1;
	  if (i < 0)
	  {
		 i = ARRAY_SIZE(_savedstate.frames) - 1;
	  }
//C++ TO C# CONVERTER TODO TASK: The following line was determined to contain a copy constructor call - this should be verified and a copy constructor should be created:
//ORIGINAL LINE: return _savedstate.frames[i];
	  return new Sync.SavedFrame(_savedstate.frames[i]);
   }

   protected bool CreateQueues(Config config)
   {
	  Arrays.DeleteArray(_input_queues);
	  _input_queues = Arrays.InitializeWithDefaultInstances<InputQueue>(_config.num_players);

	  for (int i = 0; i < _config.num_players; i++)
	  {
		 _input_queues[i].Init(i, _config.input_size);
	  }
	  return true;
   }

   protected bool CheckSimulationConsistency(ref int seekTo)
   {
	  int first_incorrect = (int)GameInput.Constants.NullFrame;
	  for (int i = 0; i < _config.num_players; i++)
	  {
		 int incorrect = _input_queues[i].GetFirstIncorrectFrame();
		 Log("considering incorrect frame %d reported by queue %d.\n", incorrect, i);

		 if (incorrect != (int)GameInput.Constants.NullFrame && (first_incorrect == (int)GameInput.Constants.NullFrame || incorrect < first_incorrect))
		 {
			first_incorrect = incorrect;
		 }
	  }

	  if (first_incorrect == (int)GameInput.Constants.NullFrame)
	  {
		 Log("prediction ok.  proceeding.\n");
		 return true;
	  }
	  seekTo = first_incorrect;
	  return false;
   }

   protected void ResetPrediction(int frameNumber)
   {
	  for (int i = 0; i < _config.num_players; i++)
	  {
		 _input_queues[i].ResetPrediction(frameNumber);
	  }
   }

   protected GGPOSessionCallbacks _callbacks = new GGPOSessionCallbacks();
   protected SavedState _savedstate = new SavedState();
   protected Config _config = new Config();

   protected bool _rollingback;
   protected int _last_confirmed_frame;
   protected int _framecount;
   protected int _max_prediction_frames;

   protected InputQueue[] _input_queues;

   protected RingBuffer<Event, 32> _event_queue = new RingBuffer<Event, 32>();
   protected UdpMsg.connect_status[] _local_connect_status;
}


