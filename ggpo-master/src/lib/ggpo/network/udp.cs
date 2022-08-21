using System.Diagnostics;

public class Udp : IPollSink, System.IDisposable
{
   public class Stats
   {
	  public int bytes_sent;
	  public int packets_sent;
	  public float kbps_sent;
   }

   public abstract class Callbacks : System.IDisposable
   {
	  public virtual void Dispose()
	  {
	  }
	  public abstract void OnMsg(sockaddr_in from, UdpMsg msg, int len);
   }


   protected void Log(string fmt, params object[] LegacyParamArray)
   {
	  string buf = new string(new char[1024]);
	  size_t offset = new size_t();
//	  va_list args;

	  strcpy_s(buf, "udp | ");
	  offset = strlen(buf);
	  int ParamCount = -1;
//	  va_start(args, fmt);
	  vsnprintf(buf + offset, ARRAY_SIZE(buf) - offset - 1, fmt, args);
	  buf = StringFunctions.ChangeCharacter(buf, ARRAY_SIZE(buf) - 1, '\0');
	  global::Log(buf);
//	  va_end(args);
   }

   public Udp()
   {
	   this._socket = INVALID_SOCKET;
	   this._callbacks = null;
   }

   public void Init(ushort port, Poll poll, Callbacks callbacks)
   {
	  _callbacks = callbacks;

	  _poll = poll;
	  _poll.RegisterLoop(this);

	  Log("binding udp socket to port %d.\n", port);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: _socket = CreateSocket(port, 0);
	  _socket.CopyFrom(Globals.CreateSocket(port, 0));
   }

   public void SendTo(ref string buffer, int len, int flags, sockaddr dst, int destlen)
   {
	  sockaddr_in to = (sockaddr_in)dst;

	  int res = sendto(_socket, buffer, len, flags, dst, destlen);
	  if (res == SOCKET_ERROR)
	  {
		 uint err = WSAGetLastError();
		 Log("unknown error in sendto (erro: %d  wsaerr: %d).\n", res, err);
		 Debug.Assert(false && "Unknown error in sendto");
	  }
	  string dst_ip = new string(new char[1024]);
	  Log("sent packet length %d to %s:%d (ret:%d).\n", len, inet_ntop(AF_INET, (object) to.sin_addr, dst_ip, ARRAY_SIZE(dst_ip)), ntohs(to.sin_port), res);
   }

   public virtual bool OnLoopPoll(object cookie)
   {
	  byte[] recv_buf = new byte[Globals.MAX_UDP_PACKET_SIZE];
	  sockaddr_in recv_addr = new sockaddr_in();
	  int recv_addr_len;

	  for (;;)
	  {
		 recv_addr_len = sizeof(sockaddr_in);
		 int len = recvfrom(_socket, (string)recv_buf, Globals.MAX_UDP_PACKET_SIZE, 0, (sockaddr) recv_addr, recv_addr_len);

		 // TODO: handle len == 0... indicates a disconnect.

		 if (len == -1)
		 {
			int error = WSAGetLastError();
			if (error != WSAEWOULDBLOCK)
			{
			   Log("recvfrom WSAGetLastError returned %d (%x).\n", error, error);
			}
			break;
		 }
		 else if (len > 0)
		 {
			string src_ip = new string(new char[1024]);
			Log("recvfrom returned (len:%d  from:%s:%d).\n", len, inet_ntop(AF_INET, (object) recv_addr.sin_addr, src_ip, ARRAY_SIZE(src_ip)), ntohs(recv_addr.sin_port));
			UdpMsg msg = (UdpMsg)recv_buf;
			_callbacks.OnMsg(recv_addr, msg, len);
		 }
	  }
	  return true;
   }

   public void Dispose()
   {
	  if (_socket != INVALID_SOCKET)
	  {
		 closesocket(_socket);
		 _socket = INVALID_SOCKET;
	  }
   }

   // Network transmission information
   protected SOCKET _socket = new SOCKET();

   // state management
   protected Callbacks _callbacks;
   protected Poll _poll;
}