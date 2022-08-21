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

//C++ TO C# CONVERTER TODO TASK: The following C++ template specifier cannot be converted to C#:
//ORIGINAL LINE: template<class T, int N>
public class RingBuffer <T>
{
//C++ TO C# CONVERTER TODO TASK: C++ template specialization was removed by C++ to C# Converter:
//ORIGINAL LINE: RingBuffer<T, N>() : _head(0), _tail(0), _size(0)
   public RingBuffer()
   {
	   this._head = 0;
	   this._tail = 0;
	   this._size = 0;
   }

   public T front()
   {
	  Debug.Assert(_size != N);
	  return _elements[_tail];
   }

   public T item(int i)
   {
	  Debug.Assert(i < _size);
	  return _elements[(_tail + i) % N];
   }

   public void pop()
   {
	  Debug.Assert(_size != N);
	  _tail = (_tail + 1) % N;
	  _size--;
   }

   public void push(in T t)
   {
	  Debug.Assert(_size != (N - 1));
	  _elements[_head] = t;
	  _head = (_head + 1) % N;
	  _size++;
   }

   public int size()
   {
	  return _size;
   }

   public bool empty()
   {
	  return _size == 0;
   }

   protected T[] _elements = Arrays.InitializeWithDefaultInstances<T>(N);
   protected int _head;
   protected int _tail;
   protected int _size;
}

