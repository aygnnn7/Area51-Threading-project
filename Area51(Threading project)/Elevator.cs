using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Area51.Agent;
using static Program;

namespace Area51
{
    internal class Elevator
    {
        const int Capacity = 1;
        const int SecondsPerFloor = 1;
        Floor CurrentFloor = Floor.G;
        Semaphore CabinEnter;
        List<Agent> Agents;
        public ManualResetEvent GoHomeSignal { get; private set; }

        public Elevator()
        {
            CabinEnter = new Semaphore(Capacity, Capacity);
            GoHomeSignal = new ManualResetEvent(false);
            Agents = new List<Agent>();
        }
        private List<Floor> HasAccesTo(SecurityLevel securityLevel)
        {
            List<Floor> floors = new List<Floor>();
            floors.Add(Floor.G);
            if (Agent.SecurityLevel.Confidential == securityLevel)
                return floors;

            floors.Add(Floor.S);
            if (Agent.SecurityLevel.Secret == securityLevel)
                return floors;

            floors.Add(Floor.T1);
            floors.Add(Floor.T2);

            return floors;

        }

        //Try enter elevator
        public void TryEnter(Agent agent)
        {
            //wait signal from cabin semaphore and than enter
            CabinEnter.WaitOne();
            lock (Agents) Agents.Add(agent);

            //wait till elevator comes
            CallElevator(agent.currentFloor);

            //Put extra message if he is not in base
            string extraMessage = "";
            if (agent.currentFloor == Floor.NotInBase)
            {
                extraMessage = $"comes from home";
                agent.currentFloor = Floor.G;
            }

            Console.WriteLine("");
            Console.WriteLine($"Agent {agent.Id} ({agent.securityLevel}) - {extraMessage} enters elevator from floor {agent.currentFloor}.");
        }

        //Try get out of elevator
        public bool TryExit(Agent agent, Floor arrivedFloor, bool isGoHome)
        {
            //All accessed floors by agent
            List<Floor> AccessedFloors = HasAccesTo(agent.securityLevel);

            //if agent has access to selected/arrived floor 
            if (AccessedFloors.Where(accessedF => accessedF.ToString() == arrivedFloor.ToString()).Count() > 0)
            {
                //getting out of elevator
                Console.WriteLine($"Agent {agent.Id} got out of the elevator on the {arrivedFloor} floor.");
                if (isGoHome)
                {
                    Console.WriteLine($"Agent {agent.Id} is going back home.");
                }

                lock (Agents) Agents.Remove(agent);
                CabinEnter.Release();
                return true;
            }
            //if agent has not access to selected/arrived floor
            else
            {
                Console.WriteLine($"{agent.Id} arrived {arrivedFloor} floor but he has not permission to enter.");
                return false;
            }
        }

        public void GoBackHome(Agent agent)
        {
            //if agent is not on the ground floor
            if (agent.currentFloor != Floor.G)
            {
                //using the elevator to get to the ground floor
                TryEnter(agent);
                MoveAgent(agent, Floor.G);
                TryExit(agent, Floor.G, isGoHome: true);
            }
            //if agent is already on the ground floor
            else
            {
                //Goes home directly, without using elevator
                CabinEnter.WaitOne();
                Console.WriteLine();
                Console.WriteLine($"Agent {agent.Id} was on the floor {Floor.G}, so he is not waiting the elevator, going back home directly.");
                CabinEnter.Release();
            }
        }

        //Moving elevator between the floors
        public void MoveAgent(Agent agent, Floor toFloor)
        {
            Move(agent.currentFloor, toFloor, agent.Id);
        }

        private void CallElevator(Floor calledFrom)
        {
            if (calledFrom == Floor.NotInBase) calledFrom = Floor.G;
            Move(CurrentFloor, calledFrom, -1);
        }

        private void Move(Floor fromFloor, Floor toFloor, int agentId)
        {

            if (agentId == -1 && fromFloor == toFloor)
            {
                //elevators CurrentFloor is same as call floor
                Console.WriteLine();
                Console.WriteLine($"The elevator is called from {toFloor} and its already there.");
                return;
            }

            //Moving speed of the elevator between two floors
            int speedInSeconds = GetFloorCountBetweenFloors(fromFloor, toFloor) * SecondsPerFloor;
            int milliSeconds = speedInSeconds * 1000;

            //if speed is 0 adding time for thinking
            if (milliSeconds == 0) milliSeconds = 500;

            //The elevator is moving with the agent
            if (agentId > 0)
            {

                Console.WriteLine($"Agent {agentId} clicked the inside elevator button {toFloor}; in {speedInSeconds} seconds will be reached from {fromFloor} to {toFloor}.");
                Console.Write("Elevator is moving");
            }
            //The elevator is empty
            else
            {
                Console.WriteLine();
                Console.WriteLine($"The elevator is called from {toFloor}; in {speedInSeconds} seconds will be reached from {fromFloor} to {toFloor}.");
                Console.Write("Elevator is coming");
            }
            int countOfDot = 3;
            for (int i = 0; i < countOfDot; i++)
            {
                Console.Write(".");
                Thread.Sleep(milliSeconds / countOfDot);
            }
            Console.WriteLine();
            //Arrived
            CurrentFloor = toFloor;
        }
        public static int GetFloorCountBetweenFloors(Floor fromFloor, Floor toFloor)
        {
            int fromFloorNumber = Floors.IndexOf(fromFloor);
            int toFloorNumber = Floors.IndexOf(toFloor);
            return Math.Abs(toFloorNumber - fromFloorNumber);
        }
        public void Activate()
        {
            Console.WriteLine("The day starts.");
            Thread.Sleep(20000);
            GoHomeSignal.Set();
            foreach (var t in agentThreads)
            {
                t.Join();
            }
            Console.WriteLine();
            Console.WriteLine("The working hours are over.");
            Console.ReadLine();
        }


    }
}
