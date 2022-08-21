using System;
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





public class IPollSink : System.IDisposable
{
   public virtual void Dispose()
   {
   }
   public virtual bool OnHandlePoll(object UnnamedParameter)
   {
	   return true;
   }
   public virtual bool OnMsgPoll(object UnnamedParameter)
   {
	   return true;
   }
   public virtual bool OnPeriodicPoll(object UnnamedParameter, int UnnamedParameter2)
   {
	   return true;
   }
   public virtual bool OnLoopPoll(object UnnamedParameter)
   {
	   return true;
   }
}

public class Poll
{
   public Poll()
   {
	   this._handle_count = 0;
	   this._start_time = 0;
	  /*
	   * Create a dummy handle to simplify things.
	   */
	  _handles[_handle_count++] = CreateEvent(null, true, false, null);
   }

   public void RegisterHandle(IPollSink sink, IntPtr h, object cookie = null)
   {
	  Debug.Assert(_handle_count < DefineConstants.MAX_POLLABLE_HANDLES - 1);

	  _handles[_handle_count] = h;
	  _handle_sinks[_handle_count] = new PollSinkCb(sink, cookie);
	  _handle_count++;
   }

   public void RegisterMsgLoop(IPollSink sink, object cookie = null)
   {
	  _msg_sinks.push_back(new PollSinkCb(sink, cookie));
   }

   public void RegisterPeriodic(IPollSink sink, int interval, object cookie = null)
   {
	  _periodic_sinks.push_back(new PollPeriodicSinkCb(sink, cookie, interval));
   }

   public void RegisterLoop(IPollSink sink, object cookie = null)
   {
	  _loop_sinks.push_back(new PollSinkCb(sink, cookie));
   }

   public void Run()
   {
	  while (Pump(100))
	  {
		 continue;
	  }
   }

   public bool Pump(int timeout)
   {
	  int i;
	  int res;
	  bool finished = false;

	  if (_start_time == 0)
	  {
		 _start_time = (int)Platform.GetCurrentTimeMS();
	  }
	  int elapsed = (int)(Platform.GetCurrentTimeMS() - _start_time);
	  int maxwait = ComputeWaitTime(elapsed);
	  if (maxwait != INFINITE)
	  {
		 timeout = MIN(timeout, maxwait);
	  }

	  res = WaitForMultipleObjects(_handle_count, _handles, false, timeout);
	  if (res >= WAIT_OBJECT_0 && res < WAIT_OBJECT_0 + _handle_count)
	  {
		 i = res - WAIT_OBJECT_0;
		 finished = !_handle_sinks[i].sink.OnHandlePoll(_handle_sinks[i].cookie) || finished;
	  }
	  for (i = 0; i < _msg_sinks.size(); i++)
	  {
		 PollSinkCb cb = _msg_sinks[i];
		 finished = !cb.sink.OnMsgPoll(cb.cookie) || finished;
	  }

	  for (i = 0; i < _periodic_sinks.size(); i++)
	  {
		 PollPeriodicSinkCb cb = _periodic_sinks[i];
		 if (cb.interval + cb.last_fired <= elapsed)
		 {
			cb.last_fired = (int)(elapsed / cb.interval) * cb.interval;
			finished = !cb.sink.OnPeriodicPoll(cb.cookie, cb.last_fired) || finished;
		 }
	  }

	  for (i = 0; i < _loop_sinks.size(); i++)
	  {
		 PollSinkCb cb = _loop_sinks[i];
		 finished = !cb.sink.OnLoopPoll(cb.cookie) || finished;
	  }
	  return finished;
   }

   protected int ComputeWaitTime(int elapsed)
   {
	  int waitTime = INFINITE;
	  size_t count = _periodic_sinks.size();

	  if (count > 0)
	  {
		 for (int i = 0; i < count; i++)
		 {
			PollPeriodicSinkCb cb = _periodic_sinks[i];
			int timeout = (int)((cb.interval + cb.last_fired) - elapsed);
			if (waitTime == INFINITE || (timeout < waitTime))
			{
			   waitTime = MAX(timeout, 0);
			}
		 }
	  }
	  return waitTime;
   }

   protected class PollSinkCb
   {
	  public IPollSink sink;
	  public object cookie;
	  public PollSinkCb()
	  {
		  this.sink = null;
		  this.cookie = null;
	  }
	  public PollSinkCb(IPollSink s, object c)
	  {
		  this.sink = s;
		  this.cookie = c;
	  }
   }

   protected class PollPeriodicSinkCb : PollSinkCb
   {
	  public int interval;
	  public int last_fired;
	  public PollPeriodicSinkCb() : base(null, null)
	  {
		  this.interval = 0;
		  this.last_fired = 0;
	  }
	  public PollPeriodicSinkCb(IPollSink s, object c, int i) : base(s, c)
	  {
		  this.interval = i;
		  this.last_fired = 0;
	  }
   }

   protected int _start_time;
   protected int _handle_count;
   protected IntPtr[] _handles = new IntPtr[DefineConstants.MAX_POLLABLE_HANDLES];
   protected PollSinkCb[] _handle_sinks = Arrays.InitializeWithDefaultInstances<PollSinkCb>(DefineConstants.MAX_POLLABLE_HANDLES);

   protected StaticBuffer<PollSinkCb, 16> _msg_sinks = new StaticBuffer<PollSinkCb, 16>();
   protected StaticBuffer<PollSinkCb, 16> _loop_sinks = new StaticBuffer<PollSinkCb, 16>();
   protected StaticBuffer<PollPeriodicSinkCb, 16> _periodic_sinks = new StaticBuffer<PollPeriodicSinkCb, 16>();
}
