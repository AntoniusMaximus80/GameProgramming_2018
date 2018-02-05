using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TankGame.WaypointSystem;
using TankGame.Systems;
using UnityEngine;

namespace TankGame.AI
{
	public class PatrolState : AIStateBase
	{
		private Path _path;
		private Direction _direction;
		private float _arriveDistance;

		public Waypoint CurrentWaypoint { get; private set; }

		public PatrolState(EnemyUnit owner, Path path, 
			Direction direction, float arriveDistance )
			: base()
		{
			State = AIStateType.Patrol;
			Owner = owner;
			AddTransition( AIStateType.FollowTarget );
			_path = path;
			_direction = direction;
			_arriveDistance = arriveDistance;
		}

		public override void StateActivated()
		{
			base.StateActivated();
			CurrentWaypoint = _path.GetClosestWaypoint( Owner.transform.position );
		}

		public override void Update()
		{
			// 1. Should we change the state?
			//   1.1 If yes, change state and return.

			if ( !ChangeState() )
			{
				// 2. Are we close enough the current waypoint?
				//   2.1 If yes, get the next waypoint
				CurrentWaypoint = GetWaypoint();
				// 3. Move towards the current waypoint
				// 4. Rotate towards the current waypoint
			}
		}

		private Waypoint GetWaypoint()
		{
			Waypoint result = CurrentWaypoint;
			Vector3 toWaypointVector = CurrentWaypoint.Position - Owner.transform.position;
			float toWaypointSqr = toWaypointVector.sqrMagnitude;
			float sqrArriveDistance = Owner.DetectEnemyDistance * Owner.DetectEnemyDistance;
			if ( toWaypointSqr <= sqrArriveDistance )
			{
				result = _path.GetNextWaypoint( CurrentWaypoint, ref _direction );
			}

			return result;
		}

		private bool ChangeState()
		{
			//int mask = LayerMask.GetMask( "Player" );
			int playerLayer = LayerMask.NameToLayer( "Player" );
			int mask = Flags.CreateMask( playerLayer );

			Collider[] players = Physics.OverlapSphere( Owner.transform.position,
				Owner.DetectEnemyDistance, mask );
			if ( players.Length > 0 )
			{
				PlayerUnit player =
					players[ 0 ].gameObject.GetComponentInHierarchy< PlayerUnit >();
				Owner.Target = player;
				Owner.PerformTransition( AIStateType.FollowTarget );
				return true;
			}
			return false;
		}
	}
}