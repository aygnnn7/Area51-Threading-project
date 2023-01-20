using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Program;


namespace Area51
{
    internal class Agent
    {
        public enum Floor { NotInBase, G, S, T1, T2 }
        public enum SecurityLevel { Confidential, Secret, TopSecret }

        public static List<Floor> Floors = Enum.GetValues(typeof(Floor)).Cast<Floor>().Where(p => p != Floor.NotInBase).ToList();


        public int Id { get; set; }
        public Floor currentFloor;
        public SecurityLevel securityLevel;
        public Agent(int id, SecurityLevel securityLevel)
        {
            this.Id = id;
            this.securityLevel = securityLevel;
            this.currentFloor = Floor.NotInBase;
        }

        private bool Throw(int chance)
        {
            Random random = new Random();
            int dice = random.Next(100);
            return dice < chance;
        }
        private Floor GetRandomFloor()
        {
            Random rn = new Random();
            return Floors[rn.Next(Floors.Count)];
        }

        public void DoWork()
        {
            //work day of agent
            while (true)
            {
                //Signal check
                if (elevator.GoHomeSignal.WaitOne(0))
                {
                    //work hours are over, give signal, all agents will go home
                    elevator.GoBackHome(this);
                    currentFloor = Floor.NotInBase;
                    //day ends
                    break;
                }

                //80% go base 
                if (Throw(80))
                {
                    //Going...
                    Thread.Sleep(1000);
                    //Try to enter elevator
                    elevator.TryEnter(this);

                    //Choose floor to go
                    while (true)
                    {
                        //Thinking..
                        Thread.Sleep(200);

                        //Get random floor
                        Floor randomFloor = GetRandomFloor();

                        //Moving...
                        elevator.MoveAgent(this, randomFloor);

                        //Arrives
                        currentFloor = randomFloor;

                        //SecurityLevel check
                        if (elevator.TryExit(this, randomFloor, false))
                        {
                            //Has permission to exit
                            break;
                        }
                        //has not permission to exit choose floor again
                    }
                }

                //20% go home
                else
                {
                    //if he is in base
                    if (this.currentFloor != Floor.NotInBase)
                    {

                        elevator.GoBackHome(this);
                        currentFloor = Floor.NotInBase;
                    }
                    //if he is already at home
                    else
                    {
                        Thread.Sleep(200);
                        Console.WriteLine($"Agent {Id} is not working today.");
                    }
                    //day ends
                    break;
                }
            }
        }



    }
}
