using System;

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
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define ARRAY_SIZE(n) (sizeof(n) / sizeof(n[0]))

/*
 * gamestate.h --
 *
 * Encapsulates all the game state for the vector war application inside
 * a single structure.  This makes it trivial to implement our GGPO
 * save and load functions.
 */

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define PI ((double)3.1415926)


public class Position
{
   public double x;
   public double y;
}

public class Velocity
{
   public double dx;
   public double dy;
}

public class Bullet
{
   public bool active;
   public Position position = new Position();
   public Velocity velocity = new Velocity();
}

public class Ship
{
   public Position position = new Position();
   public Velocity velocity = new Velocity();
   public int radius;
   public int heading;
   public int health;
   public int speed;
   public int cooldown;
   public Bullet[] bullets = Arrays.InitializeWithDefaultInstances<Bullet>(DefineConstants.MAX_BULLETS);
   public int score;
}

public class GameState
{

   /*
    * InitGameState --
    *
    * Initialize our game state.
    */

   public void Init(IntPtr hwnd, int num_players)
   {
	  int i;
	  int w;
	  int h;
	  int r;

	  GetClientRect(hwnd, _bounds);
	  InflateRect(_bounds, -8, -8);

	  w = _bounds.right - _bounds.left;
	  h = _bounds.bottom - _bounds.top;
	  r = h / 4;

	  _framenumber = 0;
	  _num_ships = num_players;
	  for (i = 0; i < _num_ships; i++)
	  {
		 int heading = i * 360 / num_players;
		 double cost;
		 double sint;
		 double theta;

		 theta = (double)heading * ((double)3.1415926) / 180;
		 cost = global::cos(theta);
		 sint = global::sin(theta);

		 _ships[i].position.x = (w / 2) + r * cost;
		 _ships[i].position.y = (h / 2) + r * sint;
		 _ships[i].heading = (heading + 180) % 360;
		 _ships[i].health = DefineConstants.STARTING_HEALTH;
		 _ships[i].radius = DefineConstants.SHIP_RADIUS;
	  }

	  InflateRect(_bounds, -8, -8);
   }

   public void GetShipAI(int i, ref double heading, ref double thrust, ref int fire)
   {
	  heading = (_ships[i].heading + 5) % 360;
	  thrust = null;
	  fire = null;
   }

   public void ParseShipInputs(int inputs, int i, ref double heading, ref double thrust, ref int fire)
   {
	  Ship ship = _ships + i;

	  ggpo_log(ggpo, "parsing ship %d inputs: %d.\n", i, inputs);

	  if ((inputs & (int)VectorWarInputs.INPUT_ROTATE_RIGHT) != 0)
	  {
		 heading = (ship.heading + DefineConstants.ROTATE_INCREMENT) % 360;
	  }
	  else if (inputs & (int)VectorWarInputs.INPUT_ROTATE_LEFT)
	  {
		 heading = (ship.heading - DefineConstants.ROTATE_INCREMENT + 360) % 360;
	  }
	  else
	  {
		 heading = ship.heading;
	  }

	  if ((inputs & (int)VectorWarInputs.INPUT_THRUST) != 0)
	  {
		 thrust = DefineConstants.SHIP_THRUST;
	  }
	  else if (inputs & (int)VectorWarInputs.INPUT_BREAK)
	  {
		 thrust = -DefineConstants.SHIP_THRUST;
	  }
	  else
	  {
		 thrust = null;
	  }
	  fire = inputs & (int)VectorWarInputs.INPUT_FIRE;
   }

   public void MoveShip(int which, double heading, double thrust, int fire)
   {
	  Ship ship = _ships + which;

	  ggpo_log(ggpo, "calculation of new ship coordinates: (thrust:%.4f heading:%.4f).\n", thrust, heading);

	  ship.heading = (int)heading;

	  if (ship.cooldown == 0)
	  {
		 if (fire != 0)
		 {
			ggpo_log(ggpo, "firing bullet.\n");
			for (int i = 0; i < DefineConstants.MAX_BULLETS; i++)
			{
			   double dx = global::cos(Globals.degtorad(ship.heading));
			   double dy = global::sin(Globals.degtorad(ship.heading));
			   if (!ship.bullets[i].active)
			   {
				  ship.bullets[i].active = true;
				  ship.bullets[i].position.x = ship.position.x + (ship.radius * dx);
				  ship.bullets[i].position.y = ship.position.y + (ship.radius * dy);
				  ship.bullets[i].velocity.dx = ship.velocity.dx + (DefineConstants.BULLET_SPEED * dx);
				  ship.bullets[i].velocity.dy = ship.velocity.dy + (DefineConstants.BULLET_SPEED * dy);
				  ship.cooldown = DefineConstants.BULLET_COOLDOWN;
				  break;
			   }
			}
		 }
	  }

	  if (thrust != 0)
	  {
		 double dx = thrust * global::cos(Globals.degtorad(heading));
		 double dy = thrust * global::sin(Globals.degtorad(heading));

		 ship.velocity.dx += dx;
		 ship.velocity.dy += dy;
		 double mag = Math.Sqrt(ship.velocity.dx * ship.velocity.dx + ship.velocity.dy * ship.velocity.dy);
		 if (mag > DefineConstants.SHIP_MAX_THRUST)
		 {
			ship.velocity.dx = (ship.velocity.dx * DefineConstants.SHIP_MAX_THRUST) / mag;
			ship.velocity.dy = (ship.velocity.dy * DefineConstants.SHIP_MAX_THRUST) / mag;
		 }
	  }
	  ggpo_log(ggpo, "new ship velocity: (dx:%.4f dy:%2.f).\n", ship.velocity.dx, ship.velocity.dy);

	  ship.position.x += ship.velocity.dx;
	  ship.position.y += ship.velocity.dy;
	  ggpo_log(ggpo, "new ship position: (dx:%.4f dy:%2.f).\n", ship.position.x, ship.position.y);

	  if (ship.position.x - ship.radius < _bounds.left || ship.position.x + ship.radius > _bounds.right)
	  {
		 ship.velocity.dx *= -1;
		 ship.position.x += (ship.velocity.dx * 2);
	  }
	  if (ship.position.y - ship.radius < _bounds.top || ship.position.y + ship.radius > _bounds.bottom)
	  {
		 ship.velocity.dy *= -1;
		 ship.position.y += (ship.velocity.dy * 2);
	  }
	  for (int i = 0; i < DefineConstants.MAX_BULLETS; i++)
	  {
		 Bullet bullet = ship.bullets + i;
		 if (bullet.active)
		 {
			bullet.position.x += bullet.velocity.dx;
			bullet.position.y += bullet.velocity.dy;
			if (bullet.position.x < _bounds.left || bullet.position.y < _bounds.top || bullet.position.x > _bounds.right || bullet.position.y > _bounds.bottom)
			{
			   bullet.active = false;
			}
			else
			{
			   for (int j = 0; j < _num_ships; j++)
			   {
				  Ship other = _ships + j;
				  if (Globals.distance(bullet.position, other.position) < other.radius)
				  {
					 ship.score++;
					 other.health -= DefineConstants.BULLET_DAMAGE;
					 bullet.active = false;
					 break;
				  }
			   }
			}
		 }
	  }
   }

   public void Update(int[] inputs, int disconnect_flags)
   {
	  _framenumber++;
	  for (int i = 0; i < _num_ships; i++)
	  {
		 double thrust;
		 double heading;
		 int fire;

		 if ((disconnect_flags & (1 << i)) != 0)
		 {
			GetShipAI(i, ref heading, ref thrust, ref fire);
		 }
		 else
		 {
			ParseShipInputs(inputs[i], i, ref heading, ref thrust, ref fire);
		 }
		 MoveShip(i, heading, thrust, fire);

		 if ((_ships[i].cooldown) != 0)
		 {
			_ships[i].cooldown--;
		 }
	  }
   }

   public int _framenumber;
   public RECT _bounds = new RECT();
   public int _num_ships;
   public Ship[] _ships = Arrays.InitializeWithDefaultInstances<Ship>(DefineConstants.MAX_SHIPS);
}