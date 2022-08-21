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


public class TimeSync : System.IDisposable
{
   public TimeSync()
   {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_local, 0, sizeof(int));
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(_remote, 0, sizeof(int));
	  _next_prediction = DefineConstants.FRAME_WINDOW_SIZE * 3;
   }

   public virtual void Dispose()
   {
   }

   public void advance_frame(GameInput input, int advantage, int radvantage)
   {
	  // Remember the last frame and frame advantage
	  _last_inputs[input.frame % ARRAY_SIZE(_last_inputs)] = input;
	  _local[input.frame % ARRAY_SIZE(_local)] = advantage;
	  _remote[input.frame % ARRAY_SIZE(_remote)] = radvantage;
   }

//C++ TO C# CONVERTER NOTE: This was formerly a static local variable declaration (not allowed in C#):
   private int recommend_frame_wait_duration_count = 0;

   public int recommend_frame_wait_duration(bool require_idle_input)
   {
	  // Average our local and remote frame advantages
	  int i;
	  int sum = 0;
	  float advantage;
	  float radvantage;
	  for (i = 0; i < ARRAY_SIZE(_local); i++)
	  {
		 sum += _local[i];
	  }
	  advantage = sum / (float)ARRAY_SIZE(_local);

	  sum = 0;
	  for (i = 0; i < ARRAY_SIZE(_remote); i++)
	  {
		 sum += _remote[i];
	  }
	  radvantage = sum / (float)ARRAY_SIZE(_remote);

//C++ TO C# CONVERTER NOTE: This static local variable declaration (not allowed in C#) has been moved just prior to the method:
//	  static int count = 0;
	  recommend_frame_wait_duration_count++;

	  // See if someone should take action.  The person furthest ahead
	  // needs to slow down so the other user can catch up.
	  // Only do this if both clients agree on who's ahead!!
	  if (advantage >= radvantage)
	  {
		 return 0;
	  }

	  // Both clients agree that we're the one ahead.  Split
	  // the difference between the two to figure out how long to
	  // sleep for.
	  int sleep_frames = (int)(((radvantage - advantage) / 2) + 0.5);

	  Log("iteration %d:  sleep frames is %d\n", recommend_frame_wait_duration_count, sleep_frames);

	  // Some things just aren't worth correcting for.  Make sure
	  // the difference is relevant before proceeding.
	  if (sleep_frames < DefineConstants.MIN_FRAME_ADVANTAGE)
	  {
		 return 0;
	  }

	  // Make sure our input had been "idle enough" before recommending
	  // a sleep.  This tries to make the emulator sleep while the
	  // user's input isn't sweeping in arcs (e.g. fireball motions in
	  // Street Fighter), which could cause the player to miss moves.
	  if (require_idle_input)
	  {
		 for (i = 1; i < ARRAY_SIZE(_last_inputs); i++)
		 {
			if (!_last_inputs[i].equal(_last_inputs[0], true))
			{
			   Log("iteration %d:  rejecting due to input stuff at position %d...!!!\n", recommend_frame_wait_duration_count, i);
			   return 0;
			}
		 }
	  }

	  // Success!!! Recommend the number of frames to sleep and adjust
	  return MIN(sleep_frames, DefineConstants.MAX_FRAME_ADVANTAGE);
   }

   protected int[] _local = new int[DefineConstants.FRAME_WINDOW_SIZE];
   protected int[] _remote = new int[DefineConstants.FRAME_WINDOW_SIZE];
   protected GameInput[] _last_inputs = Arrays.InitializeWithDefaultInstances<GameInput>(DefineConstants.MIN_UNIQUE_FRAMES);
   protected int _next_prediction;
}
