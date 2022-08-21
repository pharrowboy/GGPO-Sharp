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




public class InputQueue : System.IDisposable
{
   public InputQueue(int input_size = DefineConstants.DEFAULT_INPUT_SIZE)
   {
	  Init(-1, input_size);
   }

   public void Dispose()
   {
   }

   public void Init(int id, int input_size)
   {
	  _id = id;
	  _head = 0;
	  _tail = 0;
	  _length = 0;
	  _frame_delay = 0;
	  _first_frame = true;
	  _last_user_added_frame = (int)GameInput.Constants.NullFrame;
	  _first_incorrect_frame = (int)GameInput.Constants.NullFrame;
	  _last_frame_requested = (int)GameInput.Constants.NullFrame;
	  _last_added_frame = (int)GameInput.Constants.NullFrame;

	  _prediction.init((int)GameInput.Constants.NullFrame, null, input_size);

	  /*
	   * This is safe because we know the GameInput is a proper structure (as in,
	   * no virtual methods, no contained classes, etc.).
	   */
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_inputs, 0, sizeof (GameInput));
	  for (int i = 0; i < ARRAY_SIZE(_inputs); i++)
	  {
		 _inputs[i].size = input_size;
	  }
   }

   public int GetLastConfirmedFrame()
   {
	  Log("returning last confirmed frame %d.\n", _last_added_frame);
	  return _last_added_frame;
   }

   public int GetFirstIncorrectFrame()
   {
	  return _first_incorrect_frame;
   }

   public int GetLength()
   {
	   return _length;
   }

   public void SetFrameDelay(int delay)
   {
	   _frame_delay = delay;
   }
   public void ResetPrediction(int frame)
   {
	  Debug.Assert(_first_incorrect_frame == (int)GameInput.Constants.NullFrame || frame <= _first_incorrect_frame);

	  Log("resetting all prediction errors back to frame %d.\n", frame);

	  /*
	   * There's nothing really to do other than reset our prediction
	   * state and the incorrect frame counter...
	   */
	  _prediction.frame = (int)GameInput.Constants.NullFrame;
	  _first_incorrect_frame = (int)GameInput.Constants.NullFrame;
	  _last_frame_requested = (int)GameInput.Constants.NullFrame;
   }

   public void DiscardConfirmedFrames(int frame)
   {
	  Debug.Assert(frame >= 0);

	  if (_last_frame_requested != (int)GameInput.Constants.NullFrame)
	  {
		 frame = MIN(frame, _last_frame_requested);
	  }

	  Log("discarding confirmed frames up to %d (last_added:%d length:%d [head:%d tail:%d]).\n", frame, _last_added_frame, _length, _head, _tail);
	  if (frame >= _last_added_frame)
	  {
		 _tail = _head;
	  }
	  else
	  {
		 int offset = frame - _inputs[_tail].frame + 1;

		 Log("difference of %d frames.\n", offset);
		 Debug.Assert(offset >= 0);

		 _tail = (_tail + offset) % DefineConstants.INPUT_QUEUE_LENGTH;
		 _length -= offset;
	  }

	  Log("after discarding, new tail is %d (frame:%d).\n", _tail, _inputs[_tail].frame);
	  Debug.Assert(_length >= 0);
   }

   public bool GetConfirmedInput(int requested_frame, GameInput input)
   {
	  Debug.Assert(_first_incorrect_frame == (int)GameInput.Constants.NullFrame || requested_frame < _first_incorrect_frame);
	  int offset = requested_frame % DefineConstants.INPUT_QUEUE_LENGTH;
	  if (_inputs[offset].frame != requested_frame)
	  {
		 return false;
	  }
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *input = _inputs[offset];
	  input.CopyFrom(_inputs[offset]);
	  return true;
   }

   public bool GetInput(int requested_frame, GameInput input)
   {
	  Log("requesting input frame %d.\n", requested_frame);

	  /*
	   * No one should ever try to grab any input when we have a prediction
	   * error.  Doing so means that we're just going further down the wrong
	   * path.  ASSERT this to verify that it's true.
	   */
	  Debug.Assert(_first_incorrect_frame == (int)GameInput.Constants.NullFrame);

	  /*
	   * Remember the last requested frame number for later.  We'll need
	   * this in AddInput() to drop out of prediction mode.
	   */
	  _last_frame_requested = requested_frame;

	  Debug.Assert(requested_frame >= _inputs[_tail].frame);

	  if (_prediction.frame == (int)GameInput.Constants.NullFrame)
	  {
		 /*
		  * If the frame requested is in our range, fetch it out of the queue and
		  * return it.
		  */
		 int offset = requested_frame - _inputs[_tail].frame;

		 if (offset < _length)
		 {
			offset = (offset + _tail) % DefineConstants.INPUT_QUEUE_LENGTH;
			Debug.Assert(_inputs[offset].frame == requested_frame);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *input = _inputs[offset];
			input.CopyFrom(_inputs[offset]);
			Log("returning confirmed frame number %d.\n", input.frame);
			return true;
		 }

		 /*
		  * The requested frame isn't in the queue.  Bummer.  This means we need
		  * to return a prediction frame.  Predict that the user will do the
		  * same thing they did last time.
		  */
		 if (requested_frame == 0)
		 {
			Log("basing new prediction frame from nothing, you're client wants frame 0.\n");
			_prediction.erase();
		 }
		 else if (_last_added_frame == (int)GameInput.Constants.NullFrame)
		 {
			Log("basing new prediction frame from nothing, since we have no frames yet.\n");
			_prediction.erase();
		 }
		 else
		 {
			Log("basing new prediction frame from previously added frame (queue entry:%d, frame:%d).\n", (((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1)), _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))].frame);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _prediction = _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))];
			_prediction.CopyFrom(_inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))]);
		 }
		 _prediction.frame++;
	  }

	  Debug.Assert(_prediction.frame >= 0);

	  /*
	   * If we've made it this far, we must be predicting.  Go ahead and
	   * forward the prediction frame contents.  Be sure to return the
	   * frame number requested by the client, though.
	   */
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *input = _prediction;
	  input.CopyFrom(_prediction);
	  input.frame = requested_frame;
	  Log("returning prediction frame number %d (%d).\n", input.frame, _prediction.frame);

	  return false;
   }

   public void AddInput(GameInput input)
   {
	  int new_frame;

	  Log("adding input frame number %d to queue.\n", input.frame);

	  /*
	   * These next two lines simply verify that inputs are passed in
	   * sequentially by the user, regardless of frame delay.
	   */
	  Debug.Assert(_last_user_added_frame == (int)GameInput.Constants.NullFrame || input.frame == _last_user_added_frame + 1);
	  _last_user_added_frame = input.frame;

	  /*
	   * Move the queue head to the correct point in preparation to
	   * input the frame into the queue.
	   */
	  new_frame = AdvanceQueueHead(input.frame);
	  if (new_frame != (int)GameInput.Constants.NullFrame)
	  {
		 AddDelayedInputToQueue(input, new_frame);
	  }

	  /*
	   * Update the frame number for the input.  This will also set the
	   * frame to GameInput::NullFrame for frames that get dropped (by
	   * design).
	   */
	  input.frame = new_frame;
   }

   protected int AdvanceQueueHead(int frame)
   {
	  Log("advancing queue head to frame %d.\n", frame);

	  int expected_frame = _first_frame ? 0 : _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))].frame + 1;

	  frame += _frame_delay;

	  if (expected_frame > frame)
	  {
		 /*
		  * This can occur when the frame delay has dropped since the last
		  * time we shoved a frame into the system.  In this case, there's
		  * no room on the queue.  Toss it.
		  */
		 Log("Dropping input frame %d (expected next frame to be %d).\n", frame, expected_frame);
		 return (int)GameInput.Constants.NullFrame;
	  }

	  while (expected_frame < frame)
	  {
		 /*
		  * This can occur when the frame delay has been increased since the last
		  * time we shoved a frame into the system.  We need to replicate the
		  * last frame in the queue several times in order to fill the space
		  * left.
		  */
		 Log("Adding padding frame %d to account for change in frame delay.\n", expected_frame);
		 GameInput last_frame = _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))];
		 AddDelayedInputToQueue(last_frame, expected_frame);
		 expected_frame++;
	  }

	  Debug.Assert(frame == 0 || frame == _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))].frame + 1);
	  return frame;
   }

   protected void AddDelayedInputToQueue(GameInput input, int frame_number)
   {
	  Log("adding delayed input frame number %d to queue.\n", frame_number);

	  Debug.Assert(input.size == _prediction.size);

	  Debug.Assert(_last_added_frame == (int)GameInput.Constants.NullFrame || frame_number == _last_added_frame + 1);

	  Debug.Assert(frame_number == 0 || _inputs[(((_head) == 0) ? (DefineConstants.INPUT_QUEUE_LENGTH - 1) : ((_head) - 1))].frame == frame_number - 1);

	  /*
	   * Add the frame to the back of the queue
	   */
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _inputs[_head] = input;
	  _inputs[_head].CopyFrom(input);
	  _inputs[_head].frame = frame_number;
	  _head = (_head + 1) % DefineConstants.INPUT_QUEUE_LENGTH;
	  _length++;
	  _first_frame = false;

	  _last_added_frame = frame_number;

	  if (_prediction.frame != (int)GameInput.Constants.NullFrame)
	  {
		 Debug.Assert(frame_number == _prediction.frame);

		 /*
		  * We've been predicting...  See if the inputs we've gotten match
		  * what we've been predicting.  If so, don't worry about it.  If not,
		  * remember the first input which was incorrect so we can report it
		  * in GetFirstIncorrectFrame()
		  */
		 if (_first_incorrect_frame == (int)GameInput.Constants.NullFrame && !_prediction.equal(input, true))
		 {
			Log("frame %d does not match prediction.  marking error.\n", frame_number);
			_first_incorrect_frame = frame_number;
		 }

		 /*
		  * If this input is the same frame as the last one requested and we
		  * still haven't found any mis-predicted inputs, we can dump out
		  * of predition mode entirely!  Otherwise, advance the prediction frame
		  * count up.
		  */
		 if (_prediction.frame == _last_frame_requested && _first_incorrect_frame == (int)GameInput.Constants.NullFrame)
		 {
			Log("prediction is correct!  dumping out of prediction mode.\n");
			_prediction.frame = (int)GameInput.Constants.NullFrame;
		 }
		 else
		 {
			_prediction.frame++;
		 }
	  }
	  Debug.Assert(_length <= DefineConstants.INPUT_QUEUE_LENGTH);
   }

   protected void Log(string fmt, params object[] LegacyParamArray)
   {
	  string buf = new string(new char[1024]);
	  size_t offset = new size_t();
//	  va_list args;

	  offset = sprintf_s(buf, ARRAY_SIZE(buf), "input q%d | ", _id);
	  int ParamCount = -1;
//	  va_start(args, fmt);
	  vsnprintf(buf + offset, ARRAY_SIZE(buf) - offset - 1, fmt, args);
	  buf = StringFunctions.ChangeCharacter(buf, ARRAY_SIZE(buf) - 1, '\0');
	  global::Log(buf);
//	  va_end(args);
   }

   protected int _id;
   protected int _head;
   protected int _tail;
   protected int _length;
   protected bool _first_frame;

   protected int _last_user_added_frame;
   protected int _last_added_frame;
   protected int _first_incorrect_frame;
   protected int _last_frame_requested;

   protected int _frame_delay;

   protected GameInput[] _inputs = Arrays.InitializeWithDefaultInstances<GameInput>(DefineConstants.INPUT_QUEUE_LENGTH);
   protected GameInput _prediction = new GameInput();
}






//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define PREVIOUS_FRAME(offset) (((offset) == 0) ? (INPUT_QUEUE_LENGTH - 1) : ((offset) - 1))
